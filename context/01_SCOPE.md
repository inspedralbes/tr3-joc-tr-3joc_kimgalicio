# 01_SCOPE

## Característiques

### Sistema de Supervivència
- **Vides**: Cada entitat (Jugador o Bot) comença amb 3 vides.
- **Condicions de Mort**:
  - Explosió de la bomba (només afecta el portador quan el temps arriba a 0).
  - Caure a la "Death Zone" (afecta qualsevol que caigui).

### Mecàniques de la Bomba
- **Traspàs**: Es passa la bomba en tocar una altra entitat.
- **Bonificació de Velocitat**: El portador de la bomba rep un petit avantatge de velocitat (ex. +15%) per facilitar la persecució.
- **Visuals**:
  - Portador: Contorn vermell i un indicador flotant sobre el cap.
  - Altres: Contorn negre.

### Rondes i Joc
- **Temporitzador**: Rondes de 90 segons.
- **Mode 2 Jugadors**: El joc es pausa en morir, s'actualitzen les vides, les entitats reapareixen (respawn) a les posicions originals.
- **Mode 3 Jugadors (Sistema d'Espectador)**: La primera mort converteix el jugador en espectador (invisible/intangible). La ronda continua fins a una segona mort.

## Fora de l'Abast (Out of Scope)
- Sistema de monedes o botiga.
- Power-ups addicionals apart del buff de velocitat base.
- Transicions de nivell complexes (l'enfocament és una única arena).
