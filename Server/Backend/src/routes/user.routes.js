const { Router } = require('express');

function createUserRoutes(userController) {
  const router = Router();

  router.post('/login', userController.login.bind(userController));

  return router;
}

module.exports = createUserRoutes;
