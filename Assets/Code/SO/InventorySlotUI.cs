using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI")]
    public Image bgUnlocked;
    public Image bgLocked;
    public Image trashIcon;

    [Header("State")]
    public bool unlocked;

    TrashSO currentTrash;

    public bool IsEmpty => currentTrash == null;

    void Awake()
    {
        Refresh();
    }

    // =========================
    // SLOT STATE
    // =========================

    public void SetUnlocked(bool value)
    {
        unlocked = value;
        Refresh();
    }

    void Refresh()
    {
        // Background
        bgUnlocked.enabled = unlocked;
        bgLocked.enabled = !unlocked;

        // Trash icon
        trashIcon.enabled = unlocked && currentTrash != null;
    }

    // =========================
    // TRASH
    // =========================

    public void SetTrash(TrashSO trash)
    {
        if (!unlocked) return;

        currentTrash = trash;
        trashIcon.sprite = trash.icon;
        trashIcon.enabled = true;
    }

    public TrashSO Clear()
    {
        TrashSO t = currentTrash;
        currentTrash = null;

        trashIcon.sprite = null;
        trashIcon.enabled = false;
        return t;
    }

    public TrashSO GetTrash()
    {
        return currentTrash;
    }
}

