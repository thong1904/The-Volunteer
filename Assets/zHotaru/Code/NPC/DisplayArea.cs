using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Script cho các vị trí trưng bày - NPC chọn vị trí ngẫu nhiên xung quanh object
/// </summary>
public class DisplayArea : MonoBehaviour
{
    [Header("Area Settings")]
    [SerializeField] private Vector2 areaSizeStay = new Vector2(5f, 5f); // Vùng lớn bên ngoài - NPC có thể đứng
    [SerializeField] private Vector2 areaSizeStop = new Vector2(2f, 2f); // Vùng nhỏ bên trong - NPC KHÔNG thể đứng
    
    [Header("NavMesh Settings")]
    [SerializeField] private float navMeshSampleDistance = 5f; // Khoảng cách sample trên NavMesh
    [SerializeField] private int maxRetryAttempts = 1; // Số lần thử tìm vị trí hợp lệ
    
    [Header("Focus Settings")]
    [SerializeField] private Transform focusPoint; // Điểm để NPC nhìn vào (nếu null sẽ dùng center)
    
    private Vector3 centerPosition;
    
    void OnEnable()
    {
        centerPosition = transform.position;
    }
    
    /// <summary>
    /// Lấy vị trí ngẫu nhiên xung quanh object (hình chữ nhật)
    /// </summary>
    public Vector3 GetRandomPosition()
    {
        return GetRandomPosition(null);
    }
    
    /// <summary>
    /// Lấy vị trí ngẫu nhiên và kiểm tra có path đến được từ vị trí NPC không
    /// </summary>
    public Vector3 GetRandomPosition(Transform npcTransform)
    {
        float halfWidthStay = areaSizeStay.x * 0.5f;
        float halfLengthStay = areaSizeStay.y * 0.5f;
        float halfWidthStop = areaSizeStop.x * 0.5f;
        float halfLengthStop = areaSizeStop.y * 0.5f;
        
        // Thử nhiều lần để tìm vị trí hợp lệ
        for (int i = 0; i < maxRetryAttempts; i++)
        {
            // Chọn vị trí ngẫu nhiên trong vùng Stay (vùng lớn)
            float randomX = Random.Range(-halfWidthStay, halfWidthStay);
            float randomZ = Random.Range(-halfLengthStay, halfLengthStay);
            
            // Kiểm tra xem có nằm trong vùng Stop (vùng cấm) không
            if (Mathf.Abs(randomX) < halfWidthStop && Mathf.Abs(randomZ) < halfLengthStop)
            {
                // Nằm trong vùng cấm, bỏ qua
                continue;
            }
            
            Vector3 randomPosition = centerPosition + new Vector3(randomX, 0, randomZ);
            
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
        
        // Nếu không tìm được, thử tìm ở 4 góc của vùng Stay
        Vector3[] corners = new Vector3[]
        {
            centerPosition + new Vector3(halfWidthStay, 0, halfLengthStay),
            centerPosition + new Vector3(-halfWidthStay, 0, halfLengthStay),
            centerPosition + new Vector3(halfWidthStay, 0, -halfLengthStay),
            centerPosition + new Vector3(-halfWidthStay, 0, -halfLengthStay)
        };
        
        foreach (var corner in corners)
        {
            if (NavMesh.SamplePosition(corner, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
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
        
        // Vẽ vùng Stay (vùng lớn) - màu xanh lá (NPC có thể đứng)
        Gizmos.color = Color.green;
        DrawRectangle(pos, areaSizeStay);
        
        // Vẽ vùng Stop (vùng cấm) - màu đỏ (NPC không thể đứng)
        Gizmos.color = Color.red;
        DrawRectangle(pos, areaSizeStop);
        
        // Vẽ label
        UnityEditor.Handles.Label(pos + new Vector3(areaSizeStay.x * 0.5f + 0.5f, 0, 0), "Stay Zone (Green)");
        UnityEditor.Handles.Label(pos + new Vector3(areaSizeStop.x * 0.5f + 0.5f, 0.5f, 0), "Stop Zone (Red)");
        
        // Vẽ focus point
        if (focusPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(focusPoint.position, 0.2f);
            Gizmos.DrawLine(pos, focusPoint.position);
        }
    }
    
    private void DrawRectangle(Vector3 center, Vector2 size)
    {
        float halfWidth = size.x * 0.5f;
        float halfLength = size.y * 0.5f;
        
        Vector3 p1 = center + new Vector3(-halfWidth, 0, -halfLength);
        Vector3 p2 = center + new Vector3(halfWidth, 0, -halfLength);
        Vector3 p3 = center + new Vector3(halfWidth, 0, halfLength);
        Vector3 p4 = center + new Vector3(-halfWidth, 0, halfLength);
        
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);
    }
#endif
}
