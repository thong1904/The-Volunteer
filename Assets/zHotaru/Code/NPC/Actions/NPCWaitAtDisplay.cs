using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC")]
[TaskName("Wait At Display")]
[TaskDescription("NPC dừng lại để xem các vị trí trưng bày")]
public class NPCWaitAtDisplay : Action
{
    private NPCBehaviorTree npcBehavior;
    private float waitTime;
    private float startTime;
    
    [SerializeField] private float minWaitTime = 3f;
    [SerializeField] private float maxWaitTime = 8f;
    
    public override void OnAwake()
    {
        npcBehavior = GetComponent<NPCBehaviorTree>();
    }
    
    public override void OnStart()
    {
        // Lựa chọn ngẫu nhiên thời gian chờ
        waitTime = Random.Range(minWaitTime, maxWaitTime);
        startTime = Time.time;
        
        // Dừng di chuyển của NPC
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        
        Debug.Log($"{npcBehavior.NPCName} đang xem trưng bày trong {waitTime:F2} giây");
    }
    
    public override TaskStatus OnUpdate()
    {
        // Kiểm tra thời gian chờ
        if (Time.time - startTime >= waitTime)
        {
            return TaskStatus.Success;
        }
        
        return TaskStatus.Running;
    }
    
    public override void OnEnd()
    {
        Debug.Log($"{npcBehavior.NPCName} kết thúc xem trưng bày");
    }
}
