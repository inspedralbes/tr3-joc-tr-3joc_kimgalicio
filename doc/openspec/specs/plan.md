# Implementation Plan

## Fase 1: Setup y Movimiento Base
- Configurar el escenario base con físicas (suelo, paredes y 2-3 plataformas).
- Crear el script de movimiento del jugador (input, gravedad, colisiones, salto).

## Fase 2: Sistema de Juego (Temporizador y Bomba)
- Crear el objeto Bomba y la lógica para "anclarse" a un jugador.
- Implementar el temporizador global que se muestre en pantalla.
- Implementar la lógica de colisión entre entidades para la transferencia de la bomba (incluyendo el cooldown de 1 segundo).

## Fase 3: Integración del Bot
- Crear el controlador del Bot heredando o reutilizando las físicas de movimiento.
- Implementar la lógica para leer quién tiene la bomba.
- Programar los estados: Chase (Perseguir) y Flee (Huir) ajustando los inputs de movimiento del bot simulando teclas.

## Fase 4: Bucle de Partida (Game Loop)
- Unir todos los sistemas.
- Lógica de inicialización (asignar bomba al azar al empezar).
- Lógica de finalización (detener movimiento, efecto de explosión cuando el timer es 0).