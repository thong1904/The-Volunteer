using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC")]
[TaskName("Wait At Display")]
[TaskDescription("NPC dừng lại để xem các vị trí trưng bày, có thể hỏi câu hỏi nếu player ở gần")]
public class NPCWaitAtDisplay : Action
{
    private NPCBehaviorTree npcBehavior;
    private NPCInteractable npcInteractable;
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
    [SerializeField] private float interactionWaitTime = 10f;  // Thời gian chờ player tương tác
    [SerializeField] private string waveAnimationName = "wave";
    [SerializeField] private int correctAnswerMoney = 10;
    [UnityEngine.Tooltip("Không trừ tiền khi trả lời sai")]
    [SerializeField] private int wrongAnswerMoney = 0;
    
    // State
    private enum QuestionState
    {
        NotAsking,
        WaitingForInteraction,  // Đang chờ player tương tác
        AskingQuestion          // Đang hiển thị câu hỏi
    }
    
    private QuestionState questionState = QuestionState.NotAsking;
    private bool hasCheckedQuestion = false;
    private bool isAskingQuestion = false;
    private Transform playerTransform;
    private float interactionTimer;
    private QuestionData pendingQuestion;
    
    public override void OnAwake()
    {
        npcBehavior = GetComponent<NPCBehaviorTree>();
        
        // Lấy hoặc tạo NPCInteractable
        npcInteractable = GetComponent<NPCInteractable>();
        if (npcInteractable == null)
        {
            npcInteractable = gameObject.AddComponent<NPCInteractable>();
        }
    }
    
    public override void OnStart()
    {
        waitTime = Random.Range(minWaitTime, maxWaitTime);
        startTime = Time.time;
        hasCheckedQuestion = false;
        isAskingQuestion = false;
        questionState = QuestionState.NotAsking;
        playerTransform = null;
        pendingQuestion = null;
        interactionTimer = interactionWaitTime;
        
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
    }
    
    public override TaskStatus OnUpdate()
    {
        string npcName = npcBehavior != null ? npcBehavior.NPCName : gameObject.name;
        float elapsed = Time.time - startTime;
                // QUAN TRỌNG: Kiểm tra ngày kết thúc → return Failure để Selector chọn nhánh Leave Museum
        if (npcBehavior != null && npcBehavior.IsDayEnded())
        {
            if (questionState == QuestionState.WaitingForInteraction && npcInteractable != null)
            {
                npcInteractable.OnPlayerInteracted -= OnPlayerInteracted;
                npcInteractable.DisableInteraction();
            }
            return TaskStatus.Failure;
        }
                // === ĐANG CHỜ PLAYER TƯƠNG TÁC ===
        if (questionState == QuestionState.WaitingForInteraction)
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
            
            // Đếm ngược thời gian chờ
            interactionTimer -= Time.deltaTime;
            
            if (interactionTimer <= 0f)
            {
                if (npcInteractable != null)
                {
                    npcInteractable.OnPlayerInteracted -= OnPlayerInteracted;
                    npcInteractable.DisableInteraction();
                }
                
                questionState = QuestionState.NotAsking;
            }
            else
            {
                return TaskStatus.Running;
            }
        }
        
        // === ĐANG HỎI CÂU HỎI ===
        if (questionState == QuestionState.AskingQuestion || isAskingQuestion)
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
                    questionState = QuestionState.NotAsking;
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
                        playerTransform = player.transform;
                        StartWaitingForInteraction();
                        if (questionState == QuestionState.WaitingForInteraction)
                            return TaskStatus.Running;
                    }
                }
            }
        }
        
        // === XEM DISPLAY BÌNH THƯỜNG ===
        // Xoay NPC nhìn vào display
        if (displayArea != null)
        {
            Vector3 focusPoint = displayArea.GetFocusPoint();
            Vector3 toFocus = focusPoint - transform.position;
            toFocus.y = 0f; // Chỉ xoay theo trục Y (ngang)
            
            if (toFocus.sqrMagnitude > 0.01f)
            {
                Vector3 dir = toFocus.normalized;
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
        
        if (elapsed >= waitTime)
            return TaskStatus.Success;
        
        return TaskStatus.Running;
    }
    
    /// <summary>
    /// Bắt đầu chờ player tương tác (wave animation)
    /// </summary>
    private void StartWaitingForInteraction()
    {
        string npcName = npcBehavior != null ? npcBehavior.NPCName : gameObject.name;
        
        pendingQuestion = FindNearestQuestion();
        if (pendingQuestion == null)
            return;
        
        // Animation wave
        if (npcBehavior != null)
            npcBehavior.PlayAnimation(waveAnimationName);
        
        // Phát âm thanh hỏi ngay khi wave (không chờ UI)
        if (npcBehavior != null)
            npcBehavior.PlayQuestionSound();
        
        // Reset timer
        interactionTimer = interactionWaitTime;
        questionState = QuestionState.WaitingForInteraction;
        
        if (npcInteractable != null)
        {
            npcInteractable.OnPlayerInteracted += OnPlayerInteracted;
            npcInteractable.EnableInteraction("E để nói chuyện");
        }
    }
    
    /// <summary>
    /// Được gọi khi player tương tác với NPC
    /// </summary>
    private void OnPlayerInteracted()
    {
        if (npcInteractable != null)
        {
            npcInteractable.OnPlayerInteracted -= OnPlayerInteracted;
            npcInteractable.DisableInteraction();
        }
        
        questionState = QuestionState.AskingQuestion;
        ShowQuestionUI();
    }
    
    /// <summary>
    /// Tìm câu hỏi từ DisplayObject gần nhất
    /// </summary>
    private QuestionData FindNearestQuestion()
    {
        var displays = Object.FindObjectsByType<DisplayObject>(FindObjectsSortMode.None);
        if (displays == null || displays.Length == 0) return null;
        
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
        
        return nearest?.GetRandomQuestion();
    }
    
    /// <summary>
    /// Hiển thị UI câu hỏi
    /// </summary>
    private void ShowQuestionUI()
    {
        string npcName = npcBehavior != null ? npcBehavior.NPCName : gameObject.name;
        
        if (pendingQuestion == null)
        {
            questionState = QuestionState.NotAsking;
            return;
        }
        
        // Hiển thị UI
        if (QuestionUIController.Instance != null)
        {
            QuestionUIController.Instance.ShowQuestion(
                npcName,
                pendingQuestion,
                timeLimitSeconds,
                (selectedIndex, isCorrect) => OnAnswerReceived(pendingQuestion, selectedIndex, isCorrect)
            );
            isAskingQuestion = true;
            
            if (npcBehavior != null)
                npcBehavior.PlayQuestionSound();
        }
        else
        {
            questionState = QuestionState.NotAsking;
        }
    }
    
    // Giữ lại method cũ cho backwards compatibility
    private void StartQuestion()
    {
        StartWaitingForInteraction();
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
            }
        }
    }
    
    public override void OnEnd()
    {
        // Cleanup NPCInteractable event
        if (npcInteractable != null)
        {
            npcInteractable.OnPlayerInteracted -= OnPlayerInteracted;
            npcInteractable.DisableInteraction();
        }
        
        if (npcBehavior != null)
            npcBehavior.IsAtDisplay = false;
    }
}
