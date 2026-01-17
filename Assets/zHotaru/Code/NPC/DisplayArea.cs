using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Script cho các vị trí trưng bày - NPC chọn vị trí ngẫu nhiên trong ring (annulus)
/// </summary>
public class DisplayArea : MonoBehaviour
{
    [Header("Display Area Settings")]
    [SerializeField] private float innerRadius = 1f; // Bán kính vùng tránh (center)
    [SerializeField] private float outerRadius = 3f; // Bán kính vùng ngoài
    [SerializeField] private float navMeshSampleDistance = 5f; // Khoảng cách sample trên NavMesh
    [SerializeField] private Transform focusPoint; // Điểm để NPC nhìn vào
    
    private Vector3 centerPosition;
    
    void OnEnable()
    {
        centerPosition = transform.position;
    }
    
    /// <summary>
    /// Lấy vị trí ngẫu nhiên trong vùng ring (annulus) giữa innerRadius và outerRadius
    /// </summary>
    public Vector3 GetRandomPosition()
    {
        // Chọn vị trí ngẫu nhiên trong ring (vòng tròn)
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomRadius = Random.Range(innerRadius, outerRadius);
        
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
        // Vẽ 2 vòng tròn trong editor để visualize inner và outer radius
        Vector3 pos = transform.position;
        
        // Vòng tròn inner (vùng tránh) - màu đỏ
        Gizmos.color = Color.red;
        DrawCircle(pos, innerRadius, 32);
        
        // Vòng tròn outer (vùng ngoài) - màu xanh
        Gizmos.color = Color.green;
        DrawCircle(pos, outerRadius, 32);
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
