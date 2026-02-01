using UnityEngine;

/// <summary>
/// Makes AudioSystem persist across scene loads.
/// Ensures calibration data carries from Calibration scene to Gameplay scene.
/// </summary>
public class PersistentAudioSystem : MonoBehaviour
{
    private static PersistentAudioSystem instance;
    
    [Header("References (Auto-filled)")]
    public MicrophoneInput MicInput { get; private set; }
    public AudioSmoother AudioSmoother { get; private set; }
    public CalibrationManager CalibrationManager { get; private set; }
    
    void Awake()
    {
        // Singleton pattern - only one AudioSystem should exist
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Cache component references
        MicInput = GetComponent<MicrophoneInput>();
        AudioSmoother = GetComponent<AudioSmoother>();
        CalibrationManager = GetComponent<CalibrationManager>();
        
        if (MicInput == null || AudioSmoother == null || CalibrationManager == null)
        {
            Debug.LogError("PersistentAudioSystem: Missing required components! Make sure MicrophoneInput, AudioSmoother, and CalibrationManager are on this GameObject.");
        }
        
        Debug.Log("AudioSystem persisted across scenes");
    }
    
    /// <summary>
    /// Get the singleton instance from any script
    /// </summary>
    public static PersistentAudioSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PersistentAudioSystem>();
                if (instance == null)
                {
                    Debug.LogError("No PersistentAudioSystem found in scene!");
                }
            }
            return instance;
        }
    }
    
    /// <summary>
    /// Check if system is ready for gameplay
    /// </summary>
    public bool IsReady()
    {
        return CalibrationManager != null && CalibrationManager.IsCalibrated;
    }
    
    /// <summary>
    /// Get normalized volume for gameplay
    /// </summary>
    public float GetGameplayVolume()
    {
        if (CalibrationManager != null)
            return CalibrationManager.GetGameplayVolume();
        
        return 0f;
    }
}