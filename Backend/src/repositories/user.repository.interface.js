// src/repositories/user.repository.interface.js
// Defineix el contracte (interfície) que qualsevol implementació de repositori
// d'usuaris ha de complir. En JavaScript no existeix la paraula clau 'interface',
// per la qual cosa simulem el comportament llançant errors si els mètodes
// no estan sobreescrits per la classe filla.
//
// Qualsevol nova implementació (ex: MySQL, MongoDB, etc.) ha d'estendre
// aquesta classe i implementar TOTS els mètodes definits aquí.

class UserRepositoryInterface {
  /**
   * Cerca un usuari pel seu nickname.
   * @param {string} nickname - El nom únic de l'usuari.
   * @returns {Promise<Object|null>} L'objecte usuari si es troba, o null si no existeix.
   */
  // eslint-disable-next-line no-unused-vars
  async findByNickname(nickname) {
    throw new Error(
      `El mètode findByNickname() no està implementat a ${this.constructor.name}`
    );
  }

  /**
   * Crea i persisteix un nou usuari.
   * @param {Object} user - L'objecte amb les dades de l'usuari a crear.
   * @param {string} user.nickname - El nom únic de l'usuari.
   * @param {number} user.wins    - Victòries inicials (normalment 0).
   * @param {number} user.losses  - Derrotes inicials (normalment 0).
   * @returns {Promise<Object>} L'objecte usuari creat (amb id si escau).
   */
  // eslint-disable-next-line no-unused-vars
  async create(user) {
    throw new Error(
      `El mètode create() no està implementat a ${this.constructor.name}`
    );
  }
}

module.exports = UserRepositoryInterface;
