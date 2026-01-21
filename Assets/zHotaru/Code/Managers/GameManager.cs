using UnityEngine;
using System;

/// <summary>
/// Core Manager - Quản lý toàn bộ game flow, tích hợp các Manager con
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Sub-Managers")]
    [SerializeField] private NPCManager npcManager;
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private SaveLoadManager saveLoadManager;
    
    [Header("Game State")]
    private bool isGameRunning = false;
    private bool isPaused = false;
    
    // Properties để truy cập Sub-Managers
    public NPCManager NPCs => npcManager;
    public UpgradeManager Upgrades => upgradeManager;
    public SaveLoadManager SaveLoad => saveLoadManager;
    
    // Properties cho các Manager độc lập
    public ScoreManager Score => ScoreManager.Instance;
    public DayNightManager DayNight => DayNightManager.Instance;
    public UIManager UI => UIManager.Instance;
    
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
        
        InitializeManagers();
    }
    
    void Start()
    {
        StartNewDay();
    }
    
    void Update()
    {
        if (!isGameRunning || isPaused) return;
        
        // Kiểm tra điều kiện kết thúc ngày
        if (DayNight != null && DayNight.IsNighttime())
        {
            EndDay();
        }
    }
    
    private void InitializeManagers()
    {
        // Tự động tìm hoặc tạo Sub-Managers nếu chưa có
        if (npcManager == null)
            npcManager = GetComponentInChildren<NPCManager>();
        
        if (upgradeManager == null)
            upgradeManager = GetComponentInChildren<UpgradeManager>();
            
        if (saveLoadManager == null)
            saveLoadManager = GetComponentInChildren<SaveLoadManager>();
    }
    
    public void StartNewDay()
    {
        isGameRunning = true;
        isPaused = false;
        
        // Reset các hệ thống
        if (Score != null) Score.ResetScore();
        if (DayNight != null) DayNight.StartNewDay();
        if (npcManager != null) npcManager.StartCustomerSpawning();
        
        OnGameStart?.Invoke();
        Debug.Log("🌅 Ngày mới bắt đầu!");
    }
    
    public void EndDay()
    {
        if (!isGameRunning) return;
        
        isGameRunning = false;
        
        // Dừng spawn NPC
        if (npcManager != null) npcManager.StopCustomerSpawning();
        
        OnDayEnd?.Invoke();
        
        // Auto save khi kết thúc ngày
        AutoSave();
        
        Debug.Log($"🌙 Ngày kết thúc! Tổng điểm: {Score?.GetTotalScore() ?? 0}");
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
}
