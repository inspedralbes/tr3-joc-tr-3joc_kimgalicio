# Fonaments

## Context
Minijoc 2D de plataformes competitiu que fusiona mecàniques de "Super Smash Bros" i "Patata Calenta". El projecte és una aplicació full-stack que utilitza **Unity (exportat a WebGL)** com a client dinàmic pel navegador i Node.js com a servidor d'alt rendiment per al backend i serveis multijugador online. Està completament desplegat i hostat a producció.

## Objectius (Features)
- **Sistema de Supervivència**: 3 vides per entitat. Partida en rondes seqüencials, mort per explosió o caiguda. Victòria per resistència o per abandonament del rival.
- **Multijugador Sincronitzat Robust**: Ús de WebSockets per a una experiència competitiva en temps real on les físiques asíncrones s'esvaeixen suau i interpoladament, evitant els errors típics per múltiples pestanyes (multi-tab isolation).
- **Persistència de Dades**: MySQL per emmagatzemar perfils d'usuari, estadístiques i victòries o derrotes confirmades pel servidor.
- **Feedback Visual Avançat**: Contorns dinàmics (`GameStateSO`), i interfícies completament orgàniques de disseny responsiu creades amb UI Toolkit (UXML/USS).
- **Arquitectura de Repositoris**: Backend Node.js modular basat en Repository i serveis, facilitant tant l'API HTTP com el servidor de sockets central.

## Arquitectura i IA
- **Estat Global**: `GameStateSO` centralitza tota la veritat lògica local, sent actualitzada determinísticament pel `NetworkManager` quan juga en mode online.
- **Físiques Aïllades**: Esdeveniments d'inputs locals, delegant moviments dels rivals a scripts especials com el `NetworkPlayerSync` per sobreescriure posicions.
- **IA (Bots)**: ML-Agents actuant en mode "Fallback" o Local, amb canvi de conducta ("Brain Swapping") per alternar estats de persecució o fugida segons la possessió de la bomba.

## Restriccions
- El desenvolupament segueix la metodologia Spec-Driven Development (amb Openspec).
- La mida de compilació i l'optimització de comunicació han d'estar equilibrats per funcionar d'una forma completament fluida en una pestanya de qualsevol navegador modern amb WebGL.
