# 01_SCOPE

## Característiques

### Sistema de Supervivència
- **Vides**: Cada entitat (Jugador o Bot) comença amb 3 vides.
- **Condicions de Mort**:
  - Explosió de la bomba (afecta el portador quan el temps arriba a 0 o el detonador s'activa).
  - Caure a la "Death Zone" (afecta qualsevol que caigui).

### Mecàniques Multijugador i Xarxa
- **Multijugador Online**: Connexió en temps real mitjançant WebSockets.
- **Gestió de Sessions**: Sistema de Login i Join Game per connectar-se a partides en curs.
- **Sincronització**: Moviment, estats de la bomba i esdeveniments de mort sincronitzats entre clients.

### Backend i Persistència
- **Servidor Node.js/Express**: API REST per a la gestió d'usuaris i partides.
- **Base de Dades MySQL**: Persistència d'usuaris, estadístiques de victòries/derrotes i configuració.
- **Docker**: Arquitectura contenitzada per facilitar l'escala i el desplegament.

### Mecàniques de la Bomba
- **Traspàs**: Es passa la bomba en tocar una altra entitat.
- **Bonificació de Velocitat**: El portador rep un avantatge (+15%) per facilitar la persecució.
- **Visuals**: Contorns dinàmics (Vermell/Negre) i indicadors basats en el `GameStateSO`.

### Interfície d'Usuari (UI Toolkit)
- **Menús Moderns**: Pantalla principal de Login i Selecció de Partida.
- **Pantalla de Finalització**: Resultats dinàmics basats en dades del servidor o dades locals.

## Fora de l'Abast (Out of Scope)
- Sistema d'amics o xat.
- Power-ups addicionals apart del buff de velocitat base.
- Customització cosmètica profunda (skins habilitades però amb backend simplificat).

