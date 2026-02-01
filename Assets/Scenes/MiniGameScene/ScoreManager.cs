using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages player score and high score tracking.
/// Singleton pattern for global access.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    private static ScoreManager instance;
    public static ScoreManager Instance => instance;
    
    [Header("Score Settings")]
    [SerializeField] private int pointsPerGap = 1;
    [SerializeField] private int bonusPointsThreshold = 10; // Bonus every X gaps
    [SerializeField] private int bonusPoints = 5;
    
    [Header("Combo System (Optional)")]
    [SerializeField] private bool enableComboSystem = false;
    [SerializeField] private float comboTimeWindow = 2f; // Time to maintain combo
    [SerializeField] private int comboMultiplierMax = 5;
    
    [Header("Events")]
    public UnityEvent<int> OnScoreChanged;
    public UnityEvent<int> OnHighScoreBeaten;
    public UnityEvent<int> OnComboChanged;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    // Score data
    private int currentScore = 0;
    private int highScore = 0;
    private int gapsPassedInSession = 0;
    
    // Combo system
    private int currentCombo = 0;
    private float lastScoreTime = 0f;
    
    // High score persistence
    private const string HIGH_SCORE_KEY = "HighScore";
    
    void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        LoadHighScore();
    }
    
    void Update()
    {
        // Check combo timeout
        if (enableComboSystem && currentCombo > 0)
        {
            if (Time.time - lastScoreTime > comboTimeWindow)
            {
                ResetCombo();
            }
        }
    }
    
    /// <summary>
    /// Add score when player passes through a gap
    /// </summary>
    public void AddScore(int gaps = 1)
    {
        gapsPassedInSession += gaps;
        
        // Calculate points with combo multiplier
        int points = pointsPerGap * gaps;
        
        if (enableComboSystem)
        {
            currentCombo++;
            int multiplier = Mathf.Min(currentCombo, comboMultiplierMax);
            points *= multiplier;
            lastScoreTime = Time.time;
            
            OnComboChanged?.Invoke(currentCombo);
        }
        
        // Add bonus points at threshold
        if (gapsPassedInSession % bonusPointsThreshold == 0)
        {
            points += bonusPoints;
            if (showDebugInfo)
            {
                Debug.Log($"[SCORE] BONUS! +{bonusPoints} points!");
            }
        }
        
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
        
        // Check for high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            OnHighScoreBeaten?.Invoke(highScore);
        }
        
        if (showDebugInfo)
        {
            string comboText = enableComboSystem ? $" (x{Mathf.Min(currentCombo, comboMultiplierMax)} combo)" : "";
            Debug.Log($"[SCORE] +{points} points{comboText} | Total: {currentScore}");
        }
    }
    
    /// <summary>
    /// Reset combo multiplier
    /// </summary>
    private void ResetCombo()
    {
        if (currentCombo > 0)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[SCORE] Combo lost! Was at x{currentCombo}");
            }
            
            currentCombo = 0;
            OnComboChanged?.Invoke(currentCombo);
        }
    }
    
    /// <summary>
    /// Reset current session score (for new game)
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        gapsPassedInSession = 0;
        currentCombo = 0;
        lastScoreTime = 0f;
        
        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(currentCombo);
        
        if (showDebugInfo)
        {
            Debug.Log("[SCORE] Score reset for new game");
        }
    }
    
    /// <summary>
    /// Save high score to PlayerPrefs
    /// </summary>
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Load high score from PlayerPrefs
    /// </summary>
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        if (showDebugInfo)
        {
            Debug.Log($"[SCORE] High score loaded: {highScore}");
        }
    }
    
    /// <summary>
    /// Get current score
    /// </summary>
    public int GetScore() => currentScore;
    
    /// <summary>
    /// Get high score
    /// </summary>
    public int GetHighScore() => highScore;
    
    /// <summary>
    /// Get current combo multiplier
    /// </summary>
    public int GetCombo() => currentCombo;
    
    /// <summary>
    /// Get gaps passed in this session
    /// </summary>
    public int GetGapsPassed() => gapsPassedInSession;
    
    /// <summary>
    /// Calculate accuracy percentage (for stats screen)
    /// </summary>
    public float GetAccuracy()
    {
        CollisionHandler collisionHandler = FindObjectOfType<CollisionHandler>();
        if (collisionHandler != null)
        {
            int totalAttempts = gapsPassedInSession + collisionHandler.GetWallsHit();
            if (totalAttempts > 0)
            {
                return (float)gapsPassedInSession / totalAttempts * 100f;
            }
        }
        return 100f;
    }
}
