using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC")]
[TaskName("Question Event")]
[TaskDescription("NPC đặt câu hỏi cho người chơi qua UI - Chờ player tương tác trước")]
public class NPCQuestionEvent : Action
{
    private NPCBehaviorTree npcBehavior;
    private NPCInteractable npcInteractable;
    
    [Header("Animation")]
    [SerializeField] private string waveAnimationName = "wave";  // Đổi thành lowercase
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionWaitTime = 15f;    // Thời gian chờ player tương tác
    [SerializeField] private string interactPrompt = "E to talk";
    
    [Header("Question Settings")]
    [SerializeField] private QuestionData[] questions; // Fallback nếu không có DisplayObject
    [SerializeField] private float timeLimitSeconds = 15f;
    
    [Header("Player")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float faceRotationSpeed = 8f;
    
    [Header("Money Reward")]
    [SerializeField] private int correctAnswerMoney = 10;
    [UnityEngine.Tooltip("Không trừ tiền khi trả lời sai")]
    [SerializeField] private int wrongAnswerMoney = 0;
    
    private enum QuestionState
    {
        WaitingForInteraction,  // Đang chờ player tương tác
        AskingQuestion,          // Đang hiển thị câu hỏi
        Completed                // Hoàn thành
    }
    
    private QuestionState currentState;
    private QuestionData currentQuestion;
    private bool wasCorrect = false;
    private Transform playerTransform;
    private float interactionTimer;
    
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
        var npcName = npcBehavior != null ? npcBehavior.NPCName : gameObject.name;
        Debug.Log($"[NPCQuestionEvent] {npcName}: Start. Playing wave animation, waiting for player interaction.");
        
        playerTransform = FindPlayerTransform();
        wasCorrect = false;
        currentState = QuestionState.WaitingForInteraction;
        interactionTimer = interactionWaitTime;
        
        // Dừng di chuyển
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        
        // Phát animation wave (lowercase)
        if (npcBehavior != null)
            npcBehavior.PlayAnimation(waveAnimationName);
        
        // Bật chế độ chờ tương tác
        if (npcInteractable != null)
        {
            npcInteractable.OnPlayerInteracted += OnPlayerInteracted;
            npcInteractable.EnableInteraction(interactPrompt);
        }
    }
    
    public override void OnEnd()
    {
        // Cleanup event
        if (npcInteractable != null)
        {
            npcInteractable.OnPlayerInteracted -= OnPlayerInteracted;
            npcInteractable.DisableInteraction();
        }
    }
    
    public override TaskStatus OnUpdate()
    {
        // Xoay về phía người chơi
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
        
        switch (currentState)
        {
            case QuestionState.WaitingForInteraction:
                // Đếm ngược thời gian chờ
                interactionTimer -= Time.deltaTime;
                
                if (interactionTimer <= 0f)
                {
                    // Hết thời gian chờ → NPC không hỏi nữa
                    var npcName = npcBehavior != null ? npcBehavior.NPCName : gameObject.name;
                    Debug.Log($"[NPCQuestionEvent] {npcName}: Hết thời gian chờ ({interactionWaitTime}s) - Bỏ qua câu hỏi");
                    
                    if (npcInteractable != null)
                        npcInteractable.DisableInteraction();
                    
                    return TaskStatus.Success;
                }
                return TaskStatus.Running;
                
            case QuestionState.AskingQuestion:
                // Chờ UI xử lý xong
                if (QuestionUIController.Instance != null && QuestionUIController.Instance.HasAnswered)
                {
                    // Đợi UI ẩn xong
                    if (!QuestionUIController.Instance.IsShowing)
                    {
                        currentState = QuestionState.Completed;
                        OnQuestionComplete();
                        return TaskStatus.Success;
                    }
                }
                return TaskStatus.Running;
                
            case QuestionState.Completed:
                return TaskStatus.Success;
        }
        
        return TaskStatus.Running;
    }
    
    /// <summary>
    /// Được gọi khi player tương tác với NPC
    /// </summary>
    private void OnPlayerInteracted()
    {
        var npcName = npcBehavior != null ? npcBehavior.NPCName : gameObject.name;
        Debug.Log($"[NPCQuestionEvent] {npcName}: Player đã tương tác! Hiển thị câu hỏi.");
        
        currentState = QuestionState.AskingQuestion;
        BeginQuestion();
    }
    
    private void BeginQuestion()
    {
        var npcName = npcBehavior != null ? npcBehavior.NPCName : gameObject.name;
        
        // Ưu tiên lấy câu hỏi từ DisplayObject gần nhất, fallback sang danh sách cục bộ
        var nearest = FindNearestDisplay();
        currentQuestion = nearest != null ? nearest.GetRandomQuestion() : SelectRandomLocalQuestion();
        
        if (currentQuestion == null)
        {
            Debug.LogWarning($"[NPCQuestionEvent] {npcName}: No question available.");
            currentState = QuestionState.Completed;
            return;
        }
        
        // Hiển thị UI câu hỏi
        if (QuestionUIController.Instance != null)
        {
            QuestionUIController.Instance.ShowQuestion(
                npcName,
                currentQuestion,
                timeLimitSeconds,
                OnAnswerReceived
            );
            
            // Phát âm thanh hỏi 1 lần
            if (npcBehavior != null)
                npcBehavior.PlayQuestionSound();
            
            Debug.Log($"[NPCQuestionEvent] {npcName}: Question UI shown - {currentQuestion.questionText}");
        }
        else
        {
            Debug.LogError($"[NPCQuestionEvent] {npcName}: QuestionUIController.Instance is null!");
            currentState = QuestionState.Completed;
        }
    }
    
    private void OnAnswerReceived(int selectedIndex, bool isCorrect)
    {
        var npcName = npcBehavior != null ? npcBehavior.NPCName : gameObject.name;
        //questionAnswered = true;
        wasCorrect = isCorrect;
        
        // Phát âm thanh phản hồi 1 lần
        if (npcBehavior != null)
        {
            if (isCorrect)
                npcBehavior.PlayCorrectAnswerSound();
            else
                npcBehavior.PlayWrongAnswerSound();
        }
        
        if (selectedIndex < 0)
        {
            Debug.Log($"[NPCQuestionEvent] {npcName}: Time up - no answer given.");
        }
        else
        {
            Debug.Log($"[NPCQuestionEvent] {npcName}: Answer {selectedIndex} - {(isCorrect ? "CORRECT" : "WRONG")}");
        }
        
        // Cập nhật tiền - chỉ cộng khi đúng, không trừ khi sai
        int moneyReward = isCorrect ? correctAnswerMoney : wrongAnswerMoney;
        if (isCorrect && currentQuestion.pointsReward > 0)
            moneyReward = currentQuestion.pointsReward;
        
        // Gọi MoneyManager để cộng tiền và ghi nhận thống kê
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
        else
        {
            Debug.LogWarning($"[NPCQuestionEvent] MoneyManager.Instance is null!");
        }
        Debug.Log($"[NPCQuestionEvent] {npcName}: Money: {(isCorrect ? "+" : "")}{moneyReward}");
    }
    
    private void OnQuestionComplete()
    {
        // Phát animation phản ứng
        if (npcBehavior != null)
        {
            if (wasCorrect)
            {
                //npcBehavior.PlayAnimation(happyAnimationName);
                Debug.Log($"[NPCQuestionEvent] {npcBehavior.NPCName}: Happy reaction!");
            }
            else
            {
                //npcBehavior.PlayAnimation(sadAnimationName);
                Debug.Log($"[NPCQuestionEvent] {npcBehavior.NPCName}: Sad reaction.");
            }
        }
    }
    
    private QuestionData SelectRandomLocalQuestion()
    {
        if (questions == null || questions.Length == 0) return null;
        return questions[Random.Range(0, questions.Length)];
    }
    
    private DisplayObject FindNearestDisplay()
    {
        var displays = Object.FindObjectsByType<DisplayObject>(FindObjectsSortMode.None);
        if (displays == null || displays.Length == 0) return null;
        
        DisplayObject nearest = null;
        float bestDist = float.MaxValue;
        var pos = transform.position;
        for (int i = 0; i < displays.Length; i++)
        {
            var d = displays[i];
            var dist = Vector3.Distance(pos, d.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = d;
            }
        }
        return nearest;
    }

    private Transform FindPlayerTransform()
    {
        var playerGO = GameObject.FindGameObjectWithTag(playerTag);
        return playerGO != null ? playerGO.transform : null;
    }
}
