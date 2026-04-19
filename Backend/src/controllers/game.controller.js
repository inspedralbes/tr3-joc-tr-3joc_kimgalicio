// src/controllers/game.controller.js
// Controlador HTTP per als endpoints relacionats amb les partides.
// Única responsabilitat: gestionar la petició/resposta HTTP i
// delegar tota la lògica de negoci al GameService.

class GameController {
  /**
   * @param {import('../services/game.service')} gameService
   *   Instància del servei de partides injectada des de server.js.
   */
  constructor(gameService) {
    this._gameService = gameService;
    this._broadcastGameOver = null;
  }

  /**
   * Injecta la funció de broadcast de WebSockets des de server.js.
   * @param {Function} broadcastFn
   */
  setBroadcastGameOver(broadcastFn) {
    this._broadcastGameOver = broadcastFn;
  }

  /**
   * POST /api/games/join
   * Un jugador sol·licita unir-se a una partida (crea o s'uneix).
   *
   * Body esperat: { "userId": number, "mode": "vs_bot" | "vs_player" }
   *
   * Respostes possibles:
   *   - 200 OK:          El jugador s'ha unit a una partida existent (matchmaking).
   *   - 201 Created:     S'ha creat una nova partida (vs_bot o primera en espera).
   *   - 400 Bad Request: Falten camps o el mode és invàlid.
   *   - 500 Server Error: Error intern inesperat.
   */
  async join(req, res) {
    try {
      const { userId, mode } = req.body;

      // Validació dels camps requerits
      if (!userId || !mode) {
        return res.status(400).json({
          error: 'Els camps "userId" i "mode" són obligatoris al cos de la petició.',
        });
      }

      const partida = await this._gameService.joinGame(userId, mode);

      // Determinem el codi de resposta:
      //   201 → partida nova creada (vs_bot o pendent nova)
      //   200 → el jugador s'ha unit a una partida existent
      const esNova = partida.player2 === null || partida.mode === 'vs_bot';
      const codiEstat = esNova ? 201 : 200;

      return res.status(codiEstat).json({
        missatge: partida.status === 'pending'
          ? 'Partida creada. Esperant oponent...'
          : `Partida iniciada en mode ${partida.mode}!`,
        partida,
      });

    } catch (error) {
      // Errors de validació del servei
      if (error.message.includes('mode') || error.message.includes('userId')) {
        return res.status(400).json({ error: error.message });
      }

      console.error('[GameController.join] Error inesperat:', error);
      return res.status(500).json({ error: 'Error intern del servidor.' });
    }
  }

  /**
   * POST /api/games/finish
   * Finalitza una partida i registra el guanyador.
   *
   * Body esperat: { "gameId": number, "winnerId": number }
   *
   * Respostes possibles:
   *   - 200 OK:          La partida s'ha finalitzat correctament.
   *   - 400 Bad Request: Falten camps obligatoris.
   *   - 404 Not Found:   La partida amb el gameId indicat no existeix.
   *   - 500 Server Error: Error intern inesperat.
   */
  async finish(req, res) {
    try {
      const { gameId, winnerId, winnerHearts } = req.body;

      // Validació dels camps requerits
      if (!gameId || winnerId === undefined) {
        return res.status(400).json({
          error: 'Els camps "gameId" i "winnerId" són obligatoris al cos de la petició.',
        });
      }

      const partida = await this._gameService.finishGame(gameId, winnerId);

      // Si tenim el broadcast configurat, avisem per WebSocket
      if (this._broadcastGameOver) {
        const loserId = (partida.player1 === winnerId) ? partida.player2 : partida.player1;
        
        this._broadcastGameOver(String(gameId), {
          winnerId,
          winnerHearts: winnerHearts || 0,
          loserId: loserId || 0 // 0 per al bot si escau
        });
      }

      return res.status(200).json({
        missatge: `Partida ${partida.id} finalitzada. Guanyador: usuari ${partida.winnerId}.`,
        partida,
      });

    } catch (error) {
      // Error específic: la partida no existeix
      if (error.message.includes('No s\'ha trobat')) {
        return res.status(404).json({ error: error.message });
      }
      // Errors de validació del servei
      if (error.message.includes('gameId') || error.message.includes('winnerId')) {
        return res.status(400).json({ error: error.message });
      }

      console.error('[GameController.finish] Error inesperat:', error);
      return res.status(500).json({ error: 'Error intern del servidor.' });
    }
  }
}

module.exports = GameController;
