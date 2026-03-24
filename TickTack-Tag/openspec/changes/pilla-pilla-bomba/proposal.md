## Why

The goal is to provide a competitive and fun minigame ("Pilla-pilla bomba", style Butasan) for 2-3 players within the Unity 2D environment. This change also serves to implement a robust real-time multiplayer architecture using Node.js and WebSockets, and integrates ML-Agents for intelligent bot behavior.

## What Changes

- **Unity 2D Frontend**: Implementation of the game arena, player characters, and bomb mechanics.
- **Node.js Backend**: A new backend service using the Repository pattern to manage game state and player data.
- **WebSocket Synchronization**: Real-time communication for player movement and bomb state updates.
- **ML-Agents Bot**: A trained AI bot capable of participating in the game, running on CPU via Docker.
- **Docker Integration**: Containerization of the ML-Agents inference service for easy deployment and integration with the backend/frontend.

## Capabilities

### New Capabilities
- `bomb-tag-mechanics`: Handles movement, bomb possession, passing logic, and the explosion timer.
- `multiplayer-sync`: Manages WebSocket connections and state broadcasting between 2-3 players.
- `bot-ai-inference`: Provides an interface for the ML-Agents model to control a bot player in the game.

### Modified Capabilities
- (None)

## Impact

- **Assets/scripts**: New Unity scripts for player input, bomb behavior, and network client.
- **Backend/Server**: New Node.js project with WebSocket support and Repository layer.
- **Deployment**: New Dockerfile and configuration for the AI service.
