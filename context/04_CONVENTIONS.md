# 04_CONVENTIONS

## Estàndards de Codificació
- **Nomenclatura**:
  - Scripts/Classes: PascalCase (ex. `BotController.cs`).
  - Camps/Mètodes Públics: PascalCase.
  - Camps Privats/Interns: `_camelCase` amb prefix de guió baix.
- **Patrons d'Unity**:
  - Preferir `[SerializeField]` en lloc de camps públics sempre que sigui possible.
  - Utilitzar `ScriptableObjects` per a l'estat compartit i els esdeveniments.

## Estructura de Directoris
- `Assets/Scripts`: Lògica principal i components.
- `Assets/Scenes`: Escenes de joc i d'entrenament.
- `context`: Context d'alt nivell per a la IA i documentació.
- `doc/openspec/specs`: Especificacions tècniques.

## Instruccions per a la IA
- La documentació s'ha d'actualitzar simultàniament amb els canvis de lògica.
- Utilitzar `GameStateSO` com a interfície principal per a les modificacions de l'estat del joc.
