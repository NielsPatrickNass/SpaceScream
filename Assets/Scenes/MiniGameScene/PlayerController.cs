using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Controls player movement based on audio input.
/// Horizontal: Constant auto-scroll to the right.
/// Vertical: Audio-driven with smooth, floaty physics.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CalibrationManager calibrationManager;
    private bool autoFindCalibration = true;
    
    [Header("Horizontal Movement")]
    [SerializeField] private float horizontalSpeed = 5f;
    
    [Header("Vertical Movement Settings")]
    [SerializeField] private float minY = -4f;  // Bottom screen boundary
    [SerializeField] private float maxY = 4f;   // Top screen boundary
    [SerializeField] private float verticalSmoothTime = 0.15f; // Controls "floatiness"
    [SerializeField] private bool usePhysicsMovement = false; // Toggle between smooth damp vs force-based
    
    [Header("Physics-Based Movement (if enabled)")]
    [SerializeField] private float verticalForce = 20f;
    [SerializeField] private float drag = 5f;
    [SerializeField] private float gravityScale = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool enableKeyboardDebug = true;
    [SerializeField] private float keyboardSpeed = 5f;
    [SerializeField] private bool showVolumeDebug = false;
    
    // Components
    private Rigidbody2D rb;
    
    // Movement state
    private float targetY;
    private float currentVelocityY;
    private bool isGameActive = false;
    
    // Debug
    private float lastVolume = 0f;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Configure rigidbody
        rb.gravityScale = usePhysicsMovement ? gravityScale : 0f;
        rb.linearDamping = usePhysicsMovement ? drag : 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // CRITICAL: Enable continuous collision detection to prevent phasing through walls
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Create a frictionless physics material so player slides along walls instead of getting stuck
        SetupFrictionlessMaterial();
    }
    
    /// <summary>
    /// Create and apply a frictionless physics material to prevent getting stuck on wall edges
    /// </summary>
    private void SetupFrictionlessMaterial()
    {
        // Create a new physics material with zero friction
        PhysicsMaterial2D slipperyMaterial = new PhysicsMaterial2D("PlayerSlippery");
        slipperyMaterial.friction = 0f;
        slipperyMaterial.bounciness = 0f;
        
        // Apply to player's collider
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerCollider.sharedMaterial = slipperyMaterial;
        }
        
        // Also apply to rigidbody
        rb.sharedMaterial = slipperyMaterial;
    }
    
    void Start()
    {
        // Auto-find CalibrationManager if not assigned
        if (calibrationManager == null && autoFindCalibration)
        {
            // Try PersistentAudioSystem first
            PersistentAudioSystem audioSystem = PersistentAudioSystem.Instance;
            if (audioSystem != null)
            {
                calibrationManager = audioSystem.CalibrationManager;
                Debug.Log("PlayerController: Auto-found CalibrationManager from PersistentAudioSystem");
            }
            
            // Fallback: search in scene
            if (calibrationManager == null)
            {
                calibrationManager = FindObjectOfType<CalibrationManager>();
                if (calibrationManager != null)
                    Debug.Log("PlayerController: Found CalibrationManager via FindObjectOfType");
            }
            
            if (calibrationManager == null)
            {
                Debug.LogError("PlayerController: CANNOT FIND CalibrationManager! Voice control will not work!");
            }
        }
        
        if (calibrationManager != null)
        {
            Debug.Log($"PlayerController: CalibrationManager status - IsCalibrated: {calibrationManager.IsCalibrated}");
        }
        
        // Start at bottom of screen
        Vector3 startPos = transform.position;
        startPos.y = minY;
        transform.position = startPos;
        targetY = minY;
        
        // AUTO-START for testing (remove this for production)
        Debug.Log("PlayerController: Auto-activating player for testing!");
        ActivatePlayer();
    }
    
    /// <summary>
    /// Activate player movement (call after calibration)
    /// </summary>
    public void ActivatePlayer()
    {
        isGameActive = true;
        Debug.Log("Player activated!");
    }
    
    /// <summary>
    /// Deactivate player movement (call on game over)
    /// </summary>
    public void DeactivatePlayer()
    {
        isGameActive = false;
        rb.linearVelocity = Vector2.zero;
    }
    
    void Update()
    {
        // Manual activation with Space key (debug)
        if (!isGameActive && enableKeyboardDebug)
        {
            bool spacePressed = false;
            
            #if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null)
                spacePressed = UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame;
            #endif
            
            // Also check legacy input as fallback
            try { spacePressed = spacePressed || Input.GetKeyDown(KeyCode.Space); } catch { }
            
            if (spacePressed)
            {
                Debug.LogWarning("=== SPACE PRESSED - ACTIVATING PLAYER ===");
                ActivatePlayer();
            }
        }
        
        if (!isGameActive)
            return;
        
        // Get normalized volume (0-1) from calibration manager
        float normalizedVolume = GetInputVolume();
        lastVolume = normalizedVolume;
        
        // Map volume to Y position
        targetY = Mathf.Lerp(minY, maxY, normalizedVolume);
        
        // Debug output
        if (showVolumeDebug && Time.frameCount % 30 == 0) // Every 30 frames
        {
            Debug.Log($"Volume: {normalizedVolume:F3} → Target Y: {targetY:F2} | Current Y: {transform.position.y:F2}");
        }
    }
    
    void FixedUpdate()
    {
        if (!isGameActive)
        {
            if (Time.frameCount % 60 == 0)
                Debug.Log("[PLAYER] FixedUpdate - NOT ACTIVE");
            return;
        }
        
        if (Time.frameCount % 60 == 0)
            Debug.Log("[PLAYER] FixedUpdate - ACTIVE, moving at speed " + horizontalSpeed);
        
        // Horizontal movement (constant auto-scroll)
        Vector2 horizontalVelocity = new Vector2(horizontalSpeed, rb.linearVelocity.y);
        
        // Vertical movement (audio-driven)
        if (usePhysicsMovement)
        {
            ApplyPhysicsBasedMovement();
        }
        else
        {
            ApplySmoothDampMovement();
        }
        
        // Apply horizontal velocity
        rb.linearVelocity = new Vector2(horizontalSpeed, rb.linearVelocity.y);
        
        // Clamp position to screen boundaries
        ClampPosition();
    }
    
    /// <summary>
    /// Smooth interpolation-based movement (recommended for floaty feel)
    /// </summary>
    private void ApplySmoothDampMovement()
    {
        float currentY = transform.position.y;
        float newY = Mathf.SmoothDamp(currentY, targetY, ref currentVelocityY, verticalSmoothTime);
        
        // Only adjust Y velocity, let X velocity from rb.linearVelocity handle horizontal movement
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, (newY - currentY) / Time.fixedDeltaTime);
        
        // Debug every 30 frames
        if (Time.frameCount % 30 == 0)
        {
            Debug.Log($"[MOVEMENT] Pos X: {transform.position.x:F2} | Y: {currentY:F2} → {newY:F2}");
        }
    }
    
    /// <summary>
    /// Physics force-based movement (alternative approach)
    /// </summary>
    private void ApplyPhysicsBasedMovement()
    {
        float currentY = transform.position.y;
        float deltaY = targetY - currentY;
        
        // Apply force proportional to distance from target
        float force = deltaY * verticalForce;
        rb.AddForce(Vector2.up * force);
        
        // Additional damping when close to target
        if (Mathf.Abs(deltaY) < 0.5f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.95f);
        }
    }
    
    /// <summary>
    /// Get input volume with keyboard debug fallback
    /// </summary>
    private float GetInputVolume()
    {
        float result = 0f;
        
        // Debug keyboard controls (for testing without microphone)
        // IMPORTANT: Check arrow keys ONLY if enableKeyboardDebug is true
        bool arrowPressed = false;
        
        if (enableKeyboardDebug)
        {
            // New Input System support
            #if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.upArrowKey.isPressed)
                {
                    Debug.Log("[KEYBOARD] UP Arrow - returning 1.0");
                    return 1f;
                }
                if (UnityEngine.InputSystem.Keyboard.current.downArrowKey.isPressed)
                {
                    Debug.Log("[KEYBOARD] DOWN Arrow - returning 0.0");
                    return 0f;
                }
            }
            #else
            // Old Input System fallback
            if (Input.GetKey(KeyCode.UpArrow))
            {
                Debug.Log("[KEYBOARD] UP Arrow - returning 1.0");
                return 1f;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                Debug.Log("[KEYBOARD] DOWN Arrow - returning 0.0");
                return 0f;
            }
            #endif
        }
        
        // If we get here, use voice input
        
        // Use calibrated microphone input
        if (calibrationManager == null)
        {
            Debug.LogError("[VOICE] CalibrationManager is NULL!");
            return 0f;
        }
        
        if (!calibrationManager.IsCalibrated)
        {
            Debug.LogWarning("[VOICE] CalibrationManager not calibrated!");
            return 0f;
        }
        
        result = calibrationManager.GetGameplayVolume();
        
        // Detailed logging every 30 frames to avoid console spam
        if (Time.frameCount % 30 == 0)
        {
            Debug.Log($"[VOICE INPUT] Raw from GetGameplayVolume: {result:F3} | Silence: {calibrationManager.SilenceBaseline:F3} | Max: {calibrationManager.MaxVolume:F3}");
        }
        
        return result;
    }
    
    /// <summary>
    /// Clamp player position within screen boundaries
    /// </summary>
    private void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }
    
    /// <summary>
    /// Set screen boundaries dynamically (useful for different resolutions)
    /// </summary>
    public void SetBoundaries(float min, float max)
    {
        minY = min;
        maxY = max;
        Debug.Log($"Boundaries set: {minY} to {maxY}");
    }
    
    /// <summary>
    /// Get current normalized position (0 = bottom, 1 = top)
    /// </summary>
    public float GetNormalizedPosition()
    {
        return Mathf.InverseLerp(minY, maxY, transform.position.y);
    }
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Draw screen boundaries in editor
        Gizmos.color = Color.yellow;
        
        Vector3 leftMin = new Vector3(-10f, minY, 0f);
        Vector3 rightMin = new Vector3(10f, minY, 0f);
        Gizmos.DrawLine(leftMin, rightMin);
        
        Vector3 leftMax = new Vector3(-10f, maxY, 0f);
        Vector3 rightMax = new Vector3(10f, maxY, 0f);
        Gizmos.DrawLine(leftMax, rightMax);
        
        // Draw target position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, targetY, 0f), 0.2f);
    }
#endif
    
    void OnGUI()
    {
        if (!showVolumeDebug)
            return;
        
        // Always show activation button
        if (!isGameActive)
        {
            if (GUI.Button(new Rect(Screen.width - 210, 10, 200, 40), "ACTIVATE PLAYER (Click or Space)"))
            {
                ActivatePlayer();
            }
        }
        
        if (!isGameActive)
            return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 310, 60, 300, 140));
        GUILayout.Box("=== PLAYER CONTROL ===");
        GUILayout.Label($"Active: {isGameActive}");
        GUILayout.Label($"Volume Input: {lastVolume:F3}");
        GUILayout.Label($"Target Y: {targetY:F2}");
        GUILayout.Label($"Current Y: {transform.position.y:F2}");
        GUILayout.Label($"Has CalibMgr: {calibrationManager != null}");
        if (calibrationManager != null)
        {
            GUILayout.Label($"Is Calibrated: {calibrationManager.IsCalibrated}");
        }
        GUILayout.EndArea();
    }
}