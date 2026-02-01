using UnityEngine;

/// <summary>
/// Captures and processes real-time microphone input to calculate volume levels.
/// Uses RMS (Root Mean Square) for stable volume measurement.
/// </summary>
public class MicrophoneInput : MonoBehaviour
{
    [Header("Microphone Settings")]
    [SerializeField] private int sampleRate = 44100;
    [SerializeField] private int sampleWindow = 128;
    
    private AudioClip micClip;
    private string currentDevice;
    private float[] samples;
    
    public bool IsRecording { get; private set; }
    
    void Start()
    {
        samples = new float[sampleWindow];
        InitializeMicrophone();
    }
    
    /// <summary>
    /// Initialize microphone with the default device
    /// </summary>
    private void InitializeMicrophone()
    {
        if (Microphone.devices.Length > 0)
        {
            currentDevice = Microphone.devices[0];
            Debug.Log($"Using microphone: {currentDevice}");
        }
        else
        {
            Debug.LogError("No microphone detected!");
        }
    }
    
    /// <summary>
    /// Start recording from the microphone
    /// </summary>
    public void StartRecording()
    {
        if (string.IsNullOrEmpty(currentDevice))
        {
            Debug.LogError("No microphone device available!");
            return;
        }
        
        micClip = Microphone.Start(currentDevice, true, 1, sampleRate);
        IsRecording = true;
        
        // Wait for microphone to start
        while (!(Microphone.GetPosition(currentDevice) > 0)) { }
        
        Debug.Log("Microphone recording started");
    }
    
    /// <summary>
    /// Stop recording from the microphone
    /// </summary>
    public void StopRecording()
    {
        if (IsRecording)
        {
            Microphone.End(currentDevice);
            IsRecording = false;
            Debug.Log("Microphone recording stopped");
        }
    }
    
    /// <summary>
    /// Get the current volume level from the microphone (RMS calculation)
    /// </summary>
    /// <returns>Volume level between 0 and 1</returns>
    public float GetVolume()
    {
        if (!IsRecording || micClip == null)
            return 0f;
        
        int micPosition = Microphone.GetPosition(currentDevice) - (sampleWindow + 1);
        if (micPosition < 0)
            return 0f;
        
        micClip.GetData(samples, micPosition);
        
        // Calculate RMS (Root Mean Square) for volume
        float sum = 0f;
        for (int i = 0; i < sampleWindow; i++)
        {
            sum += samples[i] * samples[i];
        }
        
        float rms = Mathf.Sqrt(sum / sampleWindow);
        
        // Convert to decibels and normalize
        float db = 20f * Mathf.Log10(rms / 0.1f);
        float normalizedVolume = Mathf.Clamp01((db + 80f) / 80f); // Map -80db to 0db range
        
        return normalizedVolume;
    }
    
    void OnDestroy()
    {
        StopRecording();
    }
    
    void OnApplicationQuit()
    {
        StopRecording();
    }
}