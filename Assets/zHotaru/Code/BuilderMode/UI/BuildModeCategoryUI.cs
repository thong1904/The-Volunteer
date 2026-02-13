using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// BuildModeCategoryUI - Quản lý một category trong Build Mode UI
/// Mỗi category chứa nhiều item có thể build
/// </summary>
public class BuildModeCategoryUI : MonoBehaviour
{
    #region Serialized Fields
    [Header("Category Info")]
    [SerializeField] private BuildCategory category;
    [SerializeField] private string categoryName;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI categoryNameText;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Button expandButton;
    [SerializeField] private GameObject contentPanel;
    
    [Header("Settings")]
    [SerializeField] private bool startExpanded = true;
    #endregion

    #region Private Fields
    private List<BuildModeItemUI> _items = new List<BuildModeItemUI>();
    private bool _isExpanded;
    private BuildModeItemUI _selectedItem;
    private bool _initialized = false;
    #endregion

    #region Properties
    public BuildCategory Category => category;
    public List<BuildModeItemUI> Items => _items;
    public BuildModeItemUI SelectedItem => _selectedItem;
    #endregion

    #region Events
    public System.Action<BuildModeItemUI, BuildModeCategoryUI> OnItemSelected;
    public System.Action<BuildModeCategoryUI> OnCategoryExpanded;  // Gọi khi category được mở
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // Setup button listener sớm
        if (expandButton != null)
        {
            expandButton.onClick.AddListener(Expand);
        }
    }
    
    private void Start()
    {
        SetupUI();
        
        // Chỉ set expand state nếu chưa được khởi tạo bởi BuildModeInventoryUI
        if (!_initialized)
        {
            _isExpanded = startExpanded;
            UpdateExpandState();
            _initialized = true;
        }
    }
    #endregion

    #region Setup
    private void SetupUI()
    {
        if (categoryNameText != null)
        {
            categoryNameText.text = GetCategoryDisplayName();
        }
        // Button listener đã được add trong Awake()
    }
    
    private string GetCategoryDisplayName()
    {
        if (!string.IsNullOrEmpty(categoryName))
        {
            return categoryName;
        }
        
        // Default names based on enum
        return category switch
        {
            BuildCategory.Structure => "Công Trình",
            BuildCategory.Furniture => "Nội Thất",
            BuildCategory.Decoration => "Trang Trí",
            _ => category.ToString()
        };
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Populate items từ danh sách BuildableObject
    /// </summary>
    public void PopulateItems(List<BuildableObject> buildableObjects)
    {
        ClearItems();
        
        if (itemPrefab == null || itemContainer == null) return;
        
        foreach (var buildable in buildableObjects)
        {
            if (buildable == null) continue;
            if (buildable.category != category) continue;
            
            GameObject itemGO = Instantiate(itemPrefab, itemContainer);
            BuildModeItemUI itemUI = itemGO.GetComponent<BuildModeItemUI>();
            
            if (itemUI != null)
            {
                itemUI.Setup(buildable);
                itemUI.OnItemClicked += HandleItemClicked;
                _items.Add(itemUI);
            }
        }
    }
    
    /// <summary>
    /// Clear tất cả items
    /// </summary>
    public void ClearItems()
    {
        foreach (var item in _items)
        {
            if (item != null)
            {
                item.OnItemClicked -= HandleItemClicked;
                Destroy(item.gameObject);
            }
        }
        _items.Clear();
        _selectedItem = null;
    }
    
    /// <summary>
    /// Clear selection trong category này
    /// </summary>
    public void ClearSelection()
    {
        if (_selectedItem != null)
        {
            _selectedItem.SetSelected(false);
            _selectedItem = null;
        }
    }
    
    /// <summary>
    /// Mở panel - dùng cho button click
    /// </summary>
    public void Expand()
    {
        // Thông báo để ẩn các category khác
        if (OnCategoryExpanded != null)
        {
            OnCategoryExpanded.Invoke(this);
        }
        else
        {
            // Fallback: tự tìm và đóng các category khác
            CollapseOtherCategories();
        }
        
        _isExpanded = true;
        UpdateExpandState();
    }
    
    /// <summary>
    /// Đóng panel
    /// </summary>
    public void Collapse()
    {
        if (!_isExpanded) return;
        
        _isExpanded = false;
        UpdateExpandState();
    }
    
    /// <summary>
    /// Fallback: tự tìm và đóng các category khác nếu không có BuildModeInventoryUI
    /// </summary>
    private void CollapseOtherCategories()
    {
        var allCategories = FindObjectsByType<BuildModeCategoryUI>(FindObjectsSortMode.None);
        foreach (var cat in allCategories)
        {
            if (cat != this)
            {
                cat.Collapse();
            }
        }
    }
    
    /// <summary>
    /// Set expand state (dùng cho khởi tạo từ BuildModeInventoryUI)
    /// </summary>
    public void SetExpanded(bool expanded)
    {
        _isExpanded = expanded;
        _initialized = true;  // Đánh dấu đã được khởi tạo
        UpdateExpandState();
    }
    
    /// <summary>
    /// Kiểm tra đang mở không
    /// </summary>
    public bool IsExpanded => _isExpanded;
    #endregion

    #region Private Methods
    private void HandleItemClicked(BuildModeItemUI item)
    {
        // Deselect previous
        if (_selectedItem != null && _selectedItem != item)
        {
            _selectedItem.SetSelected(false);
        }
        
        // Select new
        _selectedItem = item;
        _selectedItem.SetSelected(true);
        
        OnItemSelected?.Invoke(item, this);
    }
    
    private void UpdateExpandState()
    {
        if (contentPanel != null)
        {
            contentPanel.SetActive(_isExpanded);
        }
    }
    #endregion
}
