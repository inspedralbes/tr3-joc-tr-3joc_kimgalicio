class GameService {

  constructor(gameRepository, userService = null) {
    this._gameRepository = gameRepository;

    this._userService = userService;
  }

  async joinGame(userId, mode, gameId = null) {

    if (!userId) {
      throw new Error('El camp userId és obligatori.');
    }

    if (gameId) {
      const partida = await this._gameRepository.findById(gameId);
      if (!partida) throw new Error(`No s'ha trobat la partida amb ID ${gameId}.`);
      if (partida.status !== 'pending') throw new Error(`La partida ${gameId} ja no està pendent.`);
      if (partida.player1 === userId) return partida;

      const partidaActualitzada = await this._gameRepository.updateGame({
        id: partida.id,
        player2: userId,
        status: 'playing',
      });
      return partidaActualitzada;
    }

    if (mode !== 'vs_bot' && mode !== 'vs_player') {
      throw new Error('El mode ha de ser "vs_bot" o "vs_player".');
    }

    if (mode === 'vs_bot') {
      const novaPartida = await this._gameRepository.createGame({
        mode: 'vs_bot',
        player1: userId,
        player2: null,
        status: 'playing',
        winnerId: null,
      });
      return novaPartida;
    }

    const partidaPendent = await this._gameRepository.findPendingGame();

    if (partidaPendent) {
      if (partidaPendent.player1 === userId) {
        return partidaPendent;
      }

      const partidaActualitzada = await this._gameRepository.updateGame({
        id: partidaPendent.id,
        player2: userId,
        status: 'playing',
      });
      return partidaActualitzada;
    }

    const novaPartidaPendent = await this._gameRepository.createGame({
      mode: 'vs_player',
      player1: userId,
      player2: null,
      status: 'pending',
      winnerId: null,
    });
    return novaPartidaPendent;
  }

  async handleDisconnect(gameId) {
    if (!gameId) return;
    const partida = await this._gameRepository.findById(gameId);
    if (!partida) return;

    if (partida.status === 'pending' || partida.status === 'playing') {
      console.log(`[GameService] Tancant partida ${gameId} per desconnexió.`);
      await this._gameRepository.updateGame({
        id: gameId,
        status: 'finished'
      });
    }
  }

  async getRooms() {
    return await this._gameRepository.listPending();
  }

  async finishGame(gameId, winnerId, winnerHearts = 0) {

    if (!gameId)   throw new Error('El camp gameId és obligatori.');
    
    const realWinnerId = (winnerId === 0) ? null : winnerId;

    const partidaFinalitzada = await this._gameRepository.updateGame({
      id:       gameId,
      winnerId: realWinnerId,
      winnerHearts: winnerHearts,
      status:   'finished',
    });

    if (!partidaFinalitzada) {
      throw new Error(`No s'ha trobat cap partida amb l'id ${gameId}.`);
    }

    if (this._userService) {
      const { player1, player2 } = partidaFinalitzada;

      if (realWinnerId) {
        await this._userService.addResult(realWinnerId, true);
      }

      const loserId = player1 === realWinnerId ? player2 : player1;
      if (loserId !== null && loserId !== 0) {
        await this._userService.addResult(loserId, false);
      }
    }

    return partidaFinalitzada;
  }
}

module.exports = GameService;
