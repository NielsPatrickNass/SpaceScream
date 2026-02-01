using UnityEngine;

/// <summary>
/// Bare-bones player movement test - bypasses all audio systems.
/// Use this to verify basic movement works.
/// Replace PlayerController temporarily with this script.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SimplePlayerTest : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float horizontalSpeed = 5f;
    [SerializeField] private float verticalSpeed = 5f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;
    
    private Rigidbody2D rb;
    private float targetY;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        Debug.Log("SimplePlayerTest: Ready!");
    }
    
    void Start()
    {
        targetY = minY;
        transform.position = new Vector3(transform.position.x, minY, 0f);
    }
    
    void Update()
    {
        // Simple keyboard control
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            targetY = maxY;
            Debug.Log("Moving UP - Target: " + targetY);
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            targetY = minY;
            Debug.Log("Moving DOWN - Target: " + targetY);
        }
        
        // Show current position
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log($"Current Position: {transform.position}, Target Y: {targetY}");
        }
    }
    
    void FixedUpdate()
    {
        // Horizontal movement
        float newX = transform.position.x + (horizontalSpeed * Time.fixedDeltaTime);
        
        // Vertical movement
        float newY = Mathf.MoveTowards(transform.position.y, targetY, verticalSpeed * Time.fixedDeltaTime);
        
        // Clamp
        newY = Mathf.Clamp(newY, minY, maxY);
        
        // Apply
        rb.MovePosition(new Vector2(newX, newY));
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Box("=== SIMPLE TEST ===");
        GUILayout.Label($"Position: {transform.position}");
        GUILayout.Label($"Target Y: {targetY}");
        GUILayout.Label($"Velocity: {rb.linearVelocity}");
        GUILayout.Space(10);
        GUILayout.Label("UP/W - Move to top");
        GUILayout.Label("DOWN/S - Move to bottom");
        GUILayout.Label("P - Print position");
        GUILayout.EndArea();
    }
}