using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC")]
[TaskName("Move To Target (NavMesh)")]
[TaskDescription("Di chuyển NPC tới vị trí đích sử dụng NavMesh")]
public class NPCMoveToTargetNavMesh : Action
{
    private NPCBehaviorTree npcBehavior;
    private NavMeshAgent navMeshAgent;
    private Vector3 targetPosition;
    
    [SerializeField] private float stoppingDistance = 0.5f;
    [SerializeField] private string walkAnimationName = "Walk";
    
    public override void OnAwake()
    {
        npcBehavior = GetComponent<NPCBehaviorTree>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        
        if (navMeshAgent == null)
        {
            Debug.LogError($"{gameObject.name} không có NavMeshAgent! Thêm Component NavMeshAgent.");
        }
        else
        {
            // Tối ưu NavMeshAgent để NPC có thể xuyên qua nhau khi cần
            navMeshAgent.avoidancePriority = Random.Range(0, 32); // Ưu tiên ngẫu nhiên để tránh deadlock
        }
    }
    
    public override void OnStart()
    {
        // Kích hoạt NavMeshAgent
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = true;
        }
        
        // Phát animation walk
        npcBehavior.PlayAnimation(walkAnimationName);
    }
    
    public override TaskStatus OnUpdate()
    {
        if (npcBehavior == null || navMeshAgent == null)
            return TaskStatus.Failure;
        
        if (!navMeshAgent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 3f, NavMesh.AllAreas))
            {
                navMeshAgent.Warp(hit.position);
            }
            else
            {
                Debug.LogWarning($"{gameObject.name} không nằm trên NavMesh!");
                return TaskStatus.Failure;
            }
        }
        
        targetPosition = npcBehavior.CurrentTarget;
        
        // Đặt đích cho NavMeshAgent
        if (navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.SetDestination(targetPosition);
        }
        
        // Kiểm tra xem đã đến đích hay chưa
        if (!navMeshAgent.pathPending)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            
            // Nếu đã đến đích
            if (distanceToTarget < stoppingDistance)
            {
                navMeshAgent.velocity = Vector3.zero;
                navMeshAgent.ResetPath();
                return TaskStatus.Success;
            }
        }
        
        // Còn đang di chuyển
        return TaskStatus.Running;
    }
    
    public override void OnEnd()
    {
        // Dừng di chuyển
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.ResetPath();
        }
    }
}
