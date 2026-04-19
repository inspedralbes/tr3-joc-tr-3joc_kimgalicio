const UserRepositoryInterface = require('./user.repository.interface');
const pool                    = require('../config/db');

class UserRepositoryMySQL extends UserRepositoryInterface {

  async findByNickname(nickname) {

    const [rows] = await pool.query(
      'SELECT id, nickname, wins, losses FROM Users WHERE nickname = ?',
      [nickname]
    );

    console.log('Resultat del SELECT:', rows);

    if (rows.length === 0) return null;

    const row = rows[0];
    return {
      id:       row.id,
      nickname: row.nickname,
      wins:     row.wins,
      losses:   row.losses,
    };
  }

  async create(user) {
    const { nickname, wins = 0, losses = 0 } = user;

    const [result] = await pool.query(
      'INSERT INTO Users (nickname, wins, losses) VALUES (?, ?, ?)',
      [nickname, wins, losses]
    );

    console.log('Resultat del INSERT:', result);

    return {
      id:       result.insertId,
      nickname: nickname,
      wins:     wins,
      losses:   losses,
    };
  }

  async addResult(userId, isWinner) {

    const columna = isWinner ? 'wins' : 'losses';

    await pool.query(
      `UPDATE Users SET ${columna} = ${columna} + 1 WHERE id = ?`,
      [userId]
    );
  }
}

module.exports = UserRepositoryMySQL;
