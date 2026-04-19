# 03_DECISIONS

## Implementació de la IA: ML-Agents en lloc de Màquina d'Estats
Inicialment es va considerar un enfocament de Màquina d'Estats per als bots. Tot i això, es va decidir utilitzar Unity ML-Agents per permetre comportaments més complexos i emergents (perseguir, fugir, utilitzar escaleres) que són més difícils de programar manualment en un entorn de plataformes dinàmic.

## Gestió d'Estat: Patró ScriptableObject
Utilitzem `GameStateSO` com a nucli central de dades.
- **Per què**: Permet una referència fàcil des de qualsevol script sense necessitat de singletons complexos o acoblament directe entre scripts com la `UI` i la `Bomba`.

## Combat Simplificat
S'ha eliminat qualsevol mecànica d'"atac" o de "monedes".
- **Per què**: L'objectiu és centrar-se en la mecànica pura de la "Patata Calenta" i l'habilitat de plataformes.

## Sistema d'Espectador
Implementat específicament per a rondes de 3 jugadors.
- **Per què**: Manté el flux del joc fins i tot després que un jugador sigui eliminat, evitant reinicis freqüents fins al duel final de la ronda.
