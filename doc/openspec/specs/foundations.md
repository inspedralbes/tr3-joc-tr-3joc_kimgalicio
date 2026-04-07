# Fonaments

## Context
Minijoc 2D de plataformes competitiu que fusiona mecàniques de "Super Smash Bros" (arena flotant, caigudes al buit) i "Patata Calenta". Dissenyat per a 2 o 3 entitats (Jugadors/Bots). El nucli és la supervivència mitjançant un sistema de rondes i vides, on l'objectiu és evitar la bomba i no caure de l'escenari.

## Objectius (Features)
- **Sistema de Supervivència**: Implementar 3 vides màximes per entitat. Sense sistema de monedes.
- **Doble Condició de Dany**: Es perd 1 vida si el temporitzador de la ronda (90 segons) arriba a 0 i la bomba explota, o si una entitat cau al buit (independentment de si té la bomba).
- **Feedback Visual Clar**: Diferenciar les entitats amb contorns (outlines) de color negre per defecte, i vermell per al portador de la bomba, a més d'un indicador visual sobre el seu cap.
- **Balancet Asimètric**: Atorgar un petit avantatge de velocitat de moviment al portador de la bomba.
- **Gestió Dinàmica de Partides**: Adaptar les rondes i l'estat "Espectador" depenent de si la partida és de 2 o 3 jugadors.
- **Fi de Partida**: La partida ha d'acabar immediatament en el moment en què qualsevol entitat arriba a 0 vides, mostrant un missatge de victòria/derrota.

## Restriccions
- El desenvolupament segueix la metodologia Spec-Driven Development (OpenSpec).
- Lògica de la IA (Bots) basada en Màquina d'Estats (no ML) per facilitar la iteració. Els bots han de saber ignorar els jugadors en mode "Espectador".
