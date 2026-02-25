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
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI moneyEarnedText;
    [SerializeField] private Button nextDayButton;
    
    [Header("Scene Transition")]
    [SerializeField] private LogoSceneTransition logoTransition;
    
    [Header("Player References")]
    [SerializeField] private GameObject playerObject; // Player để disable/enable

    private bool isPanelVisible = false;
    
    // Property để truy cập GameObject của panel
    private GameObject DayEndPanel => dayEndPanelVFX != null ? dayEndPanelVFX.gameObject : null;

    private void Start()
    {
        // Auto-find references nếu chưa gán
        if (logoTransition == null)
            logoTransition = FindAnyObjectByType<LogoSceneTransition>();
            
        if (playerObject == null)
        {
            var player = FindAnyObjectByType<PlayerController>();
            if (player != null)
                playerObject = player.gameObject;
        }
        
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

        // Lấy thống kê từ MoneyManager
        DayStatistics stats = new DayStatistics();
        if (GameManager.Instance != null && GameManager.Instance.Money != null)
        {
            stats = GameManager.Instance.Money.GetDayStatistics();
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

        if (moneyText != null)
            moneyText.text = $"Tổng tiền: {stats.totalMoney}";
            
        if (moneyEarnedText != null)
            moneyEarnedText.text = $"Kiếm được hôm nay: +{stats.moneyEarnedToday}";
    }

    /// <summary>
    /// Xử lý khi nhấn nút Next Day
    /// </summary>
    private void OnNextDayClicked()
    {
        Debug.Log("[MainGameButtons] Next Day clicked - Bắt đầu transition");
        
        // Disable player input
        SetPlayerEnabled(false);
        
        // Ẩn panel day end
        if (dayEndPanelVFX != null)
        {
            dayEndPanelVFX.FadeOutUI(() =>
            {
                DayEndPanel.SetActive(false);
                isPanelVisible = false;
            });
        }
        
        // Phát transition animation (full: zoom in -> hold -> zoom out)
        if (logoTransition != null && logoTransition.IsReady())
        {
            logoTransition.PlayFullTransition(() =>
            {
                // Callback sau khi transition hoàn tất
                OnTransitionComplete();
            });
        }
        else
        {
            // Fallback nếu không có transition
            Debug.LogWarning("[MainGameButtons] LogoSceneTransition không sẵn sàng, skip transition");
            OnTransitionComplete();
        }
    }
    
    /// <summary>
    /// Được gọi sau khi transition hoàn tất
    /// </summary>
    private void OnTransitionComplete()
    {
        Debug.Log("[MainGameButtons] Transition hoàn tất - Bắt đầu ngày mới");
        
        // Reset thời gian về sáng
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.StartNewDay();
        }
        
        // Auto save
        if (GameManager.Instance != null && GameManager.Instance.SaveLoad != null)
        {
            GameManager.Instance.SaveLoad.AutoSave();
            Debug.Log("[MainGameButtons] 💾 Auto saved!");
        }
        
        // Tăng ngày trong MoneyManager
        if (GameManager.Instance != null && GameManager.Instance.Money != null)
        {
            GameManager.Instance.Money.NextDay();
        }
        
        // Bắt đầu ngày mới (reset NPCs, etc.)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewDay();
        }
        
        // Enable player lại
        SetPlayerEnabled(true);
        
        Debug.Log("🌅 [MainGameButtons] Ngày mới đã bắt đầu!");
    }
    
    /// <summary>
    /// Enable/Disable player
    /// </summary>
    private void SetPlayerEnabled(bool enabled)
    {
        if (playerObject != null)
        {
            // Disable/Enable các component điều khiển
            var controller = playerObject.GetComponent<CharacterController>();
            if (controller != null)
                controller.enabled = enabled;
            
            var playerInput = playerObject.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput != null)
                playerInput.enabled = enabled;
                
            // Hoặc disable toàn bộ script điều khiển
            var playerController = playerObject.GetComponent<PlayerController>();
            if (playerController != null)
                playerController.enabled = enabled;
        }
        
        // Lock/Unlock cursor tương ứng
        if (enabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
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
