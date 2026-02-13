using UnityEngine;

/// <summary>
/// Các enums và structs dùng cho Build Mode
/// </summary>
/// 
/// <summary>
/// Trạng thái của Build Mode
/// </summary>
public enum BuildModeState
{
    Idle,           // Không làm gì, chỉ di chuyển camera
    Selecting,      // Đang chọn object để đặt
    Placing,        // Đang đặt object (preview)
    Editing         // Đang chỉnh sửa object đã đặt
}

/// <summary>
/// Kết quả kiểm tra vị trí đặt object
/// </summary>
public enum PlacementValidation
{
    Valid,              // Có thể đặt
    InvalidTerrain,     // Địa hình không hợp lệ
    Overlapping,        // Đang chồng lên object khác
    OutOfBounds,        // Ngoài phạm vi cho phép
    NotEnoughResources  // Không đủ tài nguyên
}

/// <summary>
/// Thông tin về vị trí đặt object
/// </summary>
[System.Serializable]
public struct PlacementInfo
{
    public Vector3 position;
    public Quaternion rotation;
    public bool isValid;
    public PlacementValidation validationResult;
    public Vector3 gridPosition;
    
    public PlacementInfo(Vector3 pos, Quaternion rot, bool valid = true)
    {
        position = pos;
        rotation = rot;
        isValid = valid;
        validationResult = PlacementValidation.Valid;
        gridPosition = pos;
    }
}

/// <summary>
/// Cài đặt Grid cho Build Mode
/// </summary>
[System.Serializable]
public class BuildGridSettings
{
    [Tooltip("Kích thước mỗi ô grid")]
    public float cellSize = 1f;
    
    [Tooltip("Có snap vào grid không")]
    public bool snapToGrid = true;
    
    [Tooltip("Hiển thị grid")]
    public bool showGrid = true;
    
    [Tooltip("Màu grid")]
    public Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    
    [Tooltip("Phạm vi hiển thị grid")]
    public float gridRange = 20f;
    
    /// <summary>
    /// Snap vị trí vào grid
    /// </summary>
    public Vector3 SnapToGrid(Vector3 worldPosition)
    {
        if (!snapToGrid) return worldPosition;
        
        float x = Mathf.Round(worldPosition.x / cellSize) * cellSize;
        float z = Mathf.Round(worldPosition.z / cellSize) * cellSize;
        
        return new Vector3(x, worldPosition.y, z);
    }
}
