using System;

[Serializable]
public class PlayerState
{
    public string id;
    public float x;
    public float y;
    public int score;

    public string name;
    public int colorIndex;
}

[Serializable]
public class LobbyPlayer
{
    public string id;
    public string name;
    public int colorIndex;
    public bool ready;
}


[Serializable]
public class CoinState
{
    public string id;
    public float x;
    public float y;
}

[Serializable]
public class ServerMessage
{
    public string type;          // "welcome" or "state"
    public string id;            // used only in welcome
    public PlayerState[] players;
    public CoinState[] coins;

    // lobby
    public LobbyPlayer[] lobbyPlayers;
    public int count;
    public int max;
}

[Serializable]
public class InputPayload
{
    public bool up;
    public bool down;
    public bool left;
    public bool right;
}

[Serializable]
public class InputMessage
{
    public string type;
    public InputPayload input;
}


[Serializable]
public class SimpleMessage
{
    public string type;
}

[Serializable]
public class JoinMessage
{
    public string type;
    public string name;
    public int colorIndex;
}