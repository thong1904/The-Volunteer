using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC/Conditionals")]
[TaskName("Check Question Event")]
[TaskDescription("Kiểm tra xem có nên trigger event câu hỏi hay không")]
public class CheckQuestionEvent : Conditional
{
    private NPCBehaviorTree npcBehavior;
    
    public override void OnAwake()
    {
        npcBehavior = GetComponent<NPCBehaviorTree>();
    }
    
    public override TaskStatus OnUpdate()
    {
        if (npcBehavior.ShouldTriggerQuestionEvent())
        {
            return TaskStatus.Success;
        }
        
        return TaskStatus.Failure;
    }
}
