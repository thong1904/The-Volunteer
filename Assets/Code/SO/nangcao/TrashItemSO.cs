
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Trash/Item")]
public class TrashItemSO : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite icon;
    public int basePrice;

    [Header("Category")]
    public TrashCategory category;

    [Header("Grid Size (logic cell)")]
    public Vector2Int size = Vector2Int.one;

    [Header("Shape Mask (size.x * size.y)")]
    public List<bool> shapeMask = new();

    [Header("Aura Cells (offset from origin cell)")]
    public List<TrashAura> auraCells = new();

    // ================= VALIDATION =================

    public bool IsValid()
    {
        return shapeMask != null && shapeMask.Count == size.x * size.y;
    }

    // ================= SHAPE =================

    public bool OccupiesCell(Vector2Int localCell)
    {
        return OccupiesCell(localCell.x, localCell.y);
    }

    public bool OccupiesCell(int x, int y)
    {
        if (x < 0 || y < 0 || x >= size.x || y >= size.y)
            return false;

        int index = y * size.x + x;
        if (index < 0 || index >= shapeMask.Count)
            return false;

        return shapeMask[index];
    }

    // ================= INVENTORY SUPPORT =================

    /// <summary>
    /// Trả về toàn bộ cell LOCAL mà item chiếm
    /// </summary>
    public IEnumerable<Vector2Int> GetOccupiedLocalCells()
    {
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                if (OccupiesCell(x, y))
                    yield return new Vector2Int(x, y);
            }
        }
    }

    /// <summary>
    /// Trả về toàn bộ aura offset
    /// </summary>
    public IEnumerable<TrashAura> GetAuras()
    {
        return auraCells;
    }
    
}
