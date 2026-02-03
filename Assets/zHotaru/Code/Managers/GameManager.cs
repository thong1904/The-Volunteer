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
    
    [Header("Scene Settings")]
    [SerializeField] private string[] gameplayScenes = { "GameScene", "Museum", "Gameplay" };
    
    [Header("Game State")]
    private bool isGameRunning = false;
    private bool isPaused = false;
    private bool isFirstSceneLoad = true;
    
    // Properties để truy cập Sub-Managers
    public NPCManager NPCs => npcManager;
    public UpgradeManager Upgrades => upgradeManager;
    public SaveLoadManager SaveLoad => saveLoadManager;
    public UIManager UI => uiManager;
    public ScoreManager Score => scoreManager;
    
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
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] Scene loaded: {scene.name}");
        
        FindSceneManagers();
        
        if (uiManager != null)
            uiManager.Initialize();
        
        if (!isFirstSceneLoad && IsGameplayScene(scene.name))
        {
            Debug.Log($"[GameManager] Detected gameplay scene: {scene.name}. Starting new day...");
            StartNewDay();
        }
    }
    
    private bool IsGameplayScene(string sceneName)
    {
        if (gameplayScenes == null || gameplayScenes.Length == 0)
        {
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
        SubscribeToNPCManager();
        StartNewDay();
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
        
        if (upgradeManager == null)
            upgradeManager = GetComponentInChildren<UpgradeManager>();
            
        if (saveLoadManager == null)
            saveLoadManager = GetComponentInChildren<SaveLoadManager>();
        
        if (uiManager == null)
            uiManager = GetComponentInChildren<UIManager>();
        
        if (scoreManager == null)
            scoreManager = GetComponentInChildren<ScoreManager>();
        
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
        
        if (upgradeManager == null)
            upgradeManager = FindAnyObjectByType<UpgradeManager>();
            
        if (saveLoadManager == null)
            saveLoadManager = FindAnyObjectByType<SaveLoadManager>();
        
        if (uiManager == null)
            uiManager = FindAnyObjectByType<UIManager>();
        
        if (scoreManager == null)
            scoreManager = FindAnyObjectByType<ScoreManager>();
        
        // Subscribe lại vào manager mới
        SubscribeToNPCManager();
        
        Debug.Log($"[GameManager] Managers found - NPC:{npcManager != null}, Upgrade:{upgradeManager != null}, " +
                  $"SaveLoad:{saveLoadManager != null}, UI:{uiManager != null}, Score:{scoreManager != null}");
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
        EndDay();
    }
    
    public void StartNewDay()
    {
        isGameRunning = true;
        isPaused = false;
        
        if (Score != null) Score.ResetScore();
        if (npcManager != null) 
        {
            npcManager.ResetDayCounters(); // Reset counters cho ngày mới
            npcManager.StartCustomerSpawning();
        }
        
        OnGameStart?.Invoke();
        Debug.Log("🌅 Ngày mới bắt đầu!");
    }
    
    public void EndDay()
    {
        if (!isGameRunning) return;
        
        isGameRunning = false;
        
        if (npcManager != null) npcManager.StopCustomerSpawning();
        
        OnDayEnd?.Invoke();
        
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
