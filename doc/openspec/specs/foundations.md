# Fonaments

## Context
Minijoc 2D de plataformes competitiu que fusiona mecàniques de "Super Smash Bros" i "Patata Calenta". El projecte és una aplicació full-stack que utilitza Unity com a client i Node.js com a servidor d'alt rendiment per a multijugador online.

## Objectius (Features)
- **Sistema de Supervivència**: 3 vides per entitat, amb pèrdua per explosió o caiguda.
- **Multijugador Sincronitzat**: Ús de WebSockets per a una experiència competitiva en temps real.
- **Persistència de Dades**: MySQL per emmagatzemar perfils d'usuari, estadístiques i configuracions.
- **Feedback Visual Avançat**: Contorns dinàmics i interfícies creades amb UI Toolkit.
- **Arquitectura de Repositoris**: Backend modular per facilitar el manteniment i l'escalabilitat.

## Arquitectura i IA
- **Estat Global**: `GameStateSO` centralitza la veritat local, mentre que el servidor manté l'autoritat en mode online.
- **IA (Bots)**: ML-Agents amb "Brain Swapping" per a comportaments duals (perseguidor/evasió).
- **NetworkManager**: Component clau per a l'orquestració de la xarxa al client.

## Restriccions
- El desenvolupament segueix la metodologia Spec-Driven Development.
- La comunicació ha de ser eficient per minimitzar el lag en partides online.

