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
- **Repositoris (MySQL)**: Capa d'abstracció per a la persistència de dades (usuaris, games).
- **WebSocket Gateway**: Sincronització d'estats de partida, moviments i transferències de bomba.

### Flux de Joc (`GameManager`)
- Gestiona transicions, respawns i vides.
- Adapta el comportament segons si el joc és local o online.

### Control d'Entitats (`BotController` i `PlayerModeController2D`)
- `PlayerModeController2D`: Controlador físic unificat.
- `BotController`: Capa d'IA que usa "Brain Swapping" per canviar entre perseguir (Catcher) i fugir (Evader).

