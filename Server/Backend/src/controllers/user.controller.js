class UserController {

  constructor(userService) {
    this._userService = userService;
  }

  async login(req, res) {
    try {
      const { nickname } = req.body;

      if (!nickname) {
        return res.status(400).json({
          error: 'El camp "nickname" és obligatori al cos de la petició.',
        });
      }

      const { usuari, esNou } = await this._userService.loginORegistrar(nickname);

      const codiEstat = esNou ? 201 : 200;

      return res.status(codiEstat).json({
        userId: usuari.id,
        missatge: esNou
          ? `Benvingut per primera vegada, ${usuari.nickname}!`
          : `Benvingut de nou, ${usuari.nickname}!`,
        usuari,
      });

    } catch (error) {

      console.error('Error al Login:', error);

      return res.status(500).json({ error: error.message });
    }
  }
}

module.exports = UserController;
