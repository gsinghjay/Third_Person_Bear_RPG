using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TerrainGenerator : EditorWindow
{
    private Terrain terrain;
    private float baseHeight = 20f;
    private float mountainHeight = 100f;
    private float noiseScale = 50f;
    private float villageRadius = 100f;
    private float transitionZone = 50f;
    private float villageHeight = 30f;

    // Forest and path parameters
    [System.Serializable]
    private class ForestArea
    {
        public Vector2 position; // Normalized position (0-1)
        public float radius = 75f;
        public float height = 35f;
    }

    private ForestArea[] forestAreas = new ForestArea[3];
    private float pathWidth = 15f;
    private float pathSmoothness = 20f;

    // Victory tower parameters
    private Vector2 towerPosition = new Vector2(0.5f, 0.1f); // Top center
    private float towerRadius = 50f;
    private float towerHeight = 80f;
    private float towerPathWidth = 20f; // Wider than normal paths
    private bool includeTowerPath = true; // Toggle for testing

    [System.Serializable]
    private class ArenaSettings
    {
        public string name;
        public float radius = 30f;
        public float flatness = 0.9f;
        public Vector2 position;
        public int normalBearCount = 2;
        public int specialBearCount = 2;
        public BearType specialBearType;
    }

    private enum BearType
    {
        Normal,
        Fire,
        Ice
    }

    [SerializeField] private ArenaSettings[] arenas = new ArenaSettings[3];
    private Vector2[] spawnPoints;

    [MenuItem("Tools/Terrain Generator")]
    public static void ShowWindow()
    {
        GetWindow<TerrainGenerator>("Terrain Generator");
    }

    private void OnEnable()
    {
        // Initialize forest areas in different directions
        forestAreas[0] = new ForestArea { position = new Vector2(0.2f, 0.2f) }; // Northwest forest
        forestAreas[1] = new ForestArea { position = new Vector2(0.8f, 0.3f) }; // Northeast forest
        forestAreas[2] = new ForestArea { position = new Vector2(0.5f, 0.8f) }; // South forest

        // Simplified arena settings
        arenas[0] = new ArenaSettings
        {
            name = "Northwest Arena - Fire",
            position = new Vector2(0.25f, 0.25f),
            radius = 35f,
            flatness = 0.85f,
            normalBearCount = 2,
            specialBearCount = 2,
            specialBearType = BearType.Fire
        };

        arenas[1] = new ArenaSettings
        {
            name = "Northeast Arena - Ice",
            position = new Vector2(0.75f, 0.25f),
            radius = 40f,
            flatness = 0.9f,
            normalBearCount = 1,
            specialBearCount = 3,
            specialBearType = BearType.Ice
        };

        arenas[2] = new ArenaSettings
        {
            name = "South Arena - Boss",
            position = new Vector2(0.5f, 0.75f),
            radius = 50f,
            flatness = 0.95f,
            normalBearCount = 2,
            specialBearCount = 4,
            specialBearType = BearType.Normal
        };
    }

    private void OnGUI()
    {
        terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);
        
        EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
        baseHeight = EditorGUILayout.FloatField("Base Height", baseHeight);
        mountainHeight = EditorGUILayout.FloatField("Mountain Height", mountainHeight);
        noiseScale = EditorGUILayout.FloatField("Noise Scale", noiseScale);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Village Settings", EditorStyles.boldLabel);
        villageRadius = EditorGUILayout.FloatField("Village Radius", villageRadius);
        transitionZone = EditorGUILayout.FloatField("Transition Zone", transitionZone);
        villageHeight = EditorGUILayout.FloatField("Village Height", villageHeight);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Path Settings", EditorStyles.boldLabel);
        pathWidth = EditorGUILayout.FloatField("Path Width", pathWidth);
        pathSmoothness = EditorGUILayout.FloatField("Path Smoothness", pathSmoothness);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Forest Areas", EditorStyles.boldLabel);
        for (int i = 0; i < forestAreas.Length; i++)
        {
            EditorGUILayout.LabelField($"Forest {i + 1}", EditorStyles.boldLabel);
            forestAreas[i].position = EditorGUILayout.Vector2Field("Position", forestAreas[i].position);
            forestAreas[i].radius = EditorGUILayout.FloatField("Radius", forestAreas[i].radius);
            forestAreas[i].height = EditorGUILayout.FloatField("Height", forestAreas[i].height);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Victory Tower Settings", EditorStyles.boldLabel);
        includeTowerPath = EditorGUILayout.Toggle("Include Tower Path", includeTowerPath);
        towerPosition = EditorGUILayout.Vector2Field("Tower Position", towerPosition);
        towerRadius = EditorGUILayout.FloatField("Tower Area Radius", towerRadius);
        towerHeight = EditorGUILayout.FloatField("Tower Height", towerHeight);
        towerPathWidth = EditorGUILayout.FloatField("Tower Path Width", towerPathWidth);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Arena Settings", EditorStyles.boldLabel);
        
        foreach (var arena in arenas)
        {
            EditorGUILayout.LabelField(arena.name, EditorStyles.boldLabel);
            arena.radius = EditorGUILayout.FloatField("Arena Radius", arena.radius);
            arena.flatness = EditorGUILayout.Slider("Flatness", arena.flatness, 0f, 1f);
            arena.normalBearCount = EditorGUILayout.IntField("Normal Bears", arena.normalBearCount);
            arena.specialBearCount = EditorGUILayout.IntField("Special Bears", arena.specialBearCount);
            EditorGUILayout.Space(5);
        }

        if (GUILayout.Button("Generate Terrain"))
        {
            GenerateTerrain();
        }
    }

    private float GetPathInfluence(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 startToEnd = end - start;
        Vector2 startToPoint = point - start;
        float dot = Vector2.Dot(startToEnd, startToPoint);
        float t = Mathf.Clamp01(dot / startToEnd.sqrMagnitude);
        Vector2 projection = start + t * startToEnd;
        float distance = Vector2.Distance(point, projection);
        return 1f - Mathf.Clamp01(distance / pathWidth);
    }

    private (float influence, float heightMultiplier) GetTowerPathInfluence(Vector2 point, Vector2 start, Vector2 end, float progress)
    {
        Vector2 startToEnd = end - start;
        Vector2 startToPoint = point - start;
        float dot = Vector2.Dot(startToEnd, startToPoint);
        float t = Mathf.Clamp01(dot / startToEnd.sqrMagnitude);
        Vector2 projection = start + t * startToEnd;
        float distance = Vector2.Distance(point, projection);
        
        float influence = 1f - Mathf.Clamp01(distance / towerPathWidth);
        float heightMultiplier = Mathf.Sin(t * Mathf.PI) * progress;
        
        return (influence, heightMultiplier);
    }

    private void GenerateTerrain()
    {
        if (terrain == null) return;

        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;
        float[,] heights = new float[width, height];

        // Create base terrain with Perlin noise
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = (float)x / width * noiseScale;
                float yCoord = (float)y / height * noiseScale;
                
                float perlin = Mathf.PerlinNoise(xCoord, yCoord);
                float additionalNoise = Mathf.PerlinNoise(xCoord * 2, yCoord * 2) * 0.5f;
                
                // Create village area in the center
                Vector2 villageCenter = new Vector2(width * 0.5f, height * 0.5f);
                float distanceFromCenter = Vector2.Distance(new Vector2(x, y), villageCenter);
                float villageInfluence = 1f - Mathf.Clamp01(distanceFromCenter / villageRadius);
                
                float transitionInfluence = 1f - Mathf.Clamp01((distanceFromCenter - villageRadius) / transitionZone);
                
                // Calculate path influence
                float pathInfluence = 0f;
                foreach (var arena in arenas)
                {
                    Vector2 arenaPos = new Vector2(arena.position.x * width, arena.position.y * height);
                    Vector2 currentPos = new Vector2(x, y);
                    pathInfluence = Mathf.Max(pathInfluence, GetPathInfluence(currentPos, villageCenter, arenaPos));
                }

                // Combine all influences
                float finalHeight = perlin * 0.7f + additionalNoise * 0.3f;
                finalHeight *= (1f - villageInfluence);
                finalHeight = Mathf.Lerp(finalHeight, villageHeight / mountainHeight, villageInfluence);
                finalHeight = Mathf.Lerp(finalHeight, villageHeight / mountainHeight, pathInfluence);
                
                // Add tower influence
                if (includeTowerPath)
                {
                    Vector2 towerPos = new Vector2(towerPosition.x * width, towerPosition.y * height);
                    Vector2 currentPos = new Vector2(x, y);
                    
                    // Calculate distance to tower area
                    float distanceToTower = Vector2.Distance(currentPos, towerPos);
                    if (distanceToTower < towerRadius)
                    {
                        float towerInfluence = 1f - (distanceToTower / towerRadius);
                        finalHeight = Mathf.Lerp(finalHeight, towerHeight / mountainHeight, towerInfluence);
                    }

                    // Add tower path
                    var (towerPathInfluence, heightMult) = GetTowerPathInfluence(currentPos, villageCenter, towerPos, 0.3f);
                    finalHeight = Mathf.Lerp(finalHeight, (villageHeight + heightMult * 50f) / mountainHeight, towerPathInfluence);
                }

                heights[x, y] = finalHeight * (mountainHeight / terrainData.heightmapResolution) + 
                               (baseHeight / terrainData.heightmapResolution);
            }
        }

        // Add combat arenas
        GenerateCombatArenas(heights, width, height);

        terrainData.SetHeights(0, 0, heights);
    }

    private float CreateClearings(int x, int y, int width, int height)
    {
        // Create 3 clearing areas for combat
        Vector2[] clearings = new Vector2[] {
            new Vector2(width * 0.25f, height * 0.25f),  // Northwest clearing
            new Vector2(width * 0.75f, height * 0.25f),  // Northeast clearing
            new Vector2(width * 0.5f, height * 0.75f)    // South clearing
        };

        float clearingInfluence = 1f;
        float clearingRadius = width * 0.1f; // 10% of terrain width

        foreach (Vector2 clearing in clearings)
        {
            float distance = Vector2.Distance(new Vector2(x, y), clearing);
            if (distance < clearingRadius)
            {
                float influence = distance / clearingRadius;
                clearingInfluence = Mathf.Min(clearingInfluence, influence);
            }
        }

        return Mathf.Lerp(0.7f, 1f, clearingInfluence); // Flatten clearings to 70%
    }

    private void GenerateCombatArenas(float[,] heights, int width, int height)
    {
        foreach (var arena in arenas)
        {
            GenerateArena(heights, width, height, arena);
            GenerateSpawnPoints(arena);
        }
    }

    private void GenerateArena(float[,] heights, int width, int height, ArenaSettings arena)
    {
        int centerX = Mathf.RoundToInt(arena.position.x * width);
        int centerY = Mathf.RoundToInt(arena.position.y * height);
        float radius = arena.radius;

        for (int x = -Mathf.RoundToInt(radius); x <= radius; x++)
        {
            for (int y = -Mathf.RoundToInt(radius); y <= radius; y++)
            {
                int currentX = centerX + x;
                int currentY = centerY + y;

                if (currentX < 0 || currentX >= width || currentY < 0 || currentY >= height)
                    continue;

                float distanceFromCenter = Mathf.Sqrt(x * x + y * y);
                if (distanceFromCenter <= radius)
                {
                    // Only flatten the arena center for combat
                    float flattenStrength = 1 - (distanceFromCenter / radius);
                    heights[currentX, currentY] *= (1 - (flattenStrength * arena.flatness));
                }
            }
        }
    }

    private void GenerateSpawnPoints(ArenaSettings arena)
    {
        List<Vector2> points = new List<Vector2>();
        float radius = arena.radius;
        
        // Generate spawn points in a circle pattern
        for (int i = 0; i < arena.normalBearCount + arena.specialBearCount; i++)
        {
            float angle = (360f / (arena.normalBearCount + arena.specialBearCount)) * i;
            float spawnRadius = radius * 0.6f; // Spawn at 60% of arena radius
            
            float x = arena.position.x + (Mathf.Cos(angle * Mathf.Deg2Rad) * spawnRadius);
            float y = arena.position.y + (Mathf.Sin(angle * Mathf.Deg2Rad) * spawnRadius);
            
            points.Add(new Vector2(x, y));
        }

        // Store spawn points for later use (e.g., actual bear spawning)
        spawnPoints = points.ToArray();
    }
}