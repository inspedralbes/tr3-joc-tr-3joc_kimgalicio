# 01_SCOPE

## Característiques

### Sistema de Supervivència
- **Vides**: Cada entitat (Jugador o Bot) comença amb 3 vides.
- **Condicions de Mort**:
  - Explosió de la bomba (afecta el portador quan el temps arriba a 0 o el detonador s'activa).
  - Caure a la "Death Zone" (afecta qualsevol que caigui).

### Mecàniques Multijugador i Xarxa
- **Multijugador Online**: Sincronització asíncrona mitjançant WebSockets amb broadcast de servidor.
- **Model d'Autoritat Híbrid**: Cada client és autoritat del seu propi moviment (Client-Side Prediction simple) i les col·lisions pro-actives s'emeten de forma determinista basant-se en l'ID de la sala compartida.
- **Sincronització de Dades**: Ús de DTOs estandarditzats per a la consistència del moviment (posició niada) i esdeveniments de mort.

### IA i Agents
- **ML-Agents (CPU Inference)**: Bots que operen mitjançant xarxes neuronals (.onnx).
- **Brain Swapping**: Canvi dinàmic de comportament (Catcher vs Evader) en temps real sense interrompre la simulació.
- **Recompenses Dinàmiques**: Sistema de reforç basat en el temps de possessió de la bomba i l'eficiència en el moviment vertical (escaleres).

### Mecàniques de la Bomba
- **Traspàs**: Sistema de Tag basat en el contacte de colliders 2D.
- **Bonificació de Velocitat**: Buff de velocitat unificat al `PlayerModeController2D` per al portador.
- **Visuals**: Contorns dinàmics gestionats pel `GameStateSO` i mostrats via `SpriteRenderer` amb shaders simples de contorn.

### Interfície d'Usuari (UI Toolkit)
- **Menús Moderns**: Pantalla principal de Login i Selecció de Partida.
- **Pantalla de Finalització**: Resultats dinàmics basats en dades del servidor o dades locals.

## Fora de l'Abast (Out of Scope)
- Sistema d'amics o xat.
- Power-ups addicionals apart del buff de velocitat base.
- Customització cosmètica profunda (skins habilitades però amb backend simplificat).

