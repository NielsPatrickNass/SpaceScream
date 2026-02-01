using UnityEngine;
using System.Collections.Generic;

public class WallSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private float spawnAheadDistance = 20f;
    [SerializeField] private float minDistanceBetweenWalls = 8f;

    [Header("References")]
    [SerializeField] private GapGenerator gapGenerator;
    [SerializeField] private Transform wallContainer;

    [Header("Game Control")]
    [SerializeField] private float startDelay = 2f;

    [Header("Debug")]
    [SerializeField] public bool showDebugInfo = false;

    private Camera mainCamera;
    private bool isSpawning = false;
    private int wallsSpawned = 0;
    private float furthestWallX = -999f;
    private List<GameObject> activeWalls = new List<GameObject>();
    private float startTime;
    private bool initialized = false;

    void Start()
    {
        Debug.Log("[SPAWNER] Start()");
        mainCamera = Camera.main;
        
        if (gapGenerator == null)
            gapGenerator = FindObjectOfType<GapGenerator>();
        
        if (wallContainer == null)
        {
            GameObject container = new GameObject("Walls");
            wallContainer = container.transform;
        }

        startTime = Time.time;
        
        // Validate
        if (wallPrefab == null) Debug.LogError("[SPAWNER] wallPrefab is NULL!");
        if (gapGenerator == null) Debug.LogError("[SPAWNER] gapGenerator is NULL!");
        if (mainCamera == null) Debug.LogError("[SPAWNER] mainCamera is NULL!");
    }

    void Update()
    {
        // Wait for start delay
        if (!initialized)
        {
            if (Time.time - startTime >= startDelay)
            {
                initialized = true;
                isSpawning = true;
                furthestWallX = mainCamera.transform.position.x + 5f;
                Debug.Log("[SPAWNER] Initialized! Starting spawning at X=" + furthestWallX);
            }
            return;
        }

        if (!isSpawning) return;
        if (wallPrefab == null || gapGenerator == null || mainCamera == null) return;

        // Clean up destroyed walls
        activeWalls.RemoveAll(w => w == null);

        // Spawn walls ahead of camera
        float targetX = mainCamera.transform.position.x + spawnAheadDistance;
        
        while (furthestWallX < targetX)
        {
            SpawnWall();
        }
    }

    private void SpawnWall()
    {
        wallsSpawned++;
        float spawnX = furthestWallX + minDistanceBetweenWalls;
        Vector3 spawnPosition = new Vector3(spawnX, 0f, 0f);

        var (gapCenterY, gapSize) = gapGenerator.GenerateGap();

        GameObject wall = Instantiate(wallPrefab, spawnPosition, Quaternion.identity, wallContainer);
        wall.name = "Wall_" + wallsSpawned;

        activeWalls.Add(wall);
        furthestWallX = spawnX;

        WallObstacle wallObstacle = wall.GetComponent<WallObstacle>();
        if (wallObstacle != null)
        {
            wallObstacle.SetupGap(gapCenterY, gapSize, 0f);
        }

        if (showDebugInfo)
        {
            Debug.Log("[SPAWNER] Spawned Wall #" + wallsSpawned + " at X=" + spawnX);
        }
    }

    // Called by other systems - we ignore since we auto-start
    public void StartSpawning(float delay = 0f)
    {
        Debug.Log("[SPAWNER] StartSpawning() called (ignored - using Update)");
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    public void Reset()
    {
        isSpawning = false;
        initialized = false;
        wallsSpawned = 0;
        furthestWallX = -999f;
        activeWalls.Clear();
        startTime = Time.time;

        if (gapGenerator != null)
            gapGenerator.Reset();

        if (wallContainer != null)
        {
            foreach (Transform child in wallContainer)
                Destroy(child.gameObject);
        }
    }

    public string GetStats()
    {
        return "Walls: " + wallsSpawned + " | Active: " + activeWalls.Count;
    }

    void OnGUI()
    {
        if (showDebugInfo)
        {
            string status = initialized ? (isSpawning ? "SPAWNING" : "STOPPED") : "WAITING " + (startDelay - (Time.time - startTime)).ToString("F1") + "s";
            GUILayout.BeginArea(new Rect(10, 80, 350, 60));
            GUILayout.Box("[SPAWNER] " + status + " | " + GetStats());
            GUILayout.EndArea();
        }
    }
}
