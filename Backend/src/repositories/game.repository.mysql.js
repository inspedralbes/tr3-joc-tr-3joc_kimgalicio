// src/repositories/game.repository.mysql.js
// Implementació MySQL del repositori de partides.
// Fa servir el pool de connexions de 'src/config/db.js' per persistir
// i recuperar partides de la base de dades MySQL.
//
// Implementa la interfície definida a GameRepositoryInterface:
//   - findPendingGame()
//   - createGame(game)
//   - updateGame(game)

const GameRepositoryInterface = require('./game.repository.interface');
const pool                    = require('../config/db');

class GameRepositoryMySQL extends GameRepositoryInterface {

  /**
   * Cerca la primera partida en estat 'pending' a la base de dades.
   * S'utilitza per al matchmaking en mode 'vs_player'.
   *
   * @returns {Promise<Object|null>} La partida pendent trobada, o null.
   */
  async findPendingGame() {
    // Busquem la primera partida 'pending' ordenada per id ascendent
    // per garantir que sempre s'uneix a la partida més antiga (FIFO).
    const [rows] = await pool.query(
      `SELECT id, mode, player1_id, player2_id, status, winner_id
         FROM Games
        WHERE status = 'pending'
        ORDER BY id ASC
        LIMIT 1`
    );

    if (rows.length === 0) return null;

    // Adaptem els noms de columna de MySQL (player1_id) al format
    // intern de l'aplicació (player1) per mantenir consistència
    // amb la resta de capes (servei, controlador).
    return this._mapRow(rows[0]);
  }

  /**
   * Insereix una nova partida a la base de dades.
   *
   * @param {Object} game
   * @param {string}      game.mode      - 'vs_bot' o 'vs_player'.
   * @param {number}      game.player1   - ID del primer jugador.
   * @param {number|null} game.player2   - ID del segon jugador (null si pendent o bot).
   * @param {string}      game.status    - 'pending' o 'playing'.
   * @param {number|null} game.winnerId  - Null fins que la partida acabi.
   * @returns {Promise<Object>} La partida creada amb l'id assignat per MySQL.
   */
  async createGame(game) {
    const { mode, player1, player2 = null, status, winnerId = null } = game;

    const [result] = await pool.query(
      `INSERT INTO Games (mode, player1_id, player2_id, status, winner_id)
       VALUES (?, ?, ?, ?, ?)`,
      [mode, player1, player2, status, winnerId]
    );

    // Retornem l'objecte de la partida en format normalitzat.
    return {
      id:       result.insertId,
      mode,
      player1,
      player2,
      status,
      winnerId,
    };
  }

  /**
   * Actualitza de forma parcial una partida existent.
   * Només modifica els camps que s'inclouen a l'objecte 'game'.
   *
   * @param {Object}      game          - Dades actualitzades de la partida.
   * @param {number}      game.id       - Obligatori: ID de la partida a modificar.
   * @param {number|null} [game.player2]  - Nou segon jugador (per al matchmaking).
   * @param {string}      [game.status]   - Nou estat: 'playing' o 'finished'.
   * @param {number|null} [game.winnerId] - Guanyador (en finalitzar).
   * @returns {Promise<Object|null>} La partida actualitzada, o null si no existeix.
   */
  async updateGame(game) {
    const { id, player2, status, winnerId } = game;

    // Construïm dinàmicament el SET de l'UPDATE amb els camps que arriben.
    // Permet actualitzacions parcials sense sobreescriure camps no enviats.
    const clausules = [];
    const valors    = [];

    if (player2  !== undefined) { clausules.push('player2_id = ?'); valors.push(player2);  }
    if (status   !== undefined) { clausules.push('status = ?');     valors.push(status);   }
    if (winnerId !== undefined) { clausules.push('winner_id = ?');  valors.push(winnerId); }

    // Si no hi ha cap camp per actualitzar, no fem res.
    if (clausules.length === 0) return null;

    // Afegim l'id al final per al WHERE.
    valors.push(id);

    await pool.query(
      `UPDATE Games SET ${clausules.join(', ')} WHERE id = ?`,
      valors
    );

    // Recuperem la partida actualitzada de la BD per retornar l'estat real.
    const [rows] = await pool.query(
      'SELECT id, mode, player1_id, player2_id, status, winner_id FROM Games WHERE id = ?',
      [id]
    );

    if (rows.length === 0) return null;

    return this._mapRow(rows[0]);
  }

  /**
   * Mètode privat auxiliar per convertir una fila de MySQL
   * al format d'objecte normalitzat que usa l'aplicació.
   *
   * Els noms de columna de MySQL usen snake_case amb suffix '_id',
   * mentre que la capa de servei espera camelCase sense suffix.
   *
   * @param {Object} row - Fila retornada per mysql2.
   * @returns {Object} Objecte normalitzat.
   */
  _mapRow(row) {
    return {
      id:       row.id,
      mode:     row.mode,
      player1:  row.player1_id,
      player2:  row.player2_id,
      status:   row.status,
      winnerId: row.winner_id,
    };
  }
}

module.exports = GameRepositoryMySQL;
