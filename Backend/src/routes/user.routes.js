// src/routes/user.routes.js
// Defineix i exporta les rutes HTTP associades al recurs "usuaris".
// Rep el controlador per injecció de dependències des de server.js,
// mantenint el desacoblament entre capes.
//
// Rutes definides:
//   POST /login  → UserController.login
//     (el prefix /api/users s'afegeix a server.js)

const { Router } = require('express');

/**
 * Crea i retorna un Router d'Express configurat amb totes les rutes d'usuaris.
 * @param {import('../controllers/user.controller')} userController
 *   Instància del controlador injectada per gestionar les peticions.
 * @returns {Router} El router configurat.
 */
function createUserRoutes(userController) {
  const router = Router();

  /**
   * POST /api/users/login
   * Inicia sessió o registra un nou jugador pel seu nickname.
   * El mètode .bind(userController) és essencial per preservar el context
   * de 'this' dins del controlador quan Express crida la funció.
   */
  router.post('/login', userController.login.bind(userController));

  return router;
}

module.exports = createUserRoutes;
