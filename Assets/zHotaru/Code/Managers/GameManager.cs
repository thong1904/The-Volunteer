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
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private SaveLoadManager saveLoadManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private DayNightManager dayNightManager;
    
    [Header("Scene Settings")]
    [SerializeField] private string[] gameplayScenes = { "GameScene", "Museum", "Gameplay" }; // Tên các scene gameplay
    
    [Header("Game State")]
    private bool isGameRunning = false;
    private bool isPaused = false;
    
    // Properties để truy cập Sub-Managers
    public NPCManager NPCs => npcManager;
    public UpgradeManager Upgrades => upgradeManager;
    public SaveLoadManager SaveLoad => saveLoadManager;
    public UIManager UI => uiManager;
    public ScoreManager Score => scoreManager;
    public DayNightManager DayNight => dayNightManager;
    
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
        
        // Đăng ký event khi chuyển scene
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        InitializeManagers();
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] Scene loaded: {scene.name}");
        
        // Tìm lại các manager trong scene mới
        FindSceneManagers();
        
        // Re-initialize UIManager
        if (uiManager != null)
            uiManager.Initialize();
        
        // Kiểm tra nếu đây là scene gameplay thì bắt đầu game
        if (IsGameplayScene(scene.name))
        {
            Debug.Log($"[GameManager] Detected gameplay scene: {scene.name}. Starting new day...");
            StartNewDay();
        }
    }
    
    /// <summary>
    /// Kiểm tra scene có phải là gameplay scene không
    /// </summary>
    private bool IsGameplayScene(string sceneName)
    {
        if (gameplayScenes == null || gameplayScenes.Length == 0)
        {
            // Fallback: Nếu không config, kiểm tra có NPCManager trong scene không
            return npcManager != null;
        }
        
        foreach (string gpScene in gameplayScenes)
        {
            if (sceneName.Contains(gpScene) || gpScene.Contains(sceneName))
                return true;
        }
        return false;
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
        // Ưu tiên dùng các manager đã gán trong Inspector (children của GameManager)
        if (npcManager == null)
            npcManager = GetComponentInChildren<NPCManager>();
        
        if (upgradeManager == null)
            upgradeManager = GetComponentInChildren<UpgradeManager>();
            
        if (saveLoadManager == null)
            saveLoadManager = GetComponentInChildren<SaveLoadManager>();
        
        if (uiManager == null)
            uiManager = GetComponentInChildren<UIManager>();
        
        if (scoreManager == null)
            scoreManager = GetComponentInChildren<ScoreManager>();
        
        if (dayNightManager == null)
            dayNightManager = GetComponentInChildren<DayNightManager>();
        
        // Nếu vẫn chưa tìm thấy, tìm trong scene
        FindSceneManagers();
        
        // Initialize UIManager nếu có
        if (uiManager != null)
            uiManager.Initialize();
    }
    
    /// <summary>
    /// Tìm các manager trong scene hiện tại (dùng khi chuyển scene)
    /// </summary>
    private void FindSceneManagers()
    {
        // Tìm các manager trong scene nếu chưa có hoặc bị null (đã bị destroy khi chuyển scene)
        if (npcManager == null)
            npcManager = FindAnyObjectByType<NPCManager>();
        
        if (upgradeManager == null)
            upgradeManager = FindAnyObjectByType<UpgradeManager>();
            
        if (saveLoadManager == null)
            saveLoadManager = FindAnyObjectByType<SaveLoadManager>();
        
        if (uiManager == null)
            uiManager = FindAnyObjectByType<UIManager>();
        
        if (scoreManager == null)
            scoreManager = FindAnyObjectByType<ScoreManager>();
        
        if (dayNightManager == null)
            dayNightManager = FindAnyObjectByType<DayNightManager>();
        
        Debug.Log($"[GameManager] Managers found - NPC:{npcManager != null}, Upgrade:{upgradeManager != null}, " +
                  $"SaveLoad:{saveLoadManager != null}, UI:{uiManager != null}, Score:{scoreManager != null}, DayNight:{dayNightManager != null}");
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
