# 04_CONVENTIONS

## Estàndards de Codificació

### C# (Unity)
- **Nomenclatura**: PascalCase per a Classes i Mètodes, `_camelCase` per a camps privats.
- **Patrons**: `[SerializeField]` per a camps privats exposats. Ús de `GameStateSO` per a estats compartits.

### JavaScript (Backend)
- **Repositoris**: Seguir el patró Repository per a l'accés a dades.
- **Async/Await**: Utilitzar promeses i async/await per a operacions de DB i xarxa.
- **Enviroment**: Dades sensibles en fitxers `.env`.

## Estructura de Directoris
- `Assets/Scripts`: Lògica de joc C#.
- `Assets/UI`: Fitxers UXML i USS per a la interfície.
- `Backend/src`: Codi font del servidor (controllers, repositories, services).
- `doc/context`: Documentació d'estat i arquitectura.
- `doc/openspec/specs`: Especificacions funcionals.

## Instruccions per a la IA
- Sincronitzar sempre els canvis de lògica amb la documentació a `doc/`.
- Mantenir el `NetworkManager` com a punt central per a qualsevol nova funcionalitat multijugador.

