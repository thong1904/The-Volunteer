using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// BuildModeUIController - Quản lý UI cho Build Mode
/// Hiển thị thông tin và điều khiển trong Build Mode
/// </summary>
public class BuildModeUIController : MonoBehaviour
{
    #region Serialized Fields
    [Header("UI Panels")]
    [SerializeField] private GameObject buildModePanel;
    [SerializeField] private GameObject objectSelectionPanel;
    
    [Header("Text Elements")]
    [SerializeField] private TextMeshProUGUI buildModeStatusText;
    [SerializeField] private TextMeshProUGUI instructionsText;
    [SerializeField] private TextMeshProUGUI selectedObjectText;
    
    [Header("Buttons")]
    [SerializeField] private Button exitBuildModeButton;
    
    [Header("Settings")]
    [SerializeField] private string buildModeActiveText = "BUILD MODE";
    [SerializeField] private Color buildModeActiveColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    #endregion

    #region Private Fields
    private string _instructionsDefault = 
        "Hướng dẫn:\n" +
        "• [B] - Thoát Build Mode\n" +
        "• [WASD] - Di chuyển camera\n" +
        "• [Q/E] - Lên/Xuống\n" +
        "• [Chuột phải] - Xoay camera\n" +
        "• [Scroll] - Zoom\n" +
        "• [R] - Xoay object\n" +
        "• [Click trái] - Đặt object\n" +
        "• [ESC] - Hủy chọn";
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        SetupUI();
        RegisterEvents();
    }

    private void OnDestroy()
    {
        UnregisterEvents();
    }
    #endregion

    #region Setup
    /// <summary>
    /// Khởi tạo UI
    /// </summary>
    private void SetupUI()
    {
        // Cập nhật text hướng dẫn
        if (instructionsText != null)
        {
            instructionsText.text = _instructionsDefault;
        }
        
        // Setup button events
        if (exitBuildModeButton != null)
        {
            exitBuildModeButton.onClick.AddListener(OnExitBuildModeClicked);
        }
        
        // Ẩn panel ban đầu
        if (buildModePanel != null)
        {
            buildModePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Đăng ký events từ BuilderMode
    /// </summary>
    private void RegisterEvents()
    {
        if (BuilderMode.Instance != null)
        {
            BuilderMode.Instance.OnEnterBuildMode.AddListener(OnEnterBuildMode);
            BuilderMode.Instance.OnExitBuildMode.AddListener(OnExitBuildMode);
        }
    }

    /// <summary>
    /// Hủy đăng ký events
    /// </summary>
    private void UnregisterEvents()
    {
        if (BuilderMode.Instance != null)
        {
            BuilderMode.Instance.OnEnterBuildMode.RemoveListener(OnEnterBuildMode);
            BuilderMode.Instance.OnExitBuildMode.RemoveListener(OnExitBuildMode);
        }
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Xử lý khi vào Build Mode
    /// </summary>
    private void OnEnterBuildMode()
    {
        if (buildModePanel != null)
        {
            buildModePanel.SetActive(true);
        }
        
        if (buildModeStatusText != null)
        {
            buildModeStatusText.text = buildModeActiveText;
            buildModeStatusText.color = buildModeActiveColor;
        }
        
        UpdateSelectedObjectText(null);
    }

    /// <summary>
    /// Xử lý khi thoát Build Mode
    /// </summary>
    private void OnExitBuildMode()
    {
        if (buildModePanel != null)
        {
            buildModePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Xử lý khi nhấn nút Exit
    /// </summary>
    private void OnExitBuildModeClicked()
    {
        if (BuilderMode.Instance != null)
        {
            BuilderMode.Instance.ExitBuildMode();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Cập nhật text hiển thị object đang chọn
    /// </summary>
    public void UpdateSelectedObjectText(string objectName)
    {
        if (selectedObjectText != null)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                selectedObjectText.text = "Chưa chọn object";
            }
            else
            {
                selectedObjectText.text = $"Đang chọn: {objectName}";
            }
        }
    }

    /// <summary>
    /// Hiển thị thông báo tạm thời
    /// </summary>
    public void ShowMessage(string message, float duration = 2f)
    {
        // Có thể mở rộng để hiển thị notification
        Debug.Log($"[BuildMode UI] {message}");
    }

    /// <summary>
    /// Hiển thị/ẩn panel chọn object
    /// </summary>
    public void ToggleObjectSelectionPanel(bool show)
    {
        if (objectSelectionPanel != null)
        {
            objectSelectionPanel.SetActive(show);
        }
    }
    #endregion
}
