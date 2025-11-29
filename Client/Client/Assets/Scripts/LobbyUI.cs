using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    public GameObject entryTemplate;
    public Transform listParent;

    void Start()
    {
        // Subscribe to server messages
        NetworkClient.Instance.OnServerMessage += OnServerMessage;
        // When entering lobby, always join the lobby again
        NetworkClient.Instance.SendJoinMessage();

        // Then ask server to send lobby
        StartCoroutine(NetworkClient.Instance.RequestLobbyDelayed());
    }

    void OnDestroy()
    {
        if (NetworkClient.Instance != null)
            NetworkClient.Instance.OnServerMessage -= OnServerMessage;
    }

    void OnServerMessage(ServerMessage msg)
    {
        Debug.Log("📥 MESSAGE RECEIVED TYPE = " + msg.type);

        switch (msg.type)
        {
            case "lobby":
                Debug.Log("📥 LOBBY RECEIVED. Players: " + msg.count);
                UpdateLobby(msg);
                break;

            case "startGame":
                Debug.Log("🎮 START GAME RECEIVED!");
                SceneManager.LoadScene("GameScene");
                break;

            case "returnToLobby":
                Debug.Log("🔁 RETURN TO LOBBY RECEIVED");
                SceneManager.LoadScene("Lobby");
                break;
        }
    }

    void UpdateLobby(ServerMessage msg)
    {
        if (msg.lobbyPlayers == null)
        {
            Debug.LogWarning("⚠ Lobby message had null lobbyPlayers");
            return;
        }

        // Clear old entries
        foreach (Transform child in listParent)
        {
            if (child.gameObject != entryTemplate)
                Destroy(child.gameObject);
        }

        // Rebuild UI entries
        foreach (var p in msg.lobbyPlayers)
        {
            GameObject entry = Instantiate(entryTemplate, listParent);
            entry.SetActive(true);

            TMP_Text text = entry.GetComponent<TMP_Text>();
            text.text = $"{p.name} ({(p.ready ? "Ready" : "Waiting")})";
        }
    }

    // Called by READY button
    public void OnReadyClicked()
    {
        Debug.Log("📤 Sending READY");
        NetworkClient.Instance.SendReady();
    }
}
