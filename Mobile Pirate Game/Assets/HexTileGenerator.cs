using UnityEngine;
using System.Collections.Generic;

public class HexTileGenerator : MonoBehaviour
{
    [Header("Tile Prefabs")]
    public GameObject clayPrefab;
    public GameObject coalPrefab;
    public GameObject desertPrefab;
    public GameObject diamondPrefab;
    public GameObject goldPrefab;
    public GameObject ironPrefab;
    public GameObject lakePrefab;
    public GameObject pasturePrefab;
    public GameObject riverPrefab;
    public GameObject stonePrefab;
    public GameObject wastelandPrefab;
    public GameObject waterPrefab;

    [Header("Grid Settings")]
    public int width = 20;
    public int height = 20;
    public float tileSize = 1f;

    private GameObject[,] spawnedTiles;
    private string[,] placedTiles;

    private class TileInfo
    {
        public string name;
        public GameObject prefab;
        public float baseWeight;
    }

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        Debug.Log("Generating Dense Islands on Circular Map...");
        ClearGrid();

        spawnedTiles = new GameObject[width, height];
        placedTiles = new string[width, height];

        float xOffset = tileSize * 0.75f;
        float zOffset = tileSize * Mathf.Sqrt(3f) / 2f;

        Vector2 mapCenter = new Vector2(width / 2f, height / 2f);
        float mapRadius = Mathf.Min(width, height) / 2f;

        // Grid-distributed islands
        List<Vector2> islandCenters = new List<Vector2>();
        List<float> islandRadii = new List<float>();

        int gridX = 2;
        int gridY = 2;
        float spacingX = width / (gridX + 1);
        float spacingY = height / (gridY + 1);

        for (int gx = 1; gx <= gridX; gx++)
        {
            for (int gy = 1; gy <= gridY; gy++)
            {
                Vector2 center = new Vector2(
                    gx * spacingX + Random.Range(-1f, 1f),
                    gy * spacingY + Random.Range(-1f, 1f)
                );
                float radius = Random.Range(4f, 6f);
                islandCenters.Add(center);
                islandRadii.Add(radius);
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 tilePos = new Vector2(x, y);
                float distToMapCenter = Vector2.Distance(tilePos, mapCenter);
                bool inMapCircle = distToMapCenter < mapRadius;
                bool isEdge = (x == 0 || y == 0 || x == width - 1 || y == height - 1);

                if (!inMapCircle || isEdge)
                    continue;

                Vector3 worldPos = new Vector3(x * xOffset, 0, y * zOffset);
                if (x % 2 == 1) worldPos.z += zOffset / 2f;

                bool isInIsland = false;
                float closestDist = float.MaxValue;
                float radius = 0f;

                for (int i = 0; i < islandCenters.Count; i++)
                {
                    float dist = Vector2.Distance(tilePos, islandCenters[i]);
                    if (dist < islandRadii[i])
                    {
                        isInIsland = true;
                        closestDist = dist;
                        radius = islandRadii[i];
                        break;
                    }
                }

                TileInfo chosen;

                if (!isInIsland || isEdge)
                {
                    chosen = new TileInfo { name = "Water", prefab = waterPrefab, baseWeight = 1f };
                }
                else if (closestDist > radius - 1f)
                {
                    // Fade to water near island edge
                    chosen = new TileInfo { name = "Water", prefab = waterPrefab, baseWeight = 1f };
                }
                else
                {
                    chosen = ChooseTile(x, y);
                }

                if (chosen == null || chosen.prefab == null)
                {
                    Debug.LogError($"[HexTileGen] Invalid tile at ({x},{y})");
                    continue;
                }

                placedTiles[x, y] = chosen.name;
                GameObject tile = Instantiate(chosen.prefab, worldPos, Quaternion.identity, transform);
                spawnedTiles[x, y] = tile;
            }
        }

        Debug.Log("Map complete.");
    }

    void ClearGrid()
    {
        foreach (Transform child in transform)
            DestroyImmediate(child.gameObject);
    }

    TileInfo ChooseTile(int x, int y)
    {
        List<TileInfo> tiles = new List<TileInfo>
        {
            new TileInfo { name = "Clay",      prefab = clayPrefab,      baseWeight = 1.0f },
            new TileInfo { name = "Coal",      prefab = coalPrefab,      baseWeight = 0.7f },
            new TileInfo { name = "Desert",    prefab = desertPrefab,    baseWeight = 1.0f },
            new TileInfo { name = "Diamond",   prefab = diamondPrefab,   baseWeight = 0.3f },
            new TileInfo { name = "Gold",      prefab = goldPrefab,      baseWeight = 0.5f },
            new TileInfo { name = "Iron",      prefab = ironPrefab,      baseWeight = 0.8f },
            new TileInfo { name = "Lake",      prefab = lakePrefab,      baseWeight = 0.6f },
            new TileInfo { name = "Pasture",   prefab = pasturePrefab,   baseWeight = 1.2f },
            new TileInfo { name = "River",     prefab = riverPrefab,     baseWeight = 0.7f },
            new TileInfo { name = "Stone",     prefab = stonePrefab,     baseWeight = 1.0f },
            new TileInfo { name = "Wasteland", prefab = wastelandPrefab, baseWeight = 0.8f },
            new TileInfo { name = "Water",     prefab = waterPrefab,     baseWeight = 0.8f },
        };

        Dictionary<TileInfo, float> weights = new Dictionary<TileInfo, float>();
        foreach (var tile in tiles)
            weights[tile] = tile.baseWeight;

        string left = x > 0 ? placedTiles[x - 1, y] : null;
        string down = y > 0 ? placedTiles[x, y - 1] : null;

        ApplyNeighborBias(weights, left);
        ApplyNeighborBias(weights, down);

        return WeightedRandom(weights);
    }

    void ApplyNeighborBias(Dictionary<TileInfo, float> weights, string neighbor)
    {
        if (string.IsNullOrEmpty(neighbor)) return;

        List<TileInfo> keys = new List<TileInfo>(weights.Keys);

        foreach (var tile in keys)
        {
            string t = tile.name;

            if (neighbor == "Water" || neighbor == "River")
            {
                if (t == "Water" || t == "River" || t == "Lake" || t == "Pasture")
                    weights[tile] *= 1.6f;
                if (t == "Desert" || t == "Wasteland")
                    weights[tile] *= 0.5f;
            }
            else if (neighbor == "Stone")
            {
                if (t == "Iron" || t == "Gold" || t == "Diamond" || t == "Coal")
                    weights[tile] *= 1.5f;
            }
            else if (neighbor == "Desert")
            {
                if (t == "Clay" || t == "Wasteland")
                    weights[tile] *= 1.4f;
                if (t == "Water")
                    weights[tile] *= 0.4f;
            }
            else if (neighbor == "Pasture")
            {
                if (t == "Lake" || t == "River" || t == "Water")
                    weights[tile] *= 1.2f;
            }
        }
    }

    TileInfo WeightedRandom(Dictionary<TileInfo, float> weights)
    {
        float total = 0f;
        foreach (var w in weights.Values)
            total += w;

        if (total <= 0f)
        {
            Debug.LogError("[HexTileGen] Total weight is zero — no valid tile options!");
            return null;
        }

        float roll = Random.Range(0, total);
        float sum = 0f;

        foreach (var kvp in weights)
        {
            sum += kvp.Value;
            if (roll <= sum)
                return kvp.Key;
        }

        return null;
    }
}
