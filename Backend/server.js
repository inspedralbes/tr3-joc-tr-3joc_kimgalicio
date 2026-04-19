// server.js
// Punt d'entrada principal del servidor Express per al joc multijugador.
// Configura el middleware, les rutes i inicia l'escolta al port 3000.
//
// INJECCIÓ DE DEPENDÈNCIES:
// Aquest fitxer és l'únic lloc on es decideix QUINA implementació de repositori
// s'utilitza. Per canviar d'InMemory a MySQL (o viceversa), només cal canviar
// les importacions de les línies marcades amb [SWAP] i res més.

// Carrega les variables d'entorn del fitxer .env (DB_HOST, DB_USER, etc.)
// Ha de ser la primera instrucció per garantir que totes les importacions
// posteriors ja tinguin accés a process.env.
require('dotenv').config();

const express = require('express');


// ─── [SWAP] Repositoris: canvia entre InMemory i MySQL aquí ─────────────────
// Per tornar a InMemory: descomenta les línies InMemory i comenta les MySQL.

// const UserRepositoryInMemory = require('./src/repositories/user.repository.inmemory');
// const GameRepositoryInMemory = require('./src/repositories/game.repository.inmemory');

const UserRepositoryMySQL = require('./src/repositories/user.repository.mysql');
const GameRepositoryMySQL = require('./src/repositories/game.repository.mysql');
// ─────────────────────────────────────────────────────────────────────────────

// --- Importació de capes: Usuari ---
const UserService    = require('./src/services/user.service');
const UserController = require('./src/controllers/user.controller');
const createUserRoutes = require('./src/routes/user.routes');

// --- Importació de capes: Partida ---
const GameService    = require('./src/services/game.service');
const GameController = require('./src/controllers/game.controller');
const createGameRoutes = require('./src/routes/game.routes');

// --- Importació de la capa WebSocket (temps real) ---
const inicialitzarWebSocket = require('./src/websockets/game.ws');

// --- Creació de l'aplicació Express ---
const app  = express();
const PORT = 3000;

// --- Middleware global ---
// Permet parsejar cossos de peticions en format JSON
app.use(express.json());

// --- Composició de dependències (Dependency Injection manual) ────────────────
// Ordre important: UserService s'ha de crear ABANS que GameService,
// ja que GameService necessita rebre'l com a dependència per actualitzar
// les estadístiques en finalitzar una partida.

// Entitat: Usuari
// [SWAP] Canvia UserRepositoryMySQL per UserRepositoryInMemory si cal.
const userRepository = new UserRepositoryMySQL();
const userService    = new UserService(userRepository);
const userController = new UserController(userService);

// Entitat: Partida
// [SWAP] Canvia GameRepositoryMySQL per GameRepositoryInMemory si cal.
// Nota: passem 'userService' al GameService perquè pugui actualitzar
// wins/losses en el mètode finishGame().
const gameRepository = new GameRepositoryMySQL();
const gameService    = new GameService(gameRepository, userService);
const gameController = new GameController(gameService);
// ────────────────────────────────────────────────────────────────────────────

// --- Registre de rutes ---
app.use('/api/users', createUserRoutes(userController));
app.use('/api/games', createGameRoutes(gameController));

// --- Ruta d'estat del servidor (health check) ---
app.get('/health', (req, res) => {
  res.json({ estat: 'OK', timestamp: new Date().toISOString() });
});


// --- Inici del servidor HTTP ---
// app.listen() retorna un servidor http.Server natiu de Node.
// El passem al WebSocket perquè puguin compartir el MATEIX port (3000).
// Unity es connectarà a:  ws://localhost:3000
const servidorHttp = app.listen(PORT, () => {
  console.log(`Servidor del joc iniciat al port ${PORT}`);
  console.log(`Health check disponible a:    http://localhost:${PORT}/health`);
  console.log(`API d'usuaris disponible a:   http://localhost:${PORT}/api/users/login`);
  console.log(`API de partides (join) a:     http://localhost:${PORT}/api/games/join`);
  console.log(`API de partides (finish) a:   http://localhost:${PORT}/api/games/finish`);
  console.log(`WebSocket (temps real) a:     ws://localhost:${PORT}`);
});

// --- Inicialització del servidor WebSocket ---
// S'adjunta al servidor HTTP existent: comparteix port i procés.
inicialitzarWebSocket(servidorHttp);

module.exports = app; // Exportem per a possibles tests
