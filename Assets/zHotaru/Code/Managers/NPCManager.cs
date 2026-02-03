using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System;

/// <summary>
/// Quản lý spawn, pool và active NPCs (customer + optional support)
/// </summary>
public class NPCManager : MonoBehaviour
{
    [Header("NPC Settings")]
    [SerializeField] private List<GameObject> customerPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> supportPrefabs = new List<GameObject>();
    [SerializeField] private Transform npcContainer;
    [SerializeField] private Transform entrancePoint;
    
    [Header("Spawn Points")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    
    [Header("Display Discovery")]
    [SerializeField] private bool autoFindDisplays = true;
    private Transform[] cachedDisplays;

    [Header("Spawn Settings")]
    [SerializeField] private float customerSpawnInterval = 10f;
    [SerializeField] private float supportSpawnInterval = 15f;
    [SerializeField] private int initialMaxCustomers = 5;
    [SerializeField] private int initialMaxSupport = 0;
    [SerializeField] private bool enableSupportSpawn = false;
    
    [Header("Day End Settings")]
    [SerializeField] private int totalNPCsPerDay = 10; // Tổng số NPC sẽ spawn trong ngày
    
    private readonly Dictionary<GameObject, Queue<GameObject>> pool = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly List<GameObject> activeCustomers = new List<GameObject>();
    private readonly List<GameObject> activeSupport = new List<GameObject>();

    private int currentMaxCustomers;
    private int currentMaxSupport;
    private float nextCustomerSpawnTime;
    private float nextSupportSpawnTime;
    private bool spawnCustomers;
    private bool spawnSupport;
    
    private int spawnedNPCCount; // Số NPC đã spawn trong ngày
    private int despawnedNPCCount; // Số NPC đã rời đi trong ngày

    public int ActiveCustomerCount => activeCustomers.Count;
    public int ActiveSupportCount => activeSupport.Count;
    public int MaxCustomers => currentMaxCustomers;
    public int MaxSupport => currentMaxSupport;
    public int TotalNPCsPerDay => totalNPCsPerDay;
    public int SpawnedNPCCount => spawnedNPCCount;
    public int DespawnedNPCCount => despawnedNPCCount;
    
    // Event khi tất cả NPC đã rời đi (kết thúc ngày)
    public event Action OnAllNPCsLeft;
    public event Action<int, int> OnNPCCountChanged; // (spawned, despawned)

    void Awake()
    {
        currentMaxCustomers = initialMaxCustomers;
        currentMaxSupport = initialMaxSupport;
        
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
    }

    void Update()
    {
        // Chỉ spawn nếu chưa đủ số NPC trong ngày
        if (spawnCustomers && 
            Time.time >= nextCustomerSpawnTime && 
            activeCustomers.Count < currentMaxCustomers &&
            spawnedNPCCount < totalNPCsPerDay)
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
    
    private Vector3 GetRandomSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
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

    public void SpawnCustomer() => SpawnFromList(customerPrefabs, activeCustomers, true);

    public void SpawnSupport() => SpawnFromList(supportPrefabs, activeSupport, false);

    private void SpawnFromList(List<GameObject> prefabs, List<GameObject> activeList, bool isCustomer)
    {
        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogWarning("[NPCManager] Chưa gán prefab cho nhóm NPC này.");
            return;
        }

        Vector3 spawnPosition = GetRandomSpawnPosition();
        if (spawnPosition == Vector3.zero && entrancePoint == null && (spawnPoints == null || spawnPoints.Count == 0))
        {
            Debug.LogWarning("[NPCManager] Chưa gán spawn point hoặc entrance point.");
            return;
        }

        GameObject prefab = prefabs[UnityEngine.Random.Range(0, prefabs.Count)];
        
        GameObject npc = GetFromPool(prefab);
        
        if (npc == null)
        {
            npc = Instantiate(prefab, spawnPosition, Quaternion.identity, npcContainer);
            Debug.Log($"[NPCManager] Tạo mới {npc.name}");
        }
        else
        {
            Debug.Log($"[NPCManager] Tái sử dụng {npc.name} từ pool");
        }

        NPCBehaviorTree npcBehavior = npc.GetComponent<NPCBehaviorTree>();
        if (npcBehavior != null)
        {
            npcBehavior.SetDisplays(cachedDisplays);
            npcBehavior.SetEntrancePoint(entrancePoint);
            npcBehavior.ResetState();
        }

        npc.transform.position = spawnPosition;
        npc.transform.rotation = Quaternion.identity;

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
        
        // Track spawned count cho customer
        if (isCustomer)
        {
            spawnedNPCCount++;
            OnNPCCountChanged?.Invoke(spawnedNPCCount, despawnedNPCCount);
            Debug.Log($"[NPCManager] Spawned {npc.name} ({spawnedNPCCount}/{totalNPCsPerDay})");
        }
    }

    public void DespawnNPC(GameObject npc)
    {
        bool wasCustomer = activeCustomers.Contains(npc);
        
        if (activeCustomers.Remove(npc) || activeSupport.Remove(npc))
        {
            npc.SetActive(false);
            
            string npcName = npc.name.Replace("(Clone)", "").Trim();
            GameObject matchingPrefab = FindMatchingPrefab(npcName);
            
            if (matchingPrefab != null)
            {
                if (!pool.ContainsKey(matchingPrefab))
                    pool[matchingPrefab] = new Queue<GameObject>();
                    
                pool[matchingPrefab].Enqueue(npc);
                Debug.Log($"[NPCManager] {npc.name} đã được thêm vào pool");
            }
            
            // Track despawned count cho customer
            if (wasCustomer)
            {
                despawnedNPCCount++;
                OnNPCCountChanged?.Invoke(spawnedNPCCount, despawnedNPCCount);
                Debug.Log($"[NPCManager] NPC rời đi ({despawnedNPCCount}/{spawnedNPCCount} đã spawn)");
                
                // Kiểm tra điều kiện kết thúc ngày:
                // - Đã spawn đủ số NPC trong ngày
                // - Tất cả NPC đã rời đi (không còn active)
                CheckDayEnd();
            }
        }
    }
    
    /// <summary>
    /// Kiểm tra xem có nên kết thúc ngày không
    /// </summary>
    private void CheckDayEnd()
    {
        // Điều kiện kết thúc: spawn đủ số lượng VÀ tất cả đã rời đi
        if (spawnedNPCCount >= totalNPCsPerDay && activeCustomers.Count == 0)
        {
            Debug.Log($"[NPCManager] ✅ Tất cả {totalNPCsPerDay} NPC đã rời đi!");
            OnAllNPCsLeft?.Invoke();
        }
    }
    
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
    
    /// <summary>
    /// Reset counters cho ngày mới (được gọi từ GameManager.StartNewDay)
    /// </summary>
    public void ResetDayCounters()
    {
        spawnedNPCCount = 0;
        despawnedNPCCount = 0;
        OnNPCCountChanged?.Invoke(spawnedNPCCount, despawnedNPCCount);
        Debug.Log("[NPCManager] Reset counters cho ngày mới");
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
    
    public void SetTotalNPCsPerDay(int count)
    {
        totalNPCsPerDay = count;
        Debug.Log($"[NPCManager] Total NPCs per day set to {totalNPCsPerDay}");
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        if (!pool.ContainsKey(prefab) || pool[prefab].Count == 0)
            return null;
        
        return pool[prefab].Dequeue();
    }
}