using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC")]
[TaskName("Set Entered Museum")]
[TaskDescription("Đánh dấu NPC đã vào bảo tàng")]
public class SetEnteredMuseum : Action
{
    private NPCBehaviorTree npcBehavior;
    
    public override void OnAwake()
    {
        npcBehavior = GetComponent<NPCBehaviorTree>();
    }
    
    public override TaskStatus OnUpdate()
    {
        if (npcBehavior != null && npcBehavior.IsDayEnded())
            return TaskStatus.Failure;
        
        if (Vector3.Distance(transform.position, npcBehavior.CurrentTarget) < 2f)
        {
            npcBehavior.SetEntered();
            return TaskStatus.Success;
        }
        
        return TaskStatus.Failure;
    }
}
