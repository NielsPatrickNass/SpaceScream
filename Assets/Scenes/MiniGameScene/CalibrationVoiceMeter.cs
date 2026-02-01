using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Voice meter for the Calibration scene.
/// Simply controls fill amount based on raw microphone input.
/// The gradient is in the image itself - we just crop/reveal it.
/// Uses the same raw volume as CalibrationManager's volumeBar slider.
/// </summary>
public class CalibrationVoiceMeter : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;
    
    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private bool useSmoothing = true;
    
    // References
    private MicrophoneInput microphoneInput;
    
    // State
    private float currentFill = 0f;
    private float targetFill = 0f;
    private float startupDelay = 0.5f;
    private float timeSinceStart = 0f;
    private bool isReady = false;
    
    void Start()
    {
        // Find MicrophoneInput
        microphoneInput = FindObjectOfType<MicrophoneInput>();
        
        if (microphoneInput == null)
        {
            PersistentAudioSystem audioSystem = PersistentAudioSystem.Instance;
            if (audioSystem != null)
            {
                microphoneInput = audioSystem.MicInput;
            }
        }
        
        if (microphoneInput == null)
        {
            Debug.LogError("CalibrationVoiceMeter: No MicrophoneInput found!");
        }
        
        if (fillImage == null)
        {
            Debug.LogError("CalibrationVoiceMeter: No fill image assigned!");
            return;
        }
        
        // Initialize fill to 0
        fillImage.fillAmount = 0f;
        
        // Start microphone recording if not already
        if (microphoneInput != null && !microphoneInput.IsRecording)
        {
            microphoneInput.StartRecording();
        }
    }
    
    void Update()
    {
        if (fillImage == null) return;
        
        // Wait for mic to stabilize
        if (!isReady)
        {
            timeSinceStart += Time.deltaTime;
            if (timeSinceStart < startupDelay)
            {
                fillImage.fillAmount = 0f;
                return;
            }
            isReady = true;
        }
        
        // Make sure mic is recording
        if (microphoneInput != null && !microphoneInput.IsRecording)
        {
            microphoneInput.StartRecording();
        }
        
        // Get raw voice intensity
        targetFill = GetRawVoiceIntensity();
        
        // Apply smoothing
        if (useSmoothing)
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * smoothSpeed);
        }
        else
        {
            currentFill = targetFill;
        }
        
        // Just update fill amount - that's it!
        fillImage.fillAmount = currentFill;
    }
    
    private float GetRawVoiceIntensity()
    {
        if (microphoneInput == null || !microphoneInput.IsRecording)
        {
            return 0f;
        }
        
        // Use the exact same raw volume as CalibrationManager's volumeBar
        // CalibrationManager sets: volumeBar.value = rawVolume (direct mic input)
        float rawVolume = microphoneInput.GetVolume();
        return Mathf.Clamp01(rawVolume);
    }
}
