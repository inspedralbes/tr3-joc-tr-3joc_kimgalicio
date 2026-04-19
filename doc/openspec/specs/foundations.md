# Fonaments

## Context
Minijoc 2D de plataformes competitiu que fusiona mecàniques de "Super Smash Bros" (arena flotant, caigudes al buit) i "Patata Calenta". Dissenyat per a 2 o 3 entitats (Jugadors/Bots). El nucli és la supervivència mitjançant un sistema de rondes i vides, on l'objectiu és evitar la bomba i no caure de l'escenari.

## Objectius (Features)
- **Sistema de Supervivència**: Implementar 3 vides màximes per entitat.
- **Doble Condició de Dany**: Es perd 1 vida si el temporitzador de la ronda (90 segons) arriba a 0 i la bomba explota, o si una entitat cau al buit.
- **Feedback Visual Clar**: Outlines vermells per al portador, negres per als altres, i indicador visual sobre el cap.
- **Balancet Asimètric**: Avantatge de velocitat de moviment (+15%) al portador de la bomba.
- **Gestió de Partides**: Suport per a 2 jugadors o 3 jugadors (amb sistema d'espectador).

## Arquitectura i IA
- **Estat Global**: S'utilitza `GameStateSO` (ScriptableObject) com a font única de veritat per a les vides, el cronòmetre i l'estat de la bomba.
- **IA (Bots)**: Implementada mitjançant **Unity ML-Agents** per permetre comportaments més complexos i naturals d'evasió i persecució.
- **Controlador**: `PlayerModeController2D` centralitza les físiques, sent alimentat per inputs de teclat o per les decisions de l'agent ML.

## Restriccions
- El desenvolupament segueix la metodologia Spec-Driven Development (OpenSpec).
- Els bots han de ser capaços de navegar l'escenari incloent l'ús d'escaleres.
