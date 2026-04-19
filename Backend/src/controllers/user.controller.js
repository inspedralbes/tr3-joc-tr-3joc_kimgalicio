// src/controllers/user.controller.js
// Controlador HTTP per als endpoints relacionats amb usuaris.
// La seva única responsabilitat és gestionar la petició/resposta HTTP
// i delegar tota la lògica de negoci al UserService.
// No conté lògica de dades ni de negoci.

class UserController {
  /**
   * @param {import('../services/user.service')} userService
   *   Instància del servei d'usuaris injectada des de server.js.
   */
  constructor(userService) {
    this._userService = userService;
  }

  /**
   * Gestiona la petició POST /api/users/login
   * Body esperat: { "nickname": "string" }
   *
   * Respostes possibles:
   *   - 200 OK:          L'usuari ja existia i s'ha trobat correctament.
   *   - 201 Created:     S'ha creat un nou usuari amb estadístiques inicials.
   *   - 400 Bad Request: El nickname no s'ha proporcionat o és invàlid.
   *   - 500 Server Error: Error intern inesperat.
   */
  async login(req, res) {
    try {
      const { nickname } = req.body;

      // Comprovació ràpida del camp requerit
      if (!nickname) {
        return res.status(400).json({
          error: 'El camp "nickname" és obligatori al cos de la petició.',
        });
      }

      // Delegació al servei de negoci
      const { usuari, esNou } = await this._userService.loginORegistrar(nickname);

      // Codi de resposta semàntic: 201 si és nou, 200 si ja existia
      const codiEstat = esNou ? 201 : 200;

      return res.status(codiEstat).json({
        userId: usuari.id,
        missatge: esNou
          ? `Benvingut per primera vegada, ${usuari.nickname}!`
          : `Benvingut de nou, ${usuari.nickname}!`,
        usuari,
      });

    } catch (error) {
      // Error detallat per a diagnòstic al servidor
      console.error('Error al Login:', error);
      // Retornem el missatge real perquè Unity el pugui mostrar/diagnosticar
      return res.status(500).json({ error: error.message });
    }
  }
}

module.exports = UserController;
