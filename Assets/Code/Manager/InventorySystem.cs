using UnityEngine;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    public int width = 8;
    public int height = 6;

    public InventoryItemInstance[,] grid;
    public bool[,] unlocked;

    public int unlockLevel = 0; // 0..3

    void Awake()
    {
        grid = new InventoryItemInstance[width, height];
        unlocked = new bool[width, height];
        ApplyUnlock();
    }

    // ================= UNLOCK =================

    public void ApplyUnlock()
    {
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            unlocked[x, y] = false;

        // 4x3 trái trên
        if (unlockLevel >= 0)
            UnlockBlock(0, 0, 4, 3);

        // 4x3 phải trên
        if (unlockLevel >= 1)
            UnlockBlock(4, 0, 4, 3);

        // 4x3 trái dưới
        if (unlockLevel >= 2)
            UnlockBlock(0, 3, 4, 3);

        // full
        if (unlockLevel >= 3)
            UnlockBlock(4, 3, 4, 3);
    }

    void UnlockBlock(int sx, int sy, int w, int h)
    {
        for (int y = sy; y < sy + h; y++)
        for (int x = sx; x < sx + w; x++)
            unlocked[x, y] = true;
    }

    // ================= PLACE =================

    public bool TryAddItem(TrashItemSO item)
    {
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            if (CanPlaceItem(item, x, y))
            {
                PlaceItem(new InventoryItemInstance(item, x, y));
                return true;
            }
        }
        return false;
    }

    public bool CanPlaceItem(TrashItemSO item, int startX, int startY)
    {
        for (int y = 0; y < item.size.y; y++)
        for (int x = 0; x < item.size.x; x++)
        {
            if (!item.OccupiesCell(x, y)) continue;

            int gx = startX + x;
            int gy = startY + y;

            if (gx < 0 || gy < 0 || gx >= width || gy >= height)
                return false;

            if (!unlocked[gx, gy]) return false;
            if (grid[gx, gy] != null) return false;
        }
        return true;
    }

    void PlaceItem(InventoryItemInstance inst)
    {
        for (int y = 0; y < inst.item.size.y; y++)
        for (int x = 0; x < inst.item.size.x; x++)
        {
            if (!inst.item.OccupiesCell(x, y)) continue;
            grid[inst.x + x, inst.y + y] = inst;
        }
    }

    public InventoryItemInstance GetItemAt(int x, int y)
        => grid[x, y];
}
