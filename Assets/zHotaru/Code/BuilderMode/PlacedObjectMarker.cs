using UnityEngine;

/// <summary>
/// Marker component được gắn vào mỗi object đã đặt trong Build Mode
/// Giúp dễ dàng detect object khi click/hold chuột
/// </summary>
public class PlacedObjectMarker : MonoBehaviour
{
    public BuildModePlacer.PlacedObjectData Data { get; set; }
    
    /// <summary>
    /// Lấy marker từ một GameObject bất kỳ (có thể là child)
    /// </summary>
    public static PlacedObjectMarker GetFromObject(GameObject obj)
    {
        if (obj == null) return null;
        
        // Tìm trên chính object hoặc parent
        return obj.GetComponentInParent<PlacedObjectMarker>();
    }
}
