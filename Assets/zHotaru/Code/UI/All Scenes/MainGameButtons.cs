using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý các UI buttons và panels trong game
/// </summary>
public class MainGameButtons : MonoBehaviour
{
    [Header("Day End Panel")]
    [SerializeField] private UIVFX dayEndPanelVFX; // Panel với UIVFX + CanvasGroup
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI playTimeText;
    [SerializeField] private TextMeshProUGUI interactionsText;
    [SerializeField] private TextMeshProUGUI correctWrongText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button nextDayButton;

    private bool isPanelVisible = false;
    
    // Property để truy cập GameObject của panel
    private GameObject DayEndPanel => dayEndPanelVFX != null ? dayEndPanelVFX.gameObject : null;

    private void Start()
    {
        // Ẩn panel khi bắt đầu
        if (DayEndPanel != null)
            DayEndPanel.SetActive(false);

        // Subscribe vào GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDayEnd += ShowDayEndPanel;
            GameManager.Instance.OnGameStart += HideDayEndPanel;
        }

        // Setup button
        if (nextDayButton != null)
            nextDayButton.onClick.AddListener(OnNextDayClicked);
    }

    private void OnDestroy()
    {
        // Unsubscribe
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDayEnd -= ShowDayEndPanel;
            GameManager.Instance.OnGameStart -= HideDayEndPanel;
        }

        if (nextDayButton != null)
            nextDayButton.onClick.RemoveListener(OnNextDayClicked);
    }

    /// <summary>
    /// Hiển thị panel kết thúc ngày với thống kê
    /// </summary>
    public void ShowDayEndPanel()
    {
        if (dayEndPanelVFX == null)
        {
            Debug.LogWarning("[MainGameButtons] Day End Panel VFX chưa được gán!");
            return;
        }

        if (isPanelVisible) return;
        isPanelVisible = true;

        // Lấy thống kê từ ScoreManager
        DayStatistics stats = new DayStatistics();
        if (GameManager.Instance != null && GameManager.Instance.Score != null)
        {
            stats = GameManager.Instance.Score.GetDayStatistics();
        }

        // Cập nhật UI
        UpdateDayEndUI(stats);

        // Hiển thị panel với animation
        DayEndPanel.SetActive(true);
        dayEndPanelVFX.FadeInUI(() =>
        {
            Debug.Log("[MainGameButtons] Day End Panel fade in complete");
        });

        Debug.Log("[MainGameButtons] Hiển thị Day End Panel");
    }

    /// <summary>
    /// Ẩn panel kết thúc ngày
    /// </summary>
    public void HideDayEndPanel()
    {
        if (dayEndPanelVFX == null) return;
        if (!isPanelVisible) return;

        dayEndPanelVFX.FadeOutUI(() =>
        {
            DayEndPanel.SetActive(false);
            isPanelVisible = false;
            Debug.Log("[MainGameButtons] Day End Panel fade out complete");
        });

        Debug.Log("[MainGameButtons] Ẩn Day End Panel");
    }

    /// <summary>
    /// Cập nhật các text UI với thống kê
    /// </summary>
    private void UpdateDayEndUI(DayStatistics stats)
    {
        if (dayText != null)
            dayText.text = $"Ngày {stats.day}";

        if (playTimeText != null)
            playTimeText.text = $"Thời gian chơi: {stats.GetPlayTimeString()}";

        if (interactionsText != null)
            interactionsText.text = $"Số lượt tương tác: {stats.totalInteractions}";

        if (correctWrongText != null)
            correctWrongText.text = $"Trả lời đúng: {stats.correctAnswers} - Sai: {stats.wrongAnswers}";

        if (scoreText != null)
            scoreText.text = $"Điểm: {stats.totalScore}";
    }

    /// <summary>
    /// Xử lý khi nhấn nút Next Day
    /// </summary>
    private void OnNextDayClicked()
    {
        Debug.Log("[MainGameButtons] Next Day clicked - Chức năng chưa được implement");
        
        // TODO: Implement khi cần
        // if (GameManager.Instance != null)
        // {
        //     if (GameManager.Instance.Score != null)
        //         GameManager.Instance.Score.NextDay();
        //     GameManager.Instance.StartNewDay();
        // }
    }

    #region Public Methods for External Calls

    /// <summary>
    /// Hiển thị panel với custom stats (dùng cho testing)
    /// </summary>
    public void ShowDayEndPanel(DayStatistics customStats)
    {
        if (dayEndPanelVFX == null) return;

        isPanelVisible = true;
        UpdateDayEndUI(customStats);
        DayEndPanel.SetActive(true);
        dayEndPanelVFX.FadeInUI();
    }

    #endregion
}
