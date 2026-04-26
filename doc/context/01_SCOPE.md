# 01_SCOPE

## Característiques

### Sistema de Supervivència
- **Vides**: Cada entitat (Jugador o Bot) comença amb 3 vides. La partida comença sempre des de la Ronda 1.
- **Condicions de Mort**:
  - Explosió de la bomba (afecta el portador quan el temps arriba a 0 o el detonador s'activa).
  - Caure a la "Death Zone" (afecta qualsevol que caigui).

### Mecàniques Multijugador i Xarxa
- **Multijugador Online**: Sincronització asíncrona mitjançant WebSockets amb broadcast de servidor.
- **Model d'Autoritat Híbrid i Físiques Asíncrones**: Cada client és autoritat del seu propi moviment. Per als jugadors remots, s'eliminen les escales de gravetat i s'utilitza interpolació de posicions per evitar jittering (via `NetworkPlayerSync`), evitant així conflictes amb el motor físic local.
- **Sincronització de Dades i Sessions**: Ús de DTOs estandarditzats per a la consistència del moviment i esdeveniments de mort. Protecció contra desincronització per "Múltiples Pestanyes" obertes pel mateix usuari.
- **Abandó de Partida**: Sistema automàtic que atorga la victòria si l'oponent es desconnecta o tanca el joc durant la partida.

### IA i Agents
- **ML-Agents (CPU Inference)**: Bots que operen mitjançant xarxes neuronals (.onnx) per a partides locals o com a oponents "fallback".
- **Brain Swapping**: Canvi dinàmic de comportament (Catcher vs Evader) en temps real sense interrompre la simulació.
- **Recompenses Dinàmiques**: Sistema de reforç basat en el temps de possessió de la bomba i l'eficiència en el moviment vertical (escaleres).

### Mecàniques de la Bomba
- **Traspàs**: Sistema de Tag basat en el contacte de colliders 2D.
- **Bonificació de Velocitat**: Buff de velocitat unificat al `PlayerModeController2D` per al portador.
- **Visuals**: Contorns dinàmics gestionats pel `GameStateSO` i mostrats via `SpriteRenderer` amb shaders simples de contorn.

### Interfície d'Usuari (UI Toolkit)
- **Menús Moderns**: Pantalla principal de Login i Selecció de Partida.
- **Pantalles in-game**: Controlador del HUD (`HUDController`) per mostrar "Ronda 1" i temps.
- **Pantalla de Finalització (`EndgameController`)**: Resultats dinàmics (Victòria, Derrota, Oponent Abandona) basats en dades del servidor o l'estat local, permetent tornar al menú principal de forma neta.

## Fora de l'Abast (Out of Scope)
- Sistema d'amics o xat complex (tot i que estava al backlog, es descarta per focus en gameplay).
- Power-ups addicionals apart del buff de velocitat base.
- Customització cosmètica profunda (la infraestructura base per a skins existeix, però la botiga no n'és l'objectiu immediat).
