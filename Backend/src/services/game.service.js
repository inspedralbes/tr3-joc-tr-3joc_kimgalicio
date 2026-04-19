// src/services/game.service.js
// Capa de servei per a la lògica de negoci de les partides.
// Gestiona el matchmaking i la finalització de partides.
// Rep el repositori i el userService per injecció de dependències.
//
// Modes de joc:
//   'vs_bot'    → La partida es crea i passa immediatament a 'playing'.
//                 No cal esperar per un segon jugador humà.
//   'vs_player' → Matchmaking en dos passos:
//                  1. Si hi ha una partida 'pending', l'usuari s'uneix com a player2
//                     i la partida passa a 'playing'.
//                  2. Si no hi ha cap partida pendent, es crea una de nova en 'pending'
//                     i s'espera que un altre jugador s'hi uneixi.

class GameService {
  /**
   * @param {import('../repositories/game.repository.interface')} gameRepository
   *   Instància que compleix el contracte de GameRepositoryInterface.
   * @param {import('./user.service')} userService
   *   Servei d'usuaris per actualitzar estadístiques (wins/losses) en finalitzar.
   *   S'accepta null per mantenir retrocompatibilitat amb tests sense BD.
   */
  constructor(gameRepository, userService = null) {
    this._gameRepository = gameRepository;
    // El userService és opcional per no trencar la versió InMemory sense MySQL.
    this._userService = userService;
  }

  /**
   * Gestiona l'entrada d'un jugador a una partida (matchmaking).
   *
   * @param {number} userId - ID del jugador que vol jugar.
   * @param {string} mode   - Modalitat triada: 'vs_bot' o 'vs_player'.
   * @returns {Promise<Object>} La partida resultant (creada o unida).
   * @throws {Error} Si el mode no és vàlid o els paràmetres falten.
   */
  async joinGame(userId, mode) {
    // Validació de paràmetres
    if (!userId) {
      throw new Error('El camp userId és obligatori.');
    }
    if (mode !== 'vs_bot' && mode !== 'vs_player') {
      throw new Error('El mode ha de ser "vs_bot" o "vs_player".');
    }

    // --- MODE BOT ---
    // Creem la partida directament en estat 'playing', sense necessitat de matchmaking.
    if (mode === 'vs_bot') {
      const novaPartida = await this._gameRepository.createGame({
        mode:     'vs_bot',
        player1:  userId,
        player2:  null,     // El bot no té ID d'usuari
        status:   'playing', // Comença immediatament
        winnerId: null,
      });
      return novaPartida;
    }

    // --- MODE VS PLAYER ---
    // Pas 1: Cerquem si hi ha algú esperant partida
    const partidaPendent = await this._gameRepository.findPendingGame();

    if (partidaPendent) {
      // Un altre jugador ja espera → ens unim com a player2 i comencem
      const partidaActualitzada = await this._gameRepository.updateGame({
        id:      partidaPendent.id,
        player2: userId,
        status:  'playing',
      });
      return partidaActualitzada;
    }

    // Pas 2: No hi ha ningú esperant → creem una nova partida en espera
    const novaPartidaPendent = await this._gameRepository.createGame({
      mode:     'vs_player',
      player1:  userId,
      player2:  null,      // Encara sense segon jugador
      status:   'pending', // Esperant oponent
      winnerId: null,
    });
    return novaPartidaPendent;
  }

  /**
   * Finalitza una partida i registra el guanyador.
   *
   * @param {number} gameId    - ID de la partida a finalitzar.
   * @param {number} winnerId  - ID de l'usuari que ha guanyat.
   * @returns {Promise<Object>} La partida amb status 'finished' i winnerId registrat.
   * @throws {Error} Si la partida no existeix o ja estava finalitzada.
   */
  async finishGame(gameId, winnerId) {
    // Validació de paràmetres
    if (!gameId)   throw new Error('El camp gameId és obligatori.');
    if (!winnerId) throw new Error('El camp winnerId és obligatori.');

    // Actualitzem la partida al repositori
    const partidaFinalitzada = await this._gameRepository.updateGame({
      id:       gameId,
      winnerId: winnerId,
      status:   'finished',
    });

    // El repositori retorna null si la partida no existeix
    if (!partidaFinalitzada) {
      throw new Error(`No s'ha trobat cap partida amb l'id ${gameId}.`);
    }

    // ── Actualització d'estadístiques ────────────────────────────────────────
    // Només si el userService ha estat injectat (mode MySQL).
    // En mode InMemory (tests) es pot ometre sense errors.
    if (this._userService) {
      const { player1, player2 } = partidaFinalitzada;

      // Sumem victòria al guanyador (isWinner = true).
      await this._userService.addResult(winnerId, true);

      // Sumem derrota al perdedor, que és el jugador que NO és el guanyador.
      // En mode 'vs_bot', player2 és null: no cal sumar derrota a ningú.
      const loserId = player1 === winnerId ? player2 : player1;
      if (loserId !== null) {
        await this._userService.addResult(loserId, false);
      }
    }
    // ────────────────────────────────────────────────────────────────────────

    return partidaFinalitzada;
  }
}

module.exports = GameService;
