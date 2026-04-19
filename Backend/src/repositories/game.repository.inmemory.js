// src/repositories/game.repository.inmemory.js
// Implementació en memòria del repositori de partides.
// Utilitza un array local per emmagatzemar l'estat de totes les partides.
// Ideal per a testing i desenvolupament sense base de dades externa.
//
// Estructura d'una partida:
// {
//   id:        number   → Identificador únic auto-incremental
//   mode:      string   → 'vs_bot' | 'vs_player'
//   player1:   number   → ID del jugador que ha creat la partida
//   player2:   number|null → ID del segon jugador (null si bot o pending)
//   status:    string   → 'pending' | 'playing' | 'finished'
//   winnerId:  number|null → ID del guanyador (null fins que acabi)
//   creatEn:   string   → Timestamp de creació ISO 8601
//   actualitzatEn: string → Timestamp de l'última modificació ISO 8601
// }

const GameRepositoryInterface = require('./game.repository.interface');

class GameRepositoryInMemory extends GameRepositoryInterface {
  constructor() {
    super();
    // Array que actua com a "base de dades" en memòria
    this._partides = [];
    // Comptador per generar identificadors únics incrementals
    this._comptadorId = 1;
  }

  /**
   * Cerca la primera partida en estat 'pending' (mode 'vs_player').
   * @returns {Promise<Object|null>}
   */
  async findPendingGame() {
    const partidaPendent = this._partides.find(
      (p) => p.status === 'pending' && p.mode === 'vs_player'
    );
    return partidaPendent || null;
  }

  /**
   * Crea una nova partida i l'afegeix a l'array intern.
   * @param {Object} game
   * @returns {Promise<Object>} La partida creada amb id i timestamps.
   */
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

  /**
   * Actualitza una partida existent identificada per el seu id.
   * Fusiona els camps proporcionats amb els existents (merge parcial).
   * @param {Object} game - Ha de contenir 'id' i els camps a actualitzar.
   * @returns {Promise<Object|null>} La partida actualitzada o null si no existeix.
   */
  async updateGame(game) {
    const index = this._partides.findIndex((p) => p.id === game.id);

    // La partida no existeix
    if (index === -1) return null;

    // Merge parcial: mantenim els camps existents i sobreescrivim els nous
    this._partides[index] = {
      ...this._partides[index],
      ...game,
      actualitzatEn: new Date().toISOString(),
    };

    return this._partides[index];
  }
}

module.exports = GameRepositoryInMemory;
