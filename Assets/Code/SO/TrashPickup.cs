using UnityEngine;

public class TrashPickup : MonoBehaviour, IInteractable
{
    public TrashSO trashData;
    TrashSO runtimeTrash;
    Outline outline;

    OutlineState currentState = OutlineState.None;

    void Awake()
    {
        outline = GetComponent<Outline>();

        runtimeTrash = Instantiate(trashData);
        runtimeTrash.RollPrice();

      
    }



    // 👉 CHỈ set nếu ưu tiên cao hơn
    public void RequestOutline(OutlineState state)
    {
        if (state < currentState) return;

        currentState = state;
        ApplyOutline();
    }

    // 👉 BỎ trạng thái (Interact hoặc Scan)
    public void ReleaseOutline(OutlineState state)
    {
        if (currentState != state) return;

        currentState = OutlineState.None;
        ApplyOutline();
    }

    void ApplyOutline()
    {
        if (outline == null) return;

        switch (currentState)
        {
            case OutlineState.None:
                outline.enabled = false;
                break;

            case OutlineState.Scan:
                outline.enabled = true;
                outline.OutlineColor = Color.cyan;
                outline.OutlineWidth = 5f;
                break;

            case OutlineState.Interact:
                outline.enabled = true;
                outline.OutlineColor = Color.yellow;
                outline.OutlineWidth = 10f;
                break;
        }
    }

    public void Interact()
    {
        Debug.Log("TRASH PRICE = " + runtimeTrash.price);

        bool added = InventoryManager.Instance.AddTrash(runtimeTrash);
        if (added)
        {
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Inventory full");
        }
    }
}

