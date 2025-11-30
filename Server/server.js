const WebSocket = require("ws");
const { v4: uuidv4 } = require("uuid");

const TICK_RATE = 30;
const MAP_SIZE = 20;
const MOVE_SPEED = 5;
const COIN_SPAWN_INTERVAL = 5000;
const NETWORK_DELAY = 200;

const MAX_PLAYERS = 2;

let players = {};
let coins = [];

const NAME_POOL = ["Alpha", "Bravo", "Charlie", "Delta"];
const COLOR_POOL = [0, 1, 2, 3];

const wss = new WebSocket.Server({ port: 8080 });
console.log("Server running on ws://localhost:8080");

// --------------------------------------
// SEND WITH ARTIFICIAL LAG
// --------------------------------------
function sendWithLag(ws, data) {
  setTimeout(() => {
    if (ws.readyState === WebSocket.OPEN) {
      ws.send(JSON.stringify(data));
    }
  }, NETWORK_DELAY);
}

// --------------------------------------
// RANDOM NAME
// --------------------------------------
function randomName() {
  return "Player" + Math.floor(Math.random() * 9999);
}
function getUniqueName() {
  const usedNames = Object.values(players).map((p) => p.name);

  for (const n of NAME_POOL) {
    if (!usedNames.includes(n)) return n;
  }

  // fallback if all used (rare but safe)
  return "Player" + Math.floor(Math.random() * 9999);
}

function getUniqueColor() {
  const usedColors = Object.values(players).map((p) => p.colorIndex);

  for (const c of COLOR_POOL) {
    if (!usedColors.includes(c)) return c;
  }

  // fallback (should never happen unless >4 players)
  return Math.floor(Math.random() * COLOR_POOL.length);
}

// --------------------------------------
// COIN SPAWN
// --------------------------------------
function spawnCoin() {
  if (coins.length >= 30) return;
  coins.push({
    id: uuidv4(),
    x: Math.random() * MAP_SIZE,
    y: Math.random() * MAP_SIZE,
  });
}
setInterval(spawnCoin, COIN_SPAWN_INTERVAL);

// --------------------------------------
// BUILD LOBBY MESSAGE
// --------------------------------------
function buildLobbyMessage() {
  return {
    type: "lobby",
    lobbyPlayers: Object.values(players).map((p) => ({
      id: p.id,
      name: p.name,
      colorIndex: p.colorIndex,
      ready: p.ready,
    })),
    count: Object.values(players).length,
    max: MAX_PLAYERS,
  };
}

// --------------------------------------
// BROADCAST LOBBY TO ALL
// --------------------------------------
function broadcastLobby() {
  const lobbyMsg = buildLobbyMessage();
  for (const id in players) {
    sendWithLag(players[id].ws, lobbyMsg);
  }
}

// --------------------------------------
// CHECK IF GAME CAN START
// --------------------------------------
function checkStartGame() {
  const list = Object.values(players);

  if (list.length === MAX_PLAYERS && list.every((p) => p.ready === true)) {
    console.log("🎮 ALL PLAYERS READY → START GAME");
    const msg = { type: "startGame" };

    for (const id in players) {
      sendWithLag(players[id].ws, msg);
    }
  }
}

// ============================================================
//                   NEW PLAYER CONNECTION
// ============================================================
wss.on("connection", (ws) => {
  const id = uuidv4();

  players[id] = {
    id,
    x: Math.random() * MAP_SIZE,
    y: Math.random() * MAP_SIZE,
    vx: 0,
    vy: 0,
    input: { up: false, down: false, left: false, right: false },
    score: 0,

    // Lobby fields
    name: getUniqueName(),
    colorIndex: getUniqueColor(),
    ready: false, // players start NOT READY

    ws,
  };

  console.log(
    `Player connected: ${id} (${players[id].name}, color=${players[id].colorIndex})`
  );

  sendWithLag(ws, { type: "welcome", id });

  // Update lobby for all players
  broadcastLobby();

  // --------------------------------------
  // HANDLE INCOMING CLIENT MESSAGES
  // --------------------------------------
  ws.on("message", (message) => {
    // console.log("📩 RAW MESSAGE FROM CLIENT:", message);

    try {
      const data = JSON.parse(message);

      // Client requests lobby state again
      if (data.type === "requestLobby") {
        const lobbyMsg = buildLobbyMessage();
        sendWithLag(ws, lobbyMsg);
      }

      // Movement input
      if (data.type === "input") {
        players[id].input = data.input;
      }

      // Player presses READY
      if (data.type === "playerReady") {
        players[id].ready = true;
        players[id].score = 0;
        broadcastLobby();

        const allReady =
          Object.values(players).length === MAX_PLAYERS &&
          Object.values(players).every((p) => p.ready);

        if (allReady) {
          const startMsg = { type: "startGame" };
          for (const pid in players) {
            sendWithLag(players[pid].ws, startMsg);
          }
        }
      }
    } catch (err) {
      console.error("Invalid message:", message);
    }
  });

  // --------------------------------------
  // HANDLE DISCONNECT
  // --------------------------------------
  ws.on("close", () => {
    console.log("🔥 Player disconnected:", id);

    delete players[id];

    // Tell remaining clients to go back to lobby
    const msg = { type: "returnToLobby" };
    for (const pid in players) {
      sendWithLag(players[pid].ws, msg);
    }
    for (const pid in players) {
      players[pid].ready = false; // reset ready state
      players[pid].score = 0; // 🔥 RESET SCORE HERE
    }

    // Update lobby
    broadcastLobby();
  });
});

// ============================================================
//                      GAME LOOP
// ============================================================
setInterval(() => {
  const list = Object.values(players);

  // game only runs when all players in match started
  if (list.length !== MAX_PLAYERS) return;

  const dt = 1 / TICK_RATE;

  // Update movement
  for (const id in players) {
    const p = players[id];

    p.vx = (p.input.right - p.input.left) * MOVE_SPEED;
    p.vy = (p.input.up - p.input.down) * MOVE_SPEED;

    p.x = Math.max(0, Math.min(MAP_SIZE, p.x + p.vx * dt));
    p.y = Math.max(0, Math.min(MAP_SIZE, p.y + p.vy * dt));

    // Coin collision
    coins = coins.filter((coin) => {
      const dx = coin.x - p.x;
      const dy = coin.y - p.y;
      if (dx * dx + dy * dy < 1) {
        p.score++;
        return false;
      }
      return true;
    });
  }

  // Send state update
  const state = {
    type: "state",
    players: list.map((p) => ({
      id: p.id,
      x: p.x,
      y: p.y,
      score: p.score,
      name: p.name,
      colorIndex: p.colorIndex,
    })),
    coins,
  };

  for (const id in players) {
    sendWithLag(players[id].ws, state);
  }
}, 1000 / TICK_RATE);
