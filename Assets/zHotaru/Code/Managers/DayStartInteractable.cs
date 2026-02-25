using UnityEngine;

/// <summary>
/// Object tương tác để bắt đầu ngày mới.
/// Player cần tương tác với object này để DayNightManager và NPCManager bắt đầu hoạt động.
/// </summary>
public class DayStartInteractable : MonoBehaviour, IInteractable
{
    [Header("UI Settings")]
    [SerializeField] private string promptMessage = "E to start day";
    
    [Header("Visual")]
    [SerializeField] private bool useOutline = true;
    
    private Outline outline;
    private bool dayStarted = false;
    
    /// <summary>
    /// Kiểm tra ngày đã bắt đầu chưa
    /// </summary>
    public bool DayStarted => dayStarted;
    
    void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;
    }
    
    void Start()
    {
        // Subscribe vào events để reset khi ngày mới
        if (GameManager.Instance != null)
        {
            // Khi ngày kết thúc, cho phép bắt đầu lại
            GameManager.Instance.OnDayEnd += OnDayEnd;
        }
    }
    
    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDayEnd -= OnDayEnd;
        }
    }
    
    private void OnDayEnd()
    {
        dayStarted = false;
        Debug.Log("[DayStartInteractable] Ngày kết thúc, có thể bắt đầu ngày mới");
    }
    
    // ===== OUTLINE =====
    public void ShowOutline()
    {
        if (!useOutline || outline == null) return;
        
        outline.enabled = true;
        outline.OutlineColor = Color.green;
        outline.OutlineWidth = 8f;
    }
    
    public void HideOutline()
    {
        if (outline != null)
            outline.enabled = false;
    }
    
    /// <summary>
    /// Lấy prompt message để hiển thị
    /// </summary>
    public string GetPromptMessage()
    {
        if (dayStarted)
            return "Day already started";
        return promptMessage;
    }
    
    /// <summary>
    /// Kiểm tra có thể tương tác không
    /// </summary>
    public bool CanInteract()
    {
        return !dayStarted;
    }
    
    // ===== INTERACT =====
    public void Interact()
    {
        if (dayStarted)
        {
            Debug.Log("[DayStartInteractable] Ngày đã bắt đầu rồi!");
            return;
        }
        
        StartDay();
    }
    
    /// <summary>
    /// Bắt đầu ngày mới
    /// </summary>
    private void StartDay()
    {
        dayStarted = true;
        
        // Ẩn prompt
        if (PickupPromptUI.Instance != null)
            PickupPromptUI.Instance.Hide();
        
        // Bắt đầu DayNightManager
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.SetTimeRunning(true);
            Debug.Log("[DayStartInteractable] DayNightManager started!");
        }
        
        // Bắt đầu spawn NPC
        if (GameManager.Instance != null && GameManager.Instance.NPCs != null)
        {
            GameManager.Instance.NPCs.StartCustomerSpawning();
            Debug.Log("[DayStartInteractable] NPCManager bắt đầu spawn!");
        }
        
        // Notify game started
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NotifyDayStarted();
        }
        
        Debug.Log("🌅 [DayStartInteractable] Ngày mới bắt đầu!");
    }
}
