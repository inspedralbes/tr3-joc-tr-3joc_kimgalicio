require('dotenv').config();

const express = require('express');

const UserRepositoryMySQL = require('./src/repositories/user.repository.mysql');
const GameRepositoryMySQL = require('./src/repositories/game.repository.mysql');

const UserService    = require('./src/services/user.service');
const UserController = require('./src/controllers/user.controller');
const createUserRoutes = require('./src/routes/user.routes');

const GameService    = require('./src/services/game.service');
const GameController = require('./src/controllers/game.controller');
const createGameRoutes = require('./src/routes/game.routes');

const inicialitzarWebSocket = require('./src/websockets/game.ws');

const app  = express();
const PORT = 3000;

app.use(express.json());

const userRepository = new UserRepositoryMySQL();
const userService    = new UserService(userRepository);
const userController = new UserController(userService);

const gameRepository = new GameRepositoryMySQL();
const gameService    = new GameService(gameRepository, userService);
const gameController = new GameController(gameService);

app.use('/api/users', createUserRoutes(userController));
app.use('/api/games', createGameRoutes(gameController));

app.get('/health', (req, res) => {
  res.json({ estat: 'OK', timestamp: new Date().toISOString() });
});

const servidorHttp = app.listen(PORT, () => {
  console.log(`Servidor del joc iniciat al port ${PORT}`);
  console.log(`Health check disponible a:    http://localhost:${PORT}/health`);
  console.log(`API d'usuaris disponible a:   http://localhost:${PORT}/api/users/login`);
  console.log(`API de partides (join) a:     http://localhost:${PORT}/api/games/join`);
  console.log(`API de partides (finish) a:   http://localhost:${PORT}/api/games/finish`);
  console.log(`WebSocket (temps real) a:     ws://localhost:${PORT}`);
});

const { broadcastGameOver } = inicialitzarWebSocket(servidorHttp);

gameController.setBroadcastGameOver(broadcastGameOver);

module.exports = app;
