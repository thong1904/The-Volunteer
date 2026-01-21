using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;

[TaskCategory("NPC")]
[TaskName("Littering Behavior")]
[TaskDescription("NPC vứt rác trong bảo tàng")]
public class NPCLitteringBehavior : Action
{
    private NPCBehaviorTree npcBehavior;
    private float lastLitterTime = -999f; // Thời gian vứt rác cuối cùng
    
    [SerializeField] private List<GameObject> trashPrefabs = new List<GameObject>(); // Danh sách các loại rác
    [SerializeField] private Transform trashSpawnPoint; // Transform ở dưới chân NPC để spawn rác
    [SerializeField] private float spawnHeight = 0.5f; // Độ cao spawn nếu không có trashSpawnPoint
    [SerializeField] private float litterCooldown = 5f; // Cooldown giữa các lần vứt rác (giây)
    
    public override void OnAwake()
    {
        npcBehavior = GetComponent<NPCBehaviorTree>();
        
        // Nếu chưa gán trashSpawnPoint, tìm tự động hoặc dùng transform của NPC
        if (trashSpawnPoint == null)
        {
            trashSpawnPoint = transform.Find("TrashSpawnPoint");
        }
    }
    
    public override void OnStart()
    {
        // Kiểm tra cooldown - nếu chưa đủ thời gian thì không vứt rác
        if (Time.time - lastLitterTime < litterCooldown)
        {
            Debug.Log($"{npcBehavior.NPCName} còn trong cooldown vứt rác");
            return;
        }
        
        // Dừng di chuyển
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        
        // Vứt 1 trash duy nhất
        SpawnTrash();
        lastLitterTime = Time.time;
        
        Debug.Log($"{npcBehavior.NPCName} đã vứt rác!");
    }
    
    public override TaskStatus OnUpdate()
    {
        // Hoàn thành ngay sau khi vứt rác
        return TaskStatus.Success;
    }
    
    private void SpawnTrash()
    {
        if (trashPrefabs.Count == 0)
        {
            Debug.LogWarning("Chưa gán Trash Prefabs!");
            return;
        }
        
        // Chọn loại rác ngẫu nhiên từ danh sách
        GameObject randomTrashPrefab = trashPrefabs[Random.Range(0, trashPrefabs.Count)];
        
        // Xác định vị trí spawn
        Vector3 spawnPosition;
        if (trashSpawnPoint != null)
        {
            spawnPosition = trashSpawnPoint.position;
        }
        else
        {
            // Fallback: spawn ở dưới chân NPC nếu không có trashSpawnPoint
            spawnPosition = transform.position + Vector3.up * spawnHeight;
        }
        
        // Tạo rác tại vị trí spawn
        GameObject trash = Object.Instantiate(randomTrashPrefab, spawnPosition, Quaternion.identity);
    }
}
