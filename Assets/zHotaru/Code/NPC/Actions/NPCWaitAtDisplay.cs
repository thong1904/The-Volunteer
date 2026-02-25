using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC")]
[TaskName("Wait At Display")]
[TaskDescription("NPC dừng lại để xem các vị trí trưng bày, có thể hỏi câu hỏi nếu player ở gần")]
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
    
    [Header("Question Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float faceRotationSpeed = 8f;
    [SerializeField] private float timeLimitSeconds = 15f;
    [SerializeField] private int correctAnswerMoney = 10;
    [UnityEngine.Tooltip("Không trừ tiền khi trả lời sai")]
    [SerializeField] private int wrongAnswerMoney = 0;
    
    // State
    private bool hasCheckedQuestion = false;
    private bool isAskingQuestion = false;
    private Transform playerTransform;
    
    public override void OnAwake()
    {
        npcBehavior = GetComponent<NPCBehaviorTree>();
    }
    
    public override void OnStart()
    {
        waitTime = Random.Range(minWaitTime, maxWaitTime);
        startTime = Time.time;
        hasCheckedQuestion = false;
        isAskingQuestion = false;
        playerTransform = null;
        
        // Đánh dấu đang ở display
        if (npcBehavior != null)
            npcBehavior.IsAtDisplay = true;
        
        // Dừng di chuyển
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = Vector3.zero;
        
        // Animation idle
        if (npcBehavior != null)
            npcBehavior.PlayAnimation(idleAnimationName);
        
        // Tìm DisplayArea gần nhất
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);
        foreach (Collider collider in colliders)
        {
            displayArea = collider.GetComponent<DisplayArea>();
            if (displayArea != null)
                break;
        }
        
        string npcName = npcBehavior != null ? npcBehavior.NPCName : gameObject.name;
        Debug.Log($"[NPC] {npcName}: Bắt đầu xem display, chờ {waitTime:F1}s (startTime={startTime:F2})");
    }
    
    public override TaskStatus OnUpdate()
    {
        string npcName = npcBehavior != null ? npcBehavior.NPCName : gameObject.name;
        float elapsed = Time.time - startTime;
        
        // === ĐANG HỎI CÂU HỎI ===
        if (isAskingQuestion)
        {
            // Xoay về phía player
            if (playerTransform != null)
            {
                var toPlayer = playerTransform.position - transform.position;
                toPlayer.y = 0f;
                if (toPlayer.sqrMagnitude > 0.0001f)
                {
                    var targetRot = Quaternion.LookRotation(toPlayer.normalized);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, faceRotationSpeed * Time.deltaTime);
                }
            }
            
            // Chờ UI xử lý xong
            if (QuestionUIController.Instance != null && QuestionUIController.Instance.HasAnswered)
            {
                if (!QuestionUIController.Instance.IsShowing)
                {
                    isAskingQuestion = false;
                    Debug.Log($"[NPC] {npcName}: Câu hỏi hoàn thành, đi display khác");
                    return TaskStatus.Success;
                }
            }
            return TaskStatus.Running;
        }
        
        // === KIỂM TRA CÓ NÊN HỎI KHÔNG (chỉ 1 lần khi bắt đầu) ===
        if (!hasCheckedQuestion)
        {
            hasCheckedQuestion = true;
            
            var player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                float triggerRadius = npcBehavior != null ? npcBehavior.QuestionTriggerRadius : 5f;
                float distance = Vector3.Distance(transform.position, player.transform.position);
                
                if (distance <= triggerRadius)
                {
                    // Player ở gần! Roll chance
                    float chance = npcBehavior != null ? npcBehavior.QuestionChancePercent : 50f;
                    bool rollPassed = Random.value <= (chance * 0.01f);
                    
                    if (rollPassed)
                    {
                        Debug.Log($"[NPC] {npcName}: Player ở gần ({distance:F1}m), hỏi câu hỏi!");
                        playerTransform = player.transform;
                        StartQuestion();
                        // Chỉ tiếp tục nếu câu hỏi được hiển thị thành công
                        if (isAskingQuestion)
                            return TaskStatus.Running;
                    }
                    else
                    {
                        Debug.Log($"[NPC] {npcName}: Player ở gần nhưng roll failed");
                    }
                }
            }
        }
        
        // === XEM DISPLAY BÌNH THƯỜNG ===
        // Xoay NPC nhìn vào display
        if (displayArea != null)
        {
            Vector3 focusPoint = displayArea.GetFocusPoint();
            Vector3 dir = (focusPoint - transform.position).normalized;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
        
        // Hết thời gian chờ
        if (elapsed >= waitTime)
        {
            Debug.Log($"[NPC] {npcName}: Hết thời gian xem display ({elapsed:F1}s >= {waitTime:F1}s), đi tiếp");
            return TaskStatus.Success;
        }
        
        return TaskStatus.Running;
    }
    
    private void StartQuestion()
    {
        string npcName = npcBehavior != null ? npcBehavior.NPCName : gameObject.name;
        
        // Animation wave
        if (npcBehavior != null)
            npcBehavior.PlayAnimation("Wave");
        
        // Tìm câu hỏi từ DisplayObject gần nhất
        QuestionData question = null;
        var displays = Object.FindObjectsByType<DisplayObject>(FindObjectsSortMode.None);
        if (displays != null && displays.Length > 0)
        {
            DisplayObject nearest = null;
            float bestDist = float.MaxValue;
            foreach (var d in displays)
            {
                float dist = Vector3.Distance(transform.position, d.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    nearest = d;
                }
            }
            if (nearest != null)
                question = nearest.GetRandomQuestion();
        }
        
        if (question == null)
        {
            Debug.LogWarning($"[NPC] {npcName}: Không có câu hỏi!");
            return;
        }
        
        // Hiển thị UI
        if (QuestionUIController.Instance != null)
        {
            QuestionUIController.Instance.ShowQuestion(
                npcName,
                question,
                timeLimitSeconds,
                (selectedIndex, isCorrect) => OnAnswerReceived(question, selectedIndex, isCorrect)
            );
            isAskingQuestion = true;
            
            // Phát âm thanh
            if (npcBehavior != null)
                npcBehavior.PlayQuestionSound();
                
            Debug.Log($"[NPC] {npcName}: Hiển thị câu hỏi - {question.questionText}");
        }
    }
    
    private void OnAnswerReceived(QuestionData question, int selectedIndex, bool isCorrect)
    {
        string npcName = npcBehavior != null ? npcBehavior.NPCName : gameObject.name;
        
        // Phát âm thanh phản hồi
        if (npcBehavior != null)
        {
            if (isCorrect)
                npcBehavior.PlayCorrectAnswerSound();
            else
                npcBehavior.PlayWrongAnswerSound();
        }
        
        // Tính tiền thưởng - chỉ cộng khi đúng, không trừ khi sai
        int moneyReward = isCorrect ? correctAnswerMoney : wrongAnswerMoney;
        
        // Thêm tracking thống kê và cộng tiền
        if (MoneyManager.Instance != null)
        {
            if (isCorrect)
            {
                MoneyManager.Instance.RecordCorrectAnswer();
                MoneyManager.Instance.AddMoney(moneyReward);
            }
            else
            {
                MoneyManager.Instance.RecordWrongAnswer();
                // Không trừ tiền khi trả lời sai
            }
        }
        
        Debug.Log($"[NPC] {npcName}: Trả lời {(isCorrect ? "ĐÚNG" : "SAI")}, tiền: {(isCorrect ? "+" : "")}{moneyReward}");
    }
    
    public override void OnEnd()
    {
        if (npcBehavior != null)
            npcBehavior.IsAtDisplay = false;
    }
}
