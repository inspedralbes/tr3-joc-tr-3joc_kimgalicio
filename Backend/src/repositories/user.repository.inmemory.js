// src/repositories/user.repository.inmemory.js
// Implementació en memòria del repositori d'usuaris.
// Utilitza un array local per emmagatzemar les dades sense necessitar una base
// de dades externa. Ideal per a testing i desenvolupament inicial.
// Estén UserRepositoryInterface garantint el compliment del contracte definit.

const UserRepositoryInterface = require('./user.repository.interface');

class UserRepositoryInMemory extends UserRepositoryInterface {
  constructor() {
    super();
    // Array que actua com a "base de dades" en memòria
    this._usuaris = [];
    // Comptador per generar identificadors únics incrementals
    this._comptadorId = 1;
  }

  /**
   * Cerca un usuari al array intern pel seu nickname.
   * La cerca és insensible a majúscules/minúscules per evitar duplicats com
   * "Jugador1" i "jugador1".
   * @param {string} nickname
   * @returns {Promise<Object|null>}
   */
  async findByNickname(nickname) {
    const nicknameNormalitzat = nickname.trim().toLowerCase();
    const usuariTrobat = this._usuaris.find(
      (u) => u.nickname.toLowerCase() === nicknameNormalitzat
    );
    // Retornem null de forma explícita si no es troba (evita 'undefined')
    return usuariTrobat || null;
  }

  /**
   * Afegeix un nou usuari a l'array intern i li assigna un id únic.
   * @param {Object} user - { nickname, wins, losses }
   * @returns {Promise<Object>} L'usuari creat amb el camp 'id' afegit.
   */
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
