using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Settings")]
    public string gameSceneName = "MainGame";    // Your actual gameplay scene
    public string optionsSceneName = "MicSetup"; // Your mic selection scene

    // 1. RESUME: Returns to the game
    public void ResumeGame()
    {
        // If this is a separate scene, load the game scene
        SceneManager.LoadScene(gameSceneName);
        
        // If this is just a UI panel inside the game scene:
        // gameObject.SetActive(false);
        // Time.timeScale = 1;
    }

    // 2. OPTIONS: Goes back to Mic Setup
    public void OpenOptions()
    {
        SceneManager.LoadScene(optionsSceneName);
    }

    // 3. QUIT: Exits the application
    public void QuitToDesktop()
    {
        Debug.Log("Shutting down terminal...");
        Application.Quit();
    }
}