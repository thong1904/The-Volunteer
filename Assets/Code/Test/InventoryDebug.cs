using UnityEngine;

public class InventoryDebug : MonoBehaviour
{
    public InventorySystem inventory;
    public InventoryUI ui;
    public TrashItemSO[] trashList;

    public void AddRandom()
    {
        if (trashList.Length == 0) return;

        var item = trashList[Random.Range(0, trashList.Length)];
        if (inventory.TryAddItem(item))
            ui.Refresh();
    }

    public void Upgrade()
    {
        inventory.unlockLevel++;
        inventory.ApplyUnlock();
        ui.Refresh();
    }
}
