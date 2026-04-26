# 04_CONVENTIONS

## Estàndards de Codificació

### C# (Unity)
- **Nomenclatura**: PascalCase per a Classes i Mètodes, `_camelCase` per a camps privats.
- **Patrons**: `[SerializeField]` per a camps privats exposats. Ús de `GameStateSO` per a estats compartits.
- **Sincronització Remota**: El codi per actualitzar el moviment remot es farà exclusivament en components dedicats com `NetworkPlayerSync` (evitant embrutar `PlayerModeController2D` amb codi exclusiu de xarxa).

### JavaScript (Backend)
- **Repositoris**: Seguir el patró Repository per a l'accés a dades MySQL.
- **Async/Await**: Utilitzar promeses i async/await per a operacions de DB i xarxa.
- **Enviroment**: Dades sensibles en fitxers `.env`.

## Disseny i IA
- **IA (ML-Agents)**: Les observacions han d'estar normalitzades (entre -1 i 1 si és possible).
- **UI Toolkit**: Els noms d'elements en UXML han d'utilitzar `kebab-case` (ex: `btn-vs-bot`, `nickname-input`).
- **Arquitectura de UI**: CSS (USS) ha d'estar separat del layout (UXML) per facilitar el manteniment d'estils. Els controladors de UI (ex. `HUDController`, `EndgameController`) s'han d'adjuntar exclusivament a l'escena on està el `UIDocument`.

## Estructura de Directoris
- `Assets/Scripts`: Lògica de joc C#.
- `Assets/UI`: Fitxers UXML i USS per a la interfície.
- `Backend/src`: Codi font del servidor (controllers, repositories, services, websockets).
- `doc/context`: Documentació d'estat i arquitectura.
- `doc/openspec/specs`: Especificacions funcionals i documents oberts.

## Instruccions per a la IA
- Sincronitzar sempre els canvis de lògica amb la documentació a `doc/context`.
- Mantenir el `NetworkManager` i la lògica del servidor com a punt central de veritat per al cicle de vida de la partida multijugador.
