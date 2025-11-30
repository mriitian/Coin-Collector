## 📘 Coin Collector – Multiplayer State Sync Test

_Author: Vedant Parkhe_

_Technologies: Unity (Client), Node.js (Authoritative Server), WebSockets_

_Designed for Krafton Assignment_

### 🎮 Overview

This project implements a real-time multiplayer Coin Collector game with an authoritative server, custom network code, and 200ms simulated latency.
It was built specifically to meet the requirements of the Krafton Associate Game Developer Test.

- Two clients connect to the server, join a lobby, ready up, and play a coin-collection match where:

- The server simulates all gameplay (movement, collisions, scoring).

- Clients send only inputs (W/A/S/D).

- Smooth interpolation hides real-world (simulated) latency.

- Coin spawning and scoring are fully server-validated.

- Server-driven match flow: Lobby → Game → Return to Lobby.

No networking engines like Photon/Mirror/NGO were used.
All state syncing is fully custom-built.

### 🚀 Features

- **Authoritative Game Server**
- **Lobby System** (Max 2 Players)
- **Real-Time Gameplay**
- **Smooth Interpolation**
- **Network Quality Simulation**
  - Every message sent from the server is delayed
- **Security**
- **Reconnect Handling**

### 🖥 Running the Server

1. Requires Node.js

```
cd Server
npm install
node server.js
```

2. Run the Client
   - Use Unity Play Mode + Build
   - or Use 2 Unity Builds

### Extra details

1. Unity Assets Used

To improve visual clarity (instead of plain cubes/spheres), I used:

- A simple tank model (player avatar)

- A basic coin model

- A flat ground plane

These were sourced from free Unity Asset Store packages.
Only cosmetic visuals were enhanced — the underlying game logic remains strictly simple and compliant with the assignment.

2. Assumptions Made

- Two-player cap
- - The assignment requires two clients, so the server enforces MAX_PLAYERS = 2.
  - The session begins only after both players mark themselves as ready.

- Color / Name Assignment
  - Player names are randomly and uniquely chosen from a predefined list (Alpha, Bravo, Charlie, Delta).
  - Colors are also assigned uniquely if possible for clarity.
