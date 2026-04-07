# Especificació: TickTack-Tag

## 1. Entitats i Escenari
- **Entitats**: De 2 a 3 (Jugadors i Bots). Totes comencen amb exactament 3 Vides.
- **Escenari**: Plataformes flotants. La part inferior del nivell és una zona de buit (Death Zone).

## 2. Mecànica de la Bomba i Avantatges
- A l'inici de cada ronda, una entitat rep la bomba automàticament.
- **Visuals**:
  - L'entitat amb la bomba té un **contorn (outline) VERMELL** i un sprite/bola flotant sobre el seu cap.
  - Les entitats sense la bomba tenen un **contorn NEGRE** i no tenen l'sprite sobre el cap.
  - Això s'actualitza instantàniament en tocar una altra entitat.
- **Buff de Velocitat**: El portador de la bomba és un poc més ràpid (ex. +15% de velocitat base) que els altres.

## 3. Bucle de Joc i Vides
- **Temporitzador**: Cada ronda dura 1 minut i 30 segons (90s).
- **Pèrdua de Vida (Es perd 1 vida si)**:
  - El temporitzador arriba a 0 i la bomba explota (afecta només al portador).
  - L'entitat cau al buit (afecta a qui caigui, tingui la bomba o no).

## 4. Flux de Partida (2 vs 3 Jugadors)
- **Partida de 2 Entitats**: 
  - Si un mor (per bomba o caiguda), el joc es pausa un moment.
  - Se li resta 1 vida al que ha mort.
  - Ambdós fan respawn a les seves posicions inicials, el temps es reinicia a 90s, i comença una nova ronda.
- **Partida de 3 Entitats (Sistema d'Espectador)**:
  - Si un mor (per bomba o caiguda), se li resta 1 vida i passa a estat **Espectador** (invisible/intangible).
  - Els 2 jugadors restants **continuen la ronda** sense que el joc es pausi.
  - Quan un dels 2 restants mor, la ronda acaba.
  - Després d'acabar la ronda, es reinicia la partida amb els 3 jugadors des de les seves posicions inicials (sempre que l'espectador no tingui 0 vides).

## 5. Fi del Joc
- El joc general acaba en l'instant en què **una de les entitats es queda a 0 vides**.
- En ocórrer això, es pausa tot i apareix un text en pantalla indicant qui és el guanyador i qui ha perdut.
