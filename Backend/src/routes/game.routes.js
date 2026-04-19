const { Router } = require('express');

function createGameRoutes(gameController) {
  const router = Router();

  router.post('/join', gameController.join.bind(gameController));

  router.post('/finish', gameController.finish.bind(gameController));

  return router;
}

module.exports = createGameRoutes;
