## ADDED Requirements

### Requirement: Bot Navigation
The bot SHALL navigate the arena to either avoid the bomb carrier (if not holding the bomb) or chase other players (if holding the bomb).

#### Scenario: Bot avoids carrier
- **WHEN** the bot does not have the bomb and the carrier is nearby
- **THEN** the bot moves away from the carrier

### Requirement: ML-Agent Integration
The system SHALL interface with an ML-Agents model running in a Docker container to provide movement commands for the bot.

#### Scenario: ML-Agent decision loop
- **WHEN** the game state is updated
- **THEN** the bot's observations are sent to the ML-Agent service and its decision is applied to the bot's movement
