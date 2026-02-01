using UnityEngine;

/// <summary>
/// Debug microphone input to see if it's working.
/// Attach to AudioSystem GameObject.
/// </summary>
public class MicrophoneDebugger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MicrophoneInput micInput;
    [SerializeField] private AudioSmoother audioSmoother;
    [SerializeField] private CalibrationManager calibrationManager;
    
    [Header("Debug Settings")]
    [SerializeField] private bool showOnScreenDebug = true;
    [SerializeField] private bool logToConsole = false;
    [SerializeField] private float logInterval = 0.5f;
    
    private float logTimer = 0f;
    
    void Start()
    {
        // Auto-find if not assigned
        if (micInput == null)
            micInput = GetComponent<MicrophoneInput>();
        if (audioSmoother == null)
            audioSmoother = GetComponent<AudioSmoother>();
        if (calibrationManager == null)
            calibrationManager = GetComponent<CalibrationManager>();
        
        Debug.Log("=== MICROPHONE DEBUGGER STARTED ===");
        
        // Check if components exist
        Debug.Log($"MicrophoneInput: {(micInput != null ? "Found" : "NULL")}");
        Debug.Log($"AudioSmoother: {(audioSmoother != null ? "Found" : "NULL")}");
        Debug.Log($"CalibrationManager: {(calibrationManager != null ? "Found" : "NULL")}");
        
        // Check microphone devices
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("NO MICROPHONE DEVICES FOUND!");
        }
        else
        {
            Debug.Log($"Available microphones: {Microphone.devices.Length}");
            for (int i = 0; i < Microphone.devices.Length; i++)
            {
                Debug.Log($"  [{i}] {Microphone.devices[i]}");
            }
        }
        
        // Check if recording
        if (micInput != null)
        {
            Debug.Log($"Is Recording: {micInput.IsRecording}");
        }
    }
    
    void Update()
    {
        if (logToConsole)
        {
            logTimer += Time.deltaTime;
            if (logTimer >= logInterval)
            {
                logTimer = 0f;
                LogMicrophoneStatus();
            }
        }
    }
    
    void LogMicrophoneStatus()
    {
        if (micInput == null) return;
        
        float rawVolume = micInput.GetVolume();
        float smoothedVolume = audioSmoother != null ? audioSmoother.GetSmoothedValue() : 0f;
        float gameplayVolume = calibrationManager != null ? calibrationManager.GetGameplayVolume() : 0f;
        
        Debug.Log($"Mic Status - Raw: {rawVolume:F3} | Smoothed: {smoothedVolume:F3} | Gameplay: {gameplayVolume:F3}");
    }
    
    void OnGUI()
    {
        if (!showOnScreenDebug)
            return;
        
        GUILayout.BeginArea(new Rect(10, Screen.height - 250, 400, 240));
        GUILayout.Box("=== MICROPHONE DEBUG ===");
        
        // Microphone status
        if (micInput != null)
        {
            GUILayout.Label($"Is Recording: {micInput.IsRecording}");
            float rawVolume = micInput.GetVolume();
            GUILayout.Label($"Raw Volume: {rawVolume:F3}");
            
            // Visual bar for raw volume
            GUI.color = Color.red;
            GUILayout.HorizontalScrollbar(0, rawVolume, 0f, 1f, GUILayout.Height(20));
            GUI.color = Color.white;
        }
        else
        {
            GUILayout.Label("MicrophoneInput: NULL");
        }
        
        GUILayout.Space(5);
        
        // Smoothed volume
        if (audioSmoother != null)
        {
            float smoothedVolume = audioSmoother.GetSmoothedValue();
            GUILayout.Label($"Smoothed Volume: {smoothedVolume:F3}");
            
            GUI.color = Color.yellow;
            GUILayout.HorizontalScrollbar(0, smoothedVolume, 0f, 1f, GUILayout.Height(20));
            GUI.color = Color.white;
        }
        
        GUILayout.Space(5);
        
        // Calibrated gameplay volume
        if (calibrationManager != null)
        {
            GUILayout.Label($"Calibrated: {calibrationManager.IsCalibrated}");
            GUILayout.Label($"Baseline: {calibrationManager.SilenceBaseline:F3}");
            GUILayout.Label($"Max: {calibrationManager.MaxVolume:F3}");
            
            float gameplayVolume = calibrationManager.GetGameplayVolume();
            GUILayout.Label($"Gameplay Volume: {gameplayVolume:F3}");
            
            GUI.color = Color.green;
            GUILayout.HorizontalScrollbar(0, gameplayVolume, 0f, 1f, GUILayout.Height(20));
            GUI.color = Color.white;
        }
        
        GUILayout.Space(5);
        GUILayout.Label("Speak to test microphone!");
        
        GUILayout.EndArea();
    }
}