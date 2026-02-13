using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// BuildModeInventoryUI - Quản lý toàn bộ UI inventory trong Build Mode
/// Chứa 3 categories và xử lý selection giữa các category
/// </summary>
public class BuildModeInventoryUI : MonoBehaviour
{
    #region Singleton
    public static BuildModeInventoryUI Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private List<BuildModeCategoryUI> categories = new List<BuildModeCategoryUI>();
    
    [Header("Item Prefab")]
    [SerializeField] private GameObject itemPrefab;
    
    [Header("Auto Populate")]
    [SerializeField] private bool autoPopulateFromSelector = true;
    #endregion

    #region Private Fields
    private BuildModeCategoryUI _currentSelectedCategory;
    private BuildModeItemUI _currentSelectedItem;
    #endregion

    #region Properties
    public BuildModeItemUI CurrentSelectedItem => _currentSelectedItem;
    public bool IsVisible => inventoryPanel != null && inventoryPanel.activeSelf;
    #endregion

    #region Events
    public System.Action<BuildableObject> OnBuildableSelected;
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
        
        // Setup categories trong Awake để chạy trước Start của các category
        SetupCategories();
    }

    private void Start()
    {
        RegisterEvents();
        
        if (autoPopulateFromSelector)
        {
            PopulateFromObjectSelector();
        }
        
        // Ẩn panel ban đầu
        Hide();
    }

    private void OnDestroy()
    {
        UnregisterEvents();
    }
    #endregion

    #region Setup
    private void SetupCategories()
    {
        for (int i = 0; i < categories.Count; i++)
        {
            var category = categories[i];
            if (category == null) continue;
            
            category.OnItemSelected += HandleItemSelected;
            category.OnCategoryExpanded += HandleCategoryExpanded;
            
            // Mặc định: mở category đầu tiên, đóng các category khác
            if (i == 0)
            {
                category.SetExpanded(true);
            }
            else
            {
                category.SetExpanded(false);
            }
        }
    }

    private void RegisterEvents()
    {
        if (BuilderMode.Instance != null)
        {
            BuilderMode.Instance.OnEnterBuildMode.AddListener(Show);
            BuilderMode.Instance.OnExitBuildMode.AddListener(Hide);
        }
        
        if (BuildModeObjectSelector.Instance != null)
        {
            BuildModeObjectSelector.Instance.OnObjectSelected += HandleObjectSelectedByHotkey;
            BuildModeObjectSelector.Instance.OnSelectionCleared += HandleSelectionCleared;
        }
    }

    private void UnregisterEvents()
    {
        if (BuilderMode.Instance != null)
        {
            BuilderMode.Instance.OnEnterBuildMode.RemoveListener(Show);
            BuilderMode.Instance.OnExitBuildMode.RemoveListener(Hide);
        }
        
        if (BuildModeObjectSelector.Instance != null)
        {
            BuildModeObjectSelector.Instance.OnObjectSelected -= HandleObjectSelectedByHotkey;
            BuildModeObjectSelector.Instance.OnSelectionCleared -= HandleSelectionCleared;
        }
        
        // Unsubscribe từ categories
        foreach (var category in categories)
        {
            if (category == null) continue;
            category.OnItemSelected -= HandleItemSelected;
            category.OnCategoryExpanded -= HandleCategoryExpanded;
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Hiện inventory panel
    /// </summary>
    public void Show()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Ẩn inventory panel
    /// </summary>
    public void Hide()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        
        // Ẩn tooltip khi đóng
        if (BuildModeTooltip.Instance != null)
        {
            BuildModeTooltip.Instance.Hide();
        }
    }

    /// <summary>
    /// Populate tất cả categories từ BuildModeObjectSelector
    /// </summary>
    public void PopulateFromObjectSelector()
    {
        if (BuildModeObjectSelector.Instance == null) return;
        
        var allBuildables = BuildModeObjectSelector.Instance.BuildableObjects;
        PopulateCategories(allBuildables);
    }

    /// <summary>
    /// Populate categories với danh sách BuildableObject
    /// </summary>
    public void PopulateCategories(List<BuildableObject> buildableObjects)
    {
        foreach (var category in categories)
        {
            if (category == null) continue;
            category.PopulateItems(buildableObjects);
        }
    }

    /// <summary>
    /// Clear selection trong tất cả categories
    /// </summary>
    public void ClearAllSelections()
    {
        foreach (var category in categories)
        {
            if (category != null)
            {
                category.ClearSelection();
            }
        }
        
        _currentSelectedItem = null;
        _currentSelectedCategory = null;
    }

    /// <summary>
    /// Refresh lại UI
    /// </summary>
    public void Refresh()
    {
        ClearAllSelections();
        PopulateFromObjectSelector();
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Xử lý khi một category được mở - đóng các category khác
    /// </summary>
    private void HandleCategoryExpanded(BuildModeCategoryUI expandedCategory)
    {
        foreach (var category in categories)
        {
            if (category == null) continue;
            
            // Đóng tất cả category khác
            if (category != expandedCategory)
            {
                category.Collapse();
            }
        }
    }
    
    private void HandleItemSelected(BuildModeItemUI item, BuildModeCategoryUI category)
    {
        // Clear selection từ category khác
        if (_currentSelectedCategory != null && _currentSelectedCategory != category)
        {
            _currentSelectedCategory.ClearSelection();
        }
        
        _currentSelectedCategory = category;
        _currentSelectedItem = item;
        
        if (item != null && item.BuildableObject != null)
        {
            OnBuildableSelected?.Invoke(item.BuildableObject);
        }
    }

    /// <summary>
    /// Xử lý khi object được chọn bằng hotkey (1-9)
    /// Cập nhật UI để highlight item tương ứng
    /// </summary>
    private void HandleObjectSelectedByHotkey(BuildableObject buildable)
    {
        if (buildable == null) return;
        
        // Tìm và highlight item trong UI
        ClearAllSelections();
        
        foreach (var category in categories)
        {
            if (category == null || category.Category != buildable.category) continue;
            
            foreach (var item in category.Items)
            {
                if (item.BuildableObject == buildable)
                {
                    item.SetSelected(true);
                    _currentSelectedItem = item;
                    _currentSelectedCategory = category;
                    return;
                }
            }
        }
    }

    private void HandleSelectionCleared()
    {
        ClearAllSelections();
    }
    #endregion
}
