using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller hiển thị câu hỏi từ NPC.
/// Gán các reference UI trong Inspector.
/// Truy cập qua: GameManager.Instance.UI.Question
/// </summary>
public class QuestionUIController : MonoBehaviour
{
    // Giữ static Instance để backward compatible với NPCQuestionEvent
    public static QuestionUIController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private CanvasGroup questionCanvasGroup;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Button[] answerButtons = new Button[4];
    
    [Header("VFX (optional)")]
    [SerializeField] private UIVFX uiVFX; // Nếu muốn dùng DOTween animation
    [SerializeField] private bool useVFX = false;
    
    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 8f;
    [SerializeField] private Color correctColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color wrongColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float hideDelayAfterAnswer = 1.5f;
    
    // State
    private bool isShowing = false;
    private QuestionData currentQuestion;
    private Action<int, bool> onAnswerCallback;
    private float timeRemaining;
    private bool answered = false;
    private Image[] buttonImages;

    void Awake()
    {
        Instance = this;
        
        // Cache button images
        buttonImages = new Image[answerButtons.Length];
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
                buttonImages[i] = answerButtons[i].GetComponent<Image>();
        }
        
        // Ẩn UI khi start
        if (questionCanvasGroup != null)
        {
            questionCanvasGroup.alpha = 0f;
            questionCanvasGroup.interactable = false;
            questionCanvasGroup.blocksRaycasts = false;
        }
        
        // Đăng ký với UIManager nếu có
        if (GameManager.Instance != null && GameManager.Instance.UI != null)
        {
            GameManager.Instance.UI.SetQuestionController(this);
        }
    }

    void Update()
    {
        // Fade animation (chỉ dùng khi không dùng VFX)
        if (!useVFX && questionCanvasGroup != null)
        {
            float targetAlpha = isShowing ? 1f : 0f;
            questionCanvasGroup.alpha = Mathf.MoveTowards(questionCanvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            questionCanvasGroup.interactable = isShowing && !answered;
            questionCanvasGroup.blocksRaycasts = isShowing;
        }
        
        // Timer countdown
        if (isShowing && !answered && timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay();
            
            if (timeRemaining <= 0f)
            {
                OnTimeUp();
            }
        }
    }

    /// <summary>
    /// Hiển thị câu hỏi lên UI
    /// </summary>
    public void ShowQuestion(string npcName, QuestionData question, float timeLimitSeconds, Action<int, bool> onAnswer)
    {
        if (question == null)
        {
            Debug.LogWarning("[QuestionUIController] ShowQuestion called with null question!");
            return;
        }
        
        //currentNPCName = npcName;
        currentQuestion = question;
        onAnswerCallback = onAnswer;
        timeRemaining = timeLimitSeconds;
        answered = false;
        
        // Cập nhật question text
        if (questionText != null)
        {
            questionText.text = $"<b>{npcName}</b> hỏi:\n{question.questionText}";
        }
        
        // Cập nhật các button answer
        for (int i = 0; i < answerButtons.Length && i < 4; i++)
        {
            if (answerButtons[i] != null)
            {
                int answerIndex = i;
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(answerIndex));
                answerButtons[i].interactable = true;
                
                // Reset màu button
                if (buttonImages[i] != null)
                    buttonImages[i].color = normalColor;
                
                // Cập nhật text trong button (tìm TMP_Text con)
                var btnText = answerButtons[i].GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    string answerText = (question.answers != null && i < question.answers.Length) 
                        ? question.answers[i] 
                        : "";
                    btnText.text = answerText;
                }
            }
        }
        
        UpdateTimerDisplay();
        isShowing = true;
        
        // Dùng VFX hoặc fade thường
        if (useVFX && uiVFX != null)
        {
            uiVFX.FadeInUI();
        }
        
        Debug.Log($"[QuestionUIController] Showing question from {npcName}: {question.questionText}");
    }

    /// <summary>
    /// Ẩn UI câu hỏi
    /// </summary>
    public void Hide()
    {
        isShowing = false;
        answered = true;
        
        // Dùng VFX hoặc fade thường
        if (useVFX && uiVFX != null)
        {
            uiVFX.FadeOutUI();
        }
        
        foreach (var btn in answerButtons)
        {
            if (btn != null)
                btn.interactable = false;
        }
        
        Debug.Log("[QuestionUIController] Hidden");
    }

    /// <summary>
    /// Kiểm tra UI đang hiển thị không
    /// </summary>
    public bool IsShowing => isShowing;

    /// <summary>
    /// Kiểm tra đã trả lời chưa
    /// </summary>
    public bool HasAnswered => answered;

    private void OnAnswerSelected(int index)
    {
        if (answered) return;
        answered = true;
        
        bool isCorrect = currentQuestion != null && index == currentQuestion.correctAnswerIndex;
        
        Debug.Log($"[QuestionUIController] Answer selected: {index}, Correct: {isCorrect}");
        
        // Highlight đáp án đúng/sai
        HighlightAnswers(index, currentQuestion?.correctAnswerIndex ?? -1);
        
        // Disable buttons
        foreach (var btn in answerButtons)
        {
            if (btn != null)
                btn.interactable = false;
        }
        
        // Gọi callback
        onAnswerCallback?.Invoke(index, isCorrect);
        
        // Tự động ẩn sau delay
        Invoke(nameof(Hide), hideDelayAfterAnswer);
    }

    private void OnTimeUp()
    {
        if (answered) return;
        answered = true;
        
        Debug.Log($"[QuestionUIController] Time up!");
        
        // Highlight đáp án đúng
        HighlightAnswers(-1, currentQuestion?.correctAnswerIndex ?? -1);
        
        // Disable buttons
        foreach (var btn in answerButtons)
        {
            if (btn != null)
                btn.interactable = false;
        }
        
        // Gọi callback với -1 (không trả lời)
        onAnswerCallback?.Invoke(-1, false);
        
        // Tự động ẩn sau delay
        Invoke(nameof(Hide), hideDelayAfterAnswer);
    }

    private void HighlightAnswers(int selectedIndex, int correctIndex)
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (buttonImages[i] == null) continue;
            
            if (i == correctIndex)
            {
                // Đáp án đúng - màu xanh
                buttonImages[i].color = correctColor;
            }
            else if (i == selectedIndex)
            {
                // Đáp án sai đã chọn - màu đỏ
                buttonImages[i].color = wrongColor;
            }
            else
            {
                // Các đáp án khác - màu xám
                buttonImages[i].color = new Color(0.5f, 0.5f, 0.5f);
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(timeRemaining);
            timerText.text = $"{seconds}s";
            
            // Đổi màu khi gần hết giờ
            timerText.color = timeRemaining <= 5f ? Color.red : Color.white;
        }
    }
}
