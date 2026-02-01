using UnityEngine;

/// <summary>
/// Central manager for the obstacle system.
/// Integrates with GameInitializer to start spawning at the right time.
/// </summary>
public class ObstacleSystemManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WallSpawner wallSpawner;
    [SerializeField] private GapGenerator gapGenerator;
    [SerializeField] private PlayerController playerController;
    
    [Header("Sync Settings")]
    [SerializeField] private bool syncSpeedWithPlayer = true;
    [SerializeField] private float speedMatchMultiplier = 1.0f; // Match player speed exactly
    
    [Header("Start Timing")]
    [SerializeField] private bool startWithPlayer = true;
    [SerializeField] private float delayAfterPlayerActivation = 1f;
    
    private bool isSystemActive = false;
    
    void Start()
    {
        Debug.Log("[OBSTACLE_MGR] ObstacleSystemManager Start() called");
        
        // Auto-find references if not assigned
        if (wallSpawner == null)
            wallSpawner = FindObjectOfType<WallSpawner>();
        
        if (gapGenerator == null)
            gapGenerator = FindObjectOfType<GapGenerator>();
        
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
        
        Debug.Log("[OBSTACLE_MGR] WallSpawner: " + (wallSpawner != null ? "Found" : "NOT FOUND"));
        Debug.Log("[OBSTACLE_MGR] GapGenerator: " + (gapGenerator != null ? "Found" : "NOT FOUND"));
        Debug.Log("[OBSTACLE_MGR] PlayerController: " + (playerController != null ? "Found" : "NOT FOUND"));
        Debug.Log("[OBSTACLE_MGR] startWithPlayer: " + startWithPlayer);
        
        ValidateReferences();
        
        if (startWithPlayer)
        {
            Debug.Log("[OBSTACLE_MGR] Starting WaitForPlayerAndStart coroutine...");
            // Wait for player to be activated, then start
            StartCoroutine(WaitForPlayerAndStart());
        }
        else
        {
            Debug.Log("[OBSTACLE_MGR] startWithPlayer is false - not auto-starting");
        }
    }
    
    /// <summary>
    /// Wait for player to be active, then start obstacle system
    /// </summary>
    private System.Collections.IEnumerator WaitForPlayerAndStart()
    {
        Debug.Log("[OBSTACLE_MGR] WaitForPlayerAndStart - looking for player...");
        
        // Wait until player controller exists and is active
        while (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            Debug.Log("[OBSTACLE_MGR] Still looking for PlayerController...");
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log("[OBSTACLE_MGR] Player found! Waiting " + delayAfterPlayerActivation + "s...");
        
        // Note: You may need to add a public property to PlayerController to check if active
        // For now, just wait a bit after player exists
        yield return new WaitForSeconds(delayAfterPlayerActivation);
        
        Debug.Log("[OBSTACLE_MGR] Delay complete, activating obstacle system...");
        ActivateObstacleSystem();
    }
    
    /// <summary>
    /// Start the obstacle spawning system
    /// </summary>
    public void ActivateObstacleSystem()
    {
        Debug.Log("[OBSTACLE_MGR] ActivateObstacleSystem called");
        
        if (isSystemActive)
        {
            Debug.LogWarning("[OBSTACLE_MGR] System already active!");
            return;
        }
        
        if (wallSpawner == null)
        {
            Debug.LogError("[OBSTACLE_MGR] Cannot start - WallSpawner is null!");
            return;
        }
        
        Debug.Log("[OBSTACLE_MGR] Calling wallSpawner.StartSpawning()...");
        wallSpawner.StartSpawning();
        isSystemActive = true;
        
        Debug.Log("[OBSTACLE_MGR] Obstacle system activated!");
    }
    
    /// <summary>
    /// Stop the obstacle spawning system
    /// </summary>
    public void DeactivateObstacleSystem()
    {
        if (wallSpawner != null)
        {
            wallSpawner.StopSpawning();
        }
        
        isSystemActive = false;
        Debug.Log("ObstacleSystemManager: Obstacle system deactivated");
    }
    
    /// <summary>
    /// Reset entire obstacle system (for game restart)
    /// </summary>
    public void ResetSystem()
    {
        DeactivateObstacleSystem();
        
        if (wallSpawner != null)
            wallSpawner.Reset();
        
        if (gapGenerator != null)
            gapGenerator.Reset();
        
        Debug.Log("ObstacleSystemManager: System reset");
    }
    
    /// <summary>
    /// Validate all required references
    /// </summary>
    private void ValidateReferences()
    {
        if (wallSpawner == null)
            Debug.LogError("ObstacleSystemManager: WallSpawner not assigned!");
        
        if (gapGenerator == null)
            Debug.LogWarning("ObstacleSystemManager: GapGenerator not assigned - walls may not have proper gaps!");
        
        if (playerController == null)
            Debug.LogWarning("ObstacleSystemManager: PlayerController not assigned - cannot sync speeds!");
    }
    
    /// <summary>
    /// Get system status for debugging
    /// </summary>
    public bool IsActive()
    {
        return isSystemActive;
    }
}
