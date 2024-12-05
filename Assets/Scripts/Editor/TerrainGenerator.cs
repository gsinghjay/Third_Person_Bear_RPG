using UnityEngine;
using UnityEditor;

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

        Vector2 center = new Vector2(width / 2f, height / 2f);
        float villageRadiusInHeightmap = (villageRadius / terrainData.size.x) * width;
        float transitionZoneInHeightmap = (transitionZone / terrainData.size.x) * width;
        Vector2 towerPositionInHeightmap = new Vector2(
            towerPosition.x * width,
            towerPosition.y * height
        );

        // Convert forest positions to heightmap coordinates
        Vector2[] forestPositionsInHeightmap = new Vector2[forestAreas.Length];
        for (int i = 0; i < forestAreas.Length; i++)
        {
            forestPositionsInHeightmap[i] = new Vector2(
                forestAreas[i].position.x * width,
                forestAreas[i].position.y * height
            );
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 currentPoint = new Vector2(x, y);
                float distanceFromCenter = Vector2.Distance(currentPoint, center);

                // Base terrain generation (mountains)
                float xCoord = (float)x / width * noiseScale;
                float yCoord = (float)y / height * noiseScale;
                float perlin = Mathf.PerlinNoise(xCoord, yCoord);
                float mountainValue = perlin * (mountainHeight / terrainData.heightmapResolution) 
                                    + (baseHeight / terrainData.heightmapResolution);

                // Village influence
                float villageValue = villageHeight / terrainData.heightmapResolution;
                float heightValue = mountainValue;

                // Calculate village area
                if (distanceFromCenter < villageRadiusInHeightmap)
                {
                    heightValue = villageValue;
                }
                else if (distanceFromCenter < villageRadiusInHeightmap + transitionZoneInHeightmap)
                {
                    float transitionProgress = (distanceFromCenter - villageRadiusInHeightmap) / transitionZoneInHeightmap;
                    transitionProgress = Mathf.SmoothStep(0, 1, transitionProgress);
                    heightValue = Mathf.Lerp(villageValue, mountainValue, transitionProgress);
                }

                // Calculate forest areas
                for (int i = 0; i < forestAreas.Length; i++)
                {
                    float distanceFromForest = Vector2.Distance(currentPoint, forestPositionsInHeightmap[i]);
                    float forestRadius = (forestAreas[i].radius / terrainData.size.x) * width;
                    float forestValue = forestAreas[i].height / terrainData.heightmapResolution;

                    if (distanceFromForest < forestRadius)
                    {
                        float forestInfluence = 1f - (distanceFromForest / forestRadius);
                        forestInfluence = Mathf.SmoothStep(0, 1, forestInfluence);
                        heightValue = Mathf.Lerp(heightValue, forestValue, forestInfluence);
                    }
                }

                // Calculate regular paths
                float regularPathInfluence = 0f;
                for (int i = 0; i < forestAreas.Length; i++)
                {
                    regularPathInfluence = Mathf.Max(regularPathInfluence, 
                        GetPathInfluence(currentPoint, center, forestPositionsInHeightmap[i]));
                }

                if (regularPathInfluence > 0)
                {
                    float pathHeight = villageValue * 1.1f; // Slightly elevated paths
                    heightValue = Mathf.Lerp(heightValue, pathHeight, regularPathInfluence);
                }

                // Calculate tower area and path
                if (includeTowerPath)
                {
                    float distanceFromTower = Vector2.Distance(currentPoint, towerPositionInHeightmap);
                    float towerRadiusInHeightmap = (towerRadius / terrainData.size.x) * width;
                    float towerValue = towerHeight / terrainData.heightmapResolution;

                    // Tower platform area
                    if (distanceFromTower < towerRadiusInHeightmap)
                    {
                        float towerInfluence = 1f - (distanceFromTower / towerRadiusInHeightmap);
                        towerInfluence = Mathf.SmoothStep(0, 1, towerInfluence);
                        heightValue = Mathf.Lerp(heightValue, towerValue, towerInfluence);
                    }

                    // Elevated path to tower
                    var (influence, heightMultiplier) = GetTowerPathInfluence(
                        currentPoint, 
                        center, 
                        towerPositionInHeightmap,
                        0.5f // Controls how high the path arches
                    );

                    if (influence > 0)
                    {
                        float pathBaseHeight = Mathf.Lerp(
                            villageHeight,
                            towerHeight,
                            heightMultiplier
                        ) / terrainData.heightmapResolution;

                        heightValue = Mathf.Lerp(
                            heightValue,
                            pathBaseHeight,
                            influence
                        );
                    }
                }

                heights[x, y] = heightValue;
            }
        }

        terrainData.SetHeights(0, 0, heights);
    }
}