using UnityEngine;
using System;

/// <summary>
/// Component cho phép player tương tác với NPC khi NPC đang chờ (wave animation)
/// </summary>
public class NPCInteractable : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private string promptMessage = "E to talk";
    [SerializeField] private bool useOutline = true;
    
    private Outline outline;
    private bool canInteract = false;
    
    /// <summary>
    /// Event khi player tương tác với NPC
    /// </summary>
    public event Action OnPlayerInteracted;
    
    /// <summary>
    /// Kiểm tra NPC có thể tương tác không
    /// </summary>
    public bool CanInteract => canInteract;
    
    /// <summary>
    /// Lấy prompt message
    /// </summary>
    public string GetPromptMessage() => promptMessage;
    
    void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline == null)
            outline = GetComponentInChildren<Outline>();
    }
    
    /// <summary>
    /// Bật chế độ chờ tương tác (NPC đang wave)
    /// </summary>
    public void EnableInteraction(string customPrompt = null)
    {
        canInteract = true;
        if (!string.IsNullOrEmpty(customPrompt))
            promptMessage = customPrompt;
        Debug.Log($"[NPCInteractable] {gameObject.name}: Đang chờ player tương tác");
    }
    
    /// <summary>
    /// Tắt chế độ chờ tương tác
    /// </summary>
    public void DisableInteraction()
    {
        canInteract = false;
        HideOutline();
        Debug.Log($"[NPCInteractable] {gameObject.name}: Hết thời gian chờ tương tác");
    }
    
    // ===== OUTLINE =====
    public void ShowOutline()
    {
        if (!useOutline || outline == null) return;
        
        outline.enabled = true;
        outline.OutlineColor = Color.yellow;
        outline.OutlineWidth = 6f;
    }
    
    public void HideOutline()
    {
        if (outline != null)
            outline.enabled = false;
    }
    
    // ===== INTERACT =====
    public void Interact()
    {
        if (!canInteract)
        {
            Debug.Log($"[NPCInteractable] {gameObject.name}: Không thể tương tác lúc này");
            return;
        }
        
        canInteract = false;
        HideOutline();
        
        Debug.Log($"[NPCInteractable] {gameObject.name}: Player đã tương tác!");
        OnPlayerInteracted?.Invoke();
    }
}
