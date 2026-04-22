class UserRepositoryInterface {

  async findByNickname(nickname) {
    throw new Error(
      `El mètode findByNickname() no està implementat a ${this.constructor.name}`
    );
  }

  async create(user) {
    throw new Error(
      `El mètode create() no està implementat a ${this.constructor.name}`
    );
  }
}

module.exports = UserRepositoryInterface;
