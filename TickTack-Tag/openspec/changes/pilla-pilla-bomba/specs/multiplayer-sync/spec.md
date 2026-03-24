## ADDED Requirements

### Requirement: State Synchronization
The system SHALL synchronize player positions and bomb status across all connected clients using WebSockets.

#### Scenario: Position update
- **WHEN** a player moves on their local client
- **THEN** their new position is broadcasted to all other connected clients via WebSocket

### Requirement: Real-time Bomb Transfer
The system SHALL ensure that bomb transfer events are synchronized immediately to prevent desync.

#### Scenario: Synchronized bomb transfer
- **WHEN** a bomb transfer occurs on the server
- **THEN** all clients receive a WebSocket message and update their local bomb carrier state
