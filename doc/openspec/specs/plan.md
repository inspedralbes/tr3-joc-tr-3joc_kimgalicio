# Pla d'Implementació

## Fase 1-4: Core i Sistemes Base [COMPLET]
- Implementació de `GameStateSO`, Bomb logic, Death Zone i `GameManager`.
- Feedback visual (outlines) i buff de velocitat.

## Fase 5: Backend i Infraestructura MySQL [COMPLET]
- Creació del servidor Node.js/Express.
- Migració de dades a MySQL (Repositories, Controllers).
- Contenització amb Docker.

## Fase 6: Multijugador en Temps Real (WebSockets) [COMPLET]
- Implementació del `NetworkManager` a Unity.
- Sincronització de moviments asíncrons i estats de la bomba.
- Flux de Login i Join Game funcional amb prevenció contra "múltiples pestanyes" i gestió de desconnexions ("Abandó de partida").

## Fase 7: IA Avançada i UI Toolkit [COMPLET]
- IA amb "Brain Swapping" (perseguir/fugir) per a bots.
- Migració de la interfície d'usuari a UI Toolkit (Menús i Endgame amb `HUDController` i `EndgameController`).

## Fase 8: Poliment i Optimització [COMPLET]
- Sincronització asíncrona fina d'esdeveniments de mort (explosió/caiguda) delegats al servidor i resolts localment.
- Desacoblament de físiques de rivals a través del script remot independent `NetworkPlayerSync` (ús de `isKinematic` i `Vector2.Lerp`).

## Fase 9: Desplegament i Llençament WebGL [COMPLET]
- Creació i compilació de la build de producció del client en WebGL.
- Configuració del servidor Web/Reverse Proxy Nginx a `ticktack-tag.dam.inspedralbes.cat` per servir aplicació client i API conjunta.

---
Per a una llista de tasques i backlog actual detallada, consulteu [context/05_BACKLOG.md](../../../context/05_BACKLOG.md).
