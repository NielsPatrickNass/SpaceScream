using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the game over screen UI.
/// Shows final stats and provides restart options.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Stats Display")]
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text gapsPassedText;
    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private TMP_Text timePlayedText;
    
    [Header("Messages")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private string[] gameOverMessages = 
    {
        "Game Over!",
        "Nice Try!",
        "Keep Practicing!",
        "Almost There!"
    };
    [SerializeField] private string newHighScoreMessage = "NEW HIGH SCORE!";
    
    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button recalibrateButton;
    
    [Header("References")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameManager gameManager;
    
    [Header("Animation")]
    [SerializeField] private bool animateOnShow = true;
    [SerializeField] private float animationDuration = 0.5f;
    
    private CanvasGroup canvasGroup;
    
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Setup button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        
        if (recalibrateButton != null)
            recalibrateButton.onClick.AddListener(OnRecalibrateClicked);
    }
    
    void Start()
    {
        // Auto-find references
        if (scoreManager == null)
            scoreManager = ScoreManager.Instance ?? FindObjectOfType<ScoreManager>();
        
        if (gameManager == null)
            gameManager = GameManager.Instance ?? FindObjectOfType<GameManager>();
        
        // Hide initially
        gameObject.SetActive(false);
    }
    
    void OnEnable()
    {
        UpdateStats();
        
        if (animateOnShow)
        {
            StartCoroutine(AnimateShow());
        }
    }
    
    /// <summary>
    /// Update all statistics displays
    /// </summary>
    private void UpdateStats()
    {
        if (scoreManager == null) return;
        
        int finalScore = scoreManager.GetScore();
        int highScore = scoreManager.GetHighScore();
        bool isNewHighScore = finalScore >= highScore && finalScore > 0;
        
        // Title message
        if (titleText != null)
        {
            if (isNewHighScore)
            {
                titleText.text = newHighScoreMessage;
                titleText.color = Color.yellow;
            }
            else
            {
                titleText.text = gameOverMessages[Random.Range(0, gameOverMessages.Length)];
                titleText.color = Color.white;
            }
        }
        
        // Final score
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Score: {finalScore}";
        }
        
        // High score
        if (highScoreText != null)
        {
            highScoreText.text = $"Best: {highScore}";
        }
        
        // Gaps passed
        if (gapsPassedText != null)
        {
            gapsPassedText.text = $"Gaps Cleared: {scoreManager.GetGapsPassed()}";
        }
        
        // Accuracy
        if (accuracyText != null)
        {
            float accuracy = scoreManager.GetAccuracy();
            accuracyText.text = $"Accuracy: {accuracy:F1}%";
        }
        
        // Time played
        if (timePlayedText != null && gameManager != null)
        {
            float time = gameManager.GetSessionTime();
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timePlayedText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }
    
    /// <summary>
    /// Animate the game over screen appearing
    /// </summary>
    private System.Collections.IEnumerator AnimateShow()
    {
        if (canvasGroup == null) yield break;
        
        canvasGroup.alpha = 0f;
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / animationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    /// <summary>
    /// Restart button clicked
    /// </summary>
    private void OnRestartClicked()
    {
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
    }
    
    /// <summary>
    /// Main menu button clicked
    /// </summary>
    private void OnMainMenuClicked()
    {
        if (gameManager != null)
        {
            gameManager.LoadCalibrationScene();
        }
        else
        {
            Debug.LogWarning("GameOverUI: GameManager not found, cannot load main menu");
        }
    }
    
    /// <summary>
    /// Recalibrate button clicked
    /// </summary>
    private void OnRecalibrateClicked()
    {
        if (gameManager != null)
        {
            gameManager.LoadCalibrationScene();
        }
        else
        {
            Debug.LogWarning("GameOverUI: GameManager not found, cannot load calibration");
        }
    }
}
