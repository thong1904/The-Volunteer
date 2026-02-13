using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// BuildModeObjectSelector - Quản lý việc chọn object để đặt
/// </summary>
public class BuildModeObjectSelector : MonoBehaviour
{
    #region Singleton
    public static BuildModeObjectSelector Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [Header("Buildable Objects")]
    [SerializeField] private List<BuildableObject> buildableObjects = new List<BuildableObject>();
    
    [Header("Quick Select Keys")]
    [SerializeField] private KeyCode[] quickSelectKeys = new KeyCode[]
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5,
        KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0
    };
    
    [Header("Cancel")]
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;
    #endregion

    #region Private Fields
    private BuildableObject _selectedObject;
    private int _selectedIndex = -1;
    #endregion

    #region Properties
    public BuildableObject SelectedObject => _selectedObject;
    public int SelectedIndex => _selectedIndex;
    public List<BuildableObject> BuildableObjects => buildableObjects;
    public bool HasSelection => _selectedObject != null;
    #endregion

    #region Events
    public System.Action<BuildableObject> OnObjectSelected;
    public System.Action OnSelectionCleared;
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
    }

    private void Update()
    {
        if (BuilderMode.Instance == null || !BuilderMode.Instance.IsInBuildMode)
        {
            return;
        }

        HandleQuickSelectInput();
        HandleCancelInput();
    }
    #endregion

    #region Input Handling
    private void HandleQuickSelectInput()
    {
        for (int i = 0; i < quickSelectKeys.Length && i < buildableObjects.Count; i++)
        {
            if (Input.GetKeyDown(quickSelectKeys[i]))
            {
                SelectObject(i);
                break;
            }
        }
    }

    private void HandleCancelInput()
    {
        if (Input.GetKeyDown(cancelKey))
        {
            ClearSelection();
        }
    }
    #endregion

    #region Selection Methods
    /// <summary>
    /// Chọn object theo index
    /// </summary>
    public void SelectObject(int index)
    {
        if (index < 0 || index >= buildableObjects.Count) return;
        
        _selectedIndex = index;
        _selectedObject = buildableObjects[index];
        
        // Bắt đầu preview
        if (BuildModePreview.Instance != null && _selectedObject.prefab != null)
        {
            BuildModePreview.Instance.StartPreview(_selectedObject.prefab);
            BuildModePreview.Instance.SetFootprint(_selectedObject.gridFootprint);
        }
        
        OnObjectSelected?.Invoke(_selectedObject);
    }

    /// <summary>
    /// Chọn object trực tiếp
    /// </summary>
    public void SelectObject(BuildableObject buildable)
    {
        if (buildable == null) return;
        
        int index = buildableObjects.IndexOf(buildable);
        if (index >= 0)
        {
            SelectObject(index);
        }
        else
        {
            // Object không có trong list, vẫn cho chọn
            _selectedIndex = -1;
            _selectedObject = buildable;
            
            if (BuildModePreview.Instance != null && _selectedObject.prefab != null)
            {
                BuildModePreview.Instance.StartPreview(_selectedObject.prefab);
                BuildModePreview.Instance.SetFootprint(_selectedObject.gridFootprint);
            }
            
            OnObjectSelected?.Invoke(_selectedObject);
        }
    }

    /// <summary>
    /// Xóa selection
    /// </summary>
    public void ClearSelection()
    {
        _selectedObject = null;
        _selectedIndex = -1;
        
        if (BuildModePreview.Instance != null)
        {
            BuildModePreview.Instance.ClearPreview();
        }
        
        OnSelectionCleared?.Invoke();
    }

    /// <summary>
    /// Chọn object tiếp theo
    /// </summary>
    public void SelectNext()
    {
        if (buildableObjects.Count == 0) return;
        
        int nextIndex = (_selectedIndex + 1) % buildableObjects.Count;
        SelectObject(nextIndex);
    }

    /// <summary>
    /// Chọn object trước đó
    /// </summary>
    public void SelectPrevious()
    {
        if (buildableObjects.Count == 0) return;
        
        int prevIndex = _selectedIndex - 1;
        if (prevIndex < 0) prevIndex = buildableObjects.Count - 1;
        SelectObject(prevIndex);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Thêm object vào danh sách
    /// </summary>
    public void AddBuildableObject(BuildableObject obj)
    {
        if (obj != null && !buildableObjects.Contains(obj))
        {
            buildableObjects.Add(obj);
        }
    }

    /// <summary>
    /// Xóa object khỏi danh sách
    /// </summary>
    public void RemoveBuildableObject(BuildableObject obj)
    {
        buildableObjects.Remove(obj);
    }

    /// <summary>
    /// Lấy objects theo category
    /// </summary>
    public List<BuildableObject> GetByCategory(BuildCategory category)
    {
        return buildableObjects.FindAll(x => x.category == category);
    }
    #endregion
}
