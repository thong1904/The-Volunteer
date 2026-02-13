using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
    [SerializeField] private KeyCode removeKey = KeyCode.X;         // Phím xóa object (X)
    [SerializeField] private Transform placedObjectsParent;          // Parent chứa các object đã đặt
    
    [Header("Audio")]
    [SerializeField] private AudioClip placeSound;
    [SerializeField] private AudioClip removeSound;
    [SerializeField] private AudioClip errorSound;
    
    [Header("Debug")]
    [SerializeField] private bool logPlacements = true;
    #endregion

    #region Private Fields
    private List<PlacedObjectData> _placedObjects = new List<PlacedObjectData>();
    private AudioSource _audioSource;
    #endregion

    #region Properties
    public List<PlacedObjectData> PlacedObjects => _placedObjects;
    public int PlacedCount => _placedObjects.Count;
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
        HandleRemoveInput();
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
        
        if (Input.GetKeyDown(placeKey))
        {
            TryPlaceObject();
        }
    }

    private void HandleRemoveInput()
    {
        // Không remove nếu đang click vào UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        
        if (Input.GetKeyDown(removeKey))
        {
            TryRemoveObject();
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

        // Đặt object
        GameObject placedObject = PlaceObject(buildableObject, placementInfo.position, placementInfo.rotation);

        if (placedObject != null)
        {
            PlaySound(placeSound);
            
            if (logPlacements)
            {
                Debug.Log($"<color=green>[BuildModePlacer]</color> Đã đặt {buildableObject.objectName} tại {placementInfo.position}");
            }

            OnObjectPlaced?.Invoke(placedObject, buildableObject);
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

        return placedObject;
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
            // Tìm trong danh sách đã đặt
            GameObject hitObject = hit.collider.gameObject;
            
            // Tìm root object (có thể hit vào child)
            Transform root = hitObject.transform;
            while (root.parent != null && root.parent != placedObjectsParent)
            {
                root = root.parent;
            }

            // Kiểm tra có phải object đã đặt không
            PlacedObjectData foundData = null;
            foreach (var data in _placedObjects)
            {
                if (data.gameObject == root.gameObject)
                {
                    foundData = data;
                    break;
                }
            }

            if (foundData != null)
            {
                RemoveObject(foundData);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Xóa object đã đặt
    /// </summary>
    public void RemoveObject(PlacedObjectData data)
    {
        if (data == null || data.gameObject == null)
        {
            return;
        }

        _placedObjects.Remove(data);
        
        PlaySound(removeSound);
        
        if (logPlacements)
        {
            Debug.Log($"<color=orange>[BuildModePlacer]</color> Đã xóa {data.buildableObject?.objectName}");
        }

        OnObjectRemoved?.Invoke(data.gameObject);
        
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
