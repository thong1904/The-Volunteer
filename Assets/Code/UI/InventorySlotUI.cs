using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image background;

    public Color normal;
    public Color locked;
    public Color occupied;

    public void SetState(bool unlocked, bool hasItem)
    {
        if (!unlocked)
            background.color = locked;
        else if (hasItem)
            background.color = occupied;
        else
            background.color = normal;
    }
}
