class UserService {

  constructor(userRepository) {
    this._userRepository = userRepository;
  }

  async loginORegistrar(nickname) {

    if (!nickname || typeof nickname !== 'string' || nickname.trim() === '') {
      throw new Error('El nickname no pot estar buit.');
    }

    const nicknameTrim = nickname.trim();

    const usuariExistent = await this._userRepository.findByNickname(nicknameTrim);

    if (usuariExistent) {

      return { usuari: usuariExistent, esNou: false };
    }

    const nouUsuari = await this._userRepository.create({
      nickname: nicknameTrim,
      wins:     0,
      losses:   0,
    });

    return { usuari: nouUsuari, esNou: true };
  }

  async addResult(userId, isWinner) {

    if (typeof this._userRepository.addResult !== 'function') {
      throw new Error(
        'El repositori d\'usuaris no implementa el mètode addResult(). ' +
        'Assegura\'t d\'usar UserRepositoryMySQL.'
      );
    }
    await this._userRepository.addResult(userId, isWinner);
  }
}

module.exports = UserService;
