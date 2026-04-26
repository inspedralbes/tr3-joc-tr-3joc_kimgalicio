# 03_DECISIONS

## Implementació de la IA: ML-Agents en lloc de Màquina d'Estats
Inicialment es va considerar un enfocament de Màquina d'Estats per als bots. Tot i això, es va decidir utilitzar Unity ML-Agents per permetre comportaments més complexos i emergents (perseguir, fugir, utilitzar escaleres) que són més difícils de programar manualment en un entorn de plataformes dinàmic.

## Gestió d'Estat: Patró ScriptableObject
Utilitzem `GameStateSO` com a nucli central de dades.
- **Per què**: Permet una referència fàcil des de qualsevol script sense necessitat de singletons complexos o acoblament directe entre scripts com la `UI` i la `Bomba`.

## Combat Simplificat
S'ha eliminat qualsevol mecànica d'"atac" o de "monedes".
- **Per què**: L'objectiu és centrar-se en la mecànica pura de la "Patata Calenta" i l'habilitat de plataformes.

## Migració a MySQL i Docker, Desplegament WebGL
Es va decidir migrar d'una persistència en memòria a una base de dades MySQL i empaquetar el backend amb Docker. A més, el client de Unity s'ha compilat per a **WebGL**.
- **Per què**: Permet accessibilitat universal des de qualsevol navegador modern, escalar fàcilment amb un servidor Nginx, i centralitzar l'autenticació/persistencia en el servidor de producció.

## UI Toolkit per sobre de uGUI
S'ha adoptat **UI Toolkit** (UXML/USS) per al desenvolupament de la interfície de menús i pantalles final de partida.
- **Per què**: Ofereix un flux de treball més similar al desenvolupament web, millor separació entre estil i lògica, i una gestió més eficient de layouts complexos.

## Brain Swapping a la IA
En lloc d'un únic model per a tot, s'utilitza una tècnica de "Brain Swapping".
- **Per què**: Permet al bot ser extremadament bo en rols oposats (perseguir vs fugir) canviant el model neuronal en temps real basant-se en si té la bomba o no, evitant que un sol model hagi d'aprendre dos comportaments en conflicte.

## Sincronització Determinista i Autoritat Híbrida
En lloc de fer que el servidor enviï constantment estats i col·lisions absolutistes, els clients tenen autoritat local i utilitzen `NetworkPlayerSync` per als oponents.
- **Per què**: Redueix la latència percebuda en el propi moviment. Els jugadors remots es tornen `isKinematic` per no interactuar malament amb les físiques locals, interpolant només les coordenades. 

## Prevenció de Desincronització (Multi-Tab) i Abandons
S'ha reforçat l'estat en memòria del servidor WebSocket (`activeConnections`) de tal manera que si el mateix usuari obre múltiples pestanyes, només la primera s'accepta i les següents es deneguen, i si algú abandona la partida, el client rep automàticament la victòria.
- **Per què**: Assegura l'estabilitat i la consistència. Evita que accions de pestanyes "fantasma" destrueixin l'estat d'una partida i protegeix l'experiència d'usuari (UX) atorgant la victòria en cas de fugida de l'oponent.
