using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("Reference")]
    public InventorySystem inventory;

    [Header("Layout")]
    public float cellSize = 64f;   // size 1 ô
    public float padding = 6f;     // khoảng hở giữa ô

    [Header("Roots")]
    public RectTransform slotRoot; // chứa slot nền
    public RectTransform itemRoot; // chứa icon item

    [Header("Prefabs")]
    public InventorySlotUI slotPrefab;
    public InventoryItemUI itemPrefab;

    InventorySlotUI[,] slots;
    List<InventoryItemUI> itemUIs = new();

    void Start()
    {
        CreateSlots();
        Refresh();
    }

    // ======================================================
    // CREATE SLOT GRID
    // ======================================================

    void CreateSlots()
    {
        slots = new InventorySlotUI[inventory.width, inventory.height];

        float step = cellSize + padding;

        for (int y = 0; y < inventory.height; y++)
        {
            for (int x = 0; x < inventory.width; x++)
            {
                var slot = Instantiate(slotPrefab, slotRoot);
                var rt = slot.GetComponent<RectTransform>();

                rt.sizeDelta = new Vector2(cellSize, cellSize);
                rt.anchoredPosition = new Vector2(
                    x * step,
                    -y * step
                );

                slots[x, y] = slot;
            }
        }
    }

    // ======================================================
    // REFRESH
    // ======================================================

    public void Refresh()
    {
        // ---- Update slot background ----
        for (int y = 0; y < inventory.height; y++)
        {
            for (int x = 0; x < inventory.width; x++)
            {
                bool hasItem = inventory.grid[x, y] != null;
                bool unlocked = inventory.unlocked[x, y];

                slots[x, y].SetState(unlocked, hasItem);
            }
        }

        // ---- Clear old items ----
        foreach (var ui in itemUIs)
            Destroy(ui.gameObject);

        itemUIs.Clear();

        // ---- Spawn items ----
        HashSet<InventoryItemInstance> spawned = new();

        for (int y = 0; y < inventory.height; y++)
        {
            for (int x = 0; x < inventory.width; x++)
            {
                var inst = inventory.GetItemAt(x, y);
                if (inst == null || spawned.Contains(inst))
                    continue;

                var itemUI = Instantiate(itemPrefab, itemRoot);
                itemUI.Setup(inst, cellSize, padding);

                itemUIs.Add(itemUI);
                spawned.Add(inst);
            }
        }
    }
}
