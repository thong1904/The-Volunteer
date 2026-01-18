using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC/Conditionals")]
[TaskName("Check Time To Leave")]
[TaskDescription("Kiểm tra xem có đến lúc NPC phải rời bảo tàng hay không")]
public class CheckTimeToLeaveMuseum : Conditional
{
    private NPCBehaviorTree npcBehavior;
    
    public override void OnAwake()
    {
        npcBehavior = GetComponent<NPCBehaviorTree>();
    }
    
    public override TaskStatus OnUpdate()
    {
        if (npcBehavior.IsTimeToLeaveMuseum())
        {
            return TaskStatus.Success;
        }
        
        return TaskStatus.Failure;
    }
}
