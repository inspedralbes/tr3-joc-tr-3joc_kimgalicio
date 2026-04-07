# Pla d'Implementació

## Fase 1: Core de Supervivència (GameStateSO)
- Actualitzar `GameStateSO` per rastrejar les Vides (diccionari/array) de cada entitat. Màxim 3.
- Ajustar `GameTimer` inicial a 90f.
- Crear lògica per avaluar si alguna entitat ha arribat a 0 vides per disparar l'esdeveniment de "Game Over Total".

## Fase 2: Feedback Visual i Buff de Moviment
- **Controlador**: Modificar l'script de moviment perquè la `MoveSpeed` es multipliqui per un factor (ex. 1.15) si l'entitat és el `CurrentBombOwner`.
- **Visuals**: Afegir suport per a Material Outlines (Vermell per al propietari, Negre per als altres) i activar/desactivar un GameObject fill (l'sprite de la bola sobre el cap) des de l'script `Bomb.cs` al moment de la transferència.

## Fase 3: Escenari i Death Zone
- Crear un objecte "VoidCollider" sota l'arena amb `IsTrigger = true`.
- Script `DeathZone.cs`: En col·lidir amb una entitat, invoca la lògica de mort d'aquesta entitat directament (igual que si la bomba hagués explotat en ella).

## Fase 4: Gestor de Rondes i Multijugador (GameManager)
- Refactoritzar el `GameManager` per gestionar estats: `Playing`, `RoundTransition`, i `GameOver`.
- Implementar funció de `HandleDeath(Entity deadEntity)`:
  - Restar vida.
  - Si hi ha 2 jugadors totals: Iniciar corrutina (Pausa d'1s -> Respawn de tots -> Reset Timer).
  - Si hi ha 3 jugadors totals i és la primera mort de la ronda: Desactivar físiques/visuals del mort (Espectador).
  - Si hi ha 3 jugadors totals i és la segona mort: Iniciar corrutina de Fi de Ronda (Respawn dels 3 -> Reset Timer).
- El `BotController` ha d'ometre en la seva cerca d'objectius qualsevol entitat que estigui en estat Espectador.
- **UI Final**: Quan el joc detecta 0 vides, mostrar en pantalla TextMeshPro "Guanyador: [Nom]" i "Perdedor: [Nom]".
