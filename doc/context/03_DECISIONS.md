# 03_DECISIONS

## Implementació de la IA: ML-Agents en lloc de Màquina d'Estats
Inicialment es va considerar un enfocament de Màquina d'Estats per als bots. Tot i això, es va decidir utilitzar Unity ML-Agents per permetre comportaments més complexos i emergents (perseguir, fugir, utilitzar escaleres) que són més difícils de programar manualment en un entorn de plataformes dinàmic.

## Gestió d'Estat: Patró ScriptableObject
Utilitzem `GameStateSO` com a nucli central de dades.
- **Per què**: Permet una referència fàcil des de qualsevol script sense necessitat de singletons complexos o acoblament directe entre scripts com la `UI` i la `Bomba`.

## Combat Simplificat
S'ha eliminat qualsevol mecànica d'"atac" o de "monedes".
- **Per què**: L'objectiu és centrar-se en la mecànica pura de la "Patata Calenta" i l'habilitat de plataformes.

## Migració a MySQL i Docker
Es va decidir migrar d'una persistència en memòria a una base de dades MySQL.
- **Per què**: Permet mantenir estadístiques reals dels jugadors, gestionar l'autenticació de forma persistent i escalar el servidor mitjançant Docker.

## UI Toolkit per sobre de uGUI
S'ha adoptat **UI Toolkit** (UXML/USS) per al desenvolupament de la interfície de menús i pantalles final de partida.
- **Per què**: Ofereix un flux de treball més similar al desenvolupament web, millor separació entre estil i lògica, i una gestió més eficient de layouts complexos.

## Brain Swapping a la IA
En lloc d'un únic model per a tot, s'utilitza una tècnica de "Brain Swapping".
- **Per què**: Permet al bot ser extremadament bo en rols oposats (perseguir vs fugir) canviant el model neuronal en temps real basant-se en si té la bomba o no, evitant que un sol model hagi d'aprendre dos comportaments en conflicte.

## Unity New Input System
S'ha abandonat l'enfocament d'Input.GetAxis heredat per l'Input System modern (Package Com.unity.inputsystem).
- **Per què**: Permet un mapeig més net per a diferents controladors (comandament, teclat) i facilita la injecció de controls calculats per la IA en el mode heuristic de ML-Agents.

## Patró Repository al Backend
L'accés a dades a Node.js no es fa directament des dels controladors, sinó a través de Repositoris.
- **Per què**: Facilita el manteniment i permet canviar de base de dades (ex: de MySQL a una altra) sense tocar la lògica de negocis dels serveis o controladors.

## Sincronització Determinista en servidors "Dumb"
En lloc de fer que el servidor enviï l'estat inicial de la bomba o el temps, els clients calculen aquests valors de forma determinista.
- **Per què**: Redueix la càrrega del servidor i la complexitat del codi backend, permetent que un servidor de simple broadcast (WebSockets) mantingui la coherència total de la partida usant l'ID de la sala com a llavor aleatòria compartida.

