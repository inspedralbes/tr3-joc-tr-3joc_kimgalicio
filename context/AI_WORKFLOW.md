# AI_WORKFLOW

## Protocol de Desenvolupament
1. **Sincronització d'Estat**: Sempre revisa els fitxers de `context/` abans de fer canvis arquitectònics importants.
2. **Integració amb Unity**: Assegurar-se que els scripts siguin compatibles amb Unity 2022+ i segueixin el patró `GameStateSO`.
3. **ML-Agents**: Al modificar el comportament dels bots, revisa `BotController.cs` i assegura't que els sensors/actuadors coincideixin amb la configuració d'entrenament.
4. **Documentació**: Després de completar una funcionalitat, actualitza `context/05_BACKLOG.md` i `context/06_SYSTEM_MAP.md` si cal.

## Comunicació
- Utilitzar el català en respondre a l'usuari, tal com s'ha establert en la conversa actual.
- Proporcionar resums clars dels canvis de codi en els artifacts.
