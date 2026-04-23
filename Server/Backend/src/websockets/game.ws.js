const { WebSocketServer } = require('ws');

function inicialitzarWebSocket(servidorHttp) {

  const wss = new WebSocketServer({ server: servidorHttp });

  const partides = new Map();

  function enviar(socket, dades) {
    if (socket.readyState === socket.OPEN) {
      if (dades.action === 'error') {
        console.log(`[WS] Enviant ERROR al client: ${dades.missatge}`);
      }
      socket.send(JSON.stringify(dades));
    }
  }

  function broadcast(gameId, userId, dades) {
    const jugadors = partides.get(gameId);
    if (!jugadors) return;

    jugadors.forEach((socket, idJugador) => {
      if (idJugador !== userId) {
        enviar(socket, dades);
      }
    });
  }

  function gestionarDesconnexio(gameId, userId) {
    const jugadors = partides.get(gameId);
    if (!jugadors) return;

    jugadors.delete(userId);
    console.log(`[WS] Jugador ${userId} desconnectat de la partida ${gameId}.`);

    if (jugadors.size > 0) {
      broadcast(gameId, '', { action: 'opponent_disconnected' });

    }

    if (jugadors.size === 0) {
      partides.delete(gameId);
      console.log(`[WS] Partida ${gameId} tancada (sense jugadors).`);
    }
  }

  wss.on('connection', (ws) => {

    let gameIdActual = null;
    let userIdActual = null;

    console.log('[WS] Nova connexió entrant. Esperant missatge "join"...');

    ws.on('message', (missatgeCru) => {
      let dades;

      try {
        dades = JSON.parse(missatgeCru.toString());
      } catch {
        enviar(ws, { action: 'error', missatge: 'Format de missatge invàlid. Ha de ser JSON.' });
        return;
      }

      const { action, gameId, userId } = dades;

      if (!action) {
        enviar(ws, { action: 'error', missatge: 'El camp "action" és obligatori.' });
        return;
      }

      if (action === 'join') {
        if (!gameId || !userId) {
          enviar(ws, { action: 'error', missatge: 'L\'acció "join" requereix "gameId" i "userId".' });
          return;
        }

        gameIdActual = String(gameId);
        userIdActual = String(userId);

        if (!partides.has(gameIdActual)) {
          partides.set(gameIdActual, new Map());
        }

        partides.get(gameIdActual).set(userIdActual, ws);

        const numJugadors = partides.get(gameIdActual).size;
        console.log(`[WS] Jugador ${userIdActual} unit a la partida ${gameIdActual} (${numJugadors}/2 jugadors).`);

        enviar(ws, {
          action:     'joined',
          gameId:     gameIdActual,
          userId:     userIdActual,
          numJugadors,
        });

        if (numJugadors === 2) {
          console.log(`[WS] Partida ${gameIdActual} PLENA. Enviant "game_ready" a tots els jugadors.`);
          broadcast(gameIdActual, '', {
            action: 'game_ready',
            gameId: gameIdActual
          });
        }

        return;
      }

      if (!gameIdActual || !userIdActual) {
        enviar(ws, { action: 'error', missatge: 'Has de fer "join" abans d\'enviar altres accions.' });
        return;
      }

      if (action === 'move') {
        const { position } = dades;
        if (!position || position.x === undefined || position.y === undefined) {
          enviar(ws, { action: 'error', missatge: 'L\'acció "move" requereix un objecte "position" amb "x" i "y".' });
          return;
        }
        broadcast(gameIdActual, userIdActual, { 
          action: 'move', 
          userId: userIdActual, 
          position: { x: position.x, y: position.y } 
        });
        return;
      }

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

      enviar(ws, { action: 'error', missatge: `Acció desconeguda: "${action}".`, originalMessage: missatgeCru.toString() });
    });

    ws.on('close', () => {
      if (gameIdActual && userIdActual) {
        gestionarDesconnexio(gameIdActual, userIdActual);
      }
    });

    ws.on('error', (error) => {
      console.error(`[WS] Error al socket del jugador ${userIdActual ?? 'desconegut'}:`, error.message);

    });
  });

  console.log('[WS] Servidor WebSocket inicialitzat i adjunt al port HTTP.');

  function broadcastGameOver(gameId, dades) {
    const jugadors = partides.get(gameId);
    if (!jugadors) return;

    console.log(`[WS] Enviant game_over per a la partida ${gameId}:`, dades);
    jugadors.forEach((socket) => {
      enviar(socket, { action: 'game_over', ...dades });
    });
  }

  return { wss, broadcastGameOver };
}

module.exports = inicialitzarWebSocket;
