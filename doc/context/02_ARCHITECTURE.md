# 02_ARCHITECTURE

## Stack Tecnològic
- **Client (Unity)**: Motor Unity 2022+ 2D, C#, Exportació a **WebGL**.
- **Servidor i Desplegament**: Nginx com a servidor web estàtic (Client WebGL) i proxy invers. Domini configurat: `ticktack-tag.dam.inspedralbes.cat`.
- **Backend (Servidor)**: Node.js, Express.
- **Base de Dades**: MySQL (via Docker).
- **Comunicació**: HTTP (Login/Join) + WebSockets (Gameplay en temps real, Matchmaking).
- **IA**: Unity ML-Agents per al comportament dels bots.
- **UI**: Unity UI Toolkit (UXML/USS).

## Components Clau

### Gestió d'Estat (`GameStateSO`)
- Utilitza un **ScriptableObject** com a font central de veritat local.
- Sincronitza dades amb el backend i orquestra canvis per a diferents subsistemes (Bomba, Jugadors, HUD).

### Gestió de Xarxa (`NetworkManager` i `NetworkPlayerSync`)
- **NetworkManager**: Orquestra la comunicació asíncrona amb el backend i manté l'estat de connexió i cua d'esdeveniments (gestió del buffer fins que la UI canvia d'escena).
- **NetworkPlayerSync**: Script dedicat als jugadors remots. Elimina físiques conflictives (`isKinematic = true`) i s'encarrega d'interpolar les dades de moviment (DTO) enviades per la xarxa, alhora que crida funcions de l'animador i volteja sprites.

### Backend (Arquitectura de Repositoris i WebSockets)
- **Controladors**: Gestionen les peticions HTTP de Login.
- **Repositoris (MySQL)**: Accés a taules per usuaris (credencials, wins, losses), skins i jocs.
- **WebSocket Gateway**: Cua de Matchmaking (esperant oponent), assignació de `GameId`, broadcast d'esdeveniments (moviment, traspàs de bomba, desconexions). Controla la presència d'usuaris evitant el doble-login i connexions de pestanyes inactives o múltiples per un mateix ID.
- **Sincronització Determinista**: Tot es genera a partir del `GameId`. Quan un oponent abandona, s'emet un missatge de `game_end` automàtic en lloc d'esperar el servidor com a autoritat principal de col·lisions.

### Flux de Joc (`GameManager`)
- Gestiona transicions, respawns i el cicle de vides/rondes.
- Responsable de preparar correctament als actors locals i remots, inicialitzar temporitzadors, i comunicar-se amb `HUDController` per mostrar canvis de ronda.
- Quan detecta una mort remota o victòria per abandó des del `NetworkManager`, invoca l'`EndgameController` amb els missatges pertinents.

### Interfície i Control (`UI Toolkit` & `Input System`)
- **UI Controllers**: Scripts C# (`MenuController`, `HUDController`, `EndgameController`) que vinculen i actualitzen elements gràfics (UXML) sense interrupcions en la lògica de joc base.
- **Input System Actions**: Mapeig de controls tant per a jugador com agents (IA).

### IA i Agents (`IA_Models`)
- **BotController**: Agent de ML-Agents. Utilitza "Brain Swapping" per alternar entre `Catcher` i `Evader`.
