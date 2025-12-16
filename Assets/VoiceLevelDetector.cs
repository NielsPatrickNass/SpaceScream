using UnityEngine;
using Whisper.Utils; 

public class VoiceLevelDetector : MonoBehaviour
{
    public MicrophoneRecord microphoneRecord;
    
    [Header("Live Readings")]
    public float currentDb;
    public float baselineNoiseDb = -60f; 

    [Header("Threshold Settings (dB above Noise Floor)")]
    [Tooltip("How much louder than silence to count as a whisper?")]
    public float whisperMinOffset = 5f; 
    
    [Tooltip("If you go louder than this, Whisper fails.")]
    public float whisperMaxOffset = 25f; // Increased buffer (was 20)

    [Tooltip("Minimum volume to count as Normal talking")]
    public float normalMinOffset = 15f; 

    [Tooltip("If you go louder than this, Normal fails.")]
    public float normalMaxOffset = 45f; // Increased buffer (was 35)

    [Tooltip("Minimum volume to count as a Shout")]
    public float shoutMinOffset = 40f; 

    // Helper to see the actual target numbers in Inspector for debugging
    [Header("Calculated Targets (Read Only)")]
    [SerializeField] private float targetWhisperMax;
    [SerializeField] private float targetNormalMin;
    [SerializeField] private float targetShoutMin;

    private float[] _sampleBuffer = new float[1024]; 

    void Update()
    {
        // Update debug view
        float effectiveBase = GetEffectiveBaseline();
        targetWhisperMax = effectiveBase + whisperMaxOffset;
        targetNormalMin = effectiveBase + normalMinOffset;
        targetShoutMin = effectiveBase + shoutMinOffset;

        if (microphoneRecord.IsRecording)
        {
            currentDb = CalculateRMS();
        }
    }

    private float CalculateRMS()
    {
        AudioClip clip = microphoneRecord._clip; 
        if (clip == null) return -80f;

        int micPosition = Microphone.GetPosition(microphoneRecord.RecordStartMicDevice);
        if (micPosition < _sampleBuffer.Length) return -80f; 

        clip.GetData(_sampleBuffer, micPosition - _sampleBuffer.Length);

        float sum = 0;
        for (int i = 0; i < _sampleBuffer.Length; i++)
        {
            sum += _sampleBuffer[i] * _sampleBuffer[i];
        }
        float rms = Mathf.Sqrt(sum / _sampleBuffer.Length);
        
        float db = 20 * Mathf.Log10(rms / 0.1f); 
        return Mathf.Clamp(db, -80f, 10f);
    }

    public void SetBaseline()
    {
        baselineNoiseDb = currentDb;
        Debug.Log($"BASELINE SET: {baselineNoiseDb:F1} dB.");
    }

    public float GetEffectiveBaseline()
    {
        // Ensure baseline isn't impossibly quiet (-80dB)
        // We clamp the floor to -50dB so the user has some headroom.
        return Mathf.Max(baselineNoiseDb, -50f);
    }

    public bool IsAboveNoiseFloor()
    {
        return currentDb > (GetEffectiveBaseline() + 3f); // Slightly more sensitive
    }

    // Check if user is being TOO QUIET
    public bool HasViolatedLowerLimit(string mode)
    {
        float baseline = GetEffectiveBaseline();

        switch (mode)
        {
            case "Whisper": 
                return currentDb < (baseline + whisperMinOffset); 
            
            case "Normal": 
                return currentDb < (baseline + normalMinOffset);

            case "Shout": 
                return currentDb < (baseline + shoutMinOffset); 
                
            default: return true;
        }
    }

    // Check if user is being TOO LOUD
    public bool HasViolatedUpperLimit(string mode)
    {
        float baseline = GetEffectiveBaseline();

        switch (mode)
        {
            case "Whisper": 
                return currentDb > (baseline + whisperMaxOffset); 
            
            case "Normal": 
                return currentDb > (baseline + normalMaxOffset);
            
            case "Shout": 
                return false; // No upper limit for shouting
            
            default: return false;
        }
    }
}