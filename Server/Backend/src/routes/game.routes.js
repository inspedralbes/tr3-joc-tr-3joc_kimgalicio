const { Router } = require('express');

function createGameRoutes(gameController) {
  const router = Router();

  router.post('/join', gameController.join.bind(gameController));

  router.get('/rooms', gameController.listRooms.bind(gameController));

  router.post('/finish', gameController.finish.bind(gameController));

  return router;
}

module.exports = createGameRoutes;
