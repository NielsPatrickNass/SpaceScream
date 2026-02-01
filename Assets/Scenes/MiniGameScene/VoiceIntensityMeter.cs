using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls a UI Image fill amount based on voice/microphone intensity.
/// Attach to any GameObject and assign the fill image in the Inspector.
/// </summary>
public class VoiceIntensityMeter : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TMPro.TMP_Text percentageLabel; // Optional
    
    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 8f; // How fast the fill responds
    [SerializeField] private bool useSmoothing = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool useGradientColor = false;
    [SerializeField] private Color lowColor = Color.green;
    [SerializeField] private Color midColor = Color.yellow;
    [SerializeField] private Color highColor = Color.red;
    
    // References
    private CalibrationManager calibrationManager;
    private MicrophoneInput microphoneInput;
    
    // State
    private float currentFill = 0f;
    private float targetFill = 0f;
    
    void Start()
    {
        // Find CalibrationManager
        PersistentAudioSystem audioSystem = PersistentAudioSystem.Instance;
        if (audioSystem != null)
        {
            calibrationManager = audioSystem.CalibrationManager;
        }
        
        if (calibrationManager == null)
        {
            calibrationManager = FindObjectOfType<CalibrationManager>();
        }
        
        // Find MicrophoneInput as fallback
        microphoneInput = FindObjectOfType<MicrophoneInput>();
        
        // Validate fill image
        if (fillImage == null)
        {
            Debug.LogError("VoiceIntensityMeter: No fill image assigned!");
            return;
        }
        
        // Ensure image is set to Filled type
        if (fillImage.type != Image.Type.Filled)
        {
            Debug.LogWarning("VoiceIntensityMeter: Setting image type to Filled");
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Vertical;
            fillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
        }
        
        // Initialize
        fillImage.fillAmount = 0f;
    }
    
    void Update()
    {
        // Get voice intensity
        targetFill = GetVoiceIntensity();
        
        // Apply smoothing or direct value
        if (useSmoothing)
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * smoothSpeed);
        }
        else
        {
            currentFill = targetFill;
        }
        
        // Update fill image
        if (fillImage != null)
        {
            fillImage.fillAmount = currentFill;
            
            // Apply gradient color if enabled
            if (useGradientColor)
            {
                fillImage.color = GetGradientColor(currentFill);
            }
        }
        
        // Update label if assigned
        if (percentageLabel != null)
        {
            percentageLabel.text = $"{Mathf.RoundToInt(currentFill * 100)}%";
        }
    }
    
    /// <summary>
    /// Get the current voice intensity (0-1)
    /// </summary>
    private float GetVoiceIntensity()
    {
        // Try calibrated input first
        if (calibrationManager != null && calibrationManager.IsCalibrated)
        {
            return calibrationManager.GetGameplayVolume();
        }
        
        // Fallback: raw microphone input
        if (microphoneInput != null && microphoneInput.IsRecording)
        {
            float rawVolume = microphoneInput.GetVolume();
            // Normalize (raw volume is typically 0-0.5 range)
            return Mathf.Clamp01(rawVolume * 2f);
        }
        
        return 0f;
    }
    
    /// <summary>
    /// Get color based on intensity level (green → yellow → red)
    /// </summary>
    private Color GetGradientColor(float intensity)
    {
        if (intensity < 0.5f)
        {
            // Green to Yellow (0.0 - 0.5)
            return Color.Lerp(lowColor, midColor, intensity * 2f);
        }
        else
        {
            // Yellow to Red (0.5 - 1.0)
            return Color.Lerp(midColor, highColor, (intensity - 0.5f) * 2f);
        }
    }
    
    /// <summary>
    /// Manually set the fill (for testing)
    /// </summary>
    public void SetFill(float value)
    {
        targetFill = Mathf.Clamp01(value);
    }
    
    /// <summary>
    /// Get current fill value
    /// </summary>
    public float GetCurrentFill()
    {
        return currentFill;
    }
}
