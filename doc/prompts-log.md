# Log de Traçabilitat: Desenvolupament Guiat per Especificació (SDD)

## Fase 1: Definició de l'especificació (OpenSpec)

* **Data:** 24/03/2026
* **Context / Problema:** Inici de la pràctica. Generació de l'arquitectura de l'especificació per al joc multijugador 2D.
* **Prompt utilitzat:** > `/opsx:propose Crear la funcionalitat principal del minijoc "Pilla-pilla bomba" (estil Butasan) per a 2-3 jugadors. El frontend és en Unity 2D. El backend ha d'utilitzar Node.js amb el patró Repository i WebSockets per a la sincronització en temps real limitat. A més, cal integrar un agent de ML-Agents entrenat en CPU amb Docker perquè actuï com a bot.`
* **Resultat:** L'agent ha generat una estructura completa a la ruta `openspec/changes/pilla-pilla-bomba`. Ha creat els fitxers `spec.md`, `plan.md` i `tasks.md`.

---

## Fase 2: IA Avançada (Brain Swapping amb ML-Agents)

* **Data:** 05/04/2026
* **Context / Problema:** Els bots bàsics són massa predictibles. Necessitem una IA que sàpiga tant fugir quan té la bomba com perseguir quan no la té, sense que els comportaments es barregin.
* **Prompt utilitzat:** > `Implementa el script BotController.cs que hereti d'Agent de ML-Agents. Utilitza la tècnica de "Brain Swapping" per canviar el model .onnx (Catcher vs Evader) en temps real segons si el bot és el CurrentBombOwner al GameStateSO. Defineix les observacions (posició, distància relativa, estat de la bomba i escaleres) i un sistema de recompenses punitiu per portar la bomba.`
* **Resultat:** Els bots ara mostren comportaments emergents i canvien de model instantàniament en rebre o passar la bomba.

---

## Fase 3: Interfície d'Usuari Moderna amb UI Toolkit

* **Data:** 12/04/2026
* **Context / Problema:** El sistema uGUI tradicional és feixuc de mantenir i difícil d'estilar amb coherència.
* **Prompt utilitzat:** > `Migra tot el sistema de menús i HUD a Unity UI Toolkit (UXML/USS). Crea les pantalles de MainMenu i Endgame amb estils consistents i implementa els controladors C# (MenuController, EndgameController) fent servir Query per buscar elements (btn-vs-bot, nickname-input). Integra la càrrega d'escenes i la connexió amb el NetworkManager dès de la UI.`
* **Resultat:** Una interfície més neta, estilada amb USS (CSS-like) i amb una separació clara entre disseny i lògica.

---

## Fase 4: Persistència en MySQL i Patró Repository

* **Data:** 18/04/2026
* **Context / Problema:** Les dades del joc es perden en tancar el servidor. Cal emmagatzemar victòries, derrotes i perfils d'usuari.
* **Prompt utilitzat:** > `Configura el backend per utilitzar MySQL amb Node.js. Implementa el patró Repository per a les entitats User i Game. Defineix rutes de Login que gestionin la creació automàtica d'usuaris si no existeixen i assegura't que l'API de finalització de partida actualitzi les estadístiques de wins/losses a la DB.`
* **Resultat:** Una persistència de dades robusta i una arquitectura de backend altament mantenible.

---

## Fase 5: Robustesa Multijugador i Sincronització Determinista

* **Data:** 22/04/2026
* **Context / Problema:** Desincronització en l'inici de partida i errors de contracte en l'acció "move" que trencaven el tempo real.
* **Prompt utilitzat:** > `Corregeix la discrepància del contracte de WebSockets: el camp "move" ha d'usar un objecte "position" amb X i Y niats. A més, implementa una assignació determinista de la bomba inicial usant el GameId com a llavor aleatòria i assegura que només el jugador local respon als inputs del teclat (mapeig de useAiInput des del GameManager).`
* **Resultat:** Sincronització absoluta sense errors internament ni en consola. Els jugadors es mouen correctament i coincideixen en qui té la bomba.

---

## Fase 6: Poliment Multijugador i Prevenció de Multi-Tab

* **Data:** 26/04/2026
* **Context / Problema:** Quan un usuari obria múltiples pestanyes o es recarregava la pàgina, es corrompia l'estat de la partida al WebSocket (creant jugadors "fantasma"). A més, no hi havia forma de gestionar justament quan un oponent abandonava o desconnectava a mig joc.
* **Prompt utilitzat:** > `Millora el sistema de WebSockets del backend per prevenir inicis de sessió múltiples del mateix UUID actiu (gestió de "Múltiples Pestanyes"). A més, quan detectis que un usuari tanca la connexió durant la partida, emet un esdeveniment 'opponent_abandoned'. Al client de Unity, integra això modificant el GameManager i l'EndgameController perquè es declari victòria automàtica amb el missatge "Oponent ha abandonat".`
* **Resultat:** Sessió multijugador robusta. Prevenció de conflictes per dobles connexions i atorgament just de victòries per abandonament.

---

## Fase 7: Desplegament WebGL i Interpolació de Xarxa

* **Data:** 26/04/2026
* **Context / Problema:** Els rivals tenien problemes de "jitter" (tremolor) perquè els motors físics locals i les dades asíncrones col·lidien. Calia solucionar-ho per polir la jugabilitat i publicar oficialment el joc.
* **Prompt utilitzat:** > `Extreu la lògica de sincronització de coordenades del jugador remot cap a un nou script exclusiu anomenat 'NetworkPlayerSync'. En aquest script, posa el Rigidbody2D del rival com a isKinematic=true per desconnectar-lo del motor físic local, i utilitza Vector2.Lerp per interpolar fluidament entre les posicions rebudes. Finalment, prepara l'aplicació per fer-ne una build WebGL i explica com desplegar-la i configurar Nginx per servir els fitxers a 'ticktack-tag.dam.inspedralbes.cat'.`
* **Resultat:** Moviments d'oponents molts més fluids i sense tremolors. Client empaquetat en WebGL i llançament a producció complet al servidor remot.