using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles scene transitions and ensures AudioSystem references are maintained.
/// Use this if you want separate Calibration and Gameplay scenes.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string calibrationSceneName = "Calibration";
    [SerializeField] private string gameplaySceneName = "Gameplay";
    
    [Header("References (Optional - Auto-finds if empty)")]
    [SerializeField] private CalibrationManager calibrationManager;
    [SerializeField] private PersistentAudioSystem audioSystem;
    
    private static SceneTransitionManager instance;
    
    void Awake()
    {
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
        // Auto-find AudioSystem if not assigned
        if (audioSystem == null)
        {
            audioSystem = FindObjectOfType<PersistentAudioSystem>();
            if (audioSystem != null)
            {
                calibrationManager = audioSystem.CalibrationManager;
                Debug.Log("SceneTransitionManager: Found PersistentAudioSystem");
            }
        }
        
        // If in calibration scene, wait for calibration to complete
        if (SceneManager.GetActiveScene().name == calibrationSceneName)
        {
            StartCoroutine(WaitForCalibrationThenLoad());
        }
    }
    
    /// <summary>
    /// Wait for calibration completion, then load gameplay scene
    /// </summary>
    private System.Collections.IEnumerator WaitForCalibrationThenLoad()
    {
        while (calibrationManager == null || !calibrationManager.IsCalibrated)
        {
            // Try to find calibration manager if not assigned
            if (calibrationManager == null)
            {
                calibrationManager = FindObjectOfType<CalibrationManager>();
            }
            yield return null;
        }
        
        Debug.Log("Calibration complete! Loading gameplay scene...");
        yield return new WaitForSeconds(1f);
        
        LoadGameplayScene();
    }
    
    /// <summary>
    /// Load the gameplay scene
    /// </summary>
    public void LoadGameplayScene()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }
    
    /// <summary>
    /// Load calibration scene
    /// </summary>
    public void LoadCalibrationScene()
    {
        SceneManager.LoadScene(calibrationSceneName);
    }
    
    /// <summary>
    /// Restart current scene
    /// </summary>
    public void RestartCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}