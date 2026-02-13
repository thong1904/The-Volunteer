using UnityEngine;

/// <summary>
/// BuildModeGridVisualizer - Hiển thị grid trong Build Mode
/// Chỉ hiện trên các surface có thể build được
/// </summary>
public class BuildModeGridVisualizer : MonoBehaviour
{
    public static BuildModeGridVisualizer Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] private float gridSize = 5f;           // Kích thước mỗi ô grid
    [SerializeField] private int gridExtent = 20;           // Số ô grid mỗi chiều (từ tâm ra)
    [SerializeField] private float gridHeightOffset = 0.05f; // Offset độ cao so với mặt đất
    [SerializeField] private LayerMask validPlacementLayer;  // Layer cho phép build

    [Header("Visual Settings")]
    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.3f);

    [Header("Raycast Settings")]
    [SerializeField] private float raycastHeight = 100f;     // Độ cao bắt đầu raycast
    [SerializeField] private float maxRayDistance = 200f;    // Khoảng cách raycast tối đa

    [Header("Performance")]
    [SerializeField] private bool followCamera = true;       // Grid theo camera
    [SerializeField] private float updateInterval = 0.1f;    // Thời gian cập nhật vị trí

    private Material _lineMaterial;
    private bool _isVisible = false;
    private Vector3 _gridCenter;
    private float _lastUpdateTime;

    // Cache để lưu kết quả raycast
    private float[,] _heightCache;
    private bool[,] _validCache;
    private int _cacheSize;

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        CreateLineMaterial();
        InitializeCache();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (_lineMaterial != null)
        {
            Destroy(_lineMaterial);
        }
    }

    private void Update()
    {
        if (!_isVisible) return;

        // Cập nhật vị trí grid theo camera
        if (followCamera && Time.time - _lastUpdateTime > updateInterval)
        {
            UpdateGridCenter();
            UpdateHeightCache();
            _lastUpdateTime = Time.time;
        }
    }

    private void OnRenderObject()
    {
        if (!_isVisible) return;
        DrawGrid();
    }
    #endregion

    #region Initialization
    private void InitializeCache()
    {
        _cacheSize = gridExtent * 2 + 1;
        _heightCache = new float[_cacheSize, _cacheSize];
        _validCache = new bool[_cacheSize, _cacheSize];
    }

    private void CreateLineMaterial()
    {
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        _lineMaterial = new Material(shader);
        _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        _lineMaterial.SetInt("_ZWrite", 0);
    }
    #endregion

    #region Grid Update
    private void UpdateGridCenter()
    {
        if (BuildModeRaycaster.Instance != null && BuildModeRaycaster.Instance.BuildCamera != null)
        {
            Vector3 camPos = BuildModeRaycaster.Instance.BuildCamera.transform.position;
            
            // Snap center to grid
            _gridCenter.x = Mathf.Round(camPos.x / gridSize) * gridSize;
            _gridCenter.y = 0;
            _gridCenter.z = Mathf.Round(camPos.z / gridSize) * gridSize;
        }
    }

    private void UpdateHeightCache()
    {
        // Resize cache nếu cần
        if (_cacheSize != gridExtent * 2 + 1)
        {
            InitializeCache();
        }

        // Raycast tại mỗi điểm grid để tìm độ cao
        for (int i = 0; i < _cacheSize; i++)
        {
            for (int j = 0; j < _cacheSize; j++)
            {
                float x = _gridCenter.x + (i - gridExtent) * gridSize;
                float z = _gridCenter.z + (j - gridExtent) * gridSize;
                
                Vector3 rayStart = new Vector3(x, raycastHeight, z);
                
                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, maxRayDistance, validPlacementLayer))
                {
                    _heightCache[i, j] = hit.point.y + gridHeightOffset;
                    _validCache[i, j] = true;
                }
                else
                {
                    _validCache[i, j] = false;
                }
            }
        }
    }
    #endregion

    #region Grid Drawing
    private void DrawGrid()
    {
        if (_lineMaterial == null) return;

        _lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.Begin(GL.LINES);
        GL.Color(gridColor);

        // Vẽ các đường dọc (theo trục Z)
        for (int i = 0; i < _cacheSize; i++)
        {
            for (int j = 0; j < _cacheSize - 1; j++)
            {
                // Chỉ vẽ nếu cả 2 điểm đều valid
                if (_validCache[i, j] && _validCache[i, j + 1])
                {
                    float x = _gridCenter.x + (i - gridExtent) * gridSize;
                    float z1 = _gridCenter.z + (j - gridExtent) * gridSize;
                    float z2 = _gridCenter.z + (j + 1 - gridExtent) * gridSize;
                    
                    GL.Vertex3(x, _heightCache[i, j], z1);
                    GL.Vertex3(x, _heightCache[i, j + 1], z2);
                }
            }
        }

        // Vẽ các đường ngang (theo trục X)
        for (int j = 0; j < _cacheSize; j++)
        {
            for (int i = 0; i < _cacheSize - 1; i++)
            {
                // Chỉ vẽ nếu cả 2 điểm đều valid
                if (_validCache[i, j] && _validCache[i + 1, j])
                {
                    float x1 = _gridCenter.x + (i - gridExtent) * gridSize;
                    float x2 = _gridCenter.x + (i + 1 - gridExtent) * gridSize;
                    float z = _gridCenter.z + (j - gridExtent) * gridSize;
                    
                    GL.Vertex3(x1, _heightCache[i, j], z);
                    GL.Vertex3(x2, _heightCache[i + 1, j], z);
                }
            }
        }

        GL.End();
        GL.PopMatrix();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Hiển thị grid
    /// </summary>
    public void ShowGrid()
    {
        _isVisible = true;
        UpdateGridCenter();
        UpdateHeightCache();
    }

    /// <summary>
    /// Ẩn grid
    /// </summary>
    public void HideGrid()
    {
        _isVisible = false;
    }

    /// <summary>
    /// Toggle hiển thị grid
    /// </summary>
    public void ToggleGrid()
    {
        if (_isVisible)
            HideGrid();
        else
            ShowGrid();
    }

    /// <summary>
    /// Đặt kích thước ô grid
    /// </summary>
    public void SetGridSize(float size)
    {
        gridSize = Mathf.Max(0.5f, size);
        InitializeCache();
    }

    /// <summary>
    /// Đặt số ô grid hiển thị
    /// </summary>
    public void SetGridExtent(int extent)
    {
        gridExtent = Mathf.Max(1, extent);
        InitializeCache();
    }

    /// <summary>
    /// Đặt màu grid
    /// </summary>
    public void SetGridColor(Color color)
    {
        gridColor = color;
    }

    /// <summary>
    /// Đặt layer có thể build
    /// </summary>
    public void SetValidPlacementLayer(LayerMask layer)
    {
        validPlacementLayer = layer;
    }

    /// <summary>
    /// Kiểm tra xem vị trí có nằm trong vùng grid hợp lệ không
    /// </summary>
    public bool IsPositionInValidGrid(Vector3 position)
    {
        if (!_isVisible) return false;
        
        // Tính index trong cache (dùng Floor vì position là center của ô)
        int i = Mathf.FloorToInt((position.x - _gridCenter.x) / gridSize) + gridExtent;
        int j = Mathf.FloorToInt((position.z - _gridCenter.z) / gridSize) + gridExtent;
        
        // Kiểm tra có trong phạm vi cache không
        if (i < 0 || i >= _cacheSize || j < 0 || j >= _cacheSize)
        {
            return false;
        }
        
        // Kiểm tra ô đó có valid không (có raycast hit validPlacementLayer)
        return _validCache[i, j];
    }

    /// <summary>
    /// Kiểm tra footprint có nằm hoàn toàn trong vùng grid hợp lệ không
    /// </summary>
    public bool IsFootprintInValidGrid(Vector3 centerPosition, Vector2Int footprint, float rotationY)
    {
        if (!_isVisible) return false;
        
        float rotRad = rotationY * Mathf.Deg2Rad;
        float cosR = Mathf.Cos(rotRad);
        float sinR = Mathf.Sin(rotRad);
        
        float halfW = footprint.x * gridSize * 0.5f;
        float halfH = footprint.y * gridSize * 0.5f;
        
        // Kiểm tra tất cả các ô trong footprint
        for (int fx = 0; fx < footprint.x; fx++)
        {
            for (int fy = 0; fy < footprint.y; fy++)
            {
                // Kiểm tra TẤT CẢ 4 GÓC của mỗi ô (không chỉ center)
                // Điều này đảm bảo toàn bộ ô nằm trong vùng grid valid
                float[] cornerOffsetX = { 0, 1, 0, 1 };
                float[] cornerOffsetZ = { 0, 0, 1, 1 };
                
                for (int corner = 0; corner < 4; corner++)
                {
                    // Local position của góc này
                    float localX = (fx + cornerOffsetX[corner]) * gridSize - halfW;
                    float localZ = (fy + cornerOffsetZ[corner]) * gridSize - halfH;
                    
                    // Rotate theo object
                    float rotatedX = localX * cosR - localZ * sinR;
                    float rotatedZ = localX * sinR + localZ * cosR;
                    
                    // World position
                    float worldX = centerPosition.x + rotatedX;
                    float worldZ = centerPosition.z + rotatedZ;
                    
                    // Tính index trong cache
                    int i = Mathf.RoundToInt((worldX - _gridCenter.x) / gridSize) + gridExtent;
                    int j = Mathf.RoundToInt((worldZ - _gridCenter.z) / gridSize) + gridExtent;
                    
                    // Kiểm tra có trong phạm vi cache không
                    if (i < 0 || i >= _cacheSize || j < 0 || j >= _cacheSize)
                    {
                        return false;
                    }
                    
                    // Kiểm tra điểm đó có valid không
                    if (!_validCache[i, j])
                    {
                        return false;
                    }
                }
            }
        }
        
        return true;
    }

    public bool IsVisible => _isVisible;
    public float GridSize => gridSize;
    #endregion
}