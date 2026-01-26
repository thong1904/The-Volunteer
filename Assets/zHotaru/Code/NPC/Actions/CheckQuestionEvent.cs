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
        string npcName = npcBehavior != null ? npcBehavior.NPCName : gameObject.name;
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Debug.Log($"[CheckQuestionEvent] {npcName}: No GameObject with tag '{playerTag}' found.");
            return TaskStatus.Failure;
        }
        
        // Use per-NPC settings from NPCBehaviorTree when available
        float effectiveRadius = npcBehavior != null ? npcBehavior.QuestionTriggerRadius : triggerRadius;
        float effectiveChance = npcBehavior != null ? npcBehavior.QuestionChancePercent : chancePercent;

        var distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance > effectiveRadius)
        {
            Debug.Log($"[CheckQuestionEvent] {npcName}: Player distance {distance:F2} > triggerRadius {effectiveRadius:F2}.");
            return TaskStatus.Failure;
        }
        
        if (requireIsAtDisplay && (isAtDisplay == null || !isAtDisplay.Value))
        {
            Debug.Log($"[CheckQuestionEvent] {npcName}: requireIsAtDisplay is true but isAtDisplay missing or false.");
            return TaskStatus.Failure;
        }
        
        // Xác suất xảy ra sự kiện
        if (effectiveChance < 100f)
        {
            if (Random.value > (effectiveChance * 0.01f))
            {
                Debug.Log($"[CheckQuestionEvent] {npcName}: Chance roll failed ({effectiveChance}%).");
                return TaskStatus.Failure;
            }
        }
        
        // If we passed all checks and random roll, trigger the event
        Debug.Log($"[CheckQuestionEvent] {npcName}: Conditions met. Triggering question event.");
        return TaskStatus.Success;
    }

    // Gizmo drawing moved to NPCBehaviorTree for visibility on the GameObject
}
