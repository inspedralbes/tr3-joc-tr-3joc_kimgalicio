const GameRepositoryInterface = require('./game.repository.interface');

class GameRepositoryInMemory extends GameRepositoryInterface {
  constructor() {
    super();

    this._partides = [];

    this._comptadorId = 1;
  }

  async findPendingGame() {
    const partidaPendent = this._partides.find(
      (p) => p.status === 'pending' && p.mode === 'vs_player'
    );
    return partidaPendent || null;
  }

  async createGame(game) {
    const ara = new Date().toISOString();
    const novaPartida = {
      id:            this._comptadorId++,
      mode:          game.mode,
      player1:       game.player1,
      player2:       game.player2      ?? null,
      status:        game.status,
      winnerId:      game.winnerId     ?? null,
      creatEn:       ara,
      actualitzatEn: ara,
    };
    this._partides.push(novaPartida);
    return novaPartida;
  }

  async updateGame(game) {
    const index = this._partides.findIndex((p) => p.id === game.id);

    if (index === -1) return null;

    this._partides[index] = {
      ...this._partides[index],
      ...game,
      actualitzatEn: new Date().toISOString(),
    };

    return this._partides[index];
  }
}

module.exports = GameRepositoryInMemory;
