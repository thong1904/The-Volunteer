using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC")]
[TaskName("Set Target Display")]
[TaskDescription("Chọn một vị trí trưng bày ngẫu nhiên")]
public class SetTargetDisplay : Action
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
        
        npcBehavior.SetTargetDisplayPosition();
        return TaskStatus.Success;
    }
}
