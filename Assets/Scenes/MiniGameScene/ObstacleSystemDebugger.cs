using UnityEngine;

/// <summary>
/// Visual debug helper for the obstacle system.
/// Shows spawn points, gap positions, and system status.
/// Attach to ObstacleSystem GameObject or Camera.
/// </summary>
public class ObstacleSystemDebugger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WallSpawner wallSpawner;
    [SerializeField] private GapGenerator gapGenerator;
    [SerializeField] private Camera mainCamera;
    
    [Header("Visualization")]
    [SerializeField] private bool showSpawnLine = true;
    [SerializeField] private bool showDestructionLine = true;
    [SerializeField] private bool showGapZones = true;
    [SerializeField] private bool showOnScreenStats = true;
    
    [Header("Colors")]
    [SerializeField] private Color spawnLineColor = Color.green;
    [SerializeField] private Color destructionLineColor = Color.red;
    [SerializeField] private Color gapZoneColor = new Color(0f, 1f, 0f, 0.2f);
    
    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (wallSpawner == null)
            wallSpawner = FindObjectOfType<WallSpawner>();
        
        if (gapGenerator == null)
            gapGenerator = FindObjectOfType<GapGenerator>();
    }
    
    void OnDrawGizmos()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (mainCamera == null)
            return;
        
        float screenHeight = mainCamera.orthographicSize * 2f;
        float screenTop = mainCamera.transform.position.y + mainCamera.orthographicSize;
        float screenBottom = mainCamera.transform.position.y - mainCamera.orthographicSize;
        
        // Show spawn line (where walls appear)
        if (showSpawnLine)
        {
            Gizmos.color = spawnLineColor;
            Vector3 spawnTop = new Vector3(mainCamera.transform.position.x + 15f, screenTop, 0f);
            Vector3 spawnBottom = new Vector3(mainCamera.transform.position.x + 15f, screenBottom, 0f);
            Gizmos.DrawLine(spawnTop, spawnBottom);
            
            // Label
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(spawnTop + Vector3.up * 0.5f, "SPAWN LINE");
            #endif
        }
        
        // Show destruction line (where walls get destroyed)
        if (showDestructionLine)
        {
            Gizmos.color = destructionLineColor;
            Vector3 destructTop = new Vector3(mainCamera.transform.position.x - 15f, screenTop, 0f);
            Vector3 destructBottom = new Vector3(mainCamera.transform.position.x - 15f, screenBottom, 0f);
            Gizmos.DrawLine(destructTop, destructBottom);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(destructTop + Vector3.up * 0.5f, "DESTROY LINE");
            #endif
        }
        
        // Show gap zones (where gaps can spawn)
        if (showGapZones && gapGenerator != null)
        {
            Gizmos.color = gapZoneColor;
            
            // Get gap generator settings via reflection or expose them
            float minY = -4f; // Default, should match your settings
            float maxY = 4f;
            
            Vector3 center = new Vector3(mainCamera.transform.position.x, (minY + maxY) / 2f, 0f);
            Vector3 size = new Vector3(30f, maxY - minY, 0.1f);
            
            Gizmos.DrawCube(center, size);
            
            // Draw min/max lines
            Gizmos.color = Color.yellow;
            Vector3 minLineStart = new Vector3(mainCamera.transform.position.x - 20f, minY, 0f);
            Vector3 minLineEnd = new Vector3(mainCamera.transform.position.x + 20f, minY, 0f);
            Gizmos.DrawLine(minLineStart, minLineEnd);
            
            Vector3 maxLineStart = new Vector3(mainCamera.transform.position.x - 20f, maxY, 0f);
            Vector3 maxLineEnd = new Vector3(mainCamera.transform.position.x + 20f, maxY, 0f);
            Gizmos.DrawLine(maxLineStart, maxLineEnd);
        }
    }
    
    void OnGUI()
    {
        if (!showOnScreenStats)
            return;
        
        GUILayout.BeginArea(new Rect(10, Screen.height - 150, 400, 140));
        
        GUI.backgroundColor = new Color(0f, 0f, 0f, 0.7f);
        GUILayout.Box("", GUILayout.Width(390), GUILayout.Height(135));
        
        GUILayout.BeginArea(new Rect(5, 5, 390, 130));
        
        GUILayout.Label("<b><color=yellow>OBSTACLE SYSTEM DEBUG</color></b>", 
            new GUIStyle(GUI.skin.label) { richText = true, fontSize = 14 });
        
        if (wallSpawner != null)
        {
            GUILayout.Label($"<color=lime>Spawner:</color> {wallSpawner.GetStats()}", 
                new GUIStyle(GUI.skin.label) { richText = true });
        }
        else
        {
            GUILayout.Label("<color=red>Spawner: NOT FOUND</color>", 
                new GUIStyle(GUI.skin.label) { richText = true });
        }
        
        if (gapGenerator != null)
        {
            GUILayout.Label($"<color=cyan>Generator:</color> {gapGenerator.GetStats()}", 
                new GUIStyle(GUI.skin.label) { richText = true });
        }
        else
        {
            GUILayout.Label("<color=red>Generator: NOT FOUND</color>", 
                new GUIStyle(GUI.skin.label) { richText = true });
        }
        
        // Count active walls
        WallObstacle[] activeWalls = FindObjectsOfType<WallObstacle>();
        GUILayout.Label($"<color=orange>Active Walls:</color> {activeWalls.Length}", 
            new GUIStyle(GUI.skin.label) { richText = true });
        
        GUILayout.EndArea();
        GUILayout.EndArea();
    }
}
