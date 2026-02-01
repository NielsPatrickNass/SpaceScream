using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles collision detection for the player.
/// Detects wall hits (physics collision) and successful gap passes (trigger).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class CollisionHandler : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallBounceForce = 2f;
    [SerializeField] private bool enableBounceOnHit = true;
    
    [Header("Gap Detection")]
    [SerializeField] private LayerMask gapTriggerLayer;
    [SerializeField] private float gapPassCooldown = 0.5f; // Prevent double-counting same wall
    
    [Header("Events")]
    public UnityEvent OnWallHit;
    public UnityEvent OnGapPassed;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip wallHitSound;
    [SerializeField] private AudioClip gapPassSound;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool enableScreenShake = true;
    [SerializeField] private float shakeIntensity = 0.2f;
    [SerializeField] private float shakeDuration = 0.1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true; // Temporarily enabled for debugging
    
    private Rigidbody2D rb;
    private float lastGapPassTime = -999f;
    private int wallsHit = 0;
    private int gapsPassed = 0;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Setup audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }
    
    void Start()
    {
        // Ensure player collider is NOT a trigger
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null && playerCollider.isTrigger)
        {
            Debug.LogWarning("CollisionHandler: Player collider should NOT be a trigger for wall collisions!");
        }
    }
    
    /// <summary>
    /// Detect collision with walls (solid collision)
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if we hit a wall
        if (((1 << collision.gameObject.layer) & wallLayer) != 0 || 
            collision.gameObject.CompareTag("Wall") || 
            collision.gameObject.CompareTag("Obstacle"))
        {
            HandleWallCollision(collision);
        }
    }
    
    /// <summary>
    /// Detect passing through gap (trigger zone)
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we passed through a gap trigger (by layer, tag, or name)
        bool isGapTrigger = ((1 << other.gameObject.layer) & gapTriggerLayer) != 0 || 
            other.CompareTag("Gap") || 
            other.CompareTag("GapTrigger") ||
            other.gameObject.name == "GapTrigger";
        
        if (showDebugLogs)
        {
            Debug.Log($"[TRIGGER] OnTriggerEnter2D: {other.gameObject.name} | IsGapTrigger: {isGapTrigger}");
        }
            
        if (isGapTrigger)
        {
            HandleGapPass(other);
        }
    }
    
    /// <summary>
    /// Handle collision with wall
    /// </summary>
    private void HandleWallCollision(Collision2D collision)
    {
        wallsHit++;
        
        if (showDebugLogs)
        {
            Debug.Log($"[COLLISION] Wall hit! Total: {wallsHit}");
        }
        
        // Apply bounce effect if enabled
        if (enableBounceOnHit && rb != null)
        {
            Vector2 bounceDirection = collision.contacts[0].normal;
            rb.AddForce(bounceDirection * wallBounceForce, ForceMode2D.Impulse);
        }
        
        // Trigger event
        OnWallHit?.Invoke();
        
        // Play sound
        if (wallHitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(wallHitSound);
        }
        
        // Screen shake
        if (enableScreenShake)
        {
            TriggerScreenShake();
        }
        
        // Notify GameManager
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.OnPlayerHitWall();
        }
    }
    
    /// <summary>
    /// Handle successful gap passage
    /// </summary>
    private void HandleGapPass(Collider2D gapTrigger)
    {
        // Cooldown check to prevent double-counting
        if (Time.time - lastGapPassTime < gapPassCooldown)
        {
            return;
        }
        
        lastGapPassTime = Time.time;
        gapsPassed++;
        
        if (showDebugLogs)
        {
            Debug.Log($"[SUCCESS] Gap passed! Total: {gapsPassed}");
        }
        
        // Trigger close animation on the wall obstacle
        WallObstacle wallObstacle = gapTrigger.GetComponentInParent<WallObstacle>();
        if (wallObstacle != null)
        {
            wallObstacle.OnPlayerPassed();
        }
        
        // Trigger event
        OnGapPassed?.Invoke();
        
        // Play sound
        if (gapPassSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(gapPassSound);
        }
        
        // Notify ScoreManager
        ScoreManager scoreManager = ScoreManager.Instance;
        if (scoreManager != null)
        {
            scoreManager.AddScore(1);
        }
        
        // Notify GameManager
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.OnPlayerPassedGap();
        }
    }
    
    /// <summary>
    /// Trigger screen shake effect
    /// </summary>
    private void TriggerScreenShake()
    {
        CameraShake cameraShake = Camera.main?.GetComponent<CameraShake>();
        if (cameraShake != null)
        {
            cameraShake.Shake(shakeIntensity, shakeDuration);
        }
    }
    
    /// <summary>
    /// Get statistics
    /// </summary>
    public int GetWallsHit() => wallsHit;
    public int GetGapsPassed() => gapsPassed;
    
    /// <summary>
    /// Reset statistics (for game restart)
    /// </summary>
    public void Reset()
    {
        wallsHit = 0;
        gapsPassed = 0;
        lastGapPassTime = -999f;
    }
    
    void OnGUI()
    {
        if (showDebugLogs)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 220, 10, 210, 80));
            GUILayout.Box("", GUILayout.Width(205), GUILayout.Height(75));
            GUILayout.BeginArea(new Rect(5, 5, 200, 70));
            
            GUILayout.Label("<b><color=yellow>COLLISION STATS</color></b>", 
                new GUIStyle(GUI.skin.label) { richText = true });
            GUILayout.Label($"<color=lime>Gaps Passed:</color> {gapsPassed}", 
                new GUIStyle(GUI.skin.label) { richText = true });
            GUILayout.Label($"<color=red>Walls Hit:</color> {wallsHit}", 
                new GUIStyle(GUI.skin.label) { richText = true });
            
            GUILayout.EndArea();
            GUILayout.EndArea();
        }
    }
}
