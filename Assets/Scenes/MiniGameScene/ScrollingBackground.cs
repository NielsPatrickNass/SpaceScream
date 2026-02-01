using UnityEngine;

/// <summary>
/// Keeps background synchronized with camera movement for side-scrolling games.
/// Safe to use with CameraController - doesn't interfere with camera logic.
/// Background will always fill the visible screen area.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ScrollingBackground : MonoBehaviour
{
    [Header("Camera Reference")]
    [SerializeField] private Camera targetCamera;
    
    [Header("Position Settings")]
    [SerializeField] private float zDistance = 10f; // How far behind camera (positive = behind)
    [SerializeField] private Vector2 offset = Vector2.zero; // Optional offset from camera center
    
    [Header("Parallax Effect (Optional)")]
    [SerializeField] private bool enableParallax = false;
    [SerializeField] [Range(0f, 1f)] private float parallaxAmount = 0.3f; // 0 = fixed, 1 = moves with camera
    
    [Header("Auto-Scale Settings")]
    [SerializeField] private bool autoScaleToCamera = true;
    [SerializeField] private float scalePadding = 1.2f; // Extra scale to ensure no gaps
    
    private Vector3 lastCameraPosition;
    private bool isInitialized = false;
    
    void Start()
    {
        Initialize();
    }
    
    void Initialize()
    {
        // Find camera if not assigned
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("ScrollingBackground: No camera found!");
                return;
            }
        }
        
        lastCameraPosition = targetCamera.transform.position;
        
        // Auto-scale to fit camera view
        if (autoScaleToCamera)
        {
            ScaleToFitCamera();
        }
        
        // Position background initially
        UpdatePosition(true);
        
        isInitialized = true;
        
        Debug.Log($"ScrollingBackground: Initialized. Parallax: {(enableParallax ? parallaxAmount.ToString("F2") : "Off")}");
    }
    
    void LateUpdate()
    {
        if (!isInitialized || targetCamera == null)
            return;
        
        UpdatePosition(false);
    }
    
    /// <summary>
    /// Update background position relative to camera
    /// </summary>
    void UpdatePosition(bool isInitialSetup)
    {
        Vector3 cameraPosition = targetCamera.transform.position;
        Vector3 newPosition;
        
        if (enableParallax && !isInitialSetup)
        {
            // Parallax effect: background moves slower than camera
            Vector3 deltaMovement = cameraPosition - lastCameraPosition;
            Vector3 parallaxMovement = deltaMovement * parallaxAmount;
            
            newPosition = transform.position + parallaxMovement;
            newPosition.z = cameraPosition.z + zDistance;
        }
        else
        {
            // Fixed to camera: background doesn't move relative to screen
            newPosition = new Vector3(
                cameraPosition.x + offset.x,
                cameraPosition.y + offset.y,
                cameraPosition.z + zDistance
            );
        }
        
        transform.position = newPosition;
        lastCameraPosition = cameraPosition;
    }
    
    /// <summary>
    /// Automatically scale sprite to fit camera view
    /// </summary>
    void ScaleToFitCamera()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogWarning("ScrollingBackground: No SpriteRenderer or Sprite found!");
            return;
        }
        
        // Get camera dimensions in world units
        float cameraHeight = targetCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * targetCamera.aspect;
        
        // Get sprite dimensions
        Bounds spriteBounds = sr.sprite.bounds;
        float spriteWidth = spriteBounds.size.x;
        float spriteHeight = spriteBounds.size.y;
        
        // Calculate required scale to fill camera view
        float scaleX = (cameraWidth / spriteWidth) * scalePadding;
        float scaleY = (cameraHeight / spriteHeight) * scalePadding;
        
        // Use the larger scale to ensure full coverage
        float finalScale = Mathf.Max(scaleX, scaleY);
        
        transform.localScale = new Vector3(finalScale, finalScale, 1f);
        
        Debug.Log($"ScrollingBackground: Auto-scaled to {finalScale:F2} (Camera: {cameraWidth:F1}x{cameraHeight:F1})");
    }
    
    /// <summary>
    /// Manually trigger re-scaling (useful if camera size changes)
    /// </summary>
    public void RefreshScale()
    {
        if (autoScaleToCamera)
        {
            ScaleToFitCamera();
        }
    }
    
    void OnValidate()
    {
        // Update in editor when values change
        if (Application.isPlaying && isInitialized && autoScaleToCamera)
        {
            ScaleToFitCamera();
        }
    }
}
