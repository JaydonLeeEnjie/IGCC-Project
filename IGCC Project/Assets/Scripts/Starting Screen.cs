using UnityEngine;
using UnityEngine.SceneManagement; // Needed for loading scenes

public class StartingScreen : MonoBehaviour
{
    // Quits the game (only works in a build, not in the editor)
    public void QuitOption()
    {
        Application.Quit();
        Debug.Log("Game quit! (won't show in editor)");
    }

    // Loads the next scene (replace "GameScene" with your scene name)
    public void StartOption()
    {
        SceneManager.LoadScene("Yuina Testing Scene");
    }
}

