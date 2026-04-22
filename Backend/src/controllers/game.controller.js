class GameController {

  constructor(gameService) {
    this._gameService = gameService;
    this._broadcastGameOver = null;
  }

  setBroadcastGameOver(broadcastFn) {
    this._broadcastGameOver = broadcastFn;
  }

  async join(req, res) {
    try {
      const { userId, mode, gameId } = req.body;
      console.log(`[GameController.join] Usuari ${userId} intentant unir-se (mode: ${mode || 'N/A'}, gameId: ${gameId || 'N/A'})`);

      if (!userId || (!mode && !gameId)) {
        return res.status(400).json({
          error: 'Els camps "userId" i ("mode" o "gameId") són obligatoris.',
        });
      }

      const partida = await this._gameService.joinGame(userId, mode, gameId);
      console.log(`[GameController.join] Usuari ${userId} unit a la partida ${partida.id} (status: ${partida.status})`);

      const esNova = partida.player2 === null || partida.mode === 'vs_bot';
      const codiEstat = esNova ? 201 : 200;

      return res.status(codiEstat).json({
        missatge: partida.status === 'pending'
          ? 'Partida creada. Esperant oponent...'
          : `Partida iniciada en mode ${partida.mode}!`,
        partida,
      });

    } catch (error) {
      if (error.message.includes('mode') || error.message.includes('userId') || error.message.includes('ID') || error.message.includes('pendent')) {
        return res.status(400).json({ error: error.message });
      }

      console.error('[GameController.join] Error inesperat:', error);
      return res.status(500).json({ error: 'Error intern del servidor.' });
    }
  }

  async listRooms(req, res) {
    try {
      console.log('[GameController.listRooms] Petició de llista de sales rebuda.');
      const rooms = await this._gameService.getRooms();
      console.log(`[GameController.listRooms] S'han trobat ${rooms.length} sales pendents.`);
      return res.status(200).json(rooms);
    } catch (error) {
      console.error('[GameController.listRooms] Error:', error);
      return res.status(500).json({ error: 'Error intern del servidor.' });
    }
  }

  async finish(req, res) {
    try {
      const { gameId, winnerId, winnerHearts } = req.body;
      console.log(`[GameController.finish] Finalitzant partida ${gameId}. Guanyador: ${winnerId}, Vides: ${winnerHearts}`);

      if (!gameId || winnerId === undefined) {
        return res.status(400).json({
          error: 'Els camps "gameId" i "winnerId" són obligatoris al cos de la petició.',
        });
      }

      const partida = await this._gameService.finishGame(gameId, winnerId, winnerHearts);
      console.log(`[GameController.finish] Partida ${gameId} guardada a DB correctament.`);
      
      if (this._broadcastGameOver) {
        const loserId = (partida.player1 === winnerId) ? partida.player2 : partida.player1;

        this._broadcastGameOver(String(gameId), {
          winnerId,
          winnerHearts: winnerHearts || 0,
          loserId: loserId || 0
        });
      }

      return res.status(200).json({
        missatge: `Partida ${partida.id} finalitzada. Guanyador: usuari ${partida.winnerId}.`,
        partida,
      });

    } catch (error) {
      if (error.message.includes('No s\'ha trobat')) {
        return res.status(404).json({ error: error.message });
      }

      if (error.message.includes('gameId') || error.message.includes('winnerId')) {
        return res.status(400).json({ error: error.message });
      }

      console.error('[GameController.finish] Error inesperat:', error);
      return res.status(500).json({ error: 'Error intern del servidor.' });
    }
  }
}

module.exports = GameController;
