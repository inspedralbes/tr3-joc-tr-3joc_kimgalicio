## 1. Backend Infrastructure

- [ ] 1.1 Initialize Node.js project and install dependencies (`ws`, `express`).
- [ ] 1.2 Implement `GameSessionRepository` to manage player positions and bomb state.
- [ ] 1.3 Create WebSocket server to handle player connections and disconnections.
- [ ] 1.4 Implement message protocol for position updates and bomb transfer events.

## 2. Unity Frontend Development

- [ ] 2.1 Set up the 2D arena scene with boundary collisions.
- [ ] 2.2 Create Player and Bomb prefabs with necessary scripts.
- [ ] 2.3 Implement `NetworkManager` script using WebSockets to sync state with the backend.
- [ ] 2.4 Add client-side prediction and interpolation for smoother movement.
- [ ] 2.5 Implement visual indicators for the bomb carrier and explosion countdown.

## 3. AI & ML-Agents Integration

- [ ] 3.1 Dockerize the ML-Agents inference environment (Python/PyTorch/ONNX).
- [ ] 3.2 Create a REST/gRPC API in the AI service to receive observations and return actions.
- [ ] 3.3 Implement bot logic in the Node.js backend to query the AI service.
- [ ] 3.4 Fine-tune bot behavior for "avoidance" and "chase" states.

## 4. Final Integration & Testing

- [ ] 4.1 Validate multi-client synchronization (2-3 players).
- [ ] 4.2 Verify authoritative bomb transfer on the server.
- [ ] 4.3 Test end-to-end gameplay loop with human players and bots.
- [ ] 4.4 Ensure Docker container orchestration for the AI service.
