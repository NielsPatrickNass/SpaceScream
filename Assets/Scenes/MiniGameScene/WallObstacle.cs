using UnityEngine;

public class WallObstacle : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float scrollSpeed = 5f;
    [SerializeField] private bool wallsMove = false;
    
    [Header("Auto-Destruction")]
    [SerializeField] private float destructionOffsetX = -15f;
    
    public static bool ForceStationaryWalls = true;
    
    [Header("Gap Configuration")]
    [SerializeField] private Transform topWall;
    [SerializeField] private Transform bottomWall;
    [SerializeField] private float gapSize = 2.5f;
    [SerializeField] private float gapCenterY = 0f;
    
    [Header("Gap Overlay Visual")]
    [SerializeField] private bool showGapOverlay = true;
    [SerializeField] private Color gapOverlayColor = new Color(1f, 1f, 1f, 0.1f); // White, semi-transparent
    
    [Header("Close Animation")]
    [SerializeField] private bool enableCloseAnimation = true;
    [SerializeField] private float closeSpeed = 3f; // How fast walls close
    [SerializeField] private float closeDelay = 0.1f; // Delay after player passes before closing
    
    private Camera mainCamera;
    private static PhysicsMaterial2D frictionlessMaterial;
    private GameObject gapOverlay;
    private GameObject gapTriggerZone;
    
    // Close animation state
    private bool playerPassed = false;
    private bool isClosing = false;
    private float closeTimer = 0f;
    private float topWallTargetY;
    private float bottomWallTargetY;
    private float topWallStartY;
    private float bottomWallStartY;

    void Start()
    {
        mainCamera = Camera.main;
        
        // Create frictionless material once (shared by all walls)
        if (frictionlessMaterial == null)
        {
            frictionlessMaterial = new PhysicsMaterial2D("WallFrictionless");
            frictionlessMaterial.friction = 0f;
            frictionlessMaterial.bounciness = 0f;
        }
        
        // Apply to wall colliders so player slides along them
        ApplyFrictionlessMaterial();
    }
    
    private void ApplyFrictionlessMaterial()
    {
        if (topWall != null)
        {
            Collider2D col = topWall.GetComponent<Collider2D>();
            if (col != null) col.sharedMaterial = frictionlessMaterial;
        }
        
        if (bottomWall != null)
        {
            Collider2D col = bottomWall.GetComponent<Collider2D>();
            if (col != null) col.sharedMaterial = frictionlessMaterial;
        }
    }
    
    void Update()
    {
        bool shouldMove = wallsMove && !ForceStationaryWalls;
        
        if (shouldMove)
        {
            transform.position += Vector3.left * scrollSpeed * Time.deltaTime;
        }
        
        // Handle close animation
        if (enableCloseAnimation && playerPassed)
        {
            if (!isClosing)
            {
                // Wait for delay before starting to close
                closeTimer += Time.deltaTime;
                if (closeTimer >= closeDelay)
                {
                    StartClosing();
                }
            }
            else
            {
                // Animate walls closing
                AnimateClosing();
            }
        }
        
        // Destroy when off-screen
        if (mainCamera != null && transform.position.x < mainCamera.transform.position.x + destructionOffsetX)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Called when player passes through the gap - triggers close animation
    /// </summary>
    public void OnPlayerPassed()
    {
        if (!playerPassed && enableCloseAnimation)
        {
            playerPassed = true;
            closeTimer = 0f;
            
            // Store starting positions
            if (topWall != null) topWallStartY = topWall.localPosition.y;
            if (bottomWall != null) bottomWallStartY = bottomWall.localPosition.y;
            
            // Calculate target positions so wall EDGES meet at gap center (not wall centers)
            // Top wall: move down so its bottom edge reaches gap center
            // Bottom wall: move up so its top edge reaches gap center
            
            float topWallHalfHeight = 0f;
            float bottomWallHalfHeight = 0f;
            
            if (topWall != null)
            {
                SpriteRenderer sr = topWall.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    // Half height in world space = sprite height * scale / 2
                    topWallHalfHeight = (sr.sprite.bounds.size.y * topWall.localScale.y) / 2f;
                }
            }
            
            if (bottomWall != null)
            {
                SpriteRenderer sr = bottomWall.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    bottomWallHalfHeight = (sr.sprite.bounds.size.y * bottomWall.localScale.y) / 2f;
                }
            }
            
            // Top wall center should be at gapCenter + halfHeight (so bottom edge is at gapCenter)
            topWallTargetY = gapCenterY + topWallHalfHeight;
            // Bottom wall center should be at gapCenter - halfHeight (so top edge is at gapCenter)
            bottomWallTargetY = gapCenterY - bottomWallHalfHeight;
            
            // Hide the gap overlay when closing starts
            if (gapOverlay != null)
            {
                gapOverlay.SetActive(false);
            }
        }
    }
    
    private void StartClosing()
    {
        isClosing = true;
        
        // Disable colliders so closing walls don't affect player
        if (topWall != null)
        {
            Collider2D col = topWall.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
        if (bottomWall != null)
        {
            Collider2D col = bottomWall.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }
    
    private void AnimateClosing()
    {
        float speed = closeSpeed * Time.deltaTime;
        
        // Move top wall down
        if (topWall != null)
        {
            Vector3 topPos = topWall.localPosition;
            topPos.y = Mathf.MoveTowards(topPos.y, topWallTargetY, speed);
            topWall.localPosition = topPos;
        }
        
        // Move bottom wall up
        if (bottomWall != null)
        {
            Vector3 bottomPos = bottomWall.localPosition;
            bottomPos.y = Mathf.MoveTowards(bottomPos.y, bottomWallTargetY, speed);
            bottomWall.localPosition = bottomPos;
        }
    }
    
    public void SetupGap(float centerY, float size, float speed)
    {
        gapCenterY = centerY;
        gapSize = size;
        scrollSpeed = speed;
        
        if (mainCamera == null) mainCamera = Camera.main;
        float screenTop = mainCamera != null ? mainCamera.orthographicSize : 5f;
        float screenBottom = -screenTop;
        
        // Parent stays at Y=0
        Vector3 pos = transform.position;
        pos.y = 0f;
        transform.position = pos;
        
        float halfGap = size / 2f;
        float gapTop = centerY + halfGap;
        float gapBottom = centerY - halfGap;
        
        // Top wall
        if (topWall != null)
        {
            // Get sprite's actual size
            float spriteHeight = 1f;
            float spriteWidth = 1f;
            SpriteRenderer sr = topWall.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                spriteHeight = sr.sprite.bounds.size.y;
                spriteWidth = sr.sprite.bounds.size.x;
            }
            
            float topHeight = (screenTop + 3f) - gapTop;
            float topCenterY = gapTop + (topHeight / 2f);
            float scaleY = topHeight / spriteHeight;
            
            topWall.localPosition = new Vector3(0f, topCenterY, 0f);
            topWall.localScale = new Vector3(topWall.localScale.x, scaleY, 1f);
            
            // Set collider to match the sprite bounds (in local unscaled space)
            // Make it wider to ensure solid collision
            BoxCollider2D col = topWall.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.size = new Vector2(spriteWidth * 1.5f, spriteHeight);  // 1.5x wider
                col.offset = Vector2.zero;
            }
        }
        
        // Bottom wall
        if (bottomWall != null)
        {
            float spriteHeight = 1f;
            float spriteWidth = 1f;
            SpriteRenderer sr = bottomWall.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                spriteHeight = sr.sprite.bounds.size.y;
                spriteWidth = sr.sprite.bounds.size.x;
            }
            
            float bottomHeight = gapBottom - (screenBottom - 3f);
            float bottomCenterY = gapBottom - (bottomHeight / 2f);
            float scaleY = bottomHeight / spriteHeight;
            
            bottomWall.localPosition = new Vector3(0f, bottomCenterY, 0f);
            bottomWall.localScale = new Vector3(bottomWall.localScale.x, scaleY, 1f);
            
            // Set collider to match the sprite bounds - make wider
            BoxCollider2D col = bottomWall.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.size = new Vector2(spriteWidth * 1.5f, spriteHeight);  // 1.5x wider
                col.offset = Vector2.zero;
            }
        }
        
        // Create visual overlay for the gap
        if (showGapOverlay)
        {
            CreateGapOverlay(centerY, size);
        }
        
        // Create trigger zone for gap pass detection
        CreateGapTrigger(centerY, size);
        
        Debug.Log("[WALL] Gap at Y=" + centerY + " size=" + size);
    }
    
    /// <summary>
    /// Creates an invisible trigger zone in the gap for detecting when player passes through
    /// </summary>
    private void CreateGapTrigger(float centerY, float size)
    {
        // Remove old trigger if exists
        if (gapTriggerZone != null)
        {
            Destroy(gapTriggerZone);
        }
        
        // Create trigger GameObject
        gapTriggerZone = new GameObject("GapTrigger");
        gapTriggerZone.transform.SetParent(transform);
        gapTriggerZone.transform.localPosition = new Vector3(0f, centerY, 0f);
        
        // Try to set tag (may fail if tag doesn't exist)
        try { gapTriggerZone.tag = "GapTrigger"; } 
        catch { /* Tag doesn't exist, that's ok - we'll use name matching */ }
        
        // Add trigger collider
        BoxCollider2D trigger = gapTriggerZone.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        
        // Get wall width
        float wallWidth = 1f;
        if (topWall != null)
        {
            SpriteRenderer sr = topWall.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                wallWidth = sr.sprite.bounds.size.x * topWall.localScale.x;
            }
        }
        
        // Size: slightly smaller than gap to ensure player is fully through
        trigger.size = new Vector2(wallWidth * 0.5f, size * 0.8f);
    }
    
    /// <summary>
    /// Creates a semi-transparent overlay to highlight the gap area
    /// </summary>
    private void CreateGapOverlay(float centerY, float size)
    {
        // Remove old overlay if exists
        if (gapOverlay != null)
        {
            Destroy(gapOverlay);
        }
        
        // Create new overlay GameObject
        gapOverlay = new GameObject("GapOverlay");
        gapOverlay.transform.SetParent(transform);
        gapOverlay.transform.localPosition = new Vector3(0f, centerY, 0.1f); // Slightly behind walls
        
        // Add SpriteRenderer
        SpriteRenderer sr = gapOverlay.AddComponent<SpriteRenderer>();
        
        // Create a simple white square sprite
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        
        // Set color (semi-transparent white by default)
        sr.color = gapOverlayColor;
        
        // Set sorting order behind walls but visible
        sr.sortingOrder = -1;
        
        // Get wall width from top or bottom wall sprite
        float wallWidth = 1f;
        if (topWall != null)
        {
            SpriteRenderer wallSR = topWall.GetComponent<SpriteRenderer>();
            if (wallSR != null && wallSR.sprite != null)
            {
                wallWidth = wallSR.sprite.bounds.size.x * topWall.localScale.x;
            }
        }
        
        // Scale to match gap size and wall width
        gapOverlay.transform.localScale = new Vector3(wallWidth, size, 1f);
    }

    public float GetGapCenterY() { return gapCenterY; }
    public float GetGapSize() { return gapSize; }
}
