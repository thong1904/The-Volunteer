using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<InventorySlotUI> slots = new();
    public int unlockedSlots = 3;

    void Awake()
    {
        Instance = this;
        UpdateSlots();
    }

   void UpdateSlots()
{
    for (int i = 0; i < slots.Count; i++)
    {
        slots[i].SetUnlocked(i < unlockedSlots);
    }
}

    public bool AddTrash(TrashSO trash)
    {
        foreach (var slot in slots)
        {
            if (!slot.unlocked) continue;
            if (slot.IsEmpty)
            {
                slot.SetTrash(trash);
                return true;
            }
        }
        return false;
    }

    public int SellAllTrash()
    {
        int total = 0;

        foreach (var slot in slots)
        {
            if (!slot.unlocked || slot.IsEmpty) continue;

            total += slot.GetTrash().price;
            slot.Clear();
        }

        return total;
    }

    public void UnlockNextSlot()
    {
        if (unlockedSlots >= slots.Count) return;
        unlockedSlots++;
        UpdateSlots();
    }
}
