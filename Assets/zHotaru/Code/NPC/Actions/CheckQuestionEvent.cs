using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC/Conditionals")]
[TaskName("Check Question Event")]
[TaskDescription("Kiểm tra xem có nên trigger event câu hỏi hay không")]
public class CheckQuestionEvent : Conditional
{
    private NPCBehaviorTree npcBehavior;
    
    // Fallback values kept for backward compatibility; prefer NPCBehaviorTree settings
    [SerializeField] public float triggerRadius = 5f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField, Range(0f, 100f)] private float chancePercent = 50f;
    [SerializeField] private bool requireIsAtDisplay = true;
    [SerializeField] private SharedBool isAtDisplay;
    
    public override void OnAwake()
    {
        npcBehavior = GetComponent<NPCBehaviorTree>();
    }
    
    public override TaskStatus OnUpdate()
    {
        // Ngày kết thúc → không hỏi câu hỏi
        if (npcBehavior != null && npcBehavior.IsDayEnded())
            return TaskStatus.Failure;
        
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
            return TaskStatus.Failure;
        
        float effectiveRadius = npcBehavior != null ? npcBehavior.QuestionTriggerRadius : triggerRadius;
        float effectiveChance = npcBehavior != null ? npcBehavior.QuestionChancePercent : chancePercent;

        var distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance > effectiveRadius)
            return TaskStatus.Failure;
        
        if (requireIsAtDisplay && (isAtDisplay == null || !isAtDisplay.Value))
            return TaskStatus.Failure;
        
        // Xác suất
        if (effectiveChance < 100f && Random.value > (effectiveChance * 0.01f))
            return TaskStatus.Failure;
        
        return TaskStatus.Success;
    }

    // Gizmo drawing moved to NPCBehaviorTree for visibility on the GameObject
}
