// src/repositories/user.repository.mysql.js
// Implementació MySQL del repositori d'usuaris.
// Fa servir el pool de connexions de 'src/config/db.js' per executar
// consultes reals contra la base de dades MySQL.
//
// Implementa la interfície definida a UserRepositoryInterface:
//   - findByNickname(nickname)
//   - create(user)
// I afegeix el mètode addicional per a estadístiques:
//   - addResult(userId, isWinner)

const UserRepositoryInterface = require('./user.repository.interface');
const pool                    = require('../config/db');

class UserRepositoryMySQL extends UserRepositoryInterface {

  /**
   * Cerca un usuari a la base de dades pel seu nickname.
   *
   * @param {string} nickname - El nom únic del jugador.
   * @returns {Promise<Object|null>} L'usuari trobat, o null si no existeix.
   */
  async findByNickname(nickname) {
    // Executa una consulta preparada (evita SQL Injection automàticament).
    // pool.query() retorna un array: [rows, fields].
    // Destructurem per quedar-nos només amb les files de resultats.
    const [rows] = await pool.query(
      'SELECT id, nickname, wins, losses FROM Users WHERE nickname = ?',
      [nickname]
    );

    // Si no s'ha trobat cap fila, retornem null (com marca la interfície).
    if (rows.length === 0) return null;

    // Retornem el primer (i únic, per UNIQUE) resultat.
    return rows[0];
  }

  /**
   * Insereix un nou usuari a la base de dades.
   * Si el nickname ja existeix, MySQL llançarà un error per clau única.
   *
   * @param {Object} user
   * @param {string} user.nickname - Nom de jugador únic.
   * @param {number} [user.wins=0]   - Victòries inicials.
   * @param {number} [user.losses=0] - Derrotes inicials.
   * @returns {Promise<Object>} L'usuari creat amb el 'id' assignat per MySQL.
   */
  async create(user) {
    const { nickname, wins = 0, losses = 0 } = user;

    // INSERT i recuperem l'id auto-generat via insertId.
    const [result] = await pool.query(
      'INSERT INTO Users (nickname, wins, losses) VALUES (?, ?, ?)',
      [nickname, wins, losses]
    );

    // Construïm i retornem l'objecte complet de l'usuari creat.
    return {
      id: result.insertId,
      nickname,
      wins,
      losses,
    };
  }

  /**
   * Suma +1 a 'wins' o +1 a 'losses' segons el resultat de la partida.
   * S'anomena des del GameService en finalitzar cada partida.
   *
   * @param {number}  userId   - ID de l'usuari a actualitzar.
   * @param {boolean} isWinner - true si ha guanyat, false si ha perdut.
   * @returns {Promise<void>}
   */
  async addResult(userId, isWinner) {
    // Seleccionem dinàmicament la columna a incrementar (wins o losses).
    // Nota: NO posem el nom de columna com a paràmetre '?' perquè
    // mysql2 escaparia les cometes i generaria SQL invàlid.
    // El valor isWinner prové del nostre codi (booleà), no de l'usuari,
    // de manera que no hi ha risc d'injecció SQL aquí.
    const columna = isWinner ? 'wins' : 'losses';

    await pool.query(
      `UPDATE Users SET ${columna} = ${columna} + 1 WHERE id = ?`,
      [userId]
    );
  }
}

module.exports = UserRepositoryMySQL;
