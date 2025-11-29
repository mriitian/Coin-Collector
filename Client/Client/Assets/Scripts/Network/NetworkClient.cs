using UnityEngine;
using WebSocketSharp;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetworkClient : MonoBehaviour
{
    public static NetworkClient Instance;

    private WebSocket ws;
    public string myId = "";
    public bool connected = false;

    private readonly Queue<string> messageQueue = new Queue<string>();

    public event Action<ServerMessage> OnServerMessage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Connect();
    }

    private void Connect()
    {
        ws = new WebSocket("ws://localhost:8080");

        ws.OnOpen += (sender, e) =>
        {
            connected = true;
            Debug.Log("Connected to server!");
            SendJoinMessage();
        };

        ws.OnMessage += (sender, e) =>
        {
            lock (messageQueue)
            {
                messageQueue.Enqueue(e.Data);
            }
        };

        ws.OnClose += (sender, e) =>
        {
            connected = false;
            Debug.Log("Disconnected.");
        };

        ws.OnError += (sender, e) =>
        {
            Debug.LogError("WebSocket error: " + e.Message);
        };

        ws.ConnectAsync();
    }

    private void Update()
    {
        while (true)
        {
            string json;
            lock (messageQueue)
            {
                if (messageQueue.Count == 0)
                    break;

                json = messageQueue.Dequeue();
            }

            if (string.IsNullOrWhiteSpace(json))
                continue;

            ServerMessage msg = null;
            try
            {
                msg = JsonUtility.FromJson<ServerMessage>(json);
            }
            catch
            {
                Debug.LogError("⚠ JSON ERROR: " + json);
                continue;
            }

            if (msg == null || string.IsNullOrEmpty(msg.type))
                continue;

            // ------------------------------ SPECIAL MESSAGES ------------------------------

            if (msg.type == "welcome")
            {
                myId = msg.id;
                Debug.Log("Assigned ID: " + myId);
                continue;
            }

            if (msg.type == "returnToLobby")
            {
                Debug.Log("🔁 Server told us returnToLobby");
                SceneManager.LoadScene("Lobby");
                continue;
            }

            // Forward everything else
            OnServerMessage?.Invoke(msg);
        }
    }

    // SEND JOIN MESSAGE
    public void SendJoinMessage()
    {
        var join = new JoinMessage
        {
            type = "join",
            name = "default",
            colorIndex = 0
        };

        ws.Send(JsonUtility.ToJson(join));
    }

    // SEND READY
    public void SendReady()
    {
        var msg = new SimpleMessage { type = "playerReady" };
        string json = JsonUtility.ToJson(msg);
        ws.Send(json);
    }

    // REQUEST LOBBY AFTER SOCKET IS READY
    public IEnumerator RequestLobbyDelayed()
    {
        while (!connected || ws.ReadyState != WebSocketState.Open)
            yield return null;

        var req = new SimpleMessage { type = "requestLobby" };
        string json = JsonUtility.ToJson(req);

        Debug.Log("📤 Sending REQUEST LOBBY: " + json);
        ws.Send(json);
    }

    // SEND INPUT DURING GAME
    public void SendInput(bool up, bool down, bool left, bool right)
    {
        if (!connected || ws.ReadyState != WebSocketState.Open)
            return;

        var payload = new InputPayload
        {
            up = up,
            down = down,
            left = left,
            right = right
        };

        var msg = new InputMessage
        {
            type = "input",
            input = payload
        };

        ws.Send(JsonUtility.ToJson(msg));
    }

    private void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }
    }
}
