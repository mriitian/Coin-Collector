using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    // Call this from your Play button
    public void OnPlayButtonClicked()
    {
        Debug.Log("Play button clicked — loading GameScene...");
        SceneManager.LoadScene("Lobby");
    }
}
