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
            Debug.Log($"Found {cachedDisplays.Length} display areas");
        }
        
        InitializePool(customerPrefabs, currentMaxCustomers);
        InitializePool(supportPrefabs, currentMaxSupport);
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

    private void InitializePool(List<GameObject> prefabs, int count)
    {
        if (prefabs == null || prefabs.Count == 0 || count <= 0) return;

        // Khởi tạo queue cho từng prefab
        for (int i = 0; i < prefabs.Count; i++)
        {
            if (!pool.ContainsKey(prefabs[i]))
                pool[prefabs[i]] = new Queue<GameObject>();
        }

        // Phân bổ đều số lượng cho từng prefab
        int countPerPrefab = Mathf.CeilToInt((float)count / prefabs.Count);
        
        for (int i = 0; i < prefabs.Count; i++)
        {
            GameObject prefab = prefabs[i];
            int createCount = Mathf.Min(countPerPrefab, count - i * countPerPrefab);
            
            for (int j = 0; j < createCount; j++)
            {
                GameObject npc = Instantiate(prefab, npcContainer);
                npc.SetActive(false);
                pool[prefab].Enqueue(npc);
            }
            
            Debug.Log($"Created {createCount} instances of {prefab.name}");
        }
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
            Debug.LogWarning("Chưa gán prefab cho nhóm NPC này.");
            return;
        }

        // Chọn spawn point ngẫu nhiên (hoặc dùng entrancePoint nếu không có spawnPoints)
        Vector3 spawnPosition;
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            spawnPosition = spawnPoint.position;
        }
        else if (entrancePoint != null)
        {
            spawnPosition = entrancePoint.position;
        }
        else
        {
            Debug.LogWarning("Chưa gán spawn point hoặc entrance point cho NPCManager.");
            return;
        }

        // Chọn prefab ngẫu nhiên và lấy từ pool
        GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
        GameObject npc = GetFromPool(prefab);
        
        if (npc == null)
        {
            Debug.LogWarning($"Pool hết NPC cho prefab: {prefab.name}. Tạo thêm instance mới.");
            // Tạo thêm instance mới nếu pool hết
            npc = Instantiate(prefab, npcContainer);
            pool[prefab].Enqueue(npc);
        }

        // Reset NPC state trước khi spawn
        NPCBehaviorTree npcBehavior = npc.GetComponent<NPCBehaviorTree>();
        if (npcBehavior != null)
        {
            // Inject displays và entrance vào NPC
            npcBehavior.SetDisplays(cachedDisplays);
            npcBehavior.SetEntrancePoint(entrancePoint);
            
            // Reset state
            npcBehavior.ResetState();
        }

        // place transform
        npc.transform.position = spawnPosition;
        npc.transform.rotation = Quaternion.identity;

        // ensure agent is snapped to NavMesh
        var agent = npc.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            bool wasEnabled = agent.enabled;
            agent.enabled = true;
            if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                agent.Warp(hit.position);
            else
                agent.Warp(spawnPosition);
            agent.enabled = wasEnabled || agent.enabled;
        }

        npc.SetActive(true);
        activeList.Add(npc);
        
        Debug.Log($"Spawned {prefab.name} at {spawnPosition}");
    }

    public void DespawnNPC(GameObject npc)
    {
        if (activeCustomers.Remove(npc) || activeSupport.Remove(npc))
        {
            npc.SetActive(false);
        }
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
        AddToPool(customerPrefabs, newLimit - currentMaxCustomers);
        currentMaxCustomers = newLimit;
    }

    public void UpgradeMaxSupport(int newLimit)
    {
        if (newLimit <= currentMaxSupport) return;
        AddToPool(supportPrefabs, newLimit - currentMaxSupport);
        currentMaxSupport = newLimit;
    }

    private void AddToPool(List<GameObject> prefabs, int addCount)
    {
        if (prefabs == null || prefabs.Count == 0) return;
        
        int countPerPrefab = Mathf.CeilToInt((float)addCount / prefabs.Count);
        
        for (int i = 0; i < prefabs.Count; i++)
        {
            GameObject prefab = prefabs[i];
            int createCount = Mathf.Min(countPerPrefab, addCount - i * countPerPrefab);
            
            for (int j = 0; j < createCount; j++)
            {
                GameObject npc = Instantiate(prefab, npcContainer);
                npc.SetActive(false);
                pool[prefab].Enqueue(npc);
            }
        }
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        if (!pool.ContainsKey(prefab) || pool[prefab].Count == 0) return null;
        
        // Tìm NPC đang inactive trong pool
        int count = pool[prefab].Count;
        for (int i = 0; i < count; i++)
        {
            GameObject npc = pool[prefab].Dequeue();
            pool[prefab].Enqueue(npc);
            if (!npc.activeInHierarchy) return npc;
        }
        return null;
    }
}