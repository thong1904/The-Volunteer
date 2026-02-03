using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC")]
[TaskName("Leave Museum")]
[TaskDescription("NPC rời bỏ bảo tàng sử dụng NavMesh")]
public class NPCLeaveMuseum : Action
{
    private NPCBehaviorTree npcBehavior;
    private NavMeshAgent navMeshAgent;
    private Vector3 targetPosition;
    private bool pathValid = false;
    
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
            navMeshAgent.avoidancePriority = Random.Range(0, 32);
        }
    }
    
    public override void OnStart()
    {
        pathValid = false;
        npcBehavior.SetExiting();
        
        // Kích hoạt và cấu hình NavMeshAgent
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = true;
            navMeshAgent.isStopped = false;
            navMeshAgent.updateRotation = true;
            navMeshAgent.updatePosition = true;
            
            // Đảm bảo NPC ở trên NavMesh
            if (!navMeshAgent.isOnNavMesh)
            {
                TryWarpToNavMesh();
            }
        }
        
        // Phát animation walk
        npcBehavior.PlayAnimation(walkAnimationName);
        
        Debug.Log($"[NPC] {npcBehavior.NPCName}: Bắt đầu rời bảo tàng, target: {npcBehavior.CurrentTarget}");
    }
    
    private bool TryWarpToNavMesh()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 3f, NavMesh.AllAreas))
        {
            navMeshAgent.Warp(hit.position);
            Debug.Log($"[NPC] {npcBehavior.NPCName}: Warp lên NavMesh tại {hit.position}");
            return true;
        }
        return false;
    }
    
    public override TaskStatus OnUpdate()
    {
        if (npcBehavior == null || navMeshAgent == null)
            return TaskStatus.Failure;
        
        // Kiểm tra NPC có trên NavMesh không
        if (!navMeshAgent.isOnNavMesh)
        {
            if (!TryWarpToNavMesh())
            {
                Debug.LogWarning($"[NPC] {gameObject.name}: Không nằm trên NavMesh!");
                return TaskStatus.Failure;
            }
        }
        
        targetPosition = npcBehavior.CurrentTarget;
        
        // Kiểm tra đã đến đích (target rất gần)
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        if (distanceToTarget < stoppingDistance)
        {
            Debug.Log($"[NPC] {npcBehavior.NPCName}: Đã đến entrance và despawn");
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.ResetPath();
            npcBehavior.DespawnNPC();
            return TaskStatus.Success;
        }
        
        // Đặt đích cho NavMeshAgent nếu chưa có path hợp lệ
        if (navMeshAgent.isActiveAndEnabled && !pathValid)
        {
            // Sample target position lên NavMesh
            NavMeshHit targetHit;
            Vector3 validTarget = targetPosition;
            if (NavMesh.SamplePosition(targetPosition, out targetHit, 5f, NavMesh.AllAreas))
            {
                validTarget = targetHit.position;
            }
            
            NavMeshPath path = new NavMeshPath();
            if (navMeshAgent.CalculatePath(validTarget, path))
            {
                if (path.status == NavMeshPathStatus.PathComplete || path.status == NavMeshPathStatus.PathPartial)
                {
                    navMeshAgent.SetPath(path);
                    pathValid = true;
                    Debug.Log($"[NPC] {npcBehavior.NPCName}: Bắt đầu di chuyển đến entrance {validTarget}");
                }
                else
                {
                    Debug.LogWarning($"[NPC] {npcBehavior.NPCName}: Không tìm được đường đến entrance!");
                    // Vẫn thử SetDestination trực tiếp
                    navMeshAgent.SetDestination(validTarget);
                    pathValid = true;
                }
            }
            else
            {
                // Fallback: SetDestination trực tiếp
                navMeshAgent.SetDestination(validTarget);
                pathValid = true;
            }
        }
        
        // Kiểm tra xem đã đến đích hay chưa
        if (!navMeshAgent.pathPending && pathValid)
        {
            if (navMeshAgent.remainingDistance <= stoppingDistance && navMeshAgent.velocity.sqrMagnitude < 0.01f)
            {
                Debug.Log($"[NPC] {npcBehavior.NPCName}: Đã đến entrance và despawn");
                navMeshAgent.velocity = Vector3.zero;
                navMeshAgent.ResetPath();
                npcBehavior.DespawnNPC();
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
