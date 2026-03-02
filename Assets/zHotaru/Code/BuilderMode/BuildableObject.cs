using UnityEngine;

/// <summary>
/// BuildableObject - Định nghĩa một object có thể đặt trong Build Mode
/// </summary>
[CreateAssetMenu(fileName = "New Buildable", menuName = "Build Mode/Buildable Object")]
public class BuildableObject : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("ID duy nhất để save/load. Nên giữ nguyên sau khi đã có save data!")]
    public string objectId;  // ID duy nhất cho save/load
    public string objectName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;
    
    [Header("Cost")]
    public int price = 100;                         // Giá tiền để xây
    
    [Header("Prefab")]
    public GameObject prefab;
    
    [Header("Placement Settings")]
    public Vector3 placementOffset = Vector3.zero;  // Offset vị trí khi đặt
    public bool alignToGround = true;               // Căn theo mặt đất
    
    [Header("Grid Settings")]
    public Vector2Int gridFootprint = Vector2Int.one;    // Số ô grid mà object chiếm (1x1, 2x2, v.v.)
    
    [Header("Category")]
    public BuildCategory category = BuildCategory.Structure;
}

/// <summary>
/// Enum định nghĩa các category cho Build Mode
/// </summary>
public enum BuildCategory
{
    Structure,      // Công trình, tường, sàn
    Furniture,      // Đồ nội thất
    Decoration      // Trang trí
}
