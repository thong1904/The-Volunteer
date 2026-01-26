using UnityEngine;

/// <summary>
/// UIManager - Quản lý tất cả UI controllers trong scene.
/// Là con của GameManager, không dùng singleton.
/// Truy cập qua: GameManager.Instance.UI
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI Controllers (Gán trong từng scene)")]
    [SerializeField] private QuestionUIController questionController;
    [SerializeField] private ScoreUI scoreUI;
    
    // Properties để truy cập các controller
    public QuestionUIController Question => questionController;
    public ScoreUI Score => scoreUI;
    
    /// <summary>
    /// Gọi từ GameManager.Awake() để khởi tạo
    /// </summary>
    public void Initialize()
    {
        Debug.Log("[UIManager] Initialized");
    }
    
    /// <summary>
    /// Tìm và gán các UI controllers trong scene hiện tại
    /// Gọi khi chuyển scene nếu cần
    /// </summary>
    public void FindSceneUIControllers()
    {
        if (questionController == null)
            questionController = FindAnyObjectByType<QuestionUIController>();
        
        if (scoreUI == null)
            scoreUI = FindAnyObjectByType<ScoreUI>();
        
        Debug.Log($"[UIManager] Found controllers - Question: {questionController != null}, Score: {scoreUI != null}");
    }
    
    /// <summary>
    /// Set QuestionUIController từ bên ngoài (scene-specific)
    /// </summary>
    public void SetQuestionController(QuestionUIController controller)
    {
        questionController = controller;
    }
    
    /// <summary>
    /// Set ScoreUI từ bên ngoài (scene-specific)
    /// </summary>
    public void SetScoreUI(ScoreUI ui)
    {
        scoreUI = ui;
    }
}
