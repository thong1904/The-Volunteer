using UnityEngine;

public class ShopInteract : MonoBehaviour, IInteractable
{
    Outline outline;
    public GameObject shopCanvas;
    public PlayerController player;

    bool isOpen;

    void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;

        if (shopCanvas != null)
            shopCanvas.SetActive(false);
    }

    // ===== OUTLINE =====
    public void ShowOutline()
    {
        if (outline == null) return;

        outline.enabled = true;
        outline.OutlineColor = Color.yellow;
        outline.OutlineWidth = 10f;
    }

    public void HideOutline()
    {
        if (outline == null) return;
        outline.enabled = false;
    }

    // ===== INTERACT (PRESS E) =====
    public void Interact()
    {
        OpenShop();
    }

    // ===== OPEN SHOP =====
   public void OpenShop()
{
    if (isOpen) return;

    isOpen = true;
    shopCanvas.SetActive(true);
    CursorManager.Instance.UnlockCursor();

   // 🚫 khóa camera & move
}


    // ===== CLOSE SHOP (BUTTON) =====
  public void CloseShop()
{
    if (!isOpen) return;

    isOpen = false;
    shopCanvas.SetActive(false);
    CursorManager.Instance.LockCursor();

   // ✅ bật lại camera & move
}

}
