using UnityEngine;

/// <summary>
/// BuildModePreview - Hiển thị preview object trước khi đặt
/// Bước 3: Object Preview trong Build Mode
/// </summary>
public class BuildModePreview : MonoBehaviour
{
    #region Singleton
    public static BuildModePreview Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [Header("Preview Settings")]
    [SerializeField] private Material previewValidMaterial;     // Material khi vị trí hợp lệ
    [SerializeField] private Material previewInvalidMaterial;   // Material khi vị trí không hợp lệ
    
    [Header("Rotation Settings")]
    [SerializeField] private KeyCode rotateKey = KeyCode.R;     // Phím xoay object
    [SerializeField] private float rotationStep = 90f;          // Mỗi lần xoay bao nhiêu độ
    
    [Header("Grid Footprint")]
    [SerializeField] private Color footprintValidColor = new Color(0f, 1f, 0f, 0.4f);
    [SerializeField] private Color footprintInvalidColor = new Color(1f, 0f, 0f, 0.4f);
    [SerializeField] private float footprintHeightOffset = 0.1f;  // Offset độ cao so với mặt đất
    
    [Header("Colors")]
    [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.5f);
    #endregion

    #region Private Fields
    private GameObject _currentPreview;
    private GameObject _currentPrefab;
    private float _currentRotationY = 0f;
    private bool _isPlacementValid = false;
    private Renderer[] _previewRenderers;
    private Material _footprintMaterial;
    private Vector2Int _currentFootprint = Vector2Int.one;
    #endregion

    #region Properties
    /// <summary>
    /// Có đang hiển thị preview không
    /// </summary>
    public bool HasPreview => _currentPreview != null;
    
    /// <summary>
    /// Preview object hiện tại
    /// </summary>
    public GameObject CurrentPreview => _currentPreview;
    
    /// <summary>
    /// Prefab đang được chọn
    /// </summary>
    public GameObject CurrentPrefab => _currentPrefab;
    
    /// <summary>
    /// Góc xoay Y hiện tại
    /// </summary>
    public float CurrentRotationY => _currentRotationY;
    
    /// <summary>
    /// Vị trí đặt có hợp lệ không
    /// </summary>
    public bool IsPlacementValid => _isPlacementValid;
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
        
        CreateDefaultMaterials();
        CreateFootprintMaterial();
    }

    private void Update()
    {
        if (BuilderMode.Instance == null || !BuilderMode.Instance.IsInBuildMode)
        {
            return;
        }

        if (_currentPreview != null)
        {
            UpdatePreviewPosition();
            HandleRotationInput();
        }
    }

    private void OnDisable()
    {
        ClearPreview();
    }

    private void OnRenderObject()
    {
        if (!BuilderMode.Instance.IsInBuildMode) return;
        if (_currentPreview == null || !_currentPreview.activeSelf) return;
        
        DrawFootprintGrid();
    }
    #endregion

    #region Preview Management
    /// <summary>
    /// Bắt đầu preview một object
    /// </summary>
    public void StartPreview(GameObject prefab)
    {
        if (prefab == null) return;
        
        // Xóa preview cũ nếu có
        ClearPreview();
        
        _currentPrefab = prefab;
        _currentRotationY = 0f;
        
        // Tạo preview instance
        _currentPreview = Instantiate(prefab);
        _currentPreview.name = prefab.name + "_Preview";
        
        // Disable các component không cần thiết
        DisablePreviewComponents();
        
        // Setup material preview
        SetupPreviewMaterials();
        
        // Cập nhật vị trí ban đầu
        UpdatePreviewPosition();
    }

    /// <summary>
    /// Xóa preview hiện tại
    /// </summary>
    public void ClearPreview()
    {
        if (_currentPreview != null)
        {
            Destroy(_currentPreview);
            _currentPreview = null;
        }
        
        _currentPrefab = null;
        _previewRenderers = null;
        _currentRotationY = 0f;
    }

    /// <summary>
    /// Đổi prefab đang preview
    /// </summary>
    public void ChangePrefab(GameObject newPrefab)
    {
        StartPreview(newPrefab);
    }
    #endregion

    #region Preview Update
    /// <summary>
    /// Cập nhật vị trí preview theo chuột
    /// </summary>
    private void UpdatePreviewPosition()
    {
        if (_currentPreview == null) return;
        
        // Lấy vị trí từ Raycaster
        if (BuildModeRaycaster.Instance != null && BuildModeRaycaster.Instance.HasValidHit)
        {
            Vector3 targetPosition = BuildModeRaycaster.Instance.GridPosition;
            
            // Offset để căn footprint với grid
            // Với footprint lẻ theo chiều nào, cần offset thêm gridSize * 0.5
            float gridSize = BuildModeRaycaster.Instance.GridSize;
            
            // Tính effective footprint sau rotation (0°, 90°, 180°, 270°)
            int rotationSteps = Mathf.RoundToInt(_currentRotationY / 90f) % 4;
            bool swapFootprint = (rotationSteps == 1 || rotationSteps == 3);
            
            int effectiveFootprintX = swapFootprint ? _currentFootprint.y : _currentFootprint.x;
            int effectiveFootprintZ = swapFootprint ? _currentFootprint.x : _currentFootprint.y;
            
            // Offset nếu footprint lẻ
            float offsetX = (effectiveFootprintX % 2 == 1) ? gridSize * 0.5f : 0f;
            float offsetZ = (effectiveFootprintZ % 2 == 1) ? gridSize * 0.5f : 0f;
            
            targetPosition.x += offsetX;
            targetPosition.z += offsetZ;
            
            // Áp dụng vị trí và rotation
            _currentPreview.transform.position = targetPosition;
            _currentPreview.transform.rotation = Quaternion.Euler(0, _currentRotationY, 0);
            
            // Hiển thị preview
            _currentPreview.SetActive(true);
            
            // Cập nhật trạng thái valid (sẽ implement ở Bước 4)
            UpdatePlacementValidity();
        }
        else
        {
            // Ẩn preview khi không hit ground
            _currentPreview.SetActive(false);
            _isPlacementValid = false;
        }
    }

    /// <summary>
    /// Xử lý input xoay object
    /// </summary>
    private void HandleRotationInput()
    {
        if (Input.GetKeyDown(rotateKey))
        {
            RotatePreview();
        }
    }

    /// <summary>
    /// Xoay preview
    /// </summary>
    public void RotatePreview()
    {
        _currentRotationY += rotationStep;
        if (_currentRotationY >= 360f)
        {
            _currentRotationY -= 360f;
        }
        
        if (_currentPreview != null)
        {
            _currentPreview.transform.rotation = Quaternion.Euler(0, _currentRotationY, 0);
        }
    }

    /// <summary>
    /// Set góc xoay cụ thể
    /// </summary>
    public void SetRotation(float rotationY)
    {
        _currentRotationY = rotationY % 360f;
        if (_currentPreview != null)
        {
            _currentPreview.transform.rotation = Quaternion.Euler(0, _currentRotationY, 0);
        }
    }
    #endregion

    #region Placement Validation
    /// <summary>
    /// Cập nhật trạng thái hợp lệ của vị trí đặt
    /// </summary>
    private void UpdatePlacementValidity()
    {
        _isPlacementValid = false;
        
        // Kiểm tra từ Raycaster (hit valid placement layer)
        if (BuildModeRaycaster.Instance != null && BuildModeRaycaster.Instance.IsValidPlacement)
        {
            // Kiểm tra footprint có nằm trong vùng grid hợp lệ không
            if (BuildModeGridVisualizer.Instance != null)
            {
                Vector3 position = _currentPreview.transform.position;
                _isPlacementValid = BuildModeGridVisualizer.Instance.IsFootprintInValidGrid(
                    position, 
                    _currentFootprint, 
                    _currentRotationY
                );
                
                // Kiểm tra chồng lấn với objects đã đặt
                if (_isPlacementValid && IsOverlappingPlacedObjects(position))
                {
                    _isPlacementValid = false;
                }
            }
        }
        
        UpdatePreviewColor();
    }
    
    /// <summary>
    /// Kiểm tra xem vị trí có chồng lấn với objects đã đặt không
    /// </summary>
    private bool IsOverlappingPlacedObjects(Vector3 position)
    {
        if (BuildModePlacer.Instance == null) return false;
        
        // Lấy bounds của preview
        Bounds previewBounds = GetPreviewBounds();
        if (previewBounds.size == Vector3.zero) return false;
        
        // Kiểm tra với tất cả objects đã đặt
        foreach (var placedData in BuildModePlacer.Instance.PlacedObjects)
        {
            if (placedData.gameObject == null) continue;
            
            // Nếu object đang hidden (đang move) thì bỏ qua
            if (!placedData.gameObject.activeInHierarchy) continue;
            
            Bounds placedBounds = GetObjectBounds(placedData.gameObject);
            if (placedBounds.size == Vector3.zero) continue;
            
            // Kiểm tra overlap
            if (previewBounds.Intersects(placedBounds))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Lấy bounds của preview object hiện tại
    /// </summary>
    private Bounds GetPreviewBounds()
    {
        if (_currentPreview == null) return new Bounds();
        
        Renderer[] renderers = _currentPreview.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds();
        
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        
        // Thu nhỏ bounds một chút để tránh false positive khi đặt cạnh nhau
        bounds.size *= 0.9f;
        
        return bounds;
    }
    
    /// <summary>
    /// Lấy bounds của một GameObject
    /// </summary>
    private Bounds GetObjectBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds();
        
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        
        return bounds;
    }

    /// <summary>
    /// Set trạng thái valid từ bên ngoài
    /// </summary>
    public void SetPlacementValid(bool isValid)
    {
        _isPlacementValid = isValid;
        UpdatePreviewColor();
    }

    /// <summary>
    /// Cập nhật màu preview dựa trên valid state
    /// </summary>
    private void UpdatePreviewColor()
    {
        if (_previewRenderers == null) return;
        
        Color targetColor = _isPlacementValid ? validColor : invalidColor;
        Material targetMaterial = _isPlacementValid ? previewValidMaterial : previewInvalidMaterial;
        
        foreach (var renderer in _previewRenderers)
        {
            if (renderer == null) continue;
            
            if (targetMaterial != null)
            {
                // Dùng material preset
                Material[] mats = new Material[renderer.materials.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = targetMaterial;
                }
                renderer.materials = mats;
            }
            else
            {
                // Đổi màu material hiện tại
                foreach (var mat in renderer.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        mat.color = targetColor;
                    }
                    if (mat.HasProperty("_BaseColor"))
                    {
                        mat.SetColor("_BaseColor", targetColor);
                    }
                }
            }
        }
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Disable các component không cần thiết trên preview
    /// </summary>
    private void DisablePreviewComponents()
    {
        if (_currentPreview == null) return;
        
        // Disable Colliders (để không block raycast)
        foreach (var collider in _currentPreview.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }
        
        // Disable Rigidbody
        foreach (var rb in _currentPreview.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        
        // Disable các script khác
        foreach (var behaviour in _currentPreview.GetComponentsInChildren<MonoBehaviour>())
        {
            // Giữ lại Transform
            if (behaviour != null && behaviour.GetType() != typeof(Transform))
            {
                behaviour.enabled = false;
            }
        }
        
        // Disable AudioSource
        foreach (var audio in _currentPreview.GetComponentsInChildren<AudioSource>())
        {
            audio.enabled = false;
        }
        
        // Disable Animator
        foreach (var animator in _currentPreview.GetComponentsInChildren<Animator>())
        {
            animator.enabled = false;
        }
    }

    /// <summary>
    /// Setup materials cho preview
    /// </summary>
    private void SetupPreviewMaterials()
    {
        if (_currentPreview == null) return;
        
        _previewRenderers = _currentPreview.GetComponentsInChildren<Renderer>();
        
        // Lưu materials gốc và tạo bản copy
        foreach (var renderer in _previewRenderers)
        {
            if (renderer == null) continue;
            
            Material[] newMats = new Material[renderer.materials.Length];
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                // Tạo instance mới của material
                newMats[i] = new Material(renderer.materials[i]);
                
                // Set transparency
                SetMaterialTransparent(newMats[i]);
            }
            renderer.materials = newMats;
        }
        
        // Áp dụng màu ban đầu
        UpdatePreviewColor();
    }

    /// <summary>
    /// Chuyển material sang transparent mode
    /// </summary>
    private void SetMaterialTransparent(Material mat)
    {
        if (mat == null) return;
        
        // Standard shader
        if (mat.HasProperty("_Mode"))
        {
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
        
        // URP shader
        if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 0);   // Alpha
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
    }

    /// <summary>
    /// Tạo materials mặc định nếu chưa gán
    /// </summary>
    private void CreateDefaultMaterials()
    {
        if (previewValidMaterial == null)
        {
            previewValidMaterial = new Material(Shader.Find("Standard"));
            previewValidMaterial.color = validColor;
            SetMaterialTransparent(previewValidMaterial);
        }
        
        if (previewInvalidMaterial == null)
        {
            previewInvalidMaterial = new Material(Shader.Find("Standard"));
            previewInvalidMaterial.color = invalidColor;
            SetMaterialTransparent(previewInvalidMaterial);
        }
    }
    #endregion

    #region Grid Footprint
    /// <summary>
    /// Tạo material cho vẽ footprint
    /// </summary>
    private void CreateFootprintMaterial()
    {
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        _footprintMaterial = new Material(shader);
        _footprintMaterial.hideFlags = HideFlags.HideAndDontSave;
        _footprintMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _footprintMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _footprintMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        _footprintMaterial.SetInt("_ZWrite", 0);
    }

    /// <summary>
    /// Vẽ grid footprint dưới chân object
    /// </summary>
    private void DrawFootprintGrid()
    {
        if (_footprintMaterial == null) return;
        if (BuildModeRaycaster.Instance == null) return;

        float gridSize = BuildModeRaycaster.Instance.GridSize;
        Vector3 objectPos = _currentPreview.transform.position;
        float y = objectPos.y + footprintHeightOffset;
        
        // Lấy footprint từ selected object
        Vector2Int footprint = _currentFootprint;
        
        // Tính góc rotation của object
        float rotY = _currentRotationY * Mathf.Deg2Rad;
        float cosR = Mathf.Cos(rotY);
        float sinR = Mathf.Sin(rotY);
        
        // Footprint mở rộng từ object position (góc của grid)
        // Offset để center footprint quanh object position
        float halfW = footprint.x * gridSize * 0.5f;
        float halfH = footprint.y * gridSize * 0.5f;

        _footprintMaterial.SetPass(0);
        
        Color currentColor = _isPlacementValid ? footprintValidColor : footprintInvalidColor;
        
        GL.PushMatrix();
        
        // Vẽ fill (các ô vuông filled)
        GL.Begin(GL.QUADS);
        GL.Color(new Color(currentColor.r, currentColor.g, currentColor.b, currentColor.a * 0.3f));
        
        for (int i = 0; i < footprint.x; i++)
        {
            for (int j = 0; j < footprint.y; j++)
            {
                // Local position của ô (từ góc trái-dưới của footprint)
                // Footprint center tại object, nên dch từ -halfW, -halfH
                float localX = i * gridSize - halfW;
                float localZ = j * gridSize - halfH;
                
                // 4 góc của ô (rotate theo object)
                float[] cornerLocalX = { localX, localX + gridSize, localX + gridSize, localX };
                float[] cornerLocalZ = { localZ, localZ, localZ + gridSize, localZ + gridSize };
                
                for (int c = 0; c < 4; c++)
                {
                    float crx = cornerLocalX[c] * cosR - cornerLocalZ[c] * sinR;
                    float crz = cornerLocalX[c] * sinR + cornerLocalZ[c] * cosR;
                    GL.Vertex3(objectPos.x + crx, y, objectPos.z + crz);
                }
            }
        }
        GL.End();
        
        // Vẽ border (outline)
        GL.Begin(GL.LINES);
        GL.Color(currentColor);
        
        // Tính 4 góc của footprint tổng
        Vector3[] outerCorners = new Vector3[4];
        float[] ocLocalX = { -halfW, halfW, halfW, -halfW };
        float[] ocLocalZ = { -halfH, -halfH, halfH, halfH };
        
        for (int c = 0; c < 4; c++)
        {
            float crx = ocLocalX[c] * cosR - ocLocalZ[c] * sinR;
            float crz = ocLocalX[c] * sinR + ocLocalZ[c] * cosR;
            outerCorners[c] = new Vector3(objectPos.x + crx, y, objectPos.z + crz);
        }
        
        // Vẽ 4 cạnh
        for (int i = 0; i < 4; i++)
        {
            int next = (i + 1) % 4;
            GL.Vertex3(outerCorners[i].x, outerCorners[i].y, outerCorners[i].z);
            GL.Vertex3(outerCorners[next].x, outerCorners[next].y, outerCorners[next].z);
        }
        
        // Vẽ grid lines bên trong
        // Dọc
        for (int i = 1; i < footprint.x; i++)
        {
            float localX = i * gridSize - halfW;
            
            float x1r = localX * cosR - (-halfH) * sinR;
            float z1r = localX * sinR + (-halfH) * cosR;
            float x2r = localX * cosR - halfH * sinR;
            float z2r = localX * sinR + halfH * cosR;
            
            GL.Vertex3(objectPos.x + x1r, y, objectPos.z + z1r);
            GL.Vertex3(objectPos.x + x2r, y, objectPos.z + z2r);
        }
        
        // Ngang
        for (int j = 1; j < footprint.y; j++)
        {
            float localZ = j * gridSize - halfH;
            
            float x1r = (-halfW) * cosR - localZ * sinR;
            float z1r = (-halfW) * sinR + localZ * cosR;
            float x2r = halfW * cosR - localZ * sinR;
            float z2r = halfW * sinR + localZ * cosR;
            
            GL.Vertex3(objectPos.x + x1r, y, objectPos.z + z1r);
            GL.Vertex3(objectPos.x + x2r, y, objectPos.z + z2r);
        }
        
        GL.End();
        GL.PopMatrix();
    }

    /// <summary>
    /// Set footprint cho object hiện tại
    /// </summary>
    public void SetFootprint(Vector2Int footprint)
    {
        _currentFootprint = footprint;
        if (_currentFootprint.x < 1) _currentFootprint.x = 1;
        if (_currentFootprint.y < 1) _currentFootprint.y = 1;
    }
    #endregion

    #region Public API
    /// <summary>
    /// Lấy thông tin placement hiện tại
    /// </summary>
    public PlacementInfo GetCurrentPlacementInfo()
    {
        if (_currentPreview == null || !BuildModeRaycaster.Instance.HasValidHit)
        {
            return new PlacementInfo(Vector3.zero, Quaternion.identity, false);
        }
        
        return new PlacementInfo(
            _currentPreview.transform.position,
            _currentPreview.transform.rotation,
            _isPlacementValid
        );
    }
    #endregion
}
