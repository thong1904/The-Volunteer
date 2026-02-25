using UnityEngine;

public class ShopInteract : MonoBehaviour, IInteractable
{
    Outline outline;
    public GameObject shopCanvas;
    public PlayerController player;

    bool isOpen;
    
    // Static property để các script khác check (VD: GamePauseMenu)
    public static bool IsShopOpen { get; private set; } = false;
    
    // Singleton reference để có thể gọi CloseShop từ nơi khác
    public static ShopInteract CurrentOpenShop { get; private set; } = null;

    void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;

        if (shopCanvas != null)
            shopCanvas.SetActive(false);
    }
    
    void Update()
    {
        // Đóng shop khi nhấn ESC
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
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
        IsShopOpen = true;
        CurrentOpenShop = this;
        
        shopCanvas.SetActive(true);
        CursorManager.Instance.UnlockCursor();
        
        // Ẩn PickupPromptUI khi mở shop
        if (PickupPromptUI.Instance != null)
            PickupPromptUI.Instance.Hide();
    }

    // ===== CLOSE SHOP (BUTTON) =====
    public void CloseShop()
    {
        if (!isOpen) return;

        isOpen = false;
        IsShopOpen = false;
        CurrentOpenShop = null;
        
        shopCanvas.SetActive(false);
        CursorManager.Instance.LockCursor();
    }
}
