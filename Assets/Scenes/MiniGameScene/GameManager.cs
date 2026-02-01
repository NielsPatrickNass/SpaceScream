using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Central game state manager.
/// Handles game flow: Calibration → Playing → GameOver → Restart
/// Singleton pattern for global access.
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;
    
    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.Calibration;
    
    [Header("Scene References")]
    [SerializeField] private string calibrationSceneName = "Calibration";
    [SerializeField] private string gameplaySceneName = "Gameplay";
    
    [Header("Game References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private WallSpawner wallSpawner;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private CollisionHandler collisionHandler;
    [SerializeField] private CalibrationManager calibrationManager;
    
    [Header("UI References")]
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject pauseUI;
    
    [Header("Game Settings")]
    [SerializeField] private bool allowPause = true;
    [SerializeField] private bool autoStartAfterCalibration = true;
    [SerializeField] private float startDelay = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    public enum GameState
    {
        Calibration,    // Microphone calibration phase
        Ready,          // Ready to start (after calibration)
        Playing,        // Active gameplay
        Paused,         // Game paused
        GameOver        // Game ended
    }
    
    // State tracking
    private float sessionStartTime;
    private bool isTransitioning = false;
    
    void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        // Don't destroy on load if managing multiple scenes
        // DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        AutoFindReferences();
        InitializeGame();
    }
    
    void Update()
    {
        // Handle pause input
        if (allowPause && currentState == GameState.Playing)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                TogglePause();
            }
        }
        
        // Quick restart in game over
        if (currentState == GameState.GameOver)
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space))
            {
                RestartGame();
            }
        }
    }
    
    /// <summary>
    /// Auto-find references in scene
    /// </summary>
    private void AutoFindReferences()
    {
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
        
        if (wallSpawner == null)
            wallSpawner = FindObjectOfType<WallSpawner>();
        
        if (scoreManager == null)
            scoreManager = ScoreManager.Instance ?? FindObjectOfType<ScoreManager>();
        
        if (collisionHandler == null)
            collisionHandler = FindObjectOfType<CollisionHandler>();
        
        if (calibrationManager == null)
        {
            // Check persistent system first
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
    }
    
    /// <summary>
    /// Initialize game based on current scene
    /// </summary>
    private void InitializeGame()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        if (sceneName == calibrationSceneName)
        {
            SetState(GameState.Calibration);
        }
        else if (sceneName == gameplaySceneName)
        {
            // Check if calibration was completed
            if (calibrationManager != null && calibrationManager.IsCalibrated)
            {
                if (autoStartAfterCalibration)
                {
                    StartCoroutine(StartGameplayDelayed(startDelay));
                }
                else
                {
                    SetState(GameState.Ready);
                }
            }
            else
            {
                Debug.LogWarning("GameManager: Calibration not completed! Player may not work properly.");
                SetState(GameState.Ready);
            }
        }
    }
    
    /// <summary>
    /// Start gameplay after a delay
    /// </summary>
    private IEnumerator StartGameplayDelayed(float delay)
    {
        SetState(GameState.Ready);
        yield return new WaitForSeconds(delay);
        StartGameplay();
    }
    
    /// <summary>
    /// Start active gameplay
    /// </summary>
    public void StartGameplay()
    {
        if (isTransitioning) return;
        
        SetState(GameState.Playing);
        sessionStartTime = Time.time;
        
        // Activate player
        if (playerController != null)
        {
            playerController.ActivatePlayer();
        }
        
        // Start obstacle spawning
        if (wallSpawner != null)
        {
            wallSpawner.StartSpawning();
        }
        
        // Reset score
        if (scoreManager != null)
        {
            scoreManager.ResetScore();
        }
        
        // Reset collision stats
        if (collisionHandler != null)
        {
            collisionHandler.Reset();
        }
        
        // Show gameplay UI
        if (gameplayUI != null)
            gameplayUI.SetActive(true);
        
        if (showDebugInfo)
        {
            Debug.Log("[GAME] Gameplay started!");
        }
    }
    
    /// <summary>
    /// Pause the game
    /// </summary>
    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;
        
        SetState(GameState.Paused);
        Time.timeScale = 0f;
        
        if (pauseUI != null)
            pauseUI.SetActive(true);
        
        if (showDebugInfo)
        {
            Debug.Log("[GAME] Game paused");
        }
    }
    
    /// <summary>
    /// Resume the game
    /// </summary>
    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;
        
        SetState(GameState.Playing);
        Time.timeScale = 1f;
        
        if (pauseUI != null)
            pauseUI.SetActive(false);
        
        if (showDebugInfo)
        {
            Debug.Log("[GAME] Game resumed");
        }
    }
    
    /// <summary>
    /// Toggle pause state
    /// </summary>
    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            PauseGame();
        }
        else if (currentState == GameState.Paused)
        {
            ResumeGame();
        }
    }
    
    /// <summary>
    /// End the game (called manually or by conditions)
    /// </summary>
    public void EndGame()
    {
        if (currentState == GameState.GameOver) return;
        
        SetState(GameState.GameOver);
        
        // Deactivate player
        if (playerController != null)
        {
            playerController.DeactivatePlayer();
        }
        
        // Stop obstacle spawning
        if (wallSpawner != null)
        {
            wallSpawner.StopSpawning();
        }
        
        // Show game over UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        
        if (gameplayUI != null)
        {
            gameplayUI.SetActive(false);
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[GAME] Game Over!");
        }
    }
    
    /// <summary>
    /// Restart the current game
    /// </summary>
    public void RestartGame()
    {
        if (isTransitioning) return;
        
        StartCoroutine(RestartGameCoroutine());
    }
    
    private IEnumerator RestartGameCoroutine()
    {
        isTransitioning = true;
        
        // Reset time scale if paused
        Time.timeScale = 1f;
        
        // Hide UI
        if (gameOverUI != null)
            gameOverUI.SetActive(false);
        
        if (pauseUI != null)
            pauseUI.SetActive(false);
        
        // Reset obstacle spawner
        if (wallSpawner != null)
        {
            wallSpawner.Reset();
        }
        
        // Reset player position
        if (playerController != null)
        {
            // Reload scene to reset everything
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            yield break;
        }
        
        isTransitioning = false;
    }
    
    /// <summary>
    /// Load calibration scene
    /// </summary>
    public void LoadCalibrationScene()
    {
        if (isTransitioning) return;
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(calibrationSceneName);
    }
    
    /// <summary>
    /// Load gameplay scene
    /// </summary>
    public void LoadGameplayScene()
    {
        if (isTransitioning) return;
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }
    
    /// <summary>
    /// Set game state and trigger events
    /// </summary>
    private void SetState(GameState newState)
    {
        if (currentState == newState) return;
        
        GameState previousState = currentState;
        currentState = newState;
        
        if (showDebugInfo)
        {
            Debug.Log($"[GAME] State changed: {previousState} → {newState}");
        }
    }
    
    /// <summary>
    /// Get current game state
    /// </summary>
    public GameState GetState() => currentState;
    
    /// <summary>
    /// Check if game is active
    /// </summary>
    public bool IsPlaying() => currentState == GameState.Playing;
    
    /// <summary>
    /// Get session duration
    /// </summary>
    public float GetSessionTime()
    {
        if (currentState == GameState.Playing)
        {
            return Time.time - sessionStartTime;
        }
        return 0f;
    }
    
    // Event callbacks from other systems
    public void OnPlayerHitWall()
    {
        // Player hit wall but doesn't die - just physics collision
        if (showDebugInfo)
        {
            Debug.Log("[GAME] Player hit wall (collision only, no game over)");
        }
    }
    
    public void OnPlayerPassedGap()
    {
        // Player successfully passed through gap
        if (showDebugInfo)
        {
            Debug.Log("[GAME] Player passed gap!");
        }
    }
    
    public void OnCalibrationComplete()
    {
        SetState(GameState.Ready);
        
        if (autoStartAfterCalibration)
        {
            StartCoroutine(StartGameplayDelayed(startDelay));
        }
    }
}
