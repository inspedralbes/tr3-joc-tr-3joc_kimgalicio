## Context

The "Pilla-pilla bomba" (Bomb Tag) minigame is a new addition to the TickTack-Tag project. It requires real-time interaction between 2-3 players (humans and/or bots). The current project has basic 2D assets and scripts but lacks a networked backend and AI infrastructure.

## Goals / Non-Goals

**Goals:**
- Real-time 2D multiplayer gameplay for 2-3 players.
- Authoritative backend using Node.js and WebSockets.
- Intelligent bot behavior using ML-Agents.
- Clean backend architecture using the Repository pattern.
- Containerized AI service for CPU-based inference.

**Non-Goals:**
- Persistent user accounts or global leaderboards.
- Complex matchmaking (session-based connection is sufficient).
- Advanced physics simulations beyond basic 2D collisions.

## Decisions

- **Backend Architecture**: Node.js with a Repository pattern. A `GameSessionRepository` will manage the lifecycle of game rooms and player states in-memory.
- **Communication**: WebSockets (`ws` library) for low-latency synchronization of player coordinates and bomb ownership.
- **AI Integration**: ML-Agents model exported to ONNX or run via a Python/PyTorch service.
- **Inference Hosting**: A Docker container running a FastAPI/Flask server to host the ML-Agents model. The Node.js backend will request actions from this service on behalf of bot players.
- **Unity Networking**: A custom WebSocket client in Unity to handle state updates and send player input to the server.

## Risks / Trade-offs

- **[Risk] Network Desync** → **Mitigation**: Server-side authority for bomb transfers and position validation; client-side interpolation for smooth movement.
- **[Risk] AI Latency** → **Mitigation**: Decouple AI decision frequency (e.g., 10-20Hz) from the game's rendering frame rate.
- **[Risk] Docker Resource Usage** → **Mitigation**: Optimize the ML-Agents service for CPU-only inference to minimize overhead.
