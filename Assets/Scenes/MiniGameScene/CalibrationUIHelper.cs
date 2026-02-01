using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script to visualize microphone input during calibration.
/// Attach to a UI element to show real-time volume feedback.
/// </summary>
public class CalibrationUIHelper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MicrophoneInput micInput;
    [SerializeField] private CalibrationManager calibrationManager;
    
    [Header("Visual Feedback")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Text volumePercentText;
    
    [Header("Color Feedback")]
    [SerializeField] private Gradient volumeGradient;
    [SerializeField] private bool useGradient = true;
    
    private bool useDefaultGradient = false;
    
    void Start()
    {
        // Create default gradient if none assigned
        if (volumeGradient == null || volumeGradient.colorKeys.Length == 0)
        {
            useDefaultGradient = true;
            volumeGradient = new Gradient();
            
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0] = new GradientColorKey(Color.green, 0f);
            colorKeys[1] = new GradientColorKey(Color.yellow, 0.5f);
            colorKeys[2] = new GradientColorKey(Color.red, 1f);
            
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);
            
            volumeGradient.SetKeys(colorKeys, alphaKeys);
        }
    }
    
    void Update()
    {
        if (micInput == null || !micInput.IsRecording)
            return;
        
        // Get current volume
        float volume = micInput.GetVolume();
        
        // Update slider
        if (volumeSlider != null)
        {
            volumeSlider.value = volume;
        }
        
        // Update color based on volume
        if (fillImage != null && useGradient)
        {
            fillImage.color = volumeGradient.Evaluate(volume);
        }
        
        // Update text percentage
        if (volumePercentText != null)
        {
            volumePercentText.text = $"{Mathf.RoundToInt(volume * 100)}%";
        }
    }
    
    /// <summary>
    /// Show calibrated range markers (call after calibration)
    /// </summary>
    public void ShowCalibrationMarkers()
    {
        if (calibrationManager != null && calibrationManager.IsCalibrated)
        {
            Debug.Log($"Baseline: {calibrationManager.SilenceBaseline:F2}, Max: {calibrationManager.MaxVolume:F2}");
            // You can add visual markers on the slider here if needed
        }
    }
}