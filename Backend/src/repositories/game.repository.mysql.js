const GameRepositoryInterface = require('./game.repository.interface');
const pool                    = require('../config/db');

class GameRepositoryMySQL extends GameRepositoryInterface {

  async findPendingGame() {

    const [rows] = await pool.query(
      `SELECT id, mode, player1_id, player2_id, status, winner_id
         FROM Games
        WHERE status = 'pending'
        ORDER BY id ASC
        LIMIT 1`
    );

    if (rows.length === 0) return null;

    return this._mapRow(rows[0]);
  }

  async createGame(game) {
    const { mode, player1, player2 = null, status, winnerId = null } = game;

    const [result] = await pool.query(
      `INSERT INTO Games (mode, player1_id, player2_id, status, winner_id)
       VALUES (?, ?, ?, ?, ?)`,
      [mode, player1, player2, status, winnerId]
    );

    return {
      id:       result.insertId,
      mode,
      player1,
      player2,
      status,
      winnerId,
    };
  }

  async updateGame(game) {
    const { id, player2, status, winnerId } = game;

    const clausules = [];
    const valors    = [];

    if (player2  !== undefined) { clausules.push('player2_id = ?'); valors.push(player2);  }
    if (status   !== undefined) { clausules.push('status = ?');     valors.push(status);   }
    if (winnerId !== undefined) { clausules.push('winner_id = ?');  valors.push(winnerId); }

    if (clausules.length === 0) return null;

    valors.push(id);

    await pool.query(
      `UPDATE Games SET ${clausules.join(', ')} WHERE id = ?`,
      valors
    );

    const [rows] = await pool.query(
      'SELECT id, mode, player1_id, player2_id, status, winner_id FROM Games WHERE id = ?',
      [id]
    );

    if (rows.length === 0) return null;

    return this._mapRow(rows[0]);
  }

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
