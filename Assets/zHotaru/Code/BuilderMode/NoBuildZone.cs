using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NoBuildZone - Vùng cấm đặt object trong Build Mode
/// Đặt component này vào GameObject có Collider để tạo vùng cấm
/// Có thể dùng BoxCollider, SphereCollider, hoặc bất kỳ Collider nào
/// </summary>
[RequireComponent(typeof(Collider))]
public class NoBuildZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private string zoneName = "No Build Zone";
    [SerializeField] private Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private bool showGizmo = true;
    
    [Header("Optional")]
    [Tooltip("Lý do không cho build (hiển thị cho player)")]
    [SerializeField] private string blockReason = "Không thể đặt ở vị trí này";
    
    // Static list để dễ truy cập từ các script khác
    private static List<NoBuildZone> _allZones = new List<NoBuildZone>();
    public static IReadOnlyList<NoBuildZone> AllZones => _allZones;
    
    private Collider _collider;
    private Bounds _bounds;
    
    public string ZoneName => zoneName;
    public string BlockReason => blockReason;
    public Bounds Bounds => _bounds;
    public Collider ZoneCollider => _collider;
    
    void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true; // Đảm bảo là trigger để không ảnh hưởng physics
        UpdateBounds();
    }
    
    void OnEnable()
    {
        if (!_allZones.Contains(this))
            _allZones.Add(this);
    }
    
    void OnDisable()
    {
        _allZones.Remove(this);
    }
    
    void OnValidate()
    {
        if (_collider == null)
            _collider = GetComponent<Collider>();
        UpdateBounds();
    }
    
    /// <summary>
    /// Cập nhật bounds của zone
    /// </summary>
    public void UpdateBounds()
    {
        if (_collider != null)
            _bounds = _collider.bounds;
    }
    
    /// <summary>
    /// Kiểm tra xem một điểm có nằm trong vùng cấm không
    /// </summary>
    public bool ContainsPoint(Vector3 point)
    {
        if (_collider == null) return false;
        
        // Dùng bounds check nhanh trước
        if (!_bounds.Contains(point)) return false;
        
        // Kiểm tra chính xác với collider
        return _collider.ClosestPoint(point) == point;
    }
    
    /// <summary>
    /// Kiểm tra xem một bounds có giao với vùng cấm không
    /// </summary>
    public bool IntersectsBounds(Bounds otherBounds)
    {
        if (_collider == null) return false;
        return _bounds.Intersects(otherBounds);
    }
    
    /// <summary>
    /// Static: Kiểm tra xem điểm có nằm trong BẤT KỲ vùng cấm nào không
    /// </summary>
    public static bool IsPointInAnyNoBuildZone(Vector3 point)
    {
        foreach (var zone in _allZones)
        {
            if (zone != null && zone.ContainsPoint(point))
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// Static: Kiểm tra xem bounds có giao với BẤT KỲ vùng cấm nào không
    /// </summary>
    public static bool DoesBoundsIntersectAnyNoBuildZone(Bounds bounds)
    {
        foreach (var zone in _allZones)
        {
            if (zone != null && zone.IntersectsBounds(bounds))
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// Static: Lấy vùng cấm chứa điểm (nếu có)
    /// </summary>
    public static NoBuildZone GetZoneAtPoint(Vector3 point)
    {
        foreach (var zone in _allZones)
        {
            if (zone != null && zone.ContainsPoint(point))
                return zone;
        }
        return null;
    }
    
    /// <summary>
    /// Static: Lấy vùng cấm giao với bounds (nếu có)
    /// </summary>
    public static NoBuildZone GetZoneIntersectingBounds(Bounds bounds)
    {
        foreach (var zone in _allZones)
        {
            if (zone != null && zone.IntersectsBounds(bounds))
                return zone;
        }
        return null;
    }
    
    #region Gizmos
    void OnDrawGizmos()
    {
        if (!showGizmo) return;
        DrawZoneGizmo(gizmoColor);
    }
    
    void OnDrawGizmosSelected()
    {
        // Vẽ đậm hơn khi được chọn
        DrawZoneGizmo(new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.6f));
    }
    
    private void DrawZoneGizmo(Color color)
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;
        
        Gizmos.color = color;
        
        if (col is BoxCollider box)
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(color.r, color.g, color.b, 1f);
            Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = oldMatrix;
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawSphere(transform.TransformPoint(sphere.center), sphere.radius * transform.lossyScale.x);
            Gizmos.color = new Color(color.r, color.g, color.b, 1f);
            Gizmos.DrawWireSphere(transform.TransformPoint(sphere.center), sphere.radius * transform.lossyScale.x);
        }
        else
        {
            // Fallback: vẽ bounds
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            Gizmos.color = new Color(color.r, color.g, color.b, 1f);
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
    #endregion
}
