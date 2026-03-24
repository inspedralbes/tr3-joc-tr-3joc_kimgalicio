# Log de Traçabilitat: Desenvolupament Guiat per Especificació (SDD)

## Fase 1: Definició de l'especificació (OpenSpec)

* **Data:** 24/03/2026
* **Context / Problema:** Inici de la pràctica. Generació de l'arquitectura de l'especificació per al joc multijugador 2D. (Nota d'error: Inicialment, vaig intentar executar l'ordre directament al terminal bash, però ho vaig corregir utilitzant la interfície de xat de l'extensió Gemini CLI).
* **Prompt utilitzat:** > `/opsx:propose Crear la funcionalitat principal del minijoc "Pilla-pilla bomba" (estil Butasan) per a 2-3 jugadors. El frontend és en Unity 2D. El backend ha d'utilitzar Node.js amb el patró Repository i WebSockets per a la sincronització en temps real limitat. A més, cal integrar un agent de ML-Agents entrenat en CPU amb Docker perquè actuï com a bot.`
* **Resultat:** L'agent ha generat una estructura completa a la ruta `openspec/changes/pilla-pilla-bomba`. Ha creat els fitxers `proposal.md`, `design.md` i `tasks.md` (que cobreixen el context i el pla), i ha dividit l'especificació en tres dominis clars per facilitar-ne la implementació:
  * `bomb-tag-mechanics/spec.md`
  * `bot-ai-inference/spec.md`
  * `multiplayer-sync/spec.md`

---