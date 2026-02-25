using UnityEngine;

public class TrashBin : MonoBehaviour, IInteractable
{
    Outline outline;

    void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;
    }

    public void ShowOutline()
    {
        if (outline == null) return;

        outline.enabled = true;
        outline.OutlineColor = Color.green;
        outline.OutlineWidth = 10f;
    }

    public void HideOutline()
    {
        if (outline == null) return;

        outline.enabled = false;
    }

    public void Interact()
    {
        int money = InventoryManager.Instance.SellAllTrash();
        Debug.Log("Sold trash for: " + money);
        
        if (MoneyManager.Instance != null)
            MoneyManager.Instance.AddMoney(money);
    }
}
