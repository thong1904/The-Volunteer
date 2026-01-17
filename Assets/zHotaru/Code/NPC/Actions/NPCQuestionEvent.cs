using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC")]
[TaskName("Question Event")]
[TaskDescription("NPC đặt câu hỏi cho người chơi")]
public class NPCQuestionEvent : Action
{
    private NPCBehaviorTree npcBehavior;
    
    [System.Serializable]
    public class Question
    {
        public string questionText;
        public string[] answers;
        public int correctAnswerIndex;
        public int pointsReward = 10;
    }
    
    [SerializeField] private Question[] questions;
    private Question currentQuestion;
    private bool isQuestionActive = false;
    
    public override void OnAwake()
    {
        npcBehavior = GetComponent<NPCBehaviorTree>();
    }
    
    public override void OnStart()
    {
        if (questions.Length == 0)
        {
            Debug.LogWarning("Không có câu hỏi được cấu hình!");
            return;
        }
        
        // Chọn một câu hỏi ngẫu nhiên
        currentQuestion = questions[Random.Range(0, questions.Length)];
        isQuestionActive = true;
        
        // Dừng di chuyển
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        
        Debug.Log($"[{npcBehavior.NPCName}] {currentQuestion.questionText}");
        
        // Gọi event để hiển thị câu hỏi trên UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowQuestion(npcBehavior.NPCName, currentQuestion.questionText, 
                currentQuestion.answers, currentQuestion.correctAnswerIndex, OnQuestionAnswered);
        }
    }
    
    public override TaskStatus OnUpdate()
    {
        // Chờ cho đến khi có câu trả lời
        if (isQuestionActive)
        {
            return TaskStatus.Running;
        }
        
        return TaskStatus.Success;
    }
    
    private void OnQuestionAnswered(int selectedAnswerIndex)
    {
        if (selectedAnswerIndex == currentQuestion.correctAnswerIndex)
        {
            Debug.Log($"Trả lời đúng! Cộng {currentQuestion.pointsReward} điểm");
            
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(currentQuestion.pointsReward);
            }
            
            // Phát animation hoặc âm thanh phản ứng tích cực
            PlayPositiveReaction();
        }
        else
        {
            Debug.Log("Trả lời sai!");
            
            // Phát animation hoặc âm thanh phản ứng tiêu cực
            PlayNegativeReaction();
        }
        
        isQuestionActive = false;
    }
    
    private void PlayPositiveReaction()
    {
        // TODO: Thêm animation và sound effect cho phản ứng tích cực
        Debug.Log($"{npcBehavior.NPCName} phản ứng tích cực");
    }
    
    private void PlayNegativeReaction()
    {
        // TODO: Thêm animation và sound effect cho phản ứng tiêu cực
        Debug.Log($"{npcBehavior.NPCName} phản ứng tiêu cực");
    }
}
