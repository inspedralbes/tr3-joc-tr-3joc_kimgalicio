// src/config/db.js
// Mòdul de connexió a la base de dades MySQL.
// Utilitza la llibreria 'mysql2/promise' per treballar amb Promises i async/await.
//
// S'utilitza un POOL de connexions en lloc d'una connexió única perquè:
//   - Reutilitza connexions ja obertes → millor rendiment.
//   - Permet múltiples peticions concurrents sense blocar-se.
//   - Gestiona automàticament els errors de connexió i les reconnexions.
//
// Les credencials es llegeixen des de variables d'entorn (.env) per seguretat.
// Mai s'han de posar contrasenyes directament al codi font.

const mysql = require('mysql2/promise');

// Creació del pool de connexions amb la configuració de la base de dades.
// Les variables d'entorn es defineixen al fitxer .env de la carpeta Backend.
const pool = mysql.createPool({
  // Adreça del servidor MySQL (normalment 'localhost' en desenvolupament local).
  host:     process.env.DB_HOST     || 'localhost',

  // Usuari de MySQL amb permisos sobre la base de dades del joc.
  user:     process.env.DB_USER     || 'root',

  // Contrasenya de l'usuari MySQL. Mai deixar-la buida en producció.
  password: process.env.DB_PASSWORD || '',

  // Nom de la base de dades creada amb el script Backend/database.sql.
  database: process.env.DB_NAME     || 'ticktack_tag',

  // Nombre màxim de connexions simultànies al pool.
  // 10 és un valor raonable per a un servidor de joc petit.
  connectionLimit: 10,

  // Converteix automàticament els valors TINYINT(1) de MySQL a booleans JS.
  typeCast: true,
});

module.exports = pool;
