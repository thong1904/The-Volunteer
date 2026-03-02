using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// BuildModeItemUI - Component cho mỗi item button trong Build Mode UI
/// Xử lý click để chọn object và hover để hiện tooltip
/// </summary>
public class BuildModeItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image borderImage;
    
    [Header("Colors")]
    [SerializeField] private Color borderNormalColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] private Color borderHoverColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color borderSelectedColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color iconNormalColor = Color.white;
    [SerializeField] private Color iconNotAffordableColor = new Color(1f, 0.3f, 0.3f, 0.7f); // Màu đỏ khi không đủ tiền
    
    [Header("Audio")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    #endregion

    #region Private Fields
    private BuildableObject _buildableObject;
    private bool _isSelected;
    private bool _isHovered;
    private AudioSource _audioSource;
    #endregion

    #region Properties
    public BuildableObject BuildableObject => _buildableObject;
    public bool IsSelected => _isSelected;
    #endregion

    #region Events
    public System.Action<BuildModeItemUI> OnItemClicked;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
    }

    private void OnDisable()
    {
        // Ẩn tooltip khi disable
        if (_isHovered && BuildModeTooltip.Instance != null)
        {
            BuildModeTooltip.Instance.Hide();
        }
        _isHovered = false;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Setup item với BuildableObject
    /// </summary>
    public void Setup(BuildableObject buildableObject)
    {
        _buildableObject = buildableObject;
        
        if (iconImage != null && buildableObject != null)
        {
            iconImage.sprite = buildableObject.icon;
            iconImage.gameObject.SetActive(buildableObject.icon != null);
        }
        
        UpdateVisuals();
    }
    
    /// <summary>
    /// Set trạng thái selected
    /// </summary>
    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateVisuals();
    }
    #endregion

    #region Pointer Events
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"<color=cyan>[BuildModeItemUI]</color> OnPointerEnter: {_buildableObject?.objectName}");
        
        _isHovered = true;
        UpdateVisuals();
        
        // Hiện tooltip
        if (_buildableObject != null)
        {
            var tooltip = BuildModeTooltip.Instance;
            Debug.Log($"<color=cyan>[BuildModeItemUI]</color> Tooltip Instance: {(tooltip != null ? "Exists" : "NULL")}");
            
            if (tooltip == null)
            {
                tooltip = FindAnyObjectByType<BuildModeTooltip>();
                Debug.Log($"<color=cyan>[BuildModeItemUI]</color> FindAnyObjectByType: {(tooltip != null ? "Found" : "Not Found")}");
            }
            
            if (tooltip != null)
            {
                Debug.Log($"<color=green>[BuildModeItemUI]</color> Calling ShowImmediate...");
                tooltip.ShowImmediate(_buildableObject);
            }
            else
            {
                Debug.LogError("<color=red>[BuildModeItemUI]</color> BuildModeTooltip not found in scene!");
            }
        }
        else
        {
            Debug.LogWarning("<color=yellow>[BuildModeItemUI]</color> _buildableObject is null");
        }
        
        PlaySound(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"<color=orange>[BuildModeItemUI]</color> OnPointerExit: {_buildableObject?.objectName}");
        
        _isHovered = false;
        UpdateVisuals();
        
        // Ẩn tooltip
        var tooltip = BuildModeTooltip.Instance;
        if (tooltip == null)
        {
            tooltip = FindAnyObjectByType<BuildModeTooltip>();
        }
        
        if (tooltip != null)
        {
            tooltip.Hide();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        
        // Không cho chọn nếu không đủ tiền
        if (!CanAfford())
        {
            Debug.Log($"<color=red>[BuildModeItemUI]</color> Không đủ tiền để chọn {_buildableObject?.objectName}");
            return;
        }
        
        PlaySound(clickSound);
        
        // Chọn object trong BuildModeObjectSelector
        if (BuildModeObjectSelector.Instance != null && _buildableObject != null)
        {
            BuildModeObjectSelector.Instance.SelectObject(_buildableObject);
        }
        
        OnItemClicked?.Invoke(this);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Cập nhật visual dựa trên trạng thái
    /// </summary>
    private void UpdateVisuals()
    {
        if (borderImage != null)
        {
            if (_isSelected)
            {
                borderImage.color = borderSelectedColor;
            }
            else if (_isHovered)
            {
                borderImage.color = borderHoverColor;
            }
            else
            {
                borderImage.color = borderNormalColor;
            }
        }
        
        // Cập nhật màu icon dựa trên tiền
        UpdateIconAffordability();
    }
    
    /// <summary>
    /// Cập nhật màu icon dựa trên khả năng mua
    /// </summary>
    private void UpdateIconAffordability()
    {
        if (iconImage == null || _buildableObject == null) return;
        
        bool canAfford = CanAfford();
        iconImage.color = canAfford ? iconNormalColor : iconNotAffordableColor;
    }
    
    /// <summary>
    /// Kiểm tra có đủ tiền không
    /// </summary>
    private bool CanAfford()
    {
        if (_buildableObject == null) return false;
        if (MoneyManager.Instance == null) return true; // Nếu không có MoneyManager thì cho phép
        
        // Nếu đang trong move mode thì luôn cho phép (vì không tốn tiền)
        if (BuildModePlacer.Instance != null && BuildModePlacer.Instance.IsInMoveMode) return true;
        
        return MoneyManager.Instance.GetTotalMoney() >= _buildableObject.price;
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }
    #endregion
}
