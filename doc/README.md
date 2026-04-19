# <p align="center">📚 Documentació: Tick-Tack Tag</p>

Aquesta carpeta centralitza tots els recursos tècnics, diagrames i especificacions del projecte **Tick-Tack Tag**, desenvolupat per **Kim Galicio Lamar**.

---

## 🗂️ Índex Principal

### 🏛️ Context i Arquitectura (`doc/context/`)
| Document | Descripció |
| :--- | :--- |
| 🎯 **[00_OBJECTIVE](context/00_OBJECTIVE.md)** | Visió general del projecte i resum del minijoc. |
| 📏 **[01_SCOPE](context/01_SCOPE.md)** | Definicions d'abast, funcionalitats i limitacions. |
| 🏗️ **[02_ARCHITECTURE](context/02_ARCHITECTURE.md)** | Stack tecnològic i interrelació de components. |
| ⚖️ **[03_DECISIONS](context/03_DECISIONS.md)** | Justificació de les eleccions tècniques clau. |
| 📋 **[04_CONVENTIONS](context/04_CONVENTIONS.md)** | Estàndards de codi i estructura del repositori. |
| 📅 **[05_BACKLOG](context/05_BACKLOG.md)** | Gestió de tasques, priorització i estat actual. |
| 🗺️ **[06_SYSTEM_MAP](context/06_SYSTEM_MAP.md)** | Diagrama Mermaid de la infraestructura del joc. |

### 🔍 Especificacions Tècniques (`doc/openspec/specs/`)
* 📑 **[spec.md](openspec/specs/spec.md)**: Detall de les mecàniques de joc i flux de xarxa.
* 🧱 **[foundations.md](openspec/specs/foundations.md)**: Bases teòriques i tècniques.
* 📈 **[plan.md](openspec/specs/plan.md)**: Full de ruta i fases d'implementació.

---

## ⚙️ Configuració de l'Entorn

### 🖥️ Client Unity
- **Versió:** Unity 2022.3 LTS.
- **Paquets Necessaris:** ML-Agents, Native WebSocket, UI Toolkit.
- **Configuració:** Assegurar-se que el `NetworkManager` a l'escena inicial apunta a l'adreça correcta del backend.

### 🌐 Backend Node.js
1. Navega a `Backend/`.
2. Crea un fitxer `.env` basat en `.env.example`.
3. Executa `npm install` i `npm start`.

### 🗄️ Base de Dades MySQL
- Importa el fitxer `database.sql` situat a la carpeta `Backend/` per crear l'esquema necessari.
- Configura les credencials al fitxer `.env` del backend.

---
<p align="center">
  <i>Part del projecte transversal TR3 - Creat per Kim Galicio Lamar</i>
</p>
