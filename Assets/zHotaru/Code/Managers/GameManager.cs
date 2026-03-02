using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Core Manager - Quản lý toàn bộ game flow, tích hợp các Manager con
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Sub-Managers")]
    [SerializeField] private NPCManager npcManager;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private SaveLoadManager saveLoadManager;
    [SerializeField] private UIManager uiManager;
    
    [Header("Camera Control")]
    [Tooltip("Kéo Camera hoặc GameObject chứa script điều khiển camera vào đây")]
    [SerializeField] private MonoBehaviour cameraController;
    
    [Header("Scene Settings")]
    [SerializeField] private string[] gameplayScenes = { "GameScene", "Museum", "Gameplay" };
    
    [Header("Game State")]
    private bool isGameRunning = false;
    private bool isPaused = false;
    private bool isFirstSceneLoad = true;
    
    // Properties để truy cập Sub-Managers
    public NPCManager NPCs => npcManager;
    public MoneyManager Money => moneyManager;
    public SaveLoadManager SaveLoad => saveLoadManager;
    public UIManager UI => uiManager;
    
    // Events
    public event Action OnGameStart;
    public event Action OnGamePause;
    public event Action OnGameResume;
    public event Action OnDayEnd;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        InitializeManagers();
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromNPCManager();
        UnsubscribeFromDayNight();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] Scene loaded: {scene.name}");
        
        FindSceneManagers();
        
        if (uiManager != null)
            uiManager.Initialize();
        
        if (!isFirstSceneLoad)
        {
            if (IsGameplayScene(scene.name))
            {
                Debug.Log($"[GameManager] Detected gameplay scene: {scene.name}. Starting new day...");
                StartNewDay();
            }
            else
            {
                // Non-gameplay scene (Main Menu, etc.) - hiển thị cursor
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                isGameRunning = false;
                Debug.Log($"[GameManager] Non-gameplay scene: {scene.name}. Cursor unlocked.");
            }
        }
    }
    
    private bool IsGameplayScene(string sceneName)
    {
        // Kiểm tra dựa trên sự tồn tại của DayNightManager
        // Main Menu không có DayNightManager, chỉ Gameplay scene mới có
        return DayNightManager.Instance != null;
    }
    
    void Start()
    {
        SubscribeToNPCManager();
        SubscribeToDayNight();
        
        // Chỉ StartNewDay nếu đang ở gameplay scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (IsGameplayScene(currentSceneName))
        {
            StartNewDay();
        }
        else
        {
            // Main Menu - đảm bảo cursor hiển thị
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        isFirstSceneLoad = false;
    }
    
    void Update()
    {
        if (!isGameRunning || isPaused) return;
        // Không cần check gì ở đây nữa - EndDay được gọi qua event từ NPCManager
    }
    
    private void InitializeManagers()
    {
        if (npcManager == null)
            npcManager = GetComponentInChildren<NPCManager>();
        
        if (moneyManager == null)
            moneyManager = GetComponentInChildren<MoneyManager>();
            
        if (saveLoadManager == null)
            saveLoadManager = GetComponentInChildren<SaveLoadManager>();
        
        if (uiManager == null)
            uiManager = GetComponentInChildren<UIManager>();
        
        FindSceneManagers();
        
        if (uiManager != null)
            uiManager.Initialize();
    }
    
    private void FindSceneManagers()
    {
        // Unsubscribe từ manager cũ
        UnsubscribeFromNPCManager();
        
        if (npcManager == null)
            npcManager = FindAnyObjectByType<NPCManager>();
        
        if (moneyManager == null)
            moneyManager = FindAnyObjectByType<MoneyManager>();
            
        if (saveLoadManager == null)
            saveLoadManager = FindAnyObjectByType<SaveLoadManager>();
        
        if (uiManager == null)
            uiManager = FindAnyObjectByType<UIManager>();
        
        // Subscribe lại vào manager mới
        SubscribeToNPCManager();
        
        Debug.Log($"[GameManager] Managers found - NPC:{npcManager != null}, Money:{moneyManager != null}, " +
                  $"SaveLoad:{saveLoadManager != null}, UI:{uiManager != null}");
    }
    
    private void SubscribeToNPCManager()
    {
        if (npcManager != null)
        {
            npcManager.OnAllNPCsLeft -= HandleAllNPCsLeft;
            npcManager.OnAllNPCsLeft += HandleAllNPCsLeft;
        }
    }
    
    private void UnsubscribeFromNPCManager()
    {
        if (npcManager != null)
        {
            npcManager.OnAllNPCsLeft -= HandleAllNPCsLeft;
        }
    }
    
    /// <summary>
    /// Được gọi khi tất cả NPC đã rời đi
    /// </summary>
    private void HandleAllNPCsLeft()
    {
        Debug.Log("[GameManager] Tất cả NPC đã rời đi!");
        
        // Nếu đã nighttime (21h) → kết thúc ngày và hiện panel
        if (DayNightManager.Instance != null && DayNightManager.Instance.IsNighttime())
        {
            Debug.Log("[GameManager] Đã nighttime - Kết thúc ngày!");
            EndDay();
        }
    }
    
    private void SubscribeToDayNight()
    {
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.OnSunset -= HandleSunset;
            DayNightManager.Instance.OnSunset += HandleSunset;
        }
    }
    
    private void UnsubscribeFromDayNight()
    {
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.OnSunset -= HandleSunset;
        }
    }
    
    /// <summary>
    /// Được gọi khi DayNightManager kết thúc ngày (tối)
    /// </summary>
    private void HandleSunset()
    {
        Debug.Log("[GameManager] 🌅 DayNightManager: Hoàng hôn - Kết thúc ngày!");
        
        // Bắt tất cả NPC rời đi
        if (npcManager != null)
        {
            npcManager.ForceAllNPCsToLeave();
            
            // Nếu không có NPC nào → EndDay ngay
            if (npcManager.ActiveCustomerCount == 0)
            {
                Debug.Log("[GameManager] Không có NPC trong museum - EndDay ngay");
                EndDay();
            }
            // Nếu có NPC → chờ họ rời đi hết (HandleAllNPCsLeft sẽ gọi EndDay)
        }
        else
        {
            // Fallback nếu không có NPCManager
            EndDay();
        }
    }
    
    public void StartNewDay()
    {
        isGameRunning = true;
        isPaused = false;
        
        // Khóa cursor và bật camera cho gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SetCameraEnabled(true);
        
        if (Money != null) Money.ResetDayStats();
        if (npcManager != null) 
        {
            npcManager.ResetDayCounters(); // Reset counters cho ngày mới
            // KHÔNG tự động spawn - chờ player tương tác với DayStartInteractable
        }
        
        // Dừng thời gian cho đến khi player bắt đầu ngày
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.SetTimeRunning(false);
        }
        
        OnGameStart?.Invoke();
        Debug.Log("🌅 Ngày mới đã sẵn sàng! Chờ player bắt đầu...");
    }
    
    /// <summary>
    /// Được gọi khi player tương tác với DayStartInteractable
    /// </summary>
    public void NotifyDayStarted()
    {
        Debug.Log("[GameManager] Ngày đã được bắt đầu bởi player!");
        // Có thể thêm logic khác ở đây nếu cần
    }
    
    public void EndDay()
    {
        if (!isGameRunning) return;
        
        isGameRunning = false;
        
        if (npcManager != null) npcManager.StopCustomerSpawning();
        
        // Hiển thị cursor và tắt camera khi kết thúc ngày
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SetCameraEnabled(false);
        
        OnDayEnd?.Invoke();
        
        AutoSave();
        
        Debug.Log($"🌙 Ngày kết thúc! Tổng tiền: {Money?.GetTotalMoney() ?? 0}");
    }
    
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        OnGamePause?.Invoke();
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        OnGameResume?.Invoke();
    }
    
    public void SaveGame()
    {
        if (saveLoadManager != null)
        {
            saveLoadManager.SaveGame();
            Debug.Log("💾 Game saved!");
        }
    }
    
    public void LoadGame()
    {
        if (saveLoadManager != null)
        {
            saveLoadManager.LoadGame();
            Debug.Log("📂 Game loaded!");
        }
    }
    
    private void AutoSave()
    {
        if (saveLoadManager != null && saveLoadManager.IsAutoSaveEnabled)
        {
            saveLoadManager.SaveGame();
            Debug.Log("💾 Auto-saved!");
        }
    }
    
    /// <summary>
    /// Bật/tắt camera controller
    /// </summary>
    private void SetCameraEnabled(bool enabled)
    {
        if (cameraController != null)
            cameraController.enabled = enabled;
    }
}
