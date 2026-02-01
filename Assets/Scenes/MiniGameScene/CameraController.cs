using UnityEngine;

/// <summary>
/// Controls camera behavior for the side-scrolling game.
/// Options: Follow player, fixed position, or smooth follow.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Camera Mode")]
    [SerializeField] private CameraMode mode = CameraMode.FollowHorizontal;
    
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothSpeed = 0.125f;
    
    [Header("Vertical Tracking")]
    [SerializeField] private bool followVertical = false;
    [SerializeField] private float verticalSmoothSpeed = 0.1f;
    
    [Header("Boundaries")]
    [SerializeField] private bool useBoundaries = false;
    [SerializeField] private float minX = 0f;
    [SerializeField] private float maxX = 100f;
    
    private Vector3 targetPosition;
    private float initialY;
    
    public enum CameraMode
    {
        Fixed,              // Camera doesn't move
        FollowHorizontal,   // Follow player X, fixed Y
        FollowBoth,         // Follow player X and Y
        SmoothFollow        // Smooth damped follow
    }
    
    void Start()
    {
        initialY = transform.position.y;
        
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
            else
                Debug.LogWarning("CameraController: No target assigned and no Player tag found!");
        }
    }
    
    void LateUpdate()
    {
        if (target == null)
            return;
        
        switch (mode)
        {
            case CameraMode.Fixed:
                // Camera doesn't move
                break;
                
            case CameraMode.FollowHorizontal:
                FollowHorizontalOnly();
                break;
                
            case CameraMode.FollowBoth:
                FollowPlayerCompletely();
                break;
                
            case CameraMode.SmoothFollow:
                SmoothFollowPlayer();
                break;
        }
    }
    
    /// <summary>
    /// Follow player horizontally, keep Y fixed
    /// </summary>
    private void FollowHorizontalOnly()
    {
        float targetX = target.position.x + offset.x;
        
        if (useBoundaries)
            targetX = Mathf.Clamp(targetX, minX, maxX);
        
        Vector3 newPosition = new Vector3(targetX, initialY, offset.z);
        transform.position = newPosition;
    }
    
    /// <summary>
    /// Follow player on both axes
    /// </summary>
    private void FollowPlayerCompletely()
    {
        Vector3 desiredPosition = target.position + offset;
        
        if (useBoundaries)
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        
        transform.position = desiredPosition;
    }
    
    /// <summary>
    /// Smooth damped follow
    /// </summary>
    private void SmoothFollowPlayer()
    {
        Vector3 desiredPosition = target.position + offset;
        
        if (useBoundaries)
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
    
    /// <summary>
    /// Set camera to follow target with custom offset
    /// </summary>
    public void SetTarget(Transform newTarget, Vector3 newOffset)
    {
        target = newTarget;
        offset = newOffset;
    }
    
    /// <summary>
    /// Change camera mode at runtime
    /// </summary>
    public void SetCameraMode(CameraMode newMode)
    {
        mode = newMode;
    }
    
    /// <summary>
    /// Set camera boundaries
    /// </summary>
    public void SetBoundaries(float min, float max)
    {
        useBoundaries = true;
        minX = min;
        maxX = max;
    }
}