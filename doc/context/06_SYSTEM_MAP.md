# 06_SYSTEM_MAP

## Scripts Clau i Relacions

```mermaid
graph TD
    %% Client Unity
    subgraph Unity_Client[Client Unity WebGL]
        ISA[Input System Actions] --> PC[PlayerModeController2D Local]
        BC[BotController] --> PC
        BC -- Inference --> IM[IA_Models .onnx]
        
        NPS[NetworkPlayerSync Remot] --> PC
        NPS -- "Interpola" --> PC
        
        PC --> GSO[GameStateSO]
        NM[NetworkManager] --> GSO
        NM --> NPS
        
        GM[GameManager] --> GSO
        GM --> HUD[HUDController]
        GM --> EC[EndgameController]
        GM --> TD[TimerDisplay]
        B[Bomb] --> GSO
        
        UI[MenuController] --> NM
    end

    %% Backend
    subgraph Backend_Server[Nginx & Node.js Server]
        Nginx[Nginx Reverse Proxy] --> S[Server Node.js]
        Nginx --> WebGL_Files[Static WebGL Build]
        S --> WS[WebSocket Gateway]
        WS --> UC[User Controller]
        UC --> UR[User Repository]
        UR --> DB[(MySQL DB)]
    end

    %% Connexions
    NM <==> Nginx
    Nginx <==> WS
    NM -- HTTP --> UC
```

## Descripcions

- **NetworkManager**: Punt d'entrada per a tota la comunicació externa (HTTP/WS). Sincronitza l'estat remot, gestiona errors de xarxa i delega la instanciació de components com `NetworkPlayerSync`.
- **NetworkPlayerSync**: Adjuntat als jugadors remots (en comptes del sistema d'inputs locals). Fa d'intermediari entre els esdeveniments WebSocket i l'Animator i Rigidbody (marcat com a Cinemàtic).
- **GameStateSO**: El magatzem central de dades local. Notifica a la UI i als controladors els canvis d'estat de forma desacoblada.
- **Backend Server (Nginx + Node.js)**: Nginx serveix els fitxers WebGL per a navegadors i fa proxy per a l'API Node.js i WebSockets. El servidor gestiona multijugador, cua i abandons.
- **GameManager**: Autoritat per a la lògica de joc local i la coordinació dels agents. Decideix quan cridar al HUD (inici de ronda) o l'EndgameController (victòria/derrota/abandó).
- **UI Toolkit Controllers**: 
  - `MenuController`: Gestiona Login i selecció.
  - `HUDController`: Mostra text in-game ("Comença la Ronda 1").
  - `EndgameController`: Pantalles de resultats.
- **BotController**: IA que utilitza "Brain Swapping" per decidir si perseguir o fugir basant-se en l'estat de la bomba al `GameStateSO` (útil en mode VS_BOT).
