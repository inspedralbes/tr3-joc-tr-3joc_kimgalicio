# 05_BACKLOG

## Completat
- [x] **Infraestructura Backend**: Servidor Node.js funcional amb MySQL.
- [x] **Multijugador Base**: Connexió WebSocket i sincronització d'estats asíncrons.
- [x] **UI Toolkit**: Menús de Login i pantalles final de partida (`EndgameController`) i HUD in-game (`HUDController`).
- [x] **Sincronització de Morts i Físiques**: Assegurar que les explosions i caigudes es reflecteixin igual en tots els clients, interpolant moviment per evitar "jitter".
- [x] **Matchmaking Robust**: Sincronització de "Waiting for Player", assignació de jugadors sense desincronització per "Múltiples Pestanyes", i Victòria automàtica per Abandó.
- [x] **Desplegament en Producció**: Configuració Nginx per al backend i client WebGL.
- [x] **IA "Brain Swapping"**: Canviar entre perseguir i fugir segons l'estat de la bomba (Mode Local).

## Prioritat Mitjana
- [ ] **Rànquing de Jugadors**: Mostrar un TOP global al menú basat en les dades de guanyades/perdudes de MySQL.
- [ ] **Selecció de Skins**: Permetre als jugadors triar la seva skin al menú principal (la base de dades ja suporta skins).
- [ ] **Efectes de So (Online)**: Sincronitzar correctament els sons d'explosió i música per a tots els jugadors (actualment la música pot sobreposar-se).

## Prioritat Baixa
- [ ] **Xat de Partida**: Implementar un xat senzill per a la sala d'espera i pantalla de resultats.
- [ ] **Poliment de Moviment**: Millorar la latència en la interpolació de moviments remots i animacions extres.
