# 02_ARCHITECTURE

## Stack Tecnològic
- **Motor**: Unity (2D).
- **Llenguatge**: C#.
- **IA**: Unity ML-Agents per al comportament dels bots.

## Components Clau

### Gestió d'Estat (`GameStateSO`)
- Utilitza un **ScriptableObject** com a font central de veritat per a les vides, el portador actual de la bomba, el temporitzador i l'estat d'espectador.
- Desvincular la UI i la lògica del joc dels scripts individuals de les entitats.

### Flux de Joc (`GameManager`)
- Orquestra les transicions de ronda, els respawns i la gestió de vides.
- Gestiona la lògica per canviar entre els estats de "Playing" (Jugant) i "GameOver" (Fi de partida).

### Lògica de la Bomba (`Bomb.cs` i `TagCollision.cs`)
- Gestiona el traspàs de la bomba en col·lidir i actualitza el `GameStateSO`.
- Administra el feedback visual (contorns/indicadors).

### Control d'Entitats (`PlayerModeController2D` i `BotController`)
- `PlayerModeController2D`: Controlador unificat tant per a jugadors humans com per a bots.
- `BotController`: Hereta de `ML-Agents.Agent`, enviant inputs al controlador basant-se en observacions.
