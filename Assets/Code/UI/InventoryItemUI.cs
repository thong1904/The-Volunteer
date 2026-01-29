using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
    public Image icon;
    public RectTransform rect;
   public RectTransform auraContainer; // Container chứa các icon aura
    public GameObject auraIconPrefab;  
    public void Setup(InventoryItemInstance inst, float cellSize, float padding)
    {
        if (icon == null)
            icon = GetComponentInChildren<Image>();

        if (rect == null)
            rect = GetComponent<RectTransform>();

        icon.sprite = inst.item.icon;
        icon.preserveAspect = true;

          // Prefab cho icon aura (chứa Image)
            float step = cellSize + padding;

        // SIZE = số ô chiếm * step - padding dư
        rect.sizeDelta = new Vector2(
            inst.item.size.x * step - padding,
            inst.item.size.y * step - padding
        );

        // VỊ TRÍ = góc trái trên item
        rect.anchoredPosition = new Vector2(
            inst.x * step,
            -inst.y * step
        );
        
            // Hiển thị các icon aura quanh item
            if (auraContainer != null && auraIconPrefab != null && inst.item.auraCells != null)
            {
                // Xóa các icon aura cũ
                for (int i = auraContainer.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(auraContainer.GetChild(i).gameObject);
                }

                foreach (var aura in inst.item.auraCells)
                {
                    if (aura == null || aura.rules == null) continue;
                    Sprite auraSprite = null;
                    foreach (var rule in aura.rules)
                    {
                        if (rule != null && rule.icon != null)
                        {
                            auraSprite = rule.icon;
                            break;
                        }
                    }
                    if (auraSprite == null) continue;

                    // Tạo icon aura mới
                    var go = Instantiate(auraIconPrefab, auraContainer);
                    var img = go.GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = auraSprite;
                        img.preserveAspect = true;
                        img.enabled = true;
                    }
                    // Đặt vị trí theo offset aura (tính theo step, cùng hệ quy chiếu với item)
                    var rt = go.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchoredPosition = new Vector2(
                            (inst.x + aura.offset.x) * step,
                            -(inst.y + aura.offset.y) * step
                        );
                        rt.sizeDelta = new Vector2(cellSize, cellSize);
                    }
                }
            }
    }
}
