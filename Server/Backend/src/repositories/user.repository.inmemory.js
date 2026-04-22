const UserRepositoryInterface = require('./user.repository.interface');

class UserRepositoryInMemory extends UserRepositoryInterface {
  constructor() {
    super();

    this._usuaris = [];

    this._comptadorId = 1;
  }

  async findByNickname(nickname) {
    const nicknameNormalitzat = nickname.trim().toLowerCase();
    const usuariTrobat = this._usuaris.find(
      (u) => u.nickname.toLowerCase() === nicknameNormalitzat
    );

    return usuariTrobat || null;
  }

  async create(user) {
    const nouUsuari = {
      id:       this._comptadorId++,
      nickname: user.nickname.trim(),
      wins:     user.wins,
      losses:   user.losses,
      creatEn:  new Date().toISOString(),
    };
    this._usuaris.push(nouUsuari);
    return nouUsuari;
  }
}

module.exports = UserRepositoryInMemory;
