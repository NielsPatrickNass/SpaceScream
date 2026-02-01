using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates random gap positions with intelligent placement.
/// Ensures gaps are challenging but fair (not too high/low consecutively).
/// </summary>
public class GapGenerator : MonoBehaviour
{
    [Header("Gap Position Settings")]
    [SerializeField] private float minY = -3f;
    [SerializeField] private float maxY = 3f;
    [SerializeField] private float gapMargin = 0.5f; // Keep gaps away from extreme edges
    
    [Header("Gap Size Settings")]
    [SerializeField] private float baseGapSize = 2.5f;
    [SerializeField] private float minGapSize = 1.5f;
    [SerializeField] private float maxGapSize = 3.5f;
    
    [Header("Difficulty Scaling")]
    [SerializeField] private bool enableDifficultyScaling = true;
    [SerializeField] private float gapSizeDecreaseRate = 0.05f; // Decrease per obstacle
    [SerializeField] private int obstaclesUntilMinGap = 50; // How many obstacles until minimum gap
    
    [Header("Pattern Generation")]
    [SerializeField] private GenerationMode mode = GenerationMode.Smart;
    [SerializeField] [Range(0f, 1f)] private float extremePositionChance = 0.2f; // Chance of very high/low gap
    [SerializeField] private int minGapsBetweenExtremes = 3; // Prevent consecutive extreme positions
    
    public enum GenerationMode
    {
        Random,         // Completely random Y position
        Smart,          // Avoid extreme positions too often
        Alternating,    // Alternate between high/medium/low
        Progressive     // Gradually move up and down
    }
    
    // State tracking
    private int obstaclesSpawned = 0;
    private float lastGapY = 0f;
    private int gapsSinceExtreme = 999;
    private Queue<float> recentGapPositions = new Queue<float>();
    private int maxRecentGaps = 5;
    
    // For alternating mode
    private int alternatingIndex = 0;
    
    // For progressive mode
    private float progressiveTarget = 0f;
    private int stepsAtTarget = 0;
    
    /// <summary>
    /// Generate the next gap position and size
    /// </summary>
    public (float centerY, float size) GenerateGap()
    {
        obstaclesSpawned++;
        
        // Calculate gap size (with difficulty scaling)
        float gapSize = CalculateGapSize();
        
        // Generate Y position based on mode
        float gapCenterY = mode switch
        {
            GenerationMode.Random => GenerateRandomPosition(gapSize),
            GenerationMode.Smart => GenerateSmartPosition(gapSize),
            GenerationMode.Alternating => GenerateAlternatingPosition(gapSize),
            GenerationMode.Progressive => GenerateProgressivePosition(gapSize),
            _ => GenerateRandomPosition(gapSize)
        };
        
        // Store for pattern tracking
        lastGapY = gapCenterY;
        recentGapPositions.Enqueue(gapCenterY);
        if (recentGapPositions.Count > maxRecentGaps)
            recentGapPositions.Dequeue();
        
        return (gapCenterY, gapSize);
    }
    
    /// <summary>
    /// Calculate gap size with optional difficulty scaling
    /// </summary>
    private float CalculateGapSize()
    {
        if (!enableDifficultyScaling)
            return baseGapSize;
        
        // Gradually decrease gap size over time
        float scaleFactor = Mathf.Clamp01((float)obstaclesSpawned / obstaclesUntilMinGap);
        float currentGapSize = Mathf.Lerp(baseGapSize, minGapSize, scaleFactor);
        
        return Mathf.Clamp(currentGapSize, minGapSize, maxGapSize);
    }
    
    /// <summary>
    /// Completely random position within safe boundaries
    /// </summary>
    private float GenerateRandomPosition(float gapSize)
    {
        float safeMin = minY + gapMargin + gapSize / 2f;
        float safeMax = maxY - gapMargin - gapSize / 2f;
        return Random.Range(safeMin, safeMax);
    }
    
    /// <summary>
    /// Smart generation - avoids extremes, ensures variety
    /// </summary>
    private float GenerateSmartPosition(float gapSize)
    {
        float safeMin = minY + gapMargin + gapSize / 2f;
        float safeMax = maxY - gapMargin - gapSize / 2f;
        float safeMiddle = (safeMin + safeMax) / 2f;
        float range = safeMax - safeMin;
        
        gapsSinceExtreme++;
        
        // Decide if this should be an extreme position
        bool shouldBeExtreme = gapsSinceExtreme >= minGapsBetweenExtremes && 
                                Random.value < extremePositionChance;
        
        float position;
        
        if (shouldBeExtreme)
        {
            // Generate extreme position (very high or very low)
            float extremeRange = range * 0.25f; // Top/bottom 25%
            if (Random.value > 0.5f)
            {
                // High position
                position = Random.Range(safeMax - extremeRange, safeMax);
            }
            else
            {
                // Low position
                position = Random.Range(safeMin, safeMin + extremeRange);
            }
            gapsSinceExtreme = 0;
        }
        else
        {
            // Generate moderate position (middle 60%)
            float moderateRange = range * 0.6f;
            position = Random.Range(safeMiddle - moderateRange / 2f, safeMiddle + moderateRange / 2f);
        }
        
        // Ensure some variation from last gap
        if (Mathf.Abs(position - lastGapY) < gapSize * 0.5f)
        {
            // Too similar to last gap, shift it
            position += (Random.value > 0.5f ? 1f : -1f) * gapSize * 0.75f;
            position = Mathf.Clamp(position, safeMin, safeMax);
        }
        
        return position;
    }
    
    /// <summary>
    /// Alternating pattern - cycles through low/medium/high
    /// </summary>
    private float GenerateAlternatingPosition(float gapSize)
    {
        float safeMin = minY + gapMargin + gapSize / 2f;
        float safeMax = maxY - gapMargin - gapSize / 2f;
        float safeMiddle = (safeMin + safeMax) / 2f;
        
        float position;
        
        switch (alternatingIndex % 3)
        {
            case 0: // Low
                position = Mathf.Lerp(safeMin, safeMiddle, 0.3f);
                break;
            case 1: // Middle
                position = safeMiddle;
                break;
            case 2: // High
                position = Mathf.Lerp(safeMiddle, safeMax, 0.7f);
                break;
            default:
                position = safeMiddle;
                break;
        }
        
        // Add slight randomness
        position += Random.Range(-0.3f, 0.3f);
        position = Mathf.Clamp(position, safeMin, safeMax);
        
        alternatingIndex++;
        return position;
    }
    
    /// <summary>
    /// Progressive pattern - smoothly moves up and down
    /// </summary>
    private float GenerateProgressivePosition(float gapSize)
    {
        float safeMin = minY + gapMargin + gapSize / 2f;
        float safeMax = maxY - gapMargin - gapSize / 2f;
        
        stepsAtTarget++;
        
        // Change target every 3-5 gaps
        if (stepsAtTarget >= Random.Range(3, 6))
        {
            progressiveTarget = Random.Range(safeMin, safeMax);
            stepsAtTarget = 0;
        }
        
        // Smoothly move toward target
        float position = Mathf.Lerp(lastGapY, progressiveTarget, 0.4f);
        position = Mathf.Clamp(position, safeMin, safeMax);
        
        return position;
    }
    
    /// <summary>
    /// Get current difficulty multiplier (0-1, where 1 is maximum difficulty)
    /// </summary>
    public float GetDifficultyMultiplier()
    {
        if (!enableDifficultyScaling)
            return 0f;
        
        return Mathf.Clamp01((float)obstaclesSpawned / obstaclesUntilMinGap);
    }
    
    /// <summary>
    /// Reset generator state (useful for game restart)
    /// </summary>
    public void Reset()
    {
        obstaclesSpawned = 0;
        lastGapY = 0f;
        gapsSinceExtreme = 999;
        recentGapPositions.Clear();
        alternatingIndex = 0;
        progressiveTarget = 0f;
        stepsAtTarget = 0;
    }
    
    /// <summary>
    /// Get statistics for debugging
    /// </summary>
    public string GetStats()
    {
        return $"Obstacles: {obstaclesSpawned} | Difficulty: {GetDifficultyMultiplier():P0} | Gap Size: {CalculateGapSize():F2}";
    }
}
