using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Manages the calibration process for microphone input.
/// Captures user's silence baseline and maximum comfortable volume.
/// </summary>
public class CalibrationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MicrophoneInput micInput;
    [SerializeField] private AudioSmoother audioSmoother;
    
    [Header("UI References")]
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private Slider volumeBar;
    [SerializeField] private Button startCalibrationButton;
    [SerializeField] private Button skipButton;
    
    [Header("Calibration Settings")]
    [SerializeField] private float silenceDuration = 3f;
    [SerializeField] private float loudDuration = 3f;
    [SerializeField] private float bufferMultiplier = 1.2f; // Add 20% buffer to max volume
    
    // Calibration results
    public float SilenceBaseline { get; private set; }
    public float MaxVolume { get; private set; }
    public bool IsCalibrated { get; private set; }
    
    private bool isCalibrating;
    
    void Start()
    {
        // Set default values
        SilenceBaseline = 0.05f;
        MaxVolume = 0.8f;
        IsCalibrated = false;
        
        // Setup UI
        if (startCalibrationButton != null)
            startCalibrationButton.onClick.AddListener(StartCalibration);
        
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipCalibration);
        
        UpdateUI("Press 'Start Calibration' to begin, or 'Skip' to use defaults");
    }
    
    /// <summary>
    /// Begin the calibration process
    /// </summary>
    public void StartCalibration()
    {
        if (isCalibrating) return;
        
        StartCoroutine(CalibrationSequence());
    }
    
    /// <summary>
    /// Skip calibration and use default values
    /// </summary>
    public void SkipCalibration()
    {
        IsCalibrated = true;
        UpdateUI("Calibration skipped. Using default values.");
        OnCalibrationComplete();
    }
    
    /// <summary>
    /// Main calibration sequence coroutine
    /// </summary>
    private IEnumerator CalibrationSequence()
    {
        isCalibrating = true;
        
        // Disable buttons during calibration
        if (startCalibrationButton != null)
            startCalibrationButton.interactable = false;
        if (skipButton != null)
            skipButton.interactable = false;
        
        // Start microphone
        micInput.StartRecording();
        yield return new WaitForSeconds(0.5f); // Let mic stabilize
        
        // PHASE 1: Calibrate silence
        UpdateUI("Stay silent... Calibrating background noise");
        yield return new WaitForSeconds(1f);
        
        float silenceSum = 0f;
        int silenceSamples = 0;
        float timer = 0f;
        
        while (timer < silenceDuration)
        {
            float rawVolume = micInput.GetVolume();
            silenceSum += rawVolume;
            silenceSamples++;
            
            if (volumeBar != null)
                volumeBar.value = rawVolume;
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        SilenceBaseline = (silenceSum / silenceSamples) * 1.1f; // Add 10% buffer
        audioSmoother.SetNoiseThreshold(SilenceBaseline);
        
        Debug.Log($"Silence baseline calibrated: {SilenceBaseline:F3}");
        
        // PHASE 2: Calibrate maximum volume
        UpdateUI("Now speak at your LOUDEST comfortable volume!");
        yield return new WaitForSeconds(1f);
        
        float maxDetected = 0f;
        timer = 0f;
        
        while (timer < loudDuration)
        {
            float rawVolume = micInput.GetVolume();
            if (rawVolume > maxDetected)
                maxDetected = rawVolume;
            
            if (volumeBar != null)
                volumeBar.value = rawVolume;
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        MaxVolume = Mathf.Min(maxDetected * bufferMultiplier, 1f);
        
        Debug.Log($"Max volume calibrated: {MaxVolume:F3}");
        
        // Calibration complete
        IsCalibrated = true;
        UpdateUI("Calibration complete! Get ready to play...");
        yield return new WaitForSeconds(2f);
        
        isCalibrating = false;
        OnCalibrationComplete();
    }
    
    /// <summary>
    /// Map raw microphone input to normalized 0-1 range using calibration data
    /// </summary>
    public float GetNormalizedVolume(float rawVolume)
    {
        if (!IsCalibrated)
            return Mathf.Clamp01(rawVolume);
        
        // Remove baseline
        float adjusted = rawVolume - SilenceBaseline;
        if (adjusted < 0f) adjusted = 0f;
        
        // Map to 0-1 range
        float range = MaxVolume - SilenceBaseline;
        float normalized = adjusted / range;
        
        return Mathf.Clamp01(normalized);
    }
    
    /// <summary>
    /// Update UI text
    /// </summary>
    private void UpdateUI(string message)
    {
        if (instructionText != null)
            instructionText.text = message;
    }
    
    /// <summary>
    /// Called when calibration is complete - override or assign event
    /// </summary>
    private void OnCalibrationComplete()
    {
        // This will be used to transition to gameplay
        Debug.Log("Ready to start game!");
        
        // You can add an event here or call GameManager to start game
        // For now, just update UI
        UpdateUI("Calibration Complete!");
    }
    
    /// <summary>
    /// Get real-time normalized and smoothed volume (call this during gameplay)
    /// </summary>
    public float GetGameplayVolume()
    {
        float rawVolume = micInput.GetVolume();
        float smoothed = audioSmoother.SmoothVolume(rawVolume);
        float normalized = GetNormalizedVolume(smoothed);
        
        // Debug logging every 30 frames
        if (Time.frameCount % 30 == 0)
        {
            Debug.Log($"[CALIBRATION] Raw: {rawVolume:F3} → Smoothed: {smoothed:F3} → Normalized: {normalized:F3}");
        }
        
        return normalized;
    }
}