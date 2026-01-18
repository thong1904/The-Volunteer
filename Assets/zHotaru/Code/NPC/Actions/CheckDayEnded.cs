using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC/Conditionals")]
[TaskName("Check Day Ended")]
[TaskDescription("Kiểm tra xem ngày đã kết thúc hay chưa")]
public class CheckDayEnded : Conditional
{
    private NPCBehaviorTree npcBehavior;
    
    public override void OnAwake()
    {
        npcBehavior = GetComponent<NPCBehaviorTree>();
    }
    
    public override TaskStatus OnUpdate()
    {
        if (npcBehavior.IsDayEnded())
        {
            return TaskStatus.Success;
        }
        
        return TaskStatus.Failure;
    }
}
