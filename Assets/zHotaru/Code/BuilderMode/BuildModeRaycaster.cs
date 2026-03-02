using UnityEngine;

/// <summary>
/// BuildModeRaycaster - Xử lý Raycast từ chuột xuống ground
/// Bước 2: Xác định vị trí đặt object trong Build Mode
/// </summary>
public class BuildModeRaycaster : MonoBehaviour
{
    #region Singleton
    public static BuildModeRaycaster Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask groundLayer;           // Layer của ground (đất, sàn...)
    [SerializeField] private LayerMask validPlacementLayer;   // Layer cho phép đặt object
    [SerializeField] private float maxRayDistance = 100f;     // Khoảng cách raycast tối đa
    
    [Header("Surface Filter")]
    [SerializeField] private bool filterByNormal = true;      // Chỉ accept mặt phẳng ngang
    [SerializeField] private float minGroundNormalY = 0.7f;   // Normal.Y tối thiểu (0.7 = ~45 độ)
    
    [Header("Grid Settings")]
    [SerializeField] private bool snapToGrid = true;          // Có snap vào grid không
    [SerializeField] private float gridSize = 5f;             // Kích thước mỗi ô grid (default = 5, sync với GridVisualizer)
    
    [Header("References")]
    [SerializeField] private Camera buildCamera;              // Camera Build Mode
    
    [Header("Debug")]
    [SerializeField] private bool showDebugRay = true;        // Hiển thị ray trong Scene view
    #endregion

    #region Private Fields
    private bool _hasValidHit = false;
    private bool _isValidPlacement = false;
    private Vector3 _lastHitPoint;
    private Vector3 _lastGridPosition;
    private RaycastHit _lastHit;
    #endregion

    #region Properties
    /// <summary>
    /// Có đang hit ground không
    /// </summary>
    public bool HasValidHit => _hasValidHit;
    
    /// <summary>
    /// Vị trí hit thực tế (chưa snap grid)
    /// </summary>
    public Vector3 HitPoint => _lastHitPoint;
    
    /// <summary>
    /// Vị trí đã snap vào grid
    /// </summary>
    public Vector3 GridPosition => _lastGridPosition;
    
    /// <summary>
    /// Normal của surface hit
    /// </summary>
    public Vector3 HitNormal => _lastHit.normal;
    
    /// <summary>
    /// GameObject bị hit
    /// </summary>
    public GameObject HitObject => _hasValidHit ? _lastHit.collider.gameObject : null;

    /// <summary>
    /// Có đang hợp lệ để đặt object không
    /// </summary>
    public bool IsValidPlacement => _isValidPlacement;

    /// <summary>
    /// Camera của Build Mode
    /// </summary>
    public Camera BuildCamera => buildCamera;

    /// <summary>
    /// Kích thước ô grid (lấy từ GridVisualizer nếu có)
    /// </summary>
    public float GridSize => BuildModeGridVisualizer.Instance != null 
        ? BuildModeGridVisualizer.Instance.GridSize 
        : gridSize;
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

    private void Start()
    {
        // Tự động lấy Build Camera nếu chưa gán
        if (buildCamera == null && BuilderMode.Instance != null)
        {
            // Tìm camera trong scene
            buildCamera = Camera.main;
        }
    }

    private void Update()
    {
        // Chỉ raycast khi đang ở Build Mode
        if (BuilderMode.Instance == null || !BuilderMode.Instance.IsInBuildMode)
        {
            _hasValidHit = false;
            return;
        }

        PerformRaycast();
    }
    #endregion

    #region Raycast
    /// <summary>
    /// Thực hiện raycast từ vị trí chuột
    /// </summary>
    private void PerformRaycast()
    {
        if (buildCamera == null)
        {
            buildCamera = Camera.main;
            if (buildCamera == null)
            {
                _hasValidHit = false;
                _isValidPlacement = false;
                return;
            }
        }

        Ray ray = buildCamera.ScreenPointToRay(Input.mousePosition);
        
        if (showDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * maxRayDistance, Color.yellow);
        }

        // Raycast và tìm surface phù hợp (mặt phẳng ngang, không phải tường)
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRayDistance, groundLayer);
        
        // Sắp xếp theo khoảng cách (gần nhất trước)
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        
        bool foundValidSurface = false;
        
        foreach (var hit in hits)
        {
            // Kiểm tra normal - chỉ accept mặt phẳng ngang (không phải tường)
            if (filterByNormal && hit.normal.y < minGroundNormalY)
            {
                // Đây là tường hoặc mặt dốc, bỏ qua
                if (showDebugRay)
                {
                    Debug.DrawLine(hit.point, hit.point + hit.normal * 2f, Color.magenta);
                }
                continue;
            }
            
            // Tìm thấy mặt phẳng hợp lệ
            _lastHit = hit;
            _hasValidHit = true;
            _lastHitPoint = hit.point;
            _lastGridPosition = snapToGrid ? SnapToGrid(_lastHitPoint) : _lastHitPoint;
            
            // Kiểm tra có phải valid placement layer không
            int hitLayer = hit.collider.gameObject.layer;
            _isValidPlacement = ((1 << hitLayer) & validPlacementLayer) != 0;
            
            if (showDebugRay)
            {
                Color debugColor = _isValidPlacement ? Color.green : Color.red;
                Debug.DrawLine(_lastHitPoint, _lastHitPoint + Vector3.up * 2f, debugColor);
                Debug.DrawLine(_lastHitPoint, _lastHitPoint + hit.normal * 1f, Color.blue);
            }
            
            foundValidSurface = true;
            break;
        }
        
        if (!foundValidSurface)
        {
            _hasValidHit = false;
            _isValidPlacement = false;
        }
    }

    /// <summary>
    /// Snap vị trí vào grid
    /// </summary>
    private Vector3 SnapToGrid(Vector3 worldPosition)
    {
        // Lấy gridSize từ GridVisualizer nếu có (để đồng bộ)
        float currentGridSize = gridSize;
        if (BuildModeGridVisualizer.Instance != null)
        {
            currentGridSize = BuildModeGridVisualizer.Instance.GridSize;
        }
        
        // Snap về GÓC/INTERSECTION của ô grid (0, 4, 8, 12...)
        // Footprint sẽ mở rộng từ góc này
        float x = Mathf.Round(worldPosition.x / currentGridSize) * currentGridSize;
        float z = Mathf.Round(worldPosition.z / currentGridSize) * currentGridSize;
        
        // Giữ nguyên Y từ hit point
        return new Vector3(x, worldPosition.y, z);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Lấy vị trí đặt object (đã snap grid nếu được bật)
    /// </summary>
    public bool TryGetPlacementPosition(out Vector3 position)
    {
        if (_hasValidHit)
        {
            position = _lastGridPosition;
            _isValidPlacement = true;
            return true;
        }
        
        position = Vector3.zero;
        _isValidPlacement = false;
        return false;
    }

    /// <summary>
    /// Lấy vị trí đặt object với offset Y
    /// </summary>
    public bool TryGetPlacementPosition(out Vector3 position, float yOffset)
    {
        if (_hasValidHit)
        {
            position = _lastGridPosition + Vector3.up * yOffset;
            _isValidPlacement = true;
            return true;
        }
        
        position = Vector3.zero;
        _isValidPlacement = false;
        return false;
    }

    /// <summary>
    /// Set camera cho raycaster (gọi khi chuyển camera)
    /// </summary>
    public void SetCamera(Camera camera)
    {
        buildCamera = camera;
    }

    /// <summary>
    /// Bật/tắt snap grid
    /// </summary>
    public void SetSnapToGrid(bool enabled)
    {
        snapToGrid = enabled;
    }

    /// <summary>
    /// Đổi kích thước grid
    /// </summary>
    public void SetGridSize(float size)
    {
        gridSize = Mathf.Max(0.1f, size);
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !showDebugRay) return;
        if (!_hasValidHit) return;

        // Vẽ hit point
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_lastHitPoint, 0.2f);
        
        // Vẽ grid position
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(_lastGridPosition, new Vector3(gridSize, 0.1f, gridSize));
        
        // Vẽ grid xung quanh
        if (snapToGrid)
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
            for (int x = -2; x <= 2; x++)
            {
                for (int z = -2; z <= 2; z++)
                {
                    if (x == 0 && z == 0) continue;
                    Vector3 gridPos = _lastGridPosition + new Vector3(x * gridSize, 0, z * gridSize);
                    Gizmos.DrawWireCube(gridPos, new Vector3(gridSize, 0.05f, gridSize));
                }
            }
        }
    }
    #endregion
}
