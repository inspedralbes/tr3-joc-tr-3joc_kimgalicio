// src/websockets/game.ws.js
// Gestió de la comunicació en temps real per a les partides multijugador.
// Utilitza la llibreria 'ws' (WebSockets natius) i comparteix el port HTTP
// d'Express per no necessitar un port addicional.
//
// ─── Estructura de dades interna ───────────────────────────────────────────
//
//  partides: Map<gameId, Map<userId, WebSocket>>
//
//  Cada gameId apunta a un Map interior que relaciona cada userId amb el seu
//  socket, permetent enviar missatges a un jugador concret de forma eficient.
//
// ─── Esdeveniments acceptats del client ────────────────────────────────────
//
//  { action: 'join',          gameId,      userId }         → Registre inicial
//  { action: 'move',          gameId,      userId, x, y }   → Posició dinosaure
//  { action: 'bomb_transfer', gameId,      userId, newOwnerId } → Passada de bomba
//  { action: 'explosion',     gameId,      userId, livesLeft, loserId } → Explosió
//
// ─── Esdeveniments emesos pel servidor ─────────────────────────────────────
//
//  { action: 'move',                 x, y, userId }
//  { action: 'bomb_transfer',        newOwnerId }
//  { action: 'explosion',            livesLeft, loserId }
//  { action: 'opponent_disconnected' }
//  { action: 'error',                missatge }

const { WebSocketServer } = require('ws');

/**
 * Inicialitza i configura el servidor WebSocket adjunt al servidor HTTP d'Express.
 * @param {import('http').Server} servidorHttp - El servidor HTTP retornat per app.listen().
 */
function inicialitzarWebSocket(servidorHttp) {
  // Creem el servidor WS compartint el mateix port HTTP (sense 'port' propi)
  const wss = new WebSocketServer({ server: servidorHttp });

  // Map principal: gameId (string) → Map<userId (string), WebSocket>
  const partides = new Map();

  // ── Helpers ──────────────────────────────────────────────────────────────

  /**
   * Serialitza i envia un objecte JSON a un socket concret.
   * Comprova que el socket estigui obert abans d'enviar.
   * @param {WebSocket} socket
   * @param {Object} dades
   */
  function enviar(socket, dades) {
    if (socket.readyState === socket.OPEN) {
      socket.send(JSON.stringify(dades));
    }
  }

  /**
   * Envia un missatge a tots els jugadors d'una partida excepte al remitent.
   * (Broadcast al rival)
   * @param {string}    gameId  - ID de la partida.
   * @param {string}    userId  - ID del jugador que envia (s'exclou del broadcast).
   * @param {Object}    dades   - Missatge a enviar.
   */
  function broadcast(gameId, userId, dades) {
    const jugadors = partides.get(gameId);
    if (!jugadors) return;

    jugadors.forEach((socket, idJugador) => {
      if (idJugador !== userId) {
        enviar(socket, dades);
      }
    });
  }

  /**
   * Elimina un jugador del registre intern i avisa el seu rival.
   * @param {string} gameId
   * @param {string} userId
   */
  function gestionarDesconnexio(gameId, userId) {
    const jugadors = partides.get(gameId);
    if (!jugadors) return;

    // Eliminem el jugador desconnectat
    jugadors.delete(userId);
    console.log(`[WS] Jugador ${userId} desconnectat de la partida ${gameId}.`);

    // Avisem l'oponent si encara hi és
    if (jugadors.size > 0) {
      broadcast(gameId, '', { action: 'opponent_disconnected' });
      // Nota: passem '' com a userId per fer broadcast a TOTHOM restant
      // ja que l'original ja ha estat eliminat del Map
    }

    // Si la partida queda buida, la netejem del Map principal
    if (jugadors.size === 0) {
      partides.delete(gameId);
      console.log(`[WS] Partida ${gameId} tancada (sense jugadors).`);
    }
  }

  // ── Gestió de connexions entrants ─────────────────────────────────────────

  wss.on('connection', (ws) => {
    // Variables de context per a aquest socket concret
    // S'assignen quan arriba l'acció 'join'
    let gameIdActual = null;
    let userIdActual = null;

    console.log('[WS] Nova connexió entrant. Esperant missatge "join"...');

    // ── Gestió de missatges rebuts ──────────────────────────────────────────
    ws.on('message', (missatgeCru) => {
      let dades;

      // Parsing segur del JSON
      try {
        dades = JSON.parse(missatgeCru.toString());
      } catch {
        enviar(ws, { action: 'error', missatge: 'Format de missatge invàlid. Ha de ser JSON.' });
        return;
      }

      const { action, gameId, userId } = dades;

      // Validació dels camps comuns a tots els missatges
      if (!action) {
        enviar(ws, { action: 'error', missatge: 'El camp "action" és obligatori.' });
        return;
      }

      // ── Acció: JOIN ────────────────────────────────────────────────────────
      // Primera acció que ha de fer qualsevol client en connectar-se.
      // Registra el jugador a la partida indicada.
      if (action === 'join') {
        if (!gameId || !userId) {
          enviar(ws, { action: 'error', missatge: 'L\'acció "join" requereix "gameId" i "userId".' });
          return;
        }

        // Guardem el context d'aquest socket
        gameIdActual = String(gameId);
        userIdActual = String(userId);

        // Creem el Map de jugadors si la partida és nova
        if (!partides.has(gameIdActual)) {
          partides.set(gameIdActual, new Map());
        }

        // Registrem el socket associat a aquest jugador
        partides.get(gameIdActual).set(userIdActual, ws);

        const numJugadors = partides.get(gameIdActual).size;
        console.log(`[WS] Jugador ${userIdActual} unit a la partida ${gameIdActual} (${numJugadors}/2 jugadors).`);

        // Confirmem la connexió al jugador
        enviar(ws, {
          action:     'joined',
          gameId:     gameIdActual,
          userId:     userIdActual,
          numJugadors,
        });
        return;
      }

      // ── Validació: la resta d'accions requereixen haver fet 'join' primer ──
      if (!gameIdActual || !userIdActual) {
        enviar(ws, { action: 'error', missatge: 'Has de fer "join" abans d\'enviar altres accions.' });
        return;
      }

      // ── Acció: MOVE ────────────────────────────────────────────────────────
      // Reenvia la posició del dinosaure de l'emissor al seu rival.
      // Dades esperades: { action: 'move', gameId, userId, x, y }
      if (action === 'move') {
        const { x, y } = dades;
        if (x === undefined || y === undefined) {
          enviar(ws, { action: 'error', missatge: 'L\'acció "move" requereix "x" i "y".' });
          return;
        }
        broadcast(gameIdActual, userIdActual, { action: 'move', userId: userIdActual, x, y });
        return;
      }

      // ── Acció: BOMB_TRANSFER ───────────────────────────────────────────────
      // S'emet quan un jugador toca l'altre i li passa la bomba.
      // Dades esperades: { action: 'bomb_transfer', gameId, userId, newOwnerId }
      if (action === 'bomb_transfer') {
        const { newOwnerId } = dades;
        if (!newOwnerId) {
          enviar(ws, { action: 'error', missatge: 'L\'acció "bomb_transfer" requereix "newOwnerId".' });
          return;
        }
        broadcast(gameIdActual, userIdActual, { action: 'bomb_transfer', newOwnerId });
        console.log(`[WS] Partida ${gameIdActual}: Bomba transferida a ${newOwnerId}.`);
        return;
      }

      // ── Acció: EXPLOSION ───────────────────────────────────────────────────
      // El client Unity notifica que el temporitzador de 30s ha arribat a zero.
      // El servidor reenvia l'event al rival per sincronitzar les vides.
      // Dades esperades: { action: 'explosion', gameId, userId, livesLeft, loserId }
      if (action === 'explosion') {
        const { livesLeft, loserId } = dades;
        if (livesLeft === undefined || !loserId) {
          enviar(ws, { action: 'error', missatge: 'L\'acció "explosion" requereix "livesLeft" i "loserId".' });
          return;
        }
        broadcast(gameIdActual, userIdActual, { action: 'explosion', livesLeft, loserId });
        console.log(`[WS] Partida ${gameIdActual}: Explosió! ${loserId} perd una vida. Li'n queden ${livesLeft}.`);
        return;
      }

      // ── Acció desconeguda ──────────────────────────────────────────────────
      enviar(ws, { action: 'error', missatge: `Acció desconeguda: "${action}".` });
    });

    // ── Gestió de desconnexió ───────────────────────────────────────────────
    ws.on('close', () => {
      if (gameIdActual && userIdActual) {
        gestionarDesconnexio(gameIdActual, userIdActual);
      }
    });

    // ── Gestió d'errors de socket ───────────────────────────────────────────
    ws.on('error', (error) => {
      console.error(`[WS] Error al socket del jugador ${userIdActual ?? 'desconegut'}:`, error.message);
      // La desconnexió es gestiona automàticament via l'event 'close'
    });
  });

  console.log('[WS] Servidor WebSocket inicialitzat i adjunt al port HTTP.');
  return wss;
}

module.exports = inicialitzarWebSocket;
