using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC")]
[TaskName("Littering Behavior")]
[TaskDescription("NPC vứt rác trong bảo tàng")]
public class NPCLitteringBehavior : Action
{
    private NPCBehaviorTree npcBehavior;
    private Vector3 litterPosition;
    
    [SerializeField] private GameObject trashPrefab; // Prefab của rác
    [SerializeField] private float throwForce = 5f;
    //[SerializeField] private float animationDuration = 1f;
    
    public override void OnAwake()
    {
        npcBehavior = GetComponent<NPCBehaviorTree>();
    }
    
    public override void OnStart()
    {
        // Dừng di chuyển
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        
        // Vứt rác tại vị trí hiện tại
        SpawnTrash();
        
        Debug.Log($"{npcBehavior.NPCName} đã vứt rác!");
    }
    
    public override TaskStatus OnUpdate()
    {
        // Hoàn thành ngay sau khi vứt rác
        return TaskStatus.Success;
    }
    
    private void SpawnTrash()
    {
        if (trashPrefab == null)
        {
            Debug.LogWarning("Chưa gán Trash Prefab!");
            return;
        }
        
        // Tạo rác tại vị trí trước mặt NPC
        Vector3 spawnPosition = transform.position + transform.forward * 1f + Vector3.up * 0.5f;
        GameObject trash = Object.Instantiate(trashPrefab, spawnPosition, Quaternion.identity);
        
        // Thêm lực ném cho rác
        Rigidbody trashRb = trash.GetComponent<Rigidbody>();
        if (trashRb != null)
        {
            Vector3 throwDirection = (transform.forward + Vector3.down).normalized;
            trashRb.linearVelocity = throwDirection * throwForce;
        }
        
        // Phát animation vứt rác
        PlayLitteringAnimation();
    }
    
    private void PlayLitteringAnimation()
    {
        // TODO: Thêm animation vứt rác
        Animator animator = transform.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Littering");
        }
    }
}
