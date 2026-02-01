using UnityEngine;

public class MenuNavigation : MonoBehaviour
{
    // This function will be called by your UI Button
    public void QuitGame()
    {
        Debug.Log("Exiting System..."); // Useful for testing in the Editor
        
        // This closes the actual game application
        Application.Quit();

        // Note: Application.Quit() does not work inside the Unity Editor.
        // It only works in the final built .app or .exe file.
    }
}