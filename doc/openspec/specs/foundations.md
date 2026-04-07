# Foundations

## Contexto
El proyecto es un minijuego 2D de plataformas estilo "Patata Caliente". Participan de 2 a 3 entidades (jugador contra 1 o 2 bots). El objetivo es no tener la bomba cuando el contador de tiempo llegue a cero.

## Objetivos (Features)
- Implementar físicas de plataformas básicas (movimiento lateral, salto, colisiones con el entorno).
- Implementar la mecánica de la bomba: asignación inicial, seguimiento a la entidad que la posee y transferencia al colisionar con otra entidad.
- Implementar un sistema de cuenta atrás (Timer) que finalice la partida provocando la explosión de la bomba.
- Implementar la lógica del Bot (IA de estados): comportamiento de persecución (cuando tiene la bomba) y evasión (cuando no la tiene).

## Restricciones
- El desarrollo se guiará estrictamente por especificaciones (Spec-Driven Development).
- El motor o framework (ej. Unity, Godot, o Canvas HTML5/Phaser) debe permitir un prototipado rápido.
- La IA de los bots será basada en reglas (heurística/máquina de estados) para garantizar que el LLM pueda generar y corregir el código iterativamente sin depender de tiempos de entrenamiento de Machine Learning.