using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Script cho các vị trí trưng bày - NPC chọn vị trí ngẫu nhiên trong nửa vòng tròn phía trước object
/// Dùng cho build system - display được tạo động bởi người chơi
/// </summary>
public class DisplayArea : MonoBehaviour
{
    [Header("Build Status")]
    [SerializeField] private bool isBuilt = false; // Đánh dấu object đã được đặt bởi player (không phải preset)
    [SerializeField] private bool isPresetDisplay = false; // Đánh dấu đây là display có sẵn trong scene (luôn active cho NPC)
    
    /// <summary>
    /// Property cho biết NPC có thể sử dụng display này không
    /// Object preset hoặc đã được build đều có thể sử dụng
    /// </summary>
    public bool IsAvailableForNPC => isPresetDisplay || isBuilt;
    
    /// <summary>
    /// Đánh dấu display đã được đặt bởi player thông qua build system
    /// </summary>
    public void MarkAsBuilt()
    {
        isBuilt = true;
    }
    
    [Header("Half Circle Area Settings")]
    [SerializeField] private float outerRadius = 5f; // Bán kính vòng ngoài - NPC có thể đứng
    [SerializeField] private float innerRadius = 1f; // Bán kính vòng trong - NPC KHÔNG thể đứng (tránh đứng sát center)
    
    [Header("Direction Settings")]
    [Tooltip("Hướng mở của nửa vòng tròn (forward của object này). NPC sẽ đứng ở nửa vòng tròn này, phía sau là object lớn")]
    [SerializeField] private bool useLocalForward = true; // Sử dụng forward của object này làm hướng mở
    [SerializeField] private float halfCircleAngle = 180f; // Góc mở của nửa vòng tròn (180 = nửa vòng tròn)
    
    [Header("NavMesh Settings")]
    [SerializeField] private float navMeshSampleDistance = 5f; // Khoảng cách sample trên NavMesh
    [SerializeField] private int maxRetryAttempts = 10; // Số lần thử tìm vị trí hợp lệ
    
    [Header("Focus Settings")]
    [SerializeField] private Transform focusPoint; // Điểm để NPC nhìn vào (nếu null sẽ dùng center)
    
    private Vector3 centerPosition;
    
    void OnEnable()
    {
        centerPosition = transform.position;
    }
    
    /// <summary>
    /// Lấy hướng mở của nửa vòng tròn (hướng NPC có thể đứng)
    /// </summary>
    private Vector3 GetForwardDirection()
    {
        if (useLocalForward)
            return transform.forward;
        else
            return Vector3.forward;
    }
    
    /// <summary>
    /// Lấy vị trí ngẫu nhiên trong nửa vòng tròn
    /// </summary>
    public Vector3 GetRandomPosition()
    {
        return GetRandomPosition(null);
    }
    
    /// <summary>
    /// Lấy vị trí ngẫu nhiên trong nửa vòng tròn và kiểm tra có path đến được từ vị trí NPC không
    /// </summary>
    public Vector3 GetRandomPosition(Transform npcTransform)
    {
        Vector3 forward = GetForwardDirection();
        float halfAngleRad = (halfCircleAngle * 0.5f) * Mathf.Deg2Rad;
        
        // Thử nhiều lần để tìm vị trí hợp lệ
        for (int i = 0; i < maxRetryAttempts; i++)
        {
            // Chọn góc ngẫu nhiên trong phạm vi nửa vòng tròn
            // Góc 0 là hướng forward, góc sẽ nằm trong khoảng [-halfAngle, +halfAngle]
            float randomAngle = Random.Range(-halfAngleRad, halfAngleRad);
            
            // Chọn khoảng cách ngẫu nhiên từ innerRadius đến outerRadius
            float randomDistance = Random.Range(innerRadius, outerRadius);
            
            // Tính vị trí dựa trên góc và khoảng cách
            // Xoay forward vector theo góc randomAngle
            Vector3 direction = Quaternion.Euler(0, randomAngle * Mathf.Rad2Deg, 0) * forward;
            Vector3 randomPosition = centerPosition + direction * randomDistance;
            
            // Sample vị trí trên NavMesh
            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                // Nếu có NPC transform, kiểm tra xem có path đến được không
                if (npcTransform != null)
                {
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(npcTransform.position, hit.position, NavMesh.AllAreas, path))
                    {
                        // Chỉ chấp nhận nếu path hoàn chỉnh
                        if (path.status == NavMeshPathStatus.PathComplete)
                        {
                            return hit.position;
                        }
                    }
                }
                else
                {
                    // Không có NPC transform, chỉ kiểm tra NavMesh
                    return hit.position;
                }
            }
        }
        
        // Nếu không tìm được ngẫu nhiên, thử các điểm cố định trên nửa vòng tròn
        Vector3[] fixedPoints = GetFixedPointsOnHalfCircle(5); // 5 điểm cố định
        
        foreach (var point in fixedPoints)
        {
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                if (npcTransform != null)
                {
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(npcTransform.position, hit.position, NavMesh.AllAreas, path))
                    {
                        if (path.status == NavMeshPathStatus.PathComplete)
                        {
                            return hit.position;
                        }
                    }
                }
                else
                {
                    return hit.position;
                }
            }
        }
        
        return centerPosition;
    }
    
    /// <summary>
    /// Lấy các điểm cố định phân bố đều trên nửa vòng tròn
    /// </summary>
    private Vector3[] GetFixedPointsOnHalfCircle(int pointCount)
    {
        Vector3[] points = new Vector3[pointCount];
        Vector3 forward = GetForwardDirection();
        float halfAngleRad = (halfCircleAngle * 0.5f) * Mathf.Deg2Rad;
        float midRadius = (innerRadius + outerRadius) * 0.5f;
        
        for (int i = 0; i < pointCount; i++)
        {
            // Phân bố đều trong phạm vi góc
            float t = (float)i / (pointCount - 1); // 0 -> 1
            float angle = Mathf.Lerp(-halfAngleRad, halfAngleRad, t);
            
            Vector3 direction = Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0) * forward;
            points[i] = centerPosition + direction * midRadius;
        }
        
        return points;
    }
    
    /// <summary>
    /// Lấy vị trí tâm của display area
    /// </summary>
    public Vector3 GetCenterPosition()
    {
        return centerPosition;
    }
    
    /// <summary>
    /// Lấy vị trí focus point - điểm để NPC nhìn vào
    /// </summary>
    public Vector3 GetFocusPoint()
    {
        if (focusPoint != null)
            return focusPoint.position;
        else
            return centerPosition;
    }
    
    /// <summary>
    /// Cập nhật vị trí tâm khi object bị di chuyển
    /// </summary>
    void Update()
    {
        centerPosition = transform.position;
    }
    
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector3 pos = transform.position;
        Vector3 forward = useLocalForward ? transform.forward : Vector3.forward;
        float halfAngle = halfCircleAngle * 0.5f;
        
        // Vẽ nửa vòng tròn ngoài (vùng NPC có thể đứng) - màu xanh lá
        Gizmos.color = Color.green;
        DrawHalfCircle(pos, forward, outerRadius, halfAngle, 20);
        
        // Vẽ nửa vòng tròn trong (vùng cấm) - màu đỏ
        Gizmos.color = Color.red;
        DrawHalfCircle(pos, forward, innerRadius, halfAngle, 10);
        
        // Vẽ các đường biên nối 2 vòng tròn
        Gizmos.color = Color.yellow;
        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;
        
        Gizmos.DrawLine(pos + leftDir * innerRadius, pos + leftDir * outerRadius);
        Gizmos.DrawLine(pos + rightDir * innerRadius, pos + rightDir * outerRadius);
        
        // Vẽ hướng forward
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(pos, forward * (outerRadius + 0.5f));
        
        // Vẽ label
        UnityEditor.Handles.Label(pos + forward * (outerRadius + 0.5f), "Forward (NPC Side)");
        UnityEditor.Handles.Label(pos - forward * 1f, "Object Side (Blocked)");
        
        // Vẽ focus point
        if (focusPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(focusPoint.position, 0.2f);
            Gizmos.DrawLine(pos, focusPoint.position);
        }
    }
    
    /// <summary>
    /// Vẽ nửa vòng tròn trong Editor
    /// </summary>
    private void DrawHalfCircle(Vector3 center, Vector3 forward, float radius, float halfAngleDeg, int segments)
    {
        float angleStep = (halfAngleDeg * 2f) / segments;
        Vector3 prevPoint = center + Quaternion.Euler(0, -halfAngleDeg, 0) * forward * radius;
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfAngleDeg + angleStep * i;
            Vector3 newPoint = center + Quaternion.Euler(0, angle, 0) * forward * radius;
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
#endif
}
