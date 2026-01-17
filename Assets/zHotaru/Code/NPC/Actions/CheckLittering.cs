using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC/Conditionals")]
[TaskName("Check Littering")]
[TaskDescription("Kiểm tra xem có nên vứt rác hay không")]
public class CheckLittering : Conditional
{
    private NPCBehaviorTree npcBehavior;
    
    public override void OnAwake()
    {
        npcBehavior = GetComponent<NPCBehaviorTree>();
    }
    
    public override TaskStatus OnUpdate()
    {
        if (npcBehavior.ShouldLitter())
        {
            return TaskStatus.Success;
        }
        
        return TaskStatus.Failure;
    }
}
