using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// BuildModeTooltip - Hiển thị tooltip thông tin item khi hover
/// </summary>
public class BuildModeTooltip : MonoBehaviour
{
    #region Singleton
    public static BuildModeTooltip Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI priceText;
    
    [Header("Settings")]
    [SerializeField] private Vector2 offset = new Vector2(20f, -20f);
    [SerializeField] private float showDelay = 0.3f;
    [SerializeField] private string priceFormat = "{0} $";
    
    [Header("Colors")]
    [SerializeField] private Color affordableColor = Color.white;
    [SerializeField] private Color expensiveColor = new Color(1f, 0.5f, 0.5f, 1f);
    #endregion

    #region Private Fields
    private RectTransform _tooltipRect;
    private RectTransform _canvasRect;
    private Canvas _parentCanvas;
    private float _hoverTimer;
    private bool _isWaitingToShow;
    private BuildableObject _pendingObject;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Auto-find tooltipPanel nếu chưa gán
        if (tooltipPanel == null)
        {
            // Tìm child đầu tiên có thể là tooltip panel
            if (transform.childCount > 0)
            {
                tooltipPanel = transform.GetChild(0).gameObject;
            }
        }
        
        if (tooltipPanel != null)
        {
            _tooltipRect = tooltipPanel.GetComponent<RectTransform>();
            _parentCanvas = GetComponentInParent<Canvas>();
            if (_parentCanvas != null)
            {
                _canvasRect = _parentCanvas.GetComponent<RectTransform>();
            }
        }
        else
        {
            Debug.LogWarning("<color=yellow>[BuildModeTooltip]</color> tooltipPanel chưa được gán!");
        }
        
        Hide();
    }

    private void Update()
    {
        if (_isWaitingToShow)
        {
            _hoverTimer += Time.unscaledDeltaTime;
            if (_hoverTimer >= showDelay)
            {
                ShowImmediate(_pendingObject);
                _isWaitingToShow = false;
            }
        }
        
        // Không update position - tooltip ở vị trí cố định
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Hiện tooltip sau delay
    /// </summary>
    public void Show(BuildableObject buildableObject)
    {
        if (buildableObject == null) return;
        
        _pendingObject = buildableObject;
        _hoverTimer = 0f;
        _isWaitingToShow = true;
    }
    
    /// <summary>
    /// Hiện tooltip ngay lập tức
    /// </summary>
    public void ShowImmediate(BuildableObject buildableObject)
    {
        Debug.Log($"<color=green>[BuildModeTooltip]</color> ShowImmediate called for: {buildableObject?.objectName}");
        
        if (buildableObject == null) 
        {
            Debug.LogWarning("<color=yellow>[BuildModeTooltip]</color> buildableObject is null");
            return;
        }
        
        if (tooltipPanel == null)
        {
            Debug.LogError("<color=red>[BuildModeTooltip]</color> tooltipPanel is null! Please assign it in Inspector.");
            return;
        }
        
        _isWaitingToShow = false;
        
        // Update content
        if (itemNameText != null)
        {
            itemNameText.text = buildableObject.objectName;
        }
        else
        {
            Debug.LogWarning("<color=yellow>[BuildModeTooltip]</color> itemNameText is null");
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = buildableObject.description;
        }
        
        if (priceText != null)
        {
            priceText.text = string.Format(priceFormat, buildableObject.price);
            priceText.color = affordableColor;
        }
        
        Debug.Log($"<color=green>[BuildModeTooltip]</color> Activating tooltipPanel...");
        tooltipPanel.SetActive(true);
        
        // Force rebuild layout
        if (_tooltipRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipRect);
        }
        
        Debug.Log($"<color=green>[BuildModeTooltip]</color> tooltipPanel.activeSelf = {tooltipPanel.activeSelf}");
    }
    
    /// <summary>
    /// Ẩn tooltip
    /// </summary>
    public void Hide()
    {
        _isWaitingToShow = false;
        _pendingObject = null;
        
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Update vị trí tooltip theo chuột
    /// </summary>
    private void UpdatePosition()
    {
        if (_tooltipRect == null) return;
        
        Vector2 mousePos = Input.mousePosition;
        Vector2 targetPos = mousePos + offset;
        
        // Clamp để không vượt ra ngoài màn hình
        float tooltipWidth = _tooltipRect.rect.width;
        float tooltipHeight = _tooltipRect.rect.height;
        
        // Check right edge
        if (targetPos.x + tooltipWidth > Screen.width)
        {
            targetPos.x = mousePos.x - tooltipWidth - offset.x;
        }
        
        // Check bottom edge
        if (targetPos.y - tooltipHeight < 0)
        {
            targetPos.y = mousePos.y + tooltipHeight - offset.y;
        }
        
        // Set position trực tiếp (hoạt động với ScreenSpaceOverlay)
        if (_parentCanvas == null || _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            _tooltipRect.position = targetPos;
        }
        else if (_canvasRect != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, 
                targetPos, 
                _parentCanvas.worldCamera, 
                out Vector2 localPoint
            );
            _tooltipRect.localPosition = localPoint;
        }
    }
    #endregion
}
