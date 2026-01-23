using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC")]
[TaskName("Wait At Display")]
[TaskDescription("NPC dừng lại để xem các vị trí trưng bày, có thể trigger câu hỏi khi player đến gần")]
public class NPCWaitAtDisplay : Action
{
    private NPCBehaviorTree npcBehavior;
    private float waitTime;
    private float startTime;
    private DisplayArea displayArea;
    
    [Header("Wait Settings")]
    [SerializeField] private float minWaitTime = 3f;
    [SerializeField] private float maxWaitTime = 8f;
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private float rotationSpeed = 5f;
    
    [Header("Question Trigger (optional)")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private SharedBool shouldTriggerQuestion; // Output: true nếu cần chạy Question Event
    
    private bool questionRollPassed = false;
    private bool questionAlreadyTriggered = false;
    
    public override void OnAwake()
    {
        npcBehavior = GetComponent<NPCBehaviorTree>();
    }
    
    public override void OnStart()
    {
        string npcName = npcBehavior != null ? npcBehavior.NPCName : gameObject.name;
        
        // Lựa chọn ngẫu nhiên thời gian chờ
        waitTime = Random.Range(minWaitTime, maxWaitTime);
        startTime = Time.time;
        questionAlreadyTriggered = false;
        
        // Roll chance 1 lần khi bắt đầu wait (sử dụng QuestionChancePercent từ NPCBehaviorTree)
        float chance = npcBehavior != null ? npcBehavior.QuestionChancePercent : 50f;
        questionRollPassed = Random.value <= (chance * 0.01f);
        
        // Dừng di chuyển của NPC
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        
        // Phát animation idle
        if (npcBehavior != null)
            npcBehavior.PlayAnimation(idleAnimationName);
        
        // Đánh dấu NPC đang ở display
        if (npcBehavior != null)
            npcBehavior.IsAtDisplay = true;
        
        // Tìm DisplayArea gần nhất để lấy focus point
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);
        foreach (Collider collider in colliders)
        {
            displayArea = collider.GetComponent<DisplayArea>();
            if (displayArea != null)
                break;
        }
        
        // Reset output
        if (shouldTriggerQuestion != null)
            shouldTriggerQuestion.Value = false;
        
        Debug.Log($"[NPCWaitAtDisplay] {npcName}: Đang xem trưng bày trong {waitTime:F2}s. Question roll: {(questionRollPassed ? "PASSED" : "failed")}");
    }
    
    public override TaskStatus OnUpdate()
    {
        string npcName = npcBehavior != null ? npcBehavior.NPCName : gameObject.name;
        
        // Xoay NPC để nhìn vào focus point của display area
        if (displayArea != null)
        {
            Vector3 focusPoint = displayArea.GetFocusPoint();
            Vector3 directionToFocus = (focusPoint - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToFocus);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Check nếu roll passed và chưa trigger, kiểm tra player có trong range không
        if (questionRollPassed && !questionAlreadyTriggered)
        {
            var player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                float triggerRadius = npcBehavior != null ? npcBehavior.QuestionTriggerRadius : 5f;
                float distance = Vector3.Distance(transform.position, player.transform.position);
                
                if (distance <= triggerRadius)
                {
                    // Player đến gần! Trigger question event
                    questionAlreadyTriggered = true;
                    if (shouldTriggerQuestion != null)
                        shouldTriggerQuestion.Value = true;
                    
                    Debug.Log($"[NPCWaitAtDisplay] {npcName}: Player trong range ({distance:F2}m). Triggering question!");
                    return TaskStatus.Success; // Kết thúc wait để chuyển sang Question Event
                }
            }
        }
        
        // Kiểm tra thời gian chờ
        if (Time.time - startTime >= waitTime)
        {
            Debug.Log($"[NPCWaitAtDisplay] {npcName}: Kết thúc xem trưng bày (hết thời gian)");
            return TaskStatus.Success;
        }
        
        return TaskStatus.Running;
    }
    
    public override void OnEnd()
    {
        if (npcBehavior != null)
            npcBehavior.IsAtDisplay = false;
        
        string npcName = npcBehavior != null ? npcBehavior.NPCName : gameObject.name;
        Debug.Log($"[NPCWaitAtDisplay] {npcName}: OnEnd. shouldTriggerQuestion = {(shouldTriggerQuestion != null ? shouldTriggerQuestion.Value.ToString() : "null")}");
    }
}
