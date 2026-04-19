// src/repositories/game.repository.interface.js
// Defineix el contracte (interfície) que qualsevol implementació de repositori
// de partides ha de complir. Llança errors si els mètodes no estan sobreescrits,
// simulant les interfícies de llenguatges com Java o TypeScript.

class GameRepositoryInterface {
  /**
   * Cerca la primera partida en estat 'pending' (esperant un segon jugador).
   * S'utilitza per al matchmaking en mode 'vs_player'.
   * @returns {Promise<Object|null>} La partida pendent trobada, o null si no n'hi ha cap.
   */
  async findPendingGame() {
    throw new Error(
      `El mètode findPendingGame() no està implementat a ${this.constructor.name}`
    );
  }

  /**
   * Crea i persisteix una nova partida.
   * @param {Object} game - Les dades de la partida a crear.
   * @param {string} game.mode       - Modalitat: 'vs_bot' o 'vs_player'.
   * @param {number} game.player1    - ID del jugador que crea la partida.
   * @param {number|null} game.player2 - ID del segon jugador (null si és bot o pending).
   * @param {string} game.status     - Estat inicial: 'pending' o 'playing'.
   * @param {number|null} game.winnerId - Guanyador (null fins al final).
   * @returns {Promise<Object>} La partida creada amb el camp 'id' afegit.
   */
  // eslint-disable-next-line no-unused-vars
  async createGame(game) {
    throw new Error(
      `El mètode createGame() no està implementat a ${this.constructor.name}`
    );
  }

  /**
   * Actualitza les dades d'una partida existent (status, player2, winnerId...).
   * @param {Object} game - L'objecte partida amb les dades actualitzades.
   *   Ha de contenir obligatòriament el camp 'id'.
   * @returns {Promise<Object|null>} La partida actualitzada, o null si no es troba.
   */
  // eslint-disable-next-line no-unused-vars
  async updateGame(game) {
    throw new Error(
      `El mètode updateGame() no està implementat a ${this.constructor.name}`
    );
  }
}

module.exports = GameRepositoryInterface;
