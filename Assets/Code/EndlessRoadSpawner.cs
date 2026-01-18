using UnityEngine;
using System.Collections.Generic;

public class EndlessRoadSpawner : MonoBehaviour
{
    public Transform player;
    public GameObject roadPrefab;
    public float roadLength = 8f;
    public int roadAhead = 3;
    public int roadBehind = 1;

    private HashSet<Vector2Int> spawnedPositions = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, GameObject> roadDict = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        SpawnInitialRoads();
    }

    void Update()
    {
        Vector2Int playerGrid = WorldToGrid(player.position);
        // Spawn các đoạn đường xung quanh người chơi
        for (int dx = -roadBehind; dx <= roadAhead; dx++)
        {
            for (int dz = -roadBehind; dz <= roadAhead; dz++)
            {
                Vector2Int gridPos = new Vector2Int(playerGrid.x + dx, playerGrid.y + dz);
                if (!spawnedPositions.Contains(gridPos))
                {
                    SpawnRoad(gridPos);
                }
            }
        }
        // Xóa các đoạn đường quá xa
        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var kvp in roadDict)
        {
            if (Mathf.Abs(kvp.Key.x - playerGrid.x) > roadAhead || Mathf.Abs(kvp.Key.y - playerGrid.y) > roadAhead)
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var pos in toRemove)
        {
            roadDict.Remove(pos);
            spawnedPositions.Remove(pos);
        }
    }

    void SpawnInitialRoads()
    {
        Vector2Int playerGrid = WorldToGrid(player.position);
        for (int dx = -roadBehind; dx <= roadAhead; dx++)
        {
            for (int dz = -roadBehind; dz <= roadAhead; dz++)
            {
                Vector2Int gridPos = new Vector2Int(playerGrid.x + dx, playerGrid.y + dz);
                SpawnRoad(gridPos);
            }
        }
    }

    void SpawnRoad(Vector2Int gridPos)
    {
        Vector3 spawnPos = new Vector3(gridPos.x * roadLength, 0, gridPos.y * roadLength);
        GameObject road = Instantiate(roadPrefab, spawnPos, Quaternion.identity);
        spawnedPositions.Add(gridPos);
        roadDict[gridPos] = road;
    }

    Vector2Int WorldToGrid(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x / roadLength);
        int z = Mathf.RoundToInt(pos.z / roadLength);
        return new Vector2Int(x, z);
    }
}
