using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Unity.AI.Navigation;

/// <summary>
/// BuildModePlacer - Xử lý việc đặt object thật trong Build Mode
/// Bước 5: Place actual object
/// </summary>
public class BuildModePlacer : MonoBehaviour
{
    #region Singleton
    public static BuildModePlacer Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [Header("Placement Settings")]
    [SerializeField] private KeyCode placeKey = KeyCode.Mouse0;     // Phím đặt object (chuột trái)
    [SerializeField] private KeyCode cancelKey = KeyCode.X;         // Phím hủy chọn (X)
    [SerializeField] private Transform placedObjectsParent;          // Parent chứa các object đã đặt
    
    [Header("Audio")]
    [SerializeField] private AudioClip placeSound;
    [SerializeField] private AudioClip removeSound;
    [SerializeField] private AudioClip errorSound;
    
    [Header("NavMesh")]
    [Tooltip("NavMeshSurface để rebake khi đặt/xóa object")]
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private bool rebakeNavMeshOnChange = true;
    
    [Header("Debug")]
    [SerializeField] private bool logPlacements = true;
    #endregion

    #region Private Fields
    private List<PlacedObjectData> _placedObjects = new List<PlacedObjectData>();
    private AudioSource _audioSource;
    private bool _isInMoveMode = false; // Khi đang move thì không tốn tiền
    
    [Header("Refund Settings")]
    [SerializeField] private float refundPercentage = 0.75f; // Hoàn 75% tiền khi xóa
    #endregion

    #region Properties
    public List<PlacedObjectData> PlacedObjects => _placedObjects;
    public int PlacedCount => _placedObjects.Count;
    public bool IsInMoveMode => _isInMoveMode;
    #endregion

    #region Events
    public System.Action<GameObject, BuildableObject> OnObjectPlaced;
    public System.Action<GameObject> OnObjectRemoved;
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

        // Tạo parent nếu chưa có
        if (placedObjectsParent == null)
        {
            GameObject parent = new GameObject("PlacedObjects");
            placedObjectsParent = parent.transform;
        }
    }

    private void Update()
    {
        if (BuilderMode.Instance == null || !BuilderMode.Instance.IsInBuildMode)
        {
            return;
        }

        HandlePlacementInput();
        HandleCancelInput();
    }
    #endregion

    #region Input Handling
    private void HandlePlacementInput()
    {
        // Không place nếu đang click vào UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        
        // Không place nếu context menu đang mở
        if (BuildModeContextMenu.Instance != null && BuildModeContextMenu.Instance.IsMenuOpen)
        {
            return;
        }
        
        if (Input.GetKeyDown(placeKey))
        {
            TryPlaceObject();
        }
    }

    private void HandleCancelInput()
    {
        // Không xử lý nếu đang click vào UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        
        if (Input.GetKeyDown(cancelKey))
        {
            // Nếu đang chọn object thì hủy chọn
            if (BuildModeObjectSelector.Instance != null && BuildModeObjectSelector.Instance.HasSelection)
            {
                BuildModeObjectSelector.Instance.ClearSelection();
                if (logPlacements) Debug.Log("<color=yellow>[BuildModePlacer]</color> Đã hủy chọn object");
            }
        }
    }
    #endregion

    #region Placement Methods
    /// <summary>
    /// Thử đặt object tại vị trí hiện tại
    /// </summary>
    public bool TryPlaceObject()
    {
        // Kiểm tra có đang chọn object không
        if (BuildModeObjectSelector.Instance == null || !BuildModeObjectSelector.Instance.HasSelection)
        {
            if (logPlacements) Debug.Log("<color=yellow>[BuildModePlacer]</color> Chưa chọn object để đặt");
            return false;
        }

        // Kiểm tra có preview không
        if (BuildModePreview.Instance == null || !BuildModePreview.Instance.HasPreview)
        {
            if (logPlacements) Debug.Log("<color=yellow>[BuildModePlacer]</color> Không có preview");
            return false;
        }

        // Kiểm tra vị trí có hợp lệ không
        if (!BuildModePreview.Instance.IsPlacementValid)
        {
            PlaySound(errorSound);
            if (logPlacements) Debug.Log("<color=red>[BuildModePlacer]</color> Vị trí không hợp lệ");
            return false;
        }

        // Lấy thông tin placement
        PlacementInfo placementInfo = BuildModePreview.Instance.GetCurrentPlacementInfo();
        BuildableObject buildableObject = BuildModeObjectSelector.Instance.SelectedObject;

        if (buildableObject == null || buildableObject.prefab == null)
        {
            if (logPlacements) Debug.Log("<color=red>[BuildModePlacer]</color> Không có prefab để đặt");
            return false;
        }

        // Kiểm tra đủ tiền không (bỏ qua nếu đang move)
        if (!_isInMoveMode && MoneyManager.Instance != null)
        {
            if (!MoneyManager.Instance.SpendMoney(buildableObject.price))
            {
                PlaySound(errorSound);
                if (logPlacements) Debug.Log($"<color=red>[BuildModePlacer]</color> Không đủ tiền! Cần: {buildableObject.price}");
                return false;
            }
        }

        // Đặt object
        GameObject placedObject = PlaceObject(buildableObject, placementInfo.position, placementInfo.rotation);

        if (placedObject != null)
        {
            PlaySound(placeSound);
            
            if (logPlacements)
            {
                Debug.Log($"<color=green>[BuildModePlacer]</color> Đã đặt {buildableObject.objectName} tại {placementInfo.position} (Giá: {buildableObject.price}$)");
            }

            OnObjectPlaced?.Invoke(placedObject, buildableObject);
            
            // Rebake NavMesh để NPC tránh vật thể mới
            RebakeNavMesh();
            
            // Clear selection sau khi đặt (single placement mode)
            BuildModeObjectSelector.Instance.ClearSelection();
            
            return true;
        }

        return false;
    }

    /// <summary>
    /// Đặt object với thông tin cụ thể
    /// </summary>
    public GameObject PlaceObject(BuildableObject buildableObject, Vector3 position, Quaternion rotation)
    {
        if (buildableObject == null || buildableObject.prefab == null)
        {
            return null;
        }

        // Áp dụng placement offset
        Vector3 finalPosition = position + rotation * buildableObject.placementOffset;

        // Instantiate object
        GameObject placedObject = Instantiate(buildableObject.prefab, finalPosition, rotation, placedObjectsParent);
        placedObject.name = $"{buildableObject.objectName}_{_placedObjects.Count}";

        // Lưu thông tin
        PlacedObjectData data = new PlacedObjectData
        {
            gameObject = placedObject,
            buildableObject = buildableObject,
            position = finalPosition,
            rotation = rotation,
            timestamp = Time.time
        };
        _placedObjects.Add(data);
        
        // Thêm marker để dễ detect
        var marker = placedObject.AddComponent<PlacedObjectMarker>();
        marker.Data = data;

        // Đánh dấu DisplayArea là đã được build (nếu có)
        var displayArea = placedObject.GetComponentInChildren<DisplayArea>();
        if (displayArea != null)
        {
            displayArea.MarkAsBuilt();
            if (logPlacements) Debug.Log($"<color=cyan>[BuildModePlacer]</color> Đánh dấu DisplayArea đã build: {placedObject.name}");
            
            // Refresh display cache để NPC có thể thấy display mới
            var npcManager = FindFirstObjectByType<NPCManager>();
            if (npcManager != null)
            {
                npcManager.RefreshDisplayCache();
            }
        }

        return placedObject;
    }

    /// <summary>
    /// Bật/tắt chế độ move (khi move thì đặt không tốn tiền)
    /// </summary>
    public void SetMoveMode(bool isMoving)
    {
        _isInMoveMode = isMoving;
        if (logPlacements) Debug.Log($"<color=magenta>[BuildModePlacer]</color> Move mode: {isMoving}");
    }

    /// <summary>
    /// Đặt object mà không tốn tiền (dùng khi load từ save)
    /// </summary>
    public GameObject PlaceObjectWithoutCost(BuildableObject buildableObject, Vector3 position, Quaternion rotation)
    {
        // Gọi PlaceObject trực tiếp mà không qua TryPlaceObject (không check/spend money)
        GameObject obj = PlaceObject(buildableObject, position, rotation);
        
        if (obj != null && logPlacements)
        {
            Debug.Log($"<color=blue>[BuildModePlacer]</color> Loaded từ save: {buildableObject.objectName}");
        }
        
        return obj;
    }

    /// <summary>
    /// Thử xóa object tại vị trí chuột
    /// </summary>
    public bool TryRemoveObject()
    {
        if (BuildModeRaycaster.Instance == null)
        {
            return false;
        }

        // Raycast để tìm object đã đặt
        Ray ray = BuildModeRaycaster.Instance.BuildCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            // Dùng PlacedObjectMarker để tìm object
            var marker = PlacedObjectMarker.GetFromObject(hit.collider.gameObject);
            if (marker != null && marker.Data != null)
            {
                RemoveObject(marker.Data);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Xóa object đã đặt
    /// </summary>
    /// <param name="refund">Có hoàn tiền không (mặc định: true)</param>
    public void RemoveObject(PlacedObjectData data, bool refund = true)
    {
        if (data == null || data.gameObject == null)
        {
            return;
        }

        _placedObjects.Remove(data);
        
        // Hoàn tiền khi xóa object
        if (refund && data.buildableObject != null && MoneyManager.Instance != null)
        {
            int refundAmount = Mathf.RoundToInt(data.buildableObject.price * refundPercentage);
            MoneyManager.Instance.AddMoney(refundAmount);
            if (logPlacements) Debug.Log($"<color=green>[BuildModePlacer]</color> Hoàn {refundAmount}$ ({refundPercentage * 100}% của {data.buildableObject.price}$)");
        }
        
        PlaySound(removeSound);
        
        if (logPlacements)
        {
            Debug.Log($"<color=orange>[BuildModePlacer]</color> Đã xóa {data.buildableObject?.objectName}");
        }

        OnObjectRemoved?.Invoke(data.gameObject);
        
        // Rebake NavMesh sau khi xóa vật thể
        RebakeNavMesh();
        
        Destroy(data.gameObject);
    }

    /// <summary>
    /// Xóa object theo GameObject
    /// </summary>
    public void RemoveObject(GameObject obj)
    {
        PlacedObjectData data = _placedObjects.Find(x => x.gameObject == obj);
        if (data != null)
        {
            RemoveObject(data);
        }
    }
    #endregion

    #region Utility Methods
    /// <summary>
    /// Xóa tất cả object đã đặt
    /// </summary>
    public void ClearAllPlacedObjects()
    {
        foreach (var data in _placedObjects)
        {
            if (data.gameObject != null)
            {
                Destroy(data.gameObject);
            }
        }
        _placedObjects.Clear();

        if (logPlacements)
        {
            Debug.Log("<color=orange>[BuildModePlacer]</color> Đã xóa tất cả objects");
        }
    }

    /// <summary>
    /// Undo đặt object cuối cùng
    /// </summary>
    public void UndoLastPlacement()
    {
        if (_placedObjects.Count == 0) return;

        PlacedObjectData lastData = _placedObjects[_placedObjects.Count - 1];
        RemoveObject(lastData);
    }

    /// <summary>
    /// Rebake NavMesh để NPC tránh vật thể mới đặt/xóa
    /// </summary>
    private void RebakeNavMesh()
    {
        if (!rebakeNavMeshOnChange) return;
        
        // Tự động tìm NavMeshSurface nếu chưa gán
        if (navMeshSurface == null)
        {
            navMeshSurface = FindFirstObjectByType<NavMeshSurface>();
        }
        
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            if (logPlacements) Debug.Log("<color=cyan>[BuildModePlacer]</color> NavMesh đã được rebake");
        }
        else
        {
            if (logPlacements) Debug.LogWarning("<color=yellow>[BuildModePlacer]</color> Không tìm thấy NavMeshSurface để rebake");
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }
    #endregion

    #region Data Classes
    [System.Serializable]
    public class PlacedObjectData
    {
        public GameObject gameObject;
        public BuildableObject buildableObject;
        public Vector3 position;
        public Quaternion rotation;
        public float timestamp;
    }
    #endregion
}
