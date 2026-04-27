# 00_OBJECTIVE

## Resum del Projecte
**Tick-Tack Tag** és un minijoc de plataformes competitiu en 2D per a Unity. Fusiona mecàniques de "Super Smash Bros" (arenes flotants, dany per caiguda) amb una mecànica de "Patata Calenta" amb una bomba. El projecte ha evolucionat per convertir-se en una experiència **multijugador en línia completa**, desplegada via WebGL, amb un backend robust i servidors de producció.

## Objectiu Principal
L'objectiu és ser l'última entitat en peu. Els jugadors han d'evitar portar la bomba quan el temporitzador arribi a zero i evitar caure de l'escenari cap a la "Death Zone". El joc suporta partides locals amb bots i, principalment, partides en xarxa amb altres jugadors (Matchmaking) on la sincronització, els estats de partida, i les desconnexions es gestionen de manera determinista i robusta.

## Experiència Objectiu
- **Joc de Ritme Ràpid**: Alta tensió durant el compte enrere de la bomba i ritme frenètic de plataformes.
- **Moviment Estratègic**: Maniobrar per passar la bomba o esquivar el portador, usant l'entorn com escaleres.
- **Connectivitat Fluida**: Sistema multijugador basat en WebSockets que permet partides en temps real sincronitzades i aïllades per sales, garantint estabilitat fins i tot quan els oponents abandonen la partida.
- **Accessibilitat Web**: Jugable directament des del navegador (WebGL) sense descàrregues.
- **Claredat Visual**: Feedback immediat mitjançant UI Toolkit (HUD in-game, pantalles de victòria/derrota) i contorns dinàmics dels personatges.
