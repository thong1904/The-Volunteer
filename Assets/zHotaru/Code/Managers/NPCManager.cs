using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

/// <summary>
/// Quản lý spawn, pool và active NPCs (customer + optional support)
/// </summary>
public class NPCManager : MonoBehaviour
{
    [Header("NPC Settings")]
    [SerializeField] private List<GameObject> customerPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> supportPrefabs = new List<GameObject>(); // để trống nếu chưa dùng
    [SerializeField] private Transform npcContainer;
    [SerializeField] private Transform entrancePoint;
    
    [Header("Spawn Points")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>(); // Các điểm spawn (nếu trống sẽ dùng entrancePoint)
    
    [Header("Display Discovery")]
    [SerializeField] private bool autoFindDisplays = true;
    private Transform[] cachedDisplays;

    [Header("Spawn Settings")]
    [SerializeField] private float customerSpawnInterval = 10f;
    [SerializeField] private float supportSpawnInterval = 15f;
    [SerializeField] private int initialMaxCustomers = 5;
    [SerializeField] private int initialMaxSupport = 0;      // 0 nếu chưa bật support
    [SerializeField] private bool enableSupportSpawn = false;

    private readonly Dictionary<GameObject, Queue<GameObject>> pool = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly List<GameObject> activeCustomers = new List<GameObject>();
    private readonly List<GameObject> activeSupport = new List<GameObject>();

    private int currentMaxCustomers;
    private int currentMaxSupport;
    private float nextCustomerSpawnTime;
    private float nextSupportSpawnTime;
    private bool spawnCustomers;
    private bool spawnSupport;

    public int ActiveCustomerCount => activeCustomers.Count;
    public int ActiveSupportCount => activeSupport.Count;
    public int MaxCustomers => currentMaxCustomers;
    public int MaxSupport => currentMaxSupport;

    void Awake()
    {
        currentMaxCustomers = initialMaxCustomers;
        currentMaxSupport = initialMaxSupport;
        
        // Tìm tất cả DisplayArea trong scene
        if (autoFindDisplays)
        {
            DisplayArea[] displays = FindObjectsByType<DisplayArea>(FindObjectsSortMode.None);
            cachedDisplays = new Transform[displays.Length];
            for (int i = 0; i < displays.Length; i++)
            {
                cachedDisplays[i] = displays[i].transform;
            }
            Debug.Log($"[NPCManager] Found {cachedDisplays.Length} display areas");
        }
        
        // Không cần khởi tạo pool sớm - dùng lazy pooling
        // NPC sẽ được tạo khi spawn và trở thành pool khi despawn
    }

    void Update()
    {
        if (spawnCustomers && Time.time >= nextCustomerSpawnTime && activeCustomers.Count < currentMaxCustomers)
        {
            SpawnCustomer();
            nextCustomerSpawnTime = Time.time + customerSpawnInterval;
        }

        if (enableSupportSpawn && spawnSupport && Time.time >= nextSupportSpawnTime && activeSupport.Count < currentMaxSupport)
        {
            SpawnSupport();
            nextSupportSpawnTime = Time.time + supportSpawnInterval;
        }
    }
    
    /// <summary>
    /// Lấy vị trí spawn ngẫu nhiên từ spawnPoints hoặc entrancePoint
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            return spawnPoint != null ? spawnPoint.position : Vector3.zero;
        }
        else if (entrancePoint != null)
        {
            return entrancePoint.position;
        }
        return Vector3.zero;
    }

    public void StartCustomerSpawning()
    {
        spawnCustomers = true;
        nextCustomerSpawnTime = Time.time + customerSpawnInterval;
    }

    public void StopCustomerSpawning() => spawnCustomers = false;

    public void StartSupportSpawning()
    {
        if (!enableSupportSpawn) return;
        spawnSupport = true;
        nextSupportSpawnTime = Time.time + supportSpawnInterval;
    }

    public void StopSupportSpawning() => spawnSupport = false;

    public void SpawnCustomer() => SpawnFromList(customerPrefabs, activeCustomers);

    public void SpawnSupport() => SpawnFromList(supportPrefabs, activeSupport);

    private void SpawnFromList(List<GameObject> prefabs, List<GameObject> activeList)
    {
        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogWarning("[NPCManager] Chưa gán prefab cho nhóm NPC này.");
            return;
        }

        // Chọn spawn point ngẫu nhiên
        Vector3 spawnPosition = GetRandomSpawnPosition();
        if (spawnPosition == Vector3.zero && entrancePoint == null && (spawnPoints == null || spawnPoints.Count == 0))
        {
            Debug.LogWarning("[NPCManager] Chưa gán spawn point hoặc entrance point.");
            return;
        }

        // Chọn prefab ngẫu nhiên
        GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
        
        // Thử lấy từ pool (NPC đã despawn trước đó)
        GameObject npc = GetFromPool(prefab);
        
        if (npc == null)
        {
            // Không có trong pool → tạo mới
            npc = Instantiate(prefab, spawnPosition, Quaternion.identity, npcContainer);
            Debug.Log($"[NPCManager] Tạo mới {npc.name}");
        }
        else
        {
            Debug.Log($"[NPCManager] Tái sử dụng {npc.name} từ pool");
        }

        // Reset NPC state trước khi spawn
        NPCBehaviorTree npcBehavior = npc.GetComponent<NPCBehaviorTree>();
        if (npcBehavior != null)
        {
            npcBehavior.SetDisplays(cachedDisplays);
            npcBehavior.SetEntrancePoint(entrancePoint);
            npcBehavior.ResetState();
        }

        // Đặt vị trí spawn
        npc.transform.position = spawnPosition;
        npc.transform.rotation = Quaternion.identity;

        // Ensure agent is snapped to NavMesh
        var agent = npc.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = true;
            if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                agent.Warp(hit.position);
            else
                agent.Warp(spawnPosition);
        }

        npc.SetActive(true);
        activeList.Add(npc);
        
        Debug.Log($"[NPCManager] Spawned {npc.name} at {spawnPosition}");
    }

    public void DespawnNPC(GameObject npc)
    {
        if (activeCustomers.Remove(npc) || activeSupport.Remove(npc))
        {
            npc.SetActive(false);
            
            // Thêm vào pool để tái sử dụng sau
            // Tìm prefab gốc dựa trên tên (loại bỏ "(Clone)")
            string npcName = npc.name.Replace("(Clone)", "").Trim();
            GameObject matchingPrefab = FindMatchingPrefab(npcName);
            
            if (matchingPrefab != null)
            {
                if (!pool.ContainsKey(matchingPrefab))
                    pool[matchingPrefab] = new Queue<GameObject>();
                    
                pool[matchingPrefab].Enqueue(npc);
                Debug.Log($"[NPCManager] {npc.name} đã được thêm vào pool");
            }
        }
    }
    
    /// <summary>
    /// Tìm prefab phù hợp dựa trên tên
    /// </summary>
    private GameObject FindMatchingPrefab(string npcName)
    {
        foreach (var prefab in customerPrefabs)
        {
            if (prefab != null && prefab.name == npcName)
                return prefab;
        }
        foreach (var prefab in supportPrefabs)
        {
            if (prefab != null && prefab.name == npcName)
                return prefab;
        }
        return null;
    }

    public void DespawnAllNPCs()
    {
        foreach (var npc in new List<GameObject>(activeCustomers))
            DespawnNPC(npc);
        foreach (var npc in new List<GameObject>(activeSupport))
            DespawnNPC(npc);
    }

    public void UpgradeMaxCustomers(int newLimit)
    {
        if (newLimit <= currentMaxCustomers) return;
        currentMaxCustomers = newLimit;
        Debug.Log($"[NPCManager] Max customers upgraded to {currentMaxCustomers}");
    }

    public void UpgradeMaxSupport(int newLimit)
    {
        if (newLimit <= currentMaxSupport) return;
        currentMaxSupport = newLimit;
        Debug.Log($"[NPCManager] Max support upgraded to {currentMaxSupport}");
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        if (!pool.ContainsKey(prefab) || pool[prefab].Count == 0)
            return null;
        
        // Lấy NPC từ pool (đã inactive sẵn từ despawn)
        return pool[prefab].Dequeue();
    }
}