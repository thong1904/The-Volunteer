using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC")]
[TaskName("Question Event")]
[TaskDescription("NPC đặt câu hỏi cho người chơi qua UI")]
public class NPCQuestionEvent : Action
{
    private NPCBehaviorTree npcBehavior;
    
    [Header("Animation")]
    [SerializeField] private string waveAnimationName = "Wave";
    //[SerializeField] private string happyAnimationName = "Happy";
    //[SerializeField] private string sadAnimationName = "Sad";
    
    [Header("Question Settings")]
    [SerializeField] private QuestionData[] questions; // Fallback nếu không có DisplayObject
    [SerializeField] private float timeLimitSeconds = 15f;
    
    [Header("Player")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float faceRotationSpeed = 8f;
    
    [Header("Score")]
    [SerializeField] private int correctAnswerPoints = 10;
    [SerializeField] private int wrongAnswerPoints = -5;
    
    private QuestionData currentQuestion;
    private bool isQuestionActive = false;
    //private bool questionAnswered = false;
    private bool wasCorrect = false;
    private Transform playerTransform;
    
    public override void OnAwake()
    {
        npcBehavior = GetComponent<NPCBehaviorTree>();
    }
    
    public override void OnStart()
    {
        var npcName = npcBehavior != null ? npcBehavior.NPCName : gameObject.name;
        Debug.Log($"[NPCQuestionEvent] {npcName}: Start. Facing player and showing question.");
        
        playerTransform = FindPlayerTransform();
        //questionAnswered = false;
        wasCorrect = false;
        
        // Dừng di chuyển
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        
        // Phát animation wave
        if (npcBehavior != null)
            npcBehavior.PlayAnimation(waveAnimationName);
        
        // Bắt đầu hiển thị câu hỏi ngay
        BeginQuestion();
    }
    
    public override TaskStatus OnUpdate()
    {
        // Xoay về phía người chơi
        if (playerTransform != null && isQuestionActive)
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
        if (isQuestionActive)
        {
            // Kiểm tra UI đã hoàn thành chưa
            if (QuestionUIController.Instance != null && QuestionUIController.Instance.HasAnswered)
            {
                // Đợi UI ẩn xong
                if (!QuestionUIController.Instance.IsShowing)
                {
                    isQuestionActive = false;
                    OnQuestionComplete();
                    return TaskStatus.Success;
                }
            }
            return TaskStatus.Running;
        }
        
        return TaskStatus.Success;
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
            isQuestionActive = false;
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
            isQuestionActive = true;
            
            // Bắt đầu phát âm thanh hỏi lặp lại
            if (npcBehavior != null)
                npcBehavior.StartQuestionSound();
            
            Debug.Log($"[NPCQuestionEvent] {npcName}: Question UI shown - {currentQuestion.questionText}");
        }
        else
        {
            Debug.LogError($"[NPCQuestionEvent] {npcName}: QuestionUIController.Instance is null!");
            isQuestionActive = false;
        }
    }
    
    private void OnAnswerReceived(int selectedIndex, bool isCorrect)
    {
        var npcName = npcBehavior != null ? npcBehavior.NPCName : gameObject.name;
        //questionAnswered = true;
        wasCorrect = isCorrect;
        
        // Dừng âm thanh hỏi
        if (npcBehavior != null)
            npcBehavior.StopQuestionSound();
        
        // Phát âm thanh phản hồi
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
        
        // Cập nhật điểm
        int points = isCorrect ? correctAnswerPoints : wrongAnswerPoints;
        if (currentQuestion != null && isCorrect)
        {
            points = currentQuestion.pointsReward > 0 ? currentQuestion.pointsReward : correctAnswerPoints;
        }
        
        // Gọi ScoreManager để cộng/trừ điểm
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(points);
        }
        else
        {
            Debug.LogWarning($"[NPCQuestionEvent] ScoreManager.Instance is null!");
        }
        Debug.Log($"[NPCQuestionEvent] {npcName}: Points: {points}");
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
