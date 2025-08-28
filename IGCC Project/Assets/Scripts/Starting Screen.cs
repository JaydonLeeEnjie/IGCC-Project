using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class StartingScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private List<GameObject> images = new List<GameObject>();
    [SerializeField] private float displayTime = 10f; // How long each image stays visible
    [SerializeField] private string nextSceneName = "Yuina Testing Scene"; // Scene to load afterwards

    // Quits the game (only works in a build, not in the editor)
    public void QuitOption()
    {
        Application.Quit();
        Debug.Log("Game quit! (won't show in editor)");
    }

    // Starts the coroutine for loading the next scene
    public void StartOption()
    {
        StartCoroutine(ShowImagesAndLoadScene());
    }

    private IEnumerator ShowImagesAndLoadScene()
    {
        // Hide all images first
        foreach (GameObject img in images)
        {
            if (img != null) img.SetActive(false);
        }

        // Show each image one by one
        for (int i = 0; i < images.Count; i++)
        {
            if (images[i] != null)
            {
                images[i].SetActive(true);

                // Only disable if it's NOT the last image
                if (i < images.Count - 1)
                {
                    yield return new WaitForSeconds(displayTime);
                    images[i].SetActive(false);
                }
                else
                {
                    // Last image stays visible until scene loads
                    yield return new WaitForSeconds(displayTime);
                }
            }
        }

        // After cycling, load next scene
        SceneManager.LoadScene(nextSceneName);
    }
}
