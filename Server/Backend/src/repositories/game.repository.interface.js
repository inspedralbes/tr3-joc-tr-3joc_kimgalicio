class GameRepositoryInterface {

  async findPendingGame() {
    throw new Error(
      `El mètode findPendingGame() no està implementat a ${this.constructor.name}`
    );
  }

  async listPending() {
    throw new Error(
      `El mètode listPending() no està implementat a ${this.constructor.name}`
    );
  }

  async findById(id) {
    throw new Error(
      `El mètode findById() no està implementat a ${this.constructor.name}`
    );
  }

  async createGame(game) {
    throw new Error(
      `El mètode createGame() no està implementat a ${this.constructor.name}`
    );
  }

  async updateGame(game) {
    throw new Error(
      `El mètode updateGame() no està implementat a ${this.constructor.name}`
    );
  }
}

module.exports = GameRepositoryInterface;
