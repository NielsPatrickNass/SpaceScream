using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages the gameplay UI display (score, combo, time, etc.)
/// Attach to a Canvas or UI GameObject.
/// </summary>
public class GameplayUI : MonoBehaviour
{
    [Header("Score Display")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private string scorePrefix = "Score: ";
    [SerializeField] private string highScorePrefix = "Best: ";
    
    [Header("Combo Display")]
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private GameObject comboContainer;
    [SerializeField] private string comboPrefix = "x";
    [SerializeField] private int minComboToShow = 2;
    
    [Header("Game Stats")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text gapsPassedText;
    [SerializeField] private bool showTime = true;
    [SerializeField] private bool showGapsPassed = true;
    
    [Header("Voice Intensity Display")]
    [SerializeField] private Slider voiceIntensitySlider;
    [SerializeField] private TMP_Text voiceIntensityLabel;
    [SerializeField] private bool showVoiceIntensity = true;
    
    [Header("References")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CalibrationManager calibrationManager;
    
    [Header("Animation")]
    [SerializeField] private bool animateScoreChange = true;
    [SerializeField] private float scorePopScale = 1.2f;
    [SerializeField] private float scorePopDuration = 0.1f;
    
    private int lastDisplayedScore = 0;
    private Vector3 scoreTextOriginalScale;
    
    void Start()
    {
        // Auto-find references
        if (scoreManager == null)
            scoreManager = ScoreManager.Instance ?? FindObjectOfType<ScoreManager>();
        
        if (gameManager == null)
            gameManager = GameManager.Instance ?? FindObjectOfType<GameManager>();
        
        if (calibrationManager == null)
        {
            // Try to get from PersistentAudioSystem first
            PersistentAudioSystem audioSystem = PersistentAudioSystem.Instance;
            if (audioSystem != null)
            {
                calibrationManager = audioSystem.CalibrationManager;
            }
            else
            {
                calibrationManager = FindObjectOfType<CalibrationManager>();
            }
        }
        
        // Subscribe to events
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged.AddListener(OnScoreUpdated);
            scoreManager.OnComboChanged.AddListener(OnComboUpdated);
        }
        
        // Store original scale
        if (scoreText != null)
            scoreTextOriginalScale = scoreText.transform.localScale;
        
        // Hide combo initially
        if (comboContainer != null)
            comboContainer.SetActive(false);
        
        // Initialize voice intensity slider
        if (voiceIntensitySlider != null)
        {
            voiceIntensitySlider.minValue = 0f;
            voiceIntensitySlider.maxValue = 1f;
            voiceIntensitySlider.value = 0f;
            voiceIntensitySlider.interactable = false; // Make it read-only
        }
        
        UpdateUI();
    }
    
    void Update()
    {
        UpdateUI();
    }
    
    /// <summary>
    /// Update all UI elements
    /// </summary>
    private void UpdateUI()
    {
        if (scoreManager == null) return;
        
        // Update high score
        if (highScoreText != null)
        {
            highScoreText.text = highScorePrefix + scoreManager.GetHighScore();
        }
        
        // Update time
        if (showTime && timeText != null && gameManager != null)
        {
            float time = gameManager.GetSessionTime();
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timeText.text = $"{minutes:00}:{seconds:00}";
        }
        
        // Update gaps passed
        if (showGapsPassed && gapsPassedText != null)
        {
            gapsPassedText.text = $"Gaps: {scoreManager.GetGapsPassed()}";
        }
        
        // Update voice intensity slider
        if (showVoiceIntensity && voiceIntensitySlider != null)
        {
            float volume = 0f;
            
            // Try calibration manager first
            if (calibrationManager != null && calibrationManager.IsCalibrated)
            {
                volume = calibrationManager.GetGameplayVolume();
            }
            // Fallback: get raw mic input from MicrophoneInput
            else
            {
                MicrophoneInput micInput = FindObjectOfType<MicrophoneInput>();
                if (micInput != null)
                {
                    volume = micInput.GetVolume();
                    // Normalize roughly (raw volume is usually 0-0.5 range)
                    volume = Mathf.Clamp01(volume * 2f);
                }
            }
            
            voiceIntensitySlider.value = volume;
            
            // Update label with percentage if present
            if (voiceIntensityLabel != null)
            {
                voiceIntensityLabel.text = $"Voice: {Mathf.RoundToInt(volume * 100)}%";
            }
        }
    }
    
    /// <summary>
    /// Called when score changes
    /// </summary>
    private void OnScoreUpdated(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = scorePrefix + newScore;
            
            // Animate score change
            if (animateScoreChange && newScore != lastDisplayedScore)
            {
                StartCoroutine(AnimateScorePop());
            }
            
            lastDisplayedScore = newScore;
        }
    }
    
    /// <summary>
    /// Called when combo changes
    /// </summary>
    private void OnComboUpdated(int newCombo)
    {
        if (comboText != null)
        {
            if (newCombo >= minComboToShow)
            {
                comboText.text = comboPrefix + newCombo;
                
                if (comboContainer != null && !comboContainer.activeSelf)
                {
                    comboContainer.SetActive(true);
                }
                
                // Animate combo increase
                StartCoroutine(AnimateComboPop());
            }
            else
            {
                if (comboContainer != null)
                {
                    comboContainer.SetActive(false);
                }
            }
        }
    }
    
    /// <summary>
    /// Animate score text pop
    /// </summary>
    private System.Collections.IEnumerator AnimateScorePop()
    {
        if (scoreText == null) yield break;
        
        // Scale up
        float elapsed = 0f;
        while (elapsed < scorePopDuration / 2f)
        {
            float scale = Mathf.Lerp(1f, scorePopScale, elapsed / (scorePopDuration / 2f));
            scoreText.transform.localScale = scoreTextOriginalScale * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Scale down
        elapsed = 0f;
        while (elapsed < scorePopDuration / 2f)
        {
            float scale = Mathf.Lerp(scorePopScale, 1f, elapsed / (scorePopDuration / 2f));
            scoreText.transform.localScale = scoreTextOriginalScale * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        scoreText.transform.localScale = scoreTextOriginalScale;
    }
    
    /// <summary>
    /// Animate combo text pop
    /// </summary>
    private System.Collections.IEnumerator AnimateComboPop()
    {
        if (comboText == null) yield break;
        
        Vector3 originalScale = comboText.transform.localScale;
        
        // Quick pop
        comboText.transform.localScale = originalScale * 1.3f;
        yield return new WaitForSeconds(0.05f);
        comboText.transform.localScale = originalScale;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged.RemoveListener(OnScoreUpdated);
            scoreManager.OnComboChanged.RemoveListener(OnComboUpdated);
        }
    }
}
