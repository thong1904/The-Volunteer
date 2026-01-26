using UnityEngine;
using System;

/// <summary>
/// Quản lý Save/Load với 3 slot thủ công + 1 Auto Save
/// </summary>
public class SaveLoadManager : MonoBehaviour
{
    [Header("Auto Save Settings")]
    [SerializeField] private bool autoSaveEnabled = true;
    [SerializeField] private float autoSaveInterval = 300f; // 5 phút
    
    private float nextAutoSaveTime;
    
    // Slot keys
    private const string AUTO_SAVE_KEY = "AutoSave";
    private const string SAVE_SLOT_PREFIX = "SaveSlot_"; // SaveSlot_1, SaveSlot_2, SaveSlot_3
    public const int MAX_SAVE_SLOTS = 3;
    
    public bool IsAutoSaveEnabled => autoSaveEnabled;
    
    // Events
    public event Action<int> OnGameSaved; // slot index (-1 = auto save)
    public event Action<int> OnGameLoaded;
    
    void Start()
    {
        // Khởi tạo thời gian auto save
        nextAutoSaveTime = Time.time + autoSaveInterval;
    }
    
    void Update()
    {
        // Chỉ auto save khi game đang chạy
        var gm = GameManager.Instance;
        if (gm == null) return;
        
        // Sử dụng Time.timeScale để kiểm tra pause (vì GameManager set timeScale = 0 khi pause)
        bool isGameActive = Time.timeScale > 0f;
        
        if (autoSaveEnabled && isGameActive && Time.time >= nextAutoSaveTime)
        {
            AutoSave();
            nextAutoSaveTime = Time.time + autoSaveInterval;
        }
    }
    
    #region Public Save/Load Methods
    
    /// <summary>
    /// Lưu game vào slot (1-3)
    /// </summary>
    public void SaveToSlot(int slotIndex)
    {
        if (slotIndex < 1 || slotIndex > MAX_SAVE_SLOTS)
        {
            Debug.LogError($"[SaveLoadManager] Invalid slot index: {slotIndex}. Must be 1-{MAX_SAVE_SLOTS}");
            return;
        }
        
        string key = SAVE_SLOT_PREFIX + slotIndex;
        SaveGameData(key);
        
        Debug.Log($"💾 Game saved to Slot {slotIndex}!");
        OnGameSaved?.Invoke(slotIndex);
    }
    
    /// <summary>
    /// Load game từ slot (1-3)
    /// </summary>
    public void LoadFromSlot(int slotIndex)
    {
        if (slotIndex < 1 || slotIndex > MAX_SAVE_SLOTS)
        {
            Debug.LogError($"[SaveLoadManager] Invalid slot index: {slotIndex}. Must be 1-{MAX_SAVE_SLOTS}");
            return;
        }
        
        string key = SAVE_SLOT_PREFIX + slotIndex;
        if (LoadGameData(key))
        {
            Debug.Log($"📂 Game loaded from Slot {slotIndex}!");
            OnGameLoaded?.Invoke(slotIndex);
        }
    }
    
    /// <summary>
    /// Auto Save (slot riêng, không ảnh hưởng 3 slot thủ công)
    /// </summary>
    public void AutoSave()
    {
        SaveGameData(AUTO_SAVE_KEY);
        Debug.Log("💾 Auto-saved!");
        OnGameSaved?.Invoke(-1); // -1 = auto save
    }
    
    /// <summary>
    /// Load từ Auto Save
    /// </summary>
    public void LoadAutoSave()
    {
        if (LoadGameData(AUTO_SAVE_KEY))
        {
            Debug.Log("📂 Loaded from Auto Save!");
            OnGameLoaded?.Invoke(-1);
        }
    }
    
    /// <summary>
    /// Xóa save slot
    /// </summary>
    public void DeleteSlot(int slotIndex)
    {
        if (slotIndex < 1 || slotIndex > MAX_SAVE_SLOTS)
            return;
            
        string key = SAVE_SLOT_PREFIX + slotIndex;
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
        Debug.Log($"🗑️ Slot {slotIndex} deleted!");
    }
    
    /// <summary>
    /// Xóa Auto Save
    /// </summary>
    public void DeleteAutoSave()
    {
        PlayerPrefs.DeleteKey(AUTO_SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("🗑️ Auto Save deleted!");
    }
    
    #endregion
    
    #region Slot Info (for UI)
    
    /// <summary>
    /// Kiểm tra slot có dữ liệu không
    /// </summary>
    public bool HasSaveData(int slotIndex)
    {
        if (slotIndex == -1) // Auto save
            return PlayerPrefs.HasKey(AUTO_SAVE_KEY);
            
        if (slotIndex < 1 || slotIndex > MAX_SAVE_SLOTS)
            return false;
            
        return PlayerPrefs.HasKey(SAVE_SLOT_PREFIX + slotIndex);
    }
    
    /// <summary>
    /// Lấy thông tin preview của slot (để hiển thị trên UI)
    /// </summary>
    public SaveSlotInfo GetSlotInfo(int slotIndex)
    {
        string key = slotIndex == -1 ? AUTO_SAVE_KEY : SAVE_SLOT_PREFIX + slotIndex;
        
        if (!PlayerPrefs.HasKey(key))
            return null;
            
        try
        {
            string json = PlayerPrefs.GetString(key);
            GameData data = JsonUtility.FromJson<GameData>(json);
            
            return new SaveSlotInfo
            {
                slotIndex = slotIndex,
                totalScore = data.totalScore,
                saveDateTime = data.saveDateTime,
                playTime = data.playTimeSeconds,
                isEmpty = false
            };
        }
        catch
        {
            return null;
        }
    }
    
    #endregion
    
    #region Internal Save/Load
    
    private void SaveGameData(string key)
    {
        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogWarning("[SaveLoadManager] GameManager.Instance is null!");
            return;
        }
        
        GameData data = new GameData
        {
            // Score
            totalScore = gm.Score?.GetTotalScore() ?? 0,
            
            // Upgrades
            npcLimitLevel = gm.Upgrades?.npcLimitLevel ?? 1,
            playerSpeedLevel = gm.Upgrades?.playerSpeedLevel ?? 1,
            scoreMultiplierLevel = gm.Upgrades?.scoreMultiplierLevel ?? 1,
            
            // Meta info
            saveDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            playTimeSeconds = Time.time, // Hoặc tính tổng thời gian chơi
            gameVersion = Application.version
        };
        
        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
    }
    
    private bool LoadGameData(string key)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            Debug.LogWarning($"[SaveLoadManager] No save data found for key: {key}");
            return false;
        }
        
        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogWarning("[SaveLoadManager] GameManager.Instance is null!");
            return false;
        }
        
        try
        {
            string json = PlayerPrefs.GetString(key);
            GameData data = JsonUtility.FromJson<GameData>(json);
            
            if (data == null)
            {
                Debug.LogError("[SaveLoadManager] Failed to parse save data!");
                return false;
            }
            
            // Load score
            if (gm.Score != null)
                gm.Score.SetScore(data.totalScore);
            
            // Load upgrades
            if (gm.Upgrades != null)
            {
                gm.Upgrades.npcLimitLevel = data.npcLimitLevel;
                gm.Upgrades.playerSpeedLevel = data.playerSpeedLevel;
                gm.Upgrades.scoreMultiplierLevel = data.scoreMultiplierLevel;
            }
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLoadManager] Error loading save data: {e.Message}");
            return false;
        }
    }
    
    #endregion
    
    #region Legacy Support (backward compatibility)
    
    /// <summary>
    /// Legacy method - giữ lại để không break code cũ
    /// </summary>
    public void SaveGame() => SaveToSlot(1);
    
    /// <summary>
    /// Legacy method - giữ lại để không break code cũ
    /// </summary>
    public void LoadGame() => LoadFromSlot(1);
    
    #endregion
}

[System.Serializable]
public class GameData
{
    // Game Progress
    public int totalScore;
    
    // Upgrades
    public int npcLimitLevel;
    public int playerSpeedLevel;
    public int scoreMultiplierLevel;
    
    // Meta Info (để hiển thị trên UI)
    public string saveDateTime;
    public float playTimeSeconds;
    public string gameVersion;
    
    // Thêm các field khác khi cần
}

/// <summary>
/// Thông tin slot để hiển thị trên UI
/// </summary>
public class SaveSlotInfo
{
    public int slotIndex;
    public int totalScore;
    public string saveDateTime;
    public float playTime;
    public bool isEmpty;
}