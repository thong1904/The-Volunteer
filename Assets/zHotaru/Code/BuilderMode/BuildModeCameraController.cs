using UnityEngine;

/// <summary>
/// BuildModeCameraController - Điều khiển camera trong Build Mode
/// Cho phép di chuyển camera tự do khi đang ở Build Mode
/// </summary>
public class BuildModeCameraController : MonoBehaviour
{
    #region Serialized Fields
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float fastMoveSpeed = 40f;
    [SerializeField] private float rotationSpeed = 3f;
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 10f;
    
    [Header("Boundaries")]
    [SerializeField] private bool useBoundaries = false;
    [SerializeField] private Vector3 minBoundary = new Vector3(-100, 5, -100);
    [SerializeField] private Vector3 maxBoundary = new Vector3(100, 50, 100);
    
    [Header("References")]
    [SerializeField] private Camera buildModeCamera;
    #endregion

    #region Private Fields
    private bool _isRotating = false;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (buildModeCamera == null)
        {
            buildModeCamera = GetComponent<Camera>();
            if (buildModeCamera == null)
            {
                buildModeCamera = Camera.main;
            }
        }
    }

    private void OnEnable()
    {
        _isRotating = false;
    }

    private void Update()
    {
        if (BuilderMode.Instance == null || !BuilderMode.Instance.IsInBuildMode)
        {
            return;
        }

        HandleMovement();
        HandleRotation();
        HandleZoom();
    }
    #endregion

    #region Movement
    private void HandleMovement()
    {
        if (buildModeCamera == null) return;
        
        float horizontal = 0f;
        float vertical = 0f;
        float upDown = 0f;
        
        // WASD
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            vertical = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            vertical = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            horizontal = 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            horizontal = -1f;
        
        // Q/E lên xuống
        if (Input.GetKey(KeyCode.Q))
            upDown = -1f;
        if (Input.GetKey(KeyCode.E))
            upDown = 1f;

        if (Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f) && Mathf.Approximately(upDown, 0f))
        {
            return;
        }

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? fastMoveSpeed : moveSpeed;

        // Tính hướng di chuyển dựa trên góc Y của camera (không phụ thuộc vào góc nhìn lên/xuống)
        float yaw = buildModeCamera.transform.eulerAngles.y * Mathf.Deg2Rad;
        
        // Forward và Right dựa trên góc Y
        Vector3 forward = new Vector3(Mathf.Sin(yaw), 0, Mathf.Cos(yaw));
        Vector3 right = new Vector3(Mathf.Cos(yaw), 0, -Mathf.Sin(yaw));

        Vector3 moveDirection = (forward * vertical + right * horizontal + Vector3.up * upDown);
        
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }
        
        Vector3 newPosition = buildModeCamera.transform.position + moveDirection * currentSpeed * Time.deltaTime;

        if (useBoundaries)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, minBoundary.x, maxBoundary.x);
            newPosition.y = Mathf.Clamp(newPosition.y, minBoundary.y, maxBoundary.y);
            newPosition.z = Mathf.Clamp(newPosition.z, minBoundary.z, maxBoundary.z);
        }

        buildModeCamera.transform.position = newPosition;
    }
    #endregion

    #region Rotation
    private void HandleRotation()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _isRotating = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetMouseButtonUp(1))
        {
            _isRotating = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (_isRotating)
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            Vector3 currentRotation = buildModeCamera.transform.eulerAngles;
            currentRotation.y += mouseX;
            
            float newRotationX = currentRotation.x - mouseY;
            if (newRotationX > 180) newRotationX -= 360;
            // Giới hạn góc nhìn từ -89 đến 89 để tránh lỗi khi nhìn thẳng xuống
            newRotationX = Mathf.Clamp(newRotationX, -89f, 89f);
            
            buildModeCamera.transform.eulerAngles = new Vector3(newRotationX, currentRotation.y, 0);
        }
    }
    #endregion

    #region Zoom
    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            Vector3 zoomDirection = buildModeCamera.transform.forward * scrollInput * zoomSpeed;
            Vector3 newPosition = buildModeCamera.transform.position + zoomDirection;
            
            if (useBoundaries)
            {
                newPosition.y = Mathf.Clamp(newPosition.y, minBoundary.y, maxBoundary.y);
            }
            
            buildModeCamera.transform.position = newPosition;
        }
    }
    #endregion

    #region Public Methods
    public void MoveTo(Vector3 position)
    {
        if (buildModeCamera != null)
        {
            buildModeCamera.transform.position = position;
        }
    }

    public void LookAt(Vector3 target)
    {
        if (buildModeCamera != null)
        {
            buildModeCamera.transform.LookAt(target);
        }
    }
    #endregion
}
