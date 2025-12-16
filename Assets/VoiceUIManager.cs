using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VoiceUIManager : MonoBehaviour
{
    public VoiceCalibrationManager manager; 
    public TextMeshProUGUI statusLabel; 
    public Button startButton; // Optional: Drag your button here to hide it after start

    private void Update()
    {
        if (statusLabel != null && manager != null)
        {
            statusLabel.text = manager.realtimeStatus;
        }
    }

    public void OnClickStartCalibration() 
    { 
        if(startButton) startButton.interactable = false; // Prevent double clicks
        manager.StartAutoCalibration(); 
    }

    // Optional: If you get stuck, use this to restart
    public void OnClickForceReset()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}