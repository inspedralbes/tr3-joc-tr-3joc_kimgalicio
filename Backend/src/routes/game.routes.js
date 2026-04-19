// src/routes/game.routes.js
// Defineix i exporta les rutes HTTP associades al recurs "partides".
// Rep el controlador per injecció de dependències des de server.js.
//
// Rutes definides (el prefix /api/games s'afegeix a server.js):
//   POST /join   → GameController.join   (matchmaking i creació)
//   POST /finish → GameController.finish (finalitzar una partida)

const { Router } = require('express');

/**
 * Crea i retorna un Router d'Express configurat amb totes les rutes de partides.
 * @param {import('../controllers/game.controller')} gameController
 *   Instància del controlador injectada per gestionar les peticions.
 * @returns {Router} El router configurat.
 */
function createGameRoutes(gameController) {
  const router = Router();

  /**
   * POST /api/games/join
   * Un jugador s'uneix o crea una partida.
   * .bind() és necessari per preservar el context 'this' del controlador.
   */
  router.post('/join', gameController.join.bind(gameController));

  /**
   * POST /api/games/finish
   * Finalitza una partida en curs i registra el guanyador.
   */
  router.post('/finish', gameController.finish.bind(gameController));

  return router;
}

module.exports = createGameRoutes;
