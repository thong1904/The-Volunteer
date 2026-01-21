using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Script cho các vị trí trưng bày - NPC chọn vị trí ngẫu nhiên xung quanh object
/// </summary>
public class DisplayArea : MonoBehaviour
{
    [Header("Display Area Settings")]
    [SerializeField] private float areaRadius = 3f; // Bán kính vùng NPC có thể đứng
    [SerializeField] private float navMeshSampleDistance = 5f; // Khoảng cách sample trên NavMesh
    [SerializeField] private Transform focusPoint; // Điểm để NPC nhìn vào (nếu null sẽ dùng center)
    [SerializeField] private bool useColliderBounds = true; // Tự động dùng bounds của collider
    
    private Vector3 centerPosition;
    private Collider objectCollider;
    
    void OnEnable()
    {
        centerPosition = transform.position;
        objectCollider = GetComponent<Collider>();
    }
    
    /// <summary>
    /// Lấy vị trí ngẫu nhiên xung quanh object
    /// </summary>
    public Vector3 GetRandomPosition()
    {
        float radius = areaRadius;
        
        // Nếu bật useColliderBounds và có collider, dùng bounds để tính radius
        if (useColliderBounds && objectCollider != null)
        {
            // Lấy kích thước lớn nhất của collider + thêm một chút buffer
            Vector3 size = objectCollider.bounds.size;
            radius = Mathf.Max(size.x, size.z) * 0.5f + 1f; // Thêm 1m buffer
        }
        
        // Chọn vị trí ngẫu nhiên trong vòng tròn
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomRadius = Random.Range(0f, radius);
        
        Vector3 randomPosition = centerPosition + new Vector3(
            Mathf.Cos(randomAngle) * randomRadius,
            0,
            Mathf.Sin(randomAngle) * randomRadius
        );
        
        // Sample vị trí trên NavMesh
        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
        {
            return hit.position;
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
        float radius = areaRadius;
        
        // Nếu dùng collider bounds, vẽ theo bounds
        if (useColliderBounds)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Vector3 size = col.bounds.size;
                radius = Mathf.Max(size.x, size.z) * 0.5f + 1f;
                
                // Vẽ bounds của collider
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
        }
        
        // Vẽ vòng tròn vùng NPC có thể đứng - màu xanh
        Gizmos.color = Color.green;
        DrawCircle(pos, radius, 32);
        
        // Vẽ focus point
        if (focusPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(focusPoint.position, 0.2f);
            Gizmos.DrawLine(pos, focusPoint.position);
        }
    }
    
    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * 360f * Mathf.Deg2Rad;
            float angle2 = ((i + 1) / (float)segments) * 360f * Mathf.Deg2Rad;
            
            Vector3 p1 = center + new Vector3(Mathf.Cos(angle1) * radius, 0, Mathf.Sin(angle1) * radius);
            Vector3 p2 = center + new Vector3(Mathf.Cos(angle2) * radius, 0, Mathf.Sin(angle2) * radius);
            
            Gizmos.DrawLine(p1, p2);
        }
    }
#endif
}
