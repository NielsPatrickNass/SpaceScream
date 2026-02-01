using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Initializes gameplay when the Gameplay scene loads.
/// Works with PersistentAudioSystem - auto-finds references.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("References (Auto-Found if Empty)")]
    [SerializeField] private CalibrationManager calibrationManager;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CameraController cameraController;
    
    [Header("Scene Settings")]
    [SerializeField] private bool autoStartAfterCalibration = true;
    [SerializeField] private float delayBeforeStart = 1f;
    
    [Header("Gameplay Settings")]
    [SerializeField] private float gameplayMinY = -4f;
    [SerializeField] private float gameplayMaxY = 4f;
    
    private bool gameStarted = false;
    
    void Start()
    {
        // Auto-find references if not assigned
        FindReferences();
        
        // Check if already calibrated (coming from Calibration scene)
        if (calibrationManager != null && calibrationManager.IsCalibrated)
        {
            Debug.Log("Already calibrated! Starting gameplay immediately...");
            Invoke("StartGameplay", delayBeforeStart);
        }
        else
        {
            // Wait for calibration (if in same scene as calibration UI)
            StartCoroutine(WaitForCalibration());
        }
    }
    
    /// <summary>
    /// Auto-find required references from scene
    /// </summary>
    private void FindReferences()
    {
        // Find CalibrationManager from PersistentAudioSystem
        if (calibrationManager == null)
        {
            PersistentAudioSystem audioSystem = PersistentAudioSystem.Instance;
            if (audioSystem != null)
            {
                calibrationManager = audioSystem.CalibrationManager;
                Debug.Log("GameInitializer: Found CalibrationManager from PersistentAudioSystem");
            }
            else
            {
                // Fallback: try to find in scene (for single-scene setup)
                calibrationManager = FindObjectOfType<CalibrationManager>();
                if (calibrationManager != null)
                    Debug.Log("GameInitializer: Found CalibrationManager in scene");
            }
        }
        
        // Find PlayerController if not assigned
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
                Debug.Log("GameInitializer: Found PlayerController in scene");
            else
                Debug.LogWarning("GameInitializer: No PlayerController found!");
        }
        
        // Find CameraController if not assigned
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
            if (cameraController != null)
                Debug.Log("GameInitializer: Found CameraController in scene");
        }
    }
    
    /// <summary>
    /// Wait for calibration to complete, then start game
    /// </summary>
    private System.Collections.IEnumerator WaitForCalibration()
    {
        if (calibrationManager == null)
        {
            Debug.LogError("GameInitializer: No CalibrationManager found!");
            yield break;
        }
        
        // Wait until calibration is done
        while (!calibrationManager.IsCalibrated)
        {
            yield return null;
        }
        
        Debug.Log("Calibration complete! Initializing gameplay...");
        
        if (autoStartAfterCalibration)
        {
            yield return new WaitForSeconds(delayBeforeStart);
            StartGameplay();
        }
    }
    
    /// <summary>
    /// Initialize and start gameplay
    /// </summary>
    public void StartGameplay()
    {
        if (gameStarted)
            return;
        
        gameStarted = true;
        
        // Configure player boundaries
        if (playerController != null)
        {
            playerController.SetBoundaries(gameplayMinY, gameplayMaxY);
            playerController.ActivatePlayer();
            Debug.Log("Player activated with boundaries: " + gameplayMinY + " to " + gameplayMaxY);
        }
        else
        {
            Debug.LogError("GameInitializer: Cannot start gameplay - PlayerController is null!");
        }
        
        // Configure camera if needed
        if (cameraController != null)
        {
            // Camera setup already handled in CameraController
            Debug.Log("Camera configured");
        }
        
        // Hide calibration UI if it exists
        HideCalibrationUI();
        
        Debug.Log("Gameplay started!");
    }
    
    /// <summary>
    /// Hide calibration UI elements (for single-scene setup)
    /// </summary>
    private void HideCalibrationUI()
    {
        // Find and disable calibration UI
        GameObject calibrationUI = GameObject.Find("CalibrationUI");
        if (calibrationUI != null)
        {
            calibrationUI.SetActive(false);
            Debug.Log("Calibration UI hidden");
        }
        
        // Also try to find CalibrationPanel
        GameObject calibrationPanel = GameObject.Find("CalibrationPanel");
        if (calibrationPanel != null)
        {
            calibrationPanel.SetActive(false);
            Debug.Log("Calibration Panel hidden");
        }
    }
    
    /// <summary>
    /// Restart the game (reload scene)
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    /// <summary>
    /// Get current boundaries for other systems
    /// </summary>
    public Vector2 GetBoundaries()
    {
        return new Vector2(gameplayMinY, gameplayMaxY);
    }
    
    /// <summary>
    /// Check if game is ready to play
    /// </summary>
    public bool IsReady()
    {
        return calibrationManager != null && 
               calibrationManager.IsCalibrated && 
               playerController != null;
    }
}