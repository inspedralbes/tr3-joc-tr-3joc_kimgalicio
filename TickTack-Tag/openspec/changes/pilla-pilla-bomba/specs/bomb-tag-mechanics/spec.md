## ADDED Requirements

### Requirement: Player Movement
The system SHALL allow 2-3 players to move within a 2D arena using standard input (WASD/Arrows).

#### Scenario: Player moves up
- **WHEN** the player presses the "W" or "Up Arrow" key
- **THEN** the player character moves upwards in the 2D arena

### Requirement: Bomb Possession and Passing
The system SHALL designate one player as the "bomb carrier". When the bomb carrier collides with another player, the bomb SHALL be passed to that player.

#### Scenario: Bomb transfer on collision
- **WHEN** the player with the bomb collides with another player
- **THEN** the bomb is immediately transferred to the other player

### Requirement: Explosion Timer
The system SHALL maintain a countdown timer for the bomb. When the timer reaches zero, the current bomb carrier SHALL be eliminated.

#### Scenario: Player eliminated on explosion
- **WHEN** the bomb timer reaches zero
- **THEN** the current bomb carrier is removed from the game arena
