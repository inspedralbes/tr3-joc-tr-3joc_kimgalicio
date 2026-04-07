# Specification

## 1. Entidades del Juego
- **Jugador**: Controlado por el usuario (Teclas de dirección/WASD + Salto).
- **Bot(s)**: Controlados por la CPU.
- **Bomba**: Objeto visual que se acopla a un Jugador o Bot.
- **Escenario**: Arena cerrada con plataformas estáticas.

## 2. Comportamiento Esperado (Reglas)
- **Inicio de Partida**: 
  - Las entidades aparecen en puntos de spawn predefinidos.
  - Una entidad aleatoria recibe la bomba.
  - El temporizador global inicia (ej. 30 segundos).
- **Movimiento**:
  - Las entidades sufren gravedad.
  - Pueden moverse izquierda/derecha y saltar sobre plataformas.
- **Mecánica de la Bomba**:
  - La bomba sigue exactamente la posición de la entidad que la posee.
  - **Transferencia**: Si la entidad con la bomba colisiona (superposición de hitboxes) con otra entidad, la bomba se transfiere inmediatamente a esta última.
  - **Cooldown de Transferencia**: Tras pasar la bomba, debe haber un periodo de gracia (ej. 1 segundo) donde no se puede devolver la bomba a la misma persona, para evitar transferencias infinitas en un solo frame.
- **Condición de Fin**:
  - Cuando el temporizador llega a `0.0`, la bomba explota.
  - La entidad que sostiene la bomba es eliminada/pierde.
  - El juego muestra "Game Over" o el ganador, y permite reiniciar.

## 3. Comportamiento del Bot (Lógica)
- **Estado 1 (Tiene la bomba - Perseguir)**: Calcula la distancia a las otras entidades y se mueve en dirección a la más cercana para colisionar con ella.
- **Estado 2 (No tiene la bomba - Huir)**: Detecta la posición de la entidad que tiene la bomba y se mueve en la dirección opuesta para mantener la máxima distancia posible.