// src/services/user.service.js
// Capa de servei per a la lògica de negoci relacionada amb els usuaris.
// Rep el repositori per injecció de dependències, de manera que no
// coneix ni li importa com s'emmagatzemen les dades (memòria, MySQL, etc.).
//
// Flux principal (login/registre en un pas):
//   1. Rep un nickname del controlador.
//   2. Si l'usuari ja existeix → el retorna directament.
//   3. Si no existeix → el crea amb estadístiques inicials i el retorna.

class UserService {
  /**
   * @param {import('../repositories/user.repository.interface')} userRepository
   *   Instància que compleix el contracte de UserRepositoryInterface.
   */
  constructor(userRepository) {
    this._userRepository = userRepository;
  }

  /**
   * Gestiona el procés de login o auto-registre d'un jugador.
   * No requereix contrasenya: el nickname és l'únic identificador.
   *
   * @param {string} nickname - El nom del jugador introduït al Menú Principal.
   * @returns {Promise<{ usuari: Object, esNou: boolean }>}
   *   L'usuari trobat o creat, i un booleà que indica si és un registre nou.
   * @throws {Error} Si el nickname és buit o no vàlid.
   */
  async loginORegistrar(nickname) {
    // Validació bàsica del nickname
    if (!nickname || typeof nickname !== 'string' || nickname.trim() === '') {
      throw new Error('El nickname no pot estar buit.');
    }

    const nicknameTrim = nickname.trim();

    // Pas 1: Comprovem si l'usuari ja existeix
    const usuariExistent = await this._userRepository.findByNickname(nicknameTrim);

    if (usuariExistent) {
      // Usuari ja registrat → simplement el retornem
      return { usuari: usuariExistent, esNou: false };
    }

    // Pas 2: L'usuari no existeix → el creem amb estadístiques inicials
    const nouUsuari = await this._userRepository.create({
      nickname: nicknameTrim,
      wins:     0, // Estadístiques inicials
      losses:   0, // Estadístiques inicials
    });

    return { usuari: nouUsuari, esNou: true };
  }
}

module.exports = UserService;
