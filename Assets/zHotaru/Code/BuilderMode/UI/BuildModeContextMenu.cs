using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BuildModeContextMenu - Menu ngữ cảnh khi giữ chuột trên object đã đặt
/// Hiện UI với các option: Di chuyển, Phá hủy
/// </summary>
public class BuildModeContextMenu : MonoBehaviour
{
    #region Singleton
    public static BuildModeContextMenu Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button moveButton;
    [SerializeField] private Button destroyButton;
    
    [Header("Hold Settings")]
    [SerializeField] private float holdDuration = 0.5f;  // Thời gian giữ chuột để hiện menu
    [SerializeField] private KeyCode holdKey = KeyCode.Mouse0;
    
    [Header("Colors")]
    [SerializeField] private Color moveColor = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color destroyColor = new Color(1f, 0.3f, 0.3f);
    
    [Header("Audio")]
    [SerializeField] private AudioClip openMenuSound;
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip destroySound;
    #endregion

    #region Private Fields
    private float _holdTimer = 0f;
    private bool _isHolding = false;
    private bool _isMenuOpen = false;
    private BuildModePlacer.PlacedObjectData _selectedObjectData;
    private AudioSource _audioSource;
    
    // Để phân biệt giữa việc nhấn chuột để đặt object và giữ chuột để mở menu
    private bool _wasOverPlacedObject = false;
    #endregion

    #region Properties
    public bool IsMenuOpen => _isMenuOpen;
    public BuildModePlacer.PlacedObjectData SelectedObject => _selectedObjectData;
    #endregion

    #region Events
    public System.Action<BuildModePlacer.PlacedObjectData> OnMoveRequested;
    public System.Action<BuildModePlacer.PlacedObjectData> OnDestroyRequested;
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

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        SetupButtons();
        HideMenu();
    }

    private void Update()
    {
        if (BuilderMode.Instance == null || !BuilderMode.Instance.IsInBuildMode)
        {
            HideMenu();
            return;
        }

        // Nếu đang có object được chọn để đặt thì không xử lý context menu
        if (BuildModeObjectSelector.Instance != null && BuildModeObjectSelector.Instance.HasSelection)
        {
            return;
        }

        HandleHoldInput();
    }
    #endregion

    #region Setup
    private void SetupButtons()
    {
        if (moveButton != null)
        {
            moveButton.onClick.AddListener(OnMoveButtonClicked);
            
            // Set màu
            var colors = moveButton.colors;
            colors.normalColor = moveColor;
            moveButton.colors = colors;
        }

        if (destroyButton != null)
        {
            destroyButton.onClick.AddListener(OnDestroyButtonClicked);
            
            // Set màu
            var colors = destroyButton.colors;
            colors.normalColor = destroyColor;
            destroyButton.colors = colors;
        }
    }
    #endregion

    #region Input Handling
    private void HandleHoldInput()
    {
        // Bắt đầu giữ chuột
        if (Input.GetKeyDown(holdKey))
        {
            // Kiểm tra xem có đang trỏ vào object đã đặt không
            var hitData = TryGetPlacedObjectUnderMouse();
            if (hitData != null)
            {
                _isHolding = true;
                _holdTimer = 0f;
                _selectedObjectData = hitData;
                _wasOverPlacedObject = true;
            }
            else
            {
                _wasOverPlacedObject = false;
            }
        }

        // Đang giữ chuột
        if (_isHolding && Input.GetKey(holdKey))
        {
            _holdTimer += Time.deltaTime;
            
            if (_holdTimer >= holdDuration && !_isMenuOpen)
            {
                ShowMenu();
            }
        }

        // Thả chuột
        if (Input.GetKeyUp(holdKey))
        {
            _isHolding = false;
            _holdTimer = 0f;
            
            // Không tự đóng menu nếu đang mở - để user click vào button
        }

        // Click ra ngoài để đóng menu
        if (_isMenuOpen && Input.GetMouseButtonDown(1)) // Right click để đóng
        {
            HideMenu();
        }
    }

    /// <summary>
    /// Tìm object đã đặt dưới vị trí chuột
    /// </summary>
    private BuildModePlacer.PlacedObjectData TryGetPlacedObjectUnderMouse()
    {
        if (BuildModeRaycaster.Instance == null)
            return null;

        Camera cam = BuildModeRaycaster.Instance.BuildCamera;
        if (cam == null) return null;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            // Dùng PlacedObjectMarker để tìm object đã đặt
            var marker = PlacedObjectMarker.GetFromObject(hit.collider.gameObject);
            if (marker != null && marker.Data != null)
            {
                return marker.Data;
            }
        }

        return null;
    }
    #endregion

    #region Menu Display
    public void ShowMenu()
    {
        if (_selectedObjectData == null) return;
        
        _isMenuOpen = true;
        
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }

        PlaySound(openMenuSound);
        
        Debug.Log($"<color=cyan>[ContextMenu]</color> Mở menu cho: {_selectedObjectData.buildableObject?.objectName}");
    }

    public void HideMenu()
    {
        _isMenuOpen = false;
        _selectedObjectData = null;
        
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
    }
    #endregion

    #region Button Actions
    private void OnMoveButtonClicked()
    {
        if (_selectedObjectData == null) return;

        Debug.Log($"<color=cyan>[ContextMenu]</color> Di chuyển: {_selectedObjectData.buildableObject?.objectName}");
        
        PlaySound(moveSound);
        
        // Gọi event để BuildModePlacer xử lý
        OnMoveRequested?.Invoke(_selectedObjectData);
        
        // Bắt đầu move mode
        StartMoveMode(_selectedObjectData);
        
        HideMenu();
    }

    private void OnDestroyButtonClicked()
    {
        if (_selectedObjectData == null) return;

        Debug.Log($"<color=cyan>[ContextMenu]</color> Phá hủy: {_selectedObjectData.buildableObject?.objectName}");
        
        PlaySound(destroySound);
        
        // TODO: Khi có money system, thêm hoàn trả 50% giá tiền
        // int refundAmount = _selectedObjectData.buildableObject.price / 2;
        
        // Gọi event
        OnDestroyRequested?.Invoke(_selectedObjectData);
        
        // Xóa object
        if (BuildModePlacer.Instance != null)
        {
            BuildModePlacer.Instance.RemoveObject(_selectedObjectData);
        }
        
        HideMenu();
    }
    #endregion

    #region Move Mode
    private void StartMoveMode(BuildModePlacer.PlacedObjectData objectData)
    {
        if (objectData == null || objectData.buildableObject == null) return;
        
        // Lưu lại object đang move
        var buildableObject = objectData.buildableObject;
        var oldPosition = objectData.position;
        var oldGameObject = objectData.gameObject;
        
        // Xóa object cũ khỏi danh sách (không destroy)
        BuildModePlacer.Instance.PlacedObjects.Remove(objectData);
        
        // Ẩn object cũ
        oldGameObject.SetActive(false);
        
        // Chọn lại object để đặt
        BuildModeObjectSelector.Instance.SelectObject(buildableObject);
        
        // Đăng ký callback khi đặt xong hoặc hủy
        BuildModePlacer.Instance.OnObjectPlaced += OnMoveCompleted;
        BuildModeObjectSelector.Instance.OnSelectionCleared += () => OnMoveCancelled(oldGameObject, objectData);
        
        // Lưu reference để cleanup
        _movingOldObject = oldGameObject;
        _movingOldData = objectData;
    }

    private GameObject _movingOldObject;
    private BuildModePlacer.PlacedObjectData _movingOldData;

    private void OnMoveCompleted(GameObject newObject, BuildableObject buildable)
    {
        // Đặt thành công -> xóa object cũ
        if (_movingOldObject != null)
        {
            Destroy(_movingOldObject);
            _movingOldObject = null;
            _movingOldData = null;
        }
        
        // Hủy đăng ký callback
        BuildModePlacer.Instance.OnObjectPlaced -= OnMoveCompleted;
        
        Debug.Log("<color=green>[ContextMenu]</color> Di chuyển hoàn tất!");
    }

    private void OnMoveCancelled(GameObject oldObject, BuildModePlacer.PlacedObjectData oldData)
    {
        // Hủy move -> hiện lại object cũ
        if (oldObject != null)
        {
            oldObject.SetActive(true);
            
            // Thêm lại vào danh sách
            if (!BuildModePlacer.Instance.PlacedObjects.Contains(oldData))
            {
                BuildModePlacer.Instance.PlacedObjects.Add(oldData);
            }
        }
        
        _movingOldObject = null;
        _movingOldData = null;
        
        Debug.Log("<color=yellow>[ContextMenu]</color> Đã hủy di chuyển");
    }
    #endregion

    #region Utility
    private void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Kiểm tra xem click có phải là click để mở context menu không
    /// (để phân biệt với click để đặt object)
    /// </summary>
    public bool WasHoldingOnPlacedObject()
    {
        return _wasOverPlacedObject && _holdTimer >= holdDuration;
    }
    #endregion
}
