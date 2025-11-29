using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;   // Assign your Tank prefab here
    public GameObject coinPrefab;     // Assign your Coin prefab here
    public ScoreboardUI scoreboard;

    private NetworkClient net;

    // Dictionaries for spawned objects
    private Dictionary<string, PlayerRenderer> players = new Dictionary<string, PlayerRenderer>();
    private Dictionary<string, GameObject> coins = new Dictionary<string, GameObject>();

    void Start()
    {
        net = NetworkClient.Instance;

        if (net == null)
        {
            Debug.LogError("NetworkClient not found in scene!");
            return;
        }

        // Subscribe to server messages
        
        

    }

    void OnEnable()
    {
        if (NetworkClient.Instance != null)
            NetworkClient.Instance.OnServerMessage += HandleServerMessage;
    }

    void OnDisable()
    {
        if (NetworkClient.Instance != null)
            NetworkClient.Instance.OnServerMessage -= HandleServerMessage;
    }


    void Update()
    {
        if (net == null || !net.connected)
            return;

        // Capture input (WASD)
        bool up = Input.GetKey(KeyCode.W);
        bool down = Input.GetKey(KeyCode.S);
        bool left = Input.GetKey(KeyCode.A);
        bool right = Input.GetKey(KeyCode.D);

        net.SendInput(up, down, left, right);
    }

    void HandleServerMessage(ServerMessage msg)
    {
        if (msg.type != "state")
            return;

        HandlePlayers(msg.players);
        HandleCoins(msg.coins);

        scoreboard.UpdateScores(msg.players);
    }

    // ------------------------------
    // PLAYER SYNC
    // ------------------------------
    void HandlePlayers(PlayerState[] serverPlayers)
    {
        // ------------------------------------
        // FIX 1: Handle EMPTY player list
        // ------------------------------------
        if (serverPlayers == null || serverPlayers.Length == 0)
        {
            foreach (var kvp in players)
                Destroy(kvp.Value.gameObject);

            players.Clear();
            return;
        }

        // Build a set of players that exist on the server
        HashSet<string> serverIds = new HashSet<string>();
        foreach (var p in serverPlayers)
            serverIds.Add(p.id);

        // Remove local players that no longer exist
        List<string> localIds = new List<string>(players.Keys);
        foreach (string id in localIds)
        {
            if (!serverIds.Contains(id))
            {
                Destroy(players[id].gameObject);
                players.Remove(id);
            }
        }

        // Spawn/update all players from server
        foreach (var p in serverPlayers)
        {
            if (!players.ContainsKey(p.id))
            {
                var obj = Instantiate(playerPrefab);
                var renderer = obj.GetComponent<PlayerRenderer>();
                var appearance = obj.GetComponentInChildren<PlayerAppearance>();

                renderer.id = p.id;
                players[p.id] = renderer;

                appearance.ApplyAppearance(p.name, p.colorIndex);

                obj.transform.position = new Vector3(p.x, p.y, 0f);
                renderer.SetTargetPosition(p.x, p.y);
            }
            else
            {
                // Existing player — update
                var renderer = players[p.id];
                renderer.SetTargetPosition(p.x, p.y);

                var appearance = renderer.GetComponentInChildren<PlayerAppearance>();
                appearance.ApplyAppearance(p.name, p.colorIndex);
            }
        }
    }

    // ------------------------------
    // COIN SYNC
    // ------------------------------
    void HandleCoins(CoinState[] serverCoins)
    {
        if (serverCoins == null) return;

        HashSet<string> incoming = new HashSet<string>();

        // Build a set of all server coin IDs
        foreach (var c in serverCoins)
            incoming.Add(c.id);

        // Remove coins that disappeared on server
        List<string> localIds = new List<string>(coins.Keys);
        foreach (string id in localIds)
        {
            if (!incoming.Contains(id))
            {
                Destroy(coins[id]);
                coins.Remove(id);
            }
        }

        // Add or update coins
        foreach (var c in serverCoins)
        {
            if (!coins.ContainsKey(c.id))
            {
                var obj = Instantiate(coinPrefab);
                coins[c.id] = obj;
            }

            coins[c.id].transform.position = new Vector3(c.x, c.y, 0);
        }
    }
}
