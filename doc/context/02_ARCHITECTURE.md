# 02_ARCHITECTURE

## Stack Tecnològic
- **Client (Unity)**: Motor Unity 2D, C#.
- **Backend (Servidor)**: Node.js, Express.
- **Base de Dades**: MySQL.
- **Comunicació**: HTTP (Login/Join) + WebSockets (Gameplay en temps real).
- **IA**: Unity ML-Agents per al comportament dels bots.
- **UI**: Unity UI Toolkit (UXML/USS).

## Components Clau

### Gestió d'Estat (`GameStateSO`)
- Utilitza un **ScriptableObject** com a font central de veritat local.
- Sincronitza dades amb el backend en partides multijugador.

### Gestió de Xarxa (`NetworkManager`)
- Orquestra la comunicació amb el backend.
- Gestiona el cicle de vida de la connexió WebSocket i despatxa els esdeveniments de xarxa cap als sistemes de joc.

### Backend (Arquitectura de Repositoris)
- **Controladors**: Gestionen les peticions HTTP i la lògica de negocis.
- **Repositoris (MySQL)**: Capa d'abstracció per a la persistència de dades. L'esquema es defineix a `database.sql` i inclou taules per a `users` (credencials, wins, losses), `games` i `skins`.
- **WebSocket Gateway**: Sincronització d'estats de partida (moviments, transferències de bomba i explosions).
- **Protocol de Sincronització Determinista**: S'utilitzen dades compartides de la sala (com el `GameId`) com a llavor per a esdeveniments aleatoris (ex: assignació inicial de la bomba), garantint consistència sense autoritat central de servidor.
- **Contractes de Dades Unificats**: Ús de DTOs estandarditzats (`WsMoveMessage`, `LoginResponse`) amb estructures niades per a una millor escalabilitat i compatibilitat entre Node.js i Unity.

### Flux de Joc (`GameManager`)
- Gestiona transicions, respawns i vides.
- **Identificació d'Autoritat**: Assigna el rol de local/remoto als agents basant-se en la sessió de xarxa.

### Interfície i Control (`UI Toolkit` & `Input System`)
- **UI Controllers**: Scripts C# (`MenuController`, `HUDController`) que vinculen la lògica amb els fitxers visual UXML.
- **Input System Actions**: Mapeig de tecles i controls modern que alimenta tant al jugador com al sistema d'IA (Heuristic mode).

### IA i Agents (`IA_Models`)
- **BotController**: Agent de ML-Agents que integra l'Inferència Engine per executar models `.onnx`.
- **Model Swapping**: Lògica per intercanviar actius de comportament (Catcher/Evader) programàticament.

