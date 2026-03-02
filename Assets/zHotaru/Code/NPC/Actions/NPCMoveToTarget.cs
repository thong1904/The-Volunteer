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
    [SerializeField] private float stuckCheckTime = 2f; // Thời gian kiểm tra bị kẹt
    [SerializeField] private float minMoveDistance = 0.1f; // Khoảng cách tối thiểu phải di chuyển
    [SerializeField] private int maxPathRetries = 3; // THÊM DÒNG NÀY
    
    private float stuckTimer = 0f;
    private Vector3 lastPosition;
    private bool pathValid = false;
    private int pathRetryCount = 0; // THÊM DÒNG NÀY
    
    public override void OnAwake()
    {
        npcBehavior = GetComponent<NPCBehaviorTree>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        
        if (navMeshAgent != null)
        {
            navMeshAgent.avoidancePriority = Random.Range(0, 32);
        }
    }
    
    public override void OnStart()
    {
        // Reset stuck detection
        stuckTimer = 0f;
        lastPosition = transform.position;
        pathValid = false;
        pathRetryCount = 0;
        
        // Kích hoạt và cấu hình NavMeshAgent
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = true;
            navMeshAgent.isStopped = false;
            navMeshAgent.updateRotation = true; // Đảm bảo NPC xoay theo hướng di chuyển
            navMeshAgent.updatePosition = true;
            
            // Đảm bảo NPC ở trên NavMesh
            if (!navMeshAgent.isOnNavMesh)
            {
                TryWarpToNavMesh();
            }
        }
        
        // Phát animation walk
        if (npcBehavior != null)
            npcBehavior.PlayAnimation(walkAnimationName);
    }
    
    public override TaskStatus OnUpdate()
    {
        if (npcBehavior == null || navMeshAgent == null)
            return TaskStatus.Failure;
        
        // Ngày kết thúc → dừng ngay
        if (npcBehavior.IsDayEnded())
        {
            if (navMeshAgent.isActiveAndEnabled)
            {
                navMeshAgent.velocity = Vector3.zero;
                navMeshAgent.ResetPath();
            }
            return TaskStatus.Failure;
        }
        
        if (!navMeshAgent.isOnNavMesh)
        {
            if (!TryWarpToNavMesh())
                return TaskStatus.Failure;
        }
        
        targetPosition = npcBehavior.CurrentTarget;
        
        if (Vector3.Distance(transform.position, targetPosition) < stoppingDistance)
        {
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.ResetPath();
            return TaskStatus.Success;
        }
        
        // Kiểm tra và set destination
        if (navMeshAgent.isActiveAndEnabled)
        {
            if (!pathValid)
            {
                NavMeshPath path = new NavMeshPath();
                
                // Sample target position lên NavMesh trước
                NavMeshHit targetHit;
                Vector3 validTarget = targetPosition;
                if (NavMesh.SamplePosition(targetPosition, out targetHit, 2f, NavMesh.AllAreas))
                {
                    validTarget = targetHit.position;
                }
                
                if (navMeshAgent.CalculatePath(validTarget, path))
                {
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        navMeshAgent.SetPath(path);
                        pathValid = true;
                    }
                    else if (path.status == NavMeshPathStatus.PathPartial)
                    {
                        pathRetryCount++;
                        if (pathRetryCount >= maxPathRetries)
                            return TaskStatus.Failure;
                        npcBehavior.SetTargetDisplayPosition();
                        return TaskStatus.Running;
                    }
                    else
                    {
                        pathRetryCount++;
                        if (pathRetryCount >= maxPathRetries)
                            return TaskStatus.Failure;
                        npcBehavior.SetTargetDisplayPosition();
                        return TaskStatus.Running;
                    }
                }
                else
                {
                    pathRetryCount++;
                    if (pathRetryCount >= maxPathRetries)
                        return TaskStatus.Failure;
                    npcBehavior.SetTargetDisplayPosition();
                    return TaskStatus.Running;
                }
            }
        }
        
        // Kiểm tra xem đã đến đích hay chưa
        if (!navMeshAgent.pathPending && pathValid)
        {
            if (navMeshAgent.remainingDistance <= stoppingDistance && !navMeshAgent.pathPending)
            {
                if (navMeshAgent.velocity.sqrMagnitude < 0.01f)
                {
                    navMeshAgent.velocity = Vector3.zero;
                    navMeshAgent.ResetPath();
                    return TaskStatus.Success;
                }
            }
            
            // Kiểm tra bị kẹt
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckCheckTime)
            {
                float movedDistance = Vector3.Distance(transform.position, lastPosition);
                if (movedDistance < minMoveDistance)
                {
                    pathRetryCount++;
                    if (pathRetryCount >= maxPathRetries)
                    {
                        navMeshAgent.ResetPath();
                        return TaskStatus.Failure;
                    }
                    
                    navMeshAgent.ResetPath();
                    pathValid = false;
                    npcBehavior.SetTargetDisplayPosition();
                }
                
                stuckTimer = 0f;
                lastPosition = transform.position;
            }
        }
        
        return TaskStatus.Running;
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
    
    public override void OnEnd()
    {
        // Dừng di chuyển
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.ResetPath();
            navMeshAgent.isStopped = true;
        }
    }
}
