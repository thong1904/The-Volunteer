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
        
        if (navMeshAgent != null)
            navMeshAgent.avoidancePriority = Random.Range(0, 32);
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
        
        npcBehavior.PlayAnimation(walkAnimationName);
    }
    
    private bool TryWarpToNavMesh()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 3f, NavMesh.AllAreas))
        {
            navMeshAgent.Warp(hit.position);
            return true;
        }
        return false;
    }
    
    public override TaskStatus OnUpdate()
    {
        if (npcBehavior == null || navMeshAgent == null)
            return TaskStatus.Failure;
        
        if (!navMeshAgent.isOnNavMesh)
        {
            if (!TryWarpToNavMesh())
                return TaskStatus.Failure;
        }
        
        targetPosition = npcBehavior.CurrentTarget;
        
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        if (distanceToTarget < stoppingDistance)
        {
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
                }
                else
                {
                    navMeshAgent.SetDestination(validTarget);
                    pathValid = true;
                }
            }
            else
            {
                navMeshAgent.SetDestination(validTarget);
                pathValid = true;
            }
        }
        
        if (!navMeshAgent.pathPending && pathValid)
        {
            if (navMeshAgent.remainingDistance <= stoppingDistance && navMeshAgent.velocity.sqrMagnitude < 0.01f)
            {
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
