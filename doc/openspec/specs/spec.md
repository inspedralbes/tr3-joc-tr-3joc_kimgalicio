# Especificació: Tick-Tack Tag

## 1. Entitats i Connexió
- **Entitats**: De 2 a 3 jugadors. Poden ser humans (online via **WebGL**) o bots (locals).
- **Accés**: Els jugadors han de fer login amb un nickname (autenticació asíncrona HTTP). Si el nickname no existeix, es crea un nou usuari a la base de dades MySQL. S'impedeixen connexions "fantasma" per múltiples pestanyes obertes de forma concurrent.
- **Multijugador**: Connexió mantinguda via WebSockets (sala de Matchmaking prèvia). S'intercanvien missatges de tipus `move` (amb `{position: {x,y}}`), `bomb_transfer`, `game_start`, `game_over` i abandonaments (`opponent_abandoned`).

## 2. Mecànica de la Bomba i Avantatges
- **Assignació**: Una entitat rep la bomba a l'inici de cada ronda. En mode online, s'utilitza una llavor determinista basada en l'ID de la partida (`GameId`) perquè tots els clients triïn sincronitzadament el mateix usuari inicial.
- **Visuals**:
  - Portador: Contorn VERMELL i indicador visual a la UI (`HUDController`).
  - Altres: Contorn NEGRE.
- **IA Determinista**: En partides locals o bots de fallback, els agents utilitzen "Brain Swapping" (`Catcher` o `Evader`).
- **Buff de Velocitat**: El portador de la bomba és un 15% més ràpid.

## 3. Bucle de Joc i Vides
- **Vides**: Cada entitat comença amb 3 vides. Sempre es comença des de la "Ronda 1".
- **Pèrdua de Vida / Final de Ronda**:
  - Explosió: El temps de la bomba arriba a 0 (per defecte ronda d'uns segons limitats).
  - Mort per Buits: Caure al fons de l'escenari (Death Zone).
- **Sincronització**: En partides online, quan el client local detecta la seva pròpia pèrdua de vida, ho emet al servidor.

## 4. Flux de Partida (Híbrid)
- **Mode Local**: El `GameManager` gestiona les rondes i recarrega l'estat dels actors.
- **Mode Online**:
  - Flux de dades: Unity `NetworkManager` -> Server (Broadcast) -> Unity Clients.
  - Sincronització Visual i Física: Els jugadors locals es mouen amb el seu motor físic. Els oponents remots desactiven la gravetat i físiques (`isKinematic`) i els seus moviments s'interpolen mitjançant l'script exclusiu `NetworkPlayerSync`.
  - Abandó: Si el WebSocket d'un oponent es tanca, el servidor avisa automàticament de l'abandonament, atorgant victòria immediata al supervivent.

## 5. Fi del Joc i Resultats
- **Condició de Victòria**: Ser l'última entitat amb vides restants o ser qui no ha abandonat.
- **UI Toolkit**: S'utilitza una interfície dinàmica moderna. L'`EndgameController` mostra missatges personalitzats ("Has Guanyat", "Has Perdut", "Oponent ha abandonat") i botons de flux.
- **Persistència**: Totes les victòries/derrotes reals s'emmagatzemen directament a la base de dades a través de l'API Node.js.
