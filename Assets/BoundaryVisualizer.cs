using UnityEngine;

/// <summary>
/// Optional helper to visualize screen boundaries in the game view.
/// Helps with level design and testing.
/// </summary>
public class BoundaryVisualizer : MonoBehaviour
{
    [Header("Boundary Settings")]
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;
    [SerializeField] private float boundaryWidth = 20f;
    
    [Header("Visual Settings")]
    [SerializeField] private Color boundaryColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private bool showInGame = false;
    [SerializeField] private bool showInEditor = true;
    
    [Header("Line Renderers (Optional)")]
    [SerializeField] private LineRenderer topLine;
    [SerializeField] private LineRenderer bottomLine;
    [SerializeField] private float lineWidth = 0.1f;
    
    void Start()
    {
        if (showInGame)
            CreateBoundaryLines();
    }
    
    /// <summary>
    /// Create visual lines for boundaries using LineRenderer
    /// </summary>
    private void CreateBoundaryLines()
    {
        // Create top boundary line
        if (topLine == null)
        {
            GameObject topObj = new GameObject("TopBoundaryLine");
            topObj.transform.parent = transform;
            topLine = topObj.AddComponent<LineRenderer>();
        }
        
        ConfigureLine(topLine, maxY);
        
        // Create bottom boundary line
        if (bottomLine == null)
        {
            GameObject bottomObj = new GameObject("BottomBoundaryLine");
            bottomObj.transform.parent = transform;
            bottomLine = bottomObj.AddComponent<LineRenderer>();
        }
        
        ConfigureLine(bottomLine, minY);
    }
    
    /// <summary>
    /// Configure a LineRenderer for boundary visualization
    /// </summary>
    private void ConfigureLine(LineRenderer line, float yPosition)
    {
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = 2;
        line.useWorldSpace = true;
        
        // Set color
        line.startColor = boundaryColor;
        line.endColor = boundaryColor;
        
        // Set material (use default)
        line.material = new Material(Shader.Find("Sprites/Default"));
        
        // Set positions
        UpdateLinePosition(line, yPosition);
    }
    
    /// <summary>
    /// Update line position based on camera
    /// </summary>
    private void UpdateLinePosition(LineRenderer line, float yPosition)
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        
        float leftX = cam.transform.position.x - boundaryWidth / 2f;
        float rightX = cam.transform.position.x + boundaryWidth / 2f;
        
        line.SetPosition(0, new Vector3(leftX, yPosition, 0f));
        line.SetPosition(1, new Vector3(rightX, yPosition, 0f));
    }
    
    void Update()
    {
        // Update line positions to follow camera if needed
        if (showInGame && topLine != null && bottomLine != null)
        {
            UpdateLinePosition(topLine, maxY);
            UpdateLinePosition(bottomLine, minY);
        }
    }
    
    /// <summary>
    /// Set boundaries at runtime
    /// </summary>
    public void SetBoundaries(float min, float max)
    {
        minY = min;
        maxY = max;
        
        if (showInGame && topLine != null && bottomLine != null)
        {
            UpdateLinePosition(topLine, maxY);
            UpdateLinePosition(bottomLine, minY);
        }
    }
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!showInEditor)
            return;
        
        Camera cam = Camera.main;
        if (cam == null) return;
        
        Gizmos.color = boundaryColor;
        
        float leftX = cam.transform.position.x - boundaryWidth / 2f;
        float rightX = cam.transform.position.x + boundaryWidth / 2f;
        
        // Draw top boundary
        Vector3 topLeft = new Vector3(leftX, maxY, 0f);
        Vector3 topRight = new Vector3(rightX, maxY, 0f);
        Gizmos.DrawLine(topLeft, topRight);
        
        // Draw bottom boundary
        Vector3 bottomLeft = new Vector3(leftX, minY, 0f);
        Vector3 bottomRight = new Vector3(rightX, minY, 0f);
        Gizmos.DrawLine(bottomLeft, bottomRight);
        
        // Draw connecting lines
        Gizmos.color = new Color(boundaryColor.r, boundaryColor.g, boundaryColor.b, 0.1f);
        Gizmos.DrawLine(topLeft, bottomLeft);
        Gizmos.DrawLine(topRight, bottomRight);
    }
#endif
}