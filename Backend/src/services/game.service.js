class GameService {

  constructor(gameRepository, userService = null) {
    this._gameRepository = gameRepository;

    this._userService = userService;
  }

  async joinGame(userId, mode) {

    if (!userId) {
      throw new Error('El camp userId és obligatori.');
    }
    if (mode !== 'vs_bot' && mode !== 'vs_player') {
      throw new Error('El mode ha de ser "vs_bot" o "vs_player".');
    }

    if (mode === 'vs_bot') {
      const novaPartida = await this._gameRepository.createGame({
        mode:     'vs_bot',
        player1:  userId,
        player2:  null,
        status:   'playing',
        winnerId: null,
      });
      return novaPartida;
    }

    const partidaPendent = await this._gameRepository.findPendingGame();

    if (partidaPendent) {

      const partidaActualitzada = await this._gameRepository.updateGame({
        id:      partidaPendent.id,
        player2: userId,
        status:  'playing',
      });
      return partidaActualitzada;
    }

    const novaPartidaPendent = await this._gameRepository.createGame({
      mode:     'vs_player',
      player1:  userId,
      player2:  null,
      status:   'pending',
      winnerId: null,
    });
    return novaPartidaPendent;
  }

  async finishGame(gameId, winnerId) {

    if (!gameId)   throw new Error('El camp gameId és obligatori.');
    if (!winnerId) throw new Error('El camp winnerId és obligatori.');

    const partidaFinalitzada = await this._gameRepository.updateGame({
      id:       gameId,
      winnerId: winnerId,
      status:   'finished',
    });

    if (!partidaFinalitzada) {
      throw new Error(`No s'ha trobat cap partida amb l'id ${gameId}.`);
    }

    if (this._userService) {
      const { player1, player2 } = partidaFinalitzada;

      await this._userService.addResult(winnerId, true);

      const loserId = player1 === winnerId ? player2 : player1;
      if (loserId !== null) {
        await this._userService.addResult(loserId, false);
      }
    }

    return partidaFinalitzada;
  }
}

module.exports = GameService;
