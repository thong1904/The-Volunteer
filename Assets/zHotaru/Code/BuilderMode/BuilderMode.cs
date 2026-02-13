using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// BuilderMode - Script chính quản lý Build Mode
/// Bước 1: Chuyển đổi giữa Normal Mode và Build Mode bằng phím B
/// </summary>
public class BuilderMode : MonoBehaviour
{
    #region Singleton
    public static BuilderMode Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [Header("Build Mode Settings")]
    [SerializeField] private KeyCode toggleBuildModeKey = KeyCode.B;
    [SerializeField] private bool startInBuildMode = false;
    
    [Header("References")]
    [SerializeField] private GameObject buildModeUI;           // UI hiển thị khi vào Build Mode
    [SerializeField] private GameObject normalModeUI;          // UI bình thường (ẩn khi vào Build Mode)
    [SerializeField] private MonoBehaviour[] scriptsToDisable; // Các script cần disable khi vào Build Mode (VD: PlayerController)
    
    [Header("Camera Settings")]
    [SerializeField] private Camera normalCamera;              // Camera chính (Player camera)
    [SerializeField] private Camera buildModeCamera;           // Camera riêng cho Build Mode
    [SerializeField] private GameObject normalCameraObject;    // GameObject của camera thường (để bật/tắt)
    
    [Header("Events")]
    public UnityEvent OnEnterBuildMode;   // Event khi vào Build Mode
    public UnityEvent OnExitBuildMode;    // Event khi thoát Build Mode
    #endregion

    #region Private Fields
    private bool _isInBuildMode = false;
    #endregion

    #region Properties
    /// <summary>
    /// Kiểm tra xem có đang ở Build Mode không
    /// </summary>
    public bool IsInBuildMode => _isInBuildMode;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Khởi tạo trạng thái ban đầu
        if (startInBuildMode)
        {
            EnterBuildMode();
        }
        else
        {
            ExitBuildMode();
        }
    }

    private void Update()
    {
        HandleInput();
    }
    #endregion

    #region Input Handling
    /// <summary>
    /// Xử lý input từ người chơi
    /// </summary>
    private void HandleInput()
    {
        // Nhấn phím B để toggle Build Mode
        if (Input.GetKeyDown(toggleBuildModeKey))
        {
            ToggleBuildMode();
        }
    }
    #endregion

    #region Build Mode Control
    /// <summary>
    /// Toggle giữa Build Mode và Normal Mode
    /// </summary>
    public void ToggleBuildMode()
    {
        if (_isInBuildMode)
        {
            ExitBuildMode();
        }
        else
        {
            EnterBuildMode();
        }
    }

    /// <summary>
    /// Vào Build Mode
    /// </summary>
    public void EnterBuildMode()
    {
        if (_isInBuildMode) return;
        
        _isInBuildMode = true;
        
        // Hiển thị UI Build Mode
        if (buildModeUI != null)
        {
            buildModeUI.SetActive(true);
        }
        
        // Ẩn UI Normal Mode
        if (normalModeUI != null)
        {
            normalModeUI.SetActive(false);
        }
        
        // Disable các script khác (VD: Player Controller)
        SetScriptsEnabled(false);
        
        // Chuyển đổi camera
        SwitchCamera(true);
        
        // Set camera cho Raycaster
        if (BuildModeRaycaster.Instance != null && buildModeCamera != null)
        {
            BuildModeRaycaster.Instance.SetCamera(buildModeCamera);
        }
        
        // Hiện con trỏ chuột
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.UnlockCursor();
        }
        
        // Hiển thị grid
        if (BuildModeGridVisualizer.Instance != null)
        {
            BuildModeGridVisualizer.Instance.ShowGrid();
        }
        
        // Gọi event
        OnEnterBuildMode?.Invoke();
        
        Debug.Log("<color=green>[BuilderMode]</color> Đã vào Build Mode");
    }

    /// <summary>
    /// Thoát Build Mode
    /// </summary>
    public void ExitBuildMode()
    {
        if (!_isInBuildMode && !startInBuildMode) return;
        
        _isInBuildMode = false;
        
        // Ẩn UI Build Mode
        if (buildModeUI != null)
        {
            buildModeUI.SetActive(false);
        }
        
        // Hiển thị UI Normal Mode
        if (normalModeUI != null)
        {
            normalModeUI.SetActive(true);
        }
        
        // Enable lại các script
        SetScriptsEnabled(true);
        
        // Chuyển đổi camera
        SwitchCamera(false);
        
        // Ẩn grid
        if (BuildModeGridVisualizer.Instance != null)
        {
            BuildModeGridVisualizer.Instance.HideGrid();
        }
        
        // Ẩn con trỏ chuột
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.LockCursor();
        }
        
        // Gọi event
        OnExitBuildMode?.Invoke();
        
        Debug.Log("<color=yellow>[BuilderMode]</color> Đã thoát Build Mode");
    }

    /// <summary>
    /// Bật/tắt các script được chỉ định
    /// </summary>
    private void SetScriptsEnabled(bool enabled)
    {
        if (scriptsToDisable == null) return;
        
        foreach (var script in scriptsToDisable)
        {
            if (script != null)
            {
                script.enabled = enabled;
            }
        }
    }
    
    /// <summary>
    /// Chuyển đổi giữa camera thường và camera Build Mode
    /// </summary>
    private void SwitchCamera(bool toBuildMode)
    {
        if (toBuildMode)
        {
            // Bật Build Mode Camera
            if (buildModeCamera != null)
            {
                buildModeCamera.gameObject.SetActive(true);
                buildModeCamera.enabled = true;
            }
            
            // Tắt Normal Camera
            if (normalCamera != null)
            {
                normalCamera.enabled = false;
            }
            if (normalCameraObject != null)
            {
                normalCameraObject.SetActive(false);
            }
        }
        else
        {
            // Tắt Build Mode Camera
            if (buildModeCamera != null)
            {
                buildModeCamera.enabled = false;
                buildModeCamera.gameObject.SetActive(false);
            }
            
            // Bật Normal Camera
            if (normalCamera != null)
            {
                normalCamera.enabled = true;
            }
            if (normalCameraObject != null)
            {
                normalCameraObject.SetActive(true);
            }
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Kiểm tra xem có thể đặt object không (sẽ dùng ở bước sau)
    /// </summary>
    public bool CanPlaceObject()
    {
        return _isInBuildMode;
    }
    #endregion
}
