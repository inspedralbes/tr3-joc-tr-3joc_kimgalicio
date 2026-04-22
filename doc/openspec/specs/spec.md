# Especificació: Tick-Tack Tag

## 1. Entitats i Connexió
- **Entitats**: De 2 a 3 jugadors. Poden ser humans (online) o bots (locals).
- **Accés**: Els jugadors han de fer login amb un nickname. Si el nickname no existeix, es crea un nou usuari a la base de dades MySQL.
- **Multijugador**: Connexió via WebSockets. S'intercanvien missatges de tipus `move` (amb `{position: {x,y}}`), `bomb_transfer` i `game_over`.

## 2. Mecànica de la Bomba i Avantatges
- **Assignació**: Una entitat rep la bomba a l'inici. En mode online, s'utilitza una llavor determinista basada en l'ID de la partida perquè tots els clients triïn el mateix usuari inicial.
- **Visuals**:
  - Portador: Contorn VERMELL i indicador flotant.
  - Altres: Contorn NEGRE.
- **IA Determinista**: Els bots actuen com a `Catcher` (persegueixen el target) o `Evader` (fugeixen del portador) mitjançant el sistema de Brain Swapping.
- **Buff de Velocitat**: El portador rep un increment de velocitat d'un 15% gestionat pel `PlayerModeController2D`.

## 3. Bucle de Joc i Vides
- **Vides**: Cada entitat comença amb 3 vides (valor configurable via `GameStateSO`).
- **Pèrdua de Vida**:
  - Explosió: El temps arriba a 0 (90 segons per defecte).
  - Mort per Buits: Caure a la Death Zone.
- **Sincronització**: En partides online, la pèrdua de vida es comunica al servidor per actualitzar les estadístiques globals.

## 4. Flux de Partida (Híbrid)
- **Mode Local**: El `GameManager` gestiona els respawns i el reinici de la ronda ràpidament.
- **Mode Online**:
  - Flux de dades: Unity `NetworkManager` -> Server (Broadcast) -> Unity Clients.
  - Sincronització de bomba: L'usuari que detecta la col·lisió emet un `bomb_transfer` que l'altre client valida determinísticament.

## 5. Fi del Joc i Resultats
- **Condició de Victòria**: L'última entitat amb vides és el guanyador.
- **UI Toolkit**: S'utilitza una interfície dinàmica per mostrar el rànquing final i els botons per tornar al menú o reintentar.
- **Persistència**: Les victòries i derrotes s'emmagatzemen a la base de dades MySQL a través de l'API del backend.
