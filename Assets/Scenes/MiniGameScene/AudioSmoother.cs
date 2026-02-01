using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Smooths raw audio input to prevent jittery movement.
/// Uses a combination of running average and interpolation.
/// </summary>
public class AudioSmoother : MonoBehaviour
{
    [Header("Smoothing Settings")]
    [SerializeField] [Range(2, 20)] private int bufferSize = 8;
    [SerializeField] [Range(0.1f, 0.9f)] private float lerpSpeed = 0.3f;
    [SerializeField] [Range(0f, 0.1f)] private float noiseThreshold = 0.05f;
    
    private Queue<float> volumeBuffer;
    private float smoothedValue;
    private float targetValue;
    
    void Awake()
    {
        volumeBuffer = new Queue<float>(bufferSize);
        smoothedValue = 0f;
        targetValue = 0f;
    }
    
    /// <summary>
    /// Apply smoothing to raw volume value
    /// </summary>
    /// <param name="rawVolume">Raw volume from microphone (0-1)</param>
    /// <returns>Smoothed volume value</returns>
    public float SmoothVolume(float rawVolume)
    {
        // Apply noise gate
        if (rawVolume < noiseThreshold)
            rawVolume = 0f;
        
        // Add to running average buffer
        volumeBuffer.Enqueue(rawVolume);
        if (volumeBuffer.Count > bufferSize)
            volumeBuffer.Dequeue();
        
        // Calculate running average
        float sum = 0f;
        foreach (float val in volumeBuffer)
        {
            sum += val;
        }
        targetValue = sum / volumeBuffer.Count;
        
        // Apply lerp interpolation for smooth transitions
        smoothedValue = Mathf.Lerp(smoothedValue, targetValue, lerpSpeed);
        
        return smoothedValue;
    }
    
    /// <summary>
    /// Get the current smoothed value without updating
    /// </summary>
    public float GetSmoothedValue()
    {
        return smoothedValue;
    }
    
    /// <summary>
    /// Reset the smoother (useful when restarting or recalibrating)
    /// </summary>
    public void Reset()
    {
        volumeBuffer.Clear();
        smoothedValue = 0f;
        targetValue = 0f;
    }
    
    /// <summary>
    /// Adjust noise threshold dynamically
    /// </summary>
    public void SetNoiseThreshold(float threshold)
    {
        noiseThreshold = Mathf.Clamp01(threshold);
    }
}