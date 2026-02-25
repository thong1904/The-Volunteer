using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Quản lý Save/Load với 3 slot thủ công + 1 Auto Save
/// Bao gồm: Game data + Building data
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
                totalMoney = data.totalMoney,
                buildingCount = data.buildings?.Count ?? 0,
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
            // Money
            totalMoney = gm.Money?.GetTotalMoney() ?? 0,
            
            // Buildings
            buildings = GetBuildingSaveData(),
            
            // Meta info
            saveDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            playTimeSeconds = Time.time,
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
            
            // Load money
            if (gm.Money != null)
                gm.Money.SetMoney(data.totalMoney);
            
            // Load buildings
            LoadBuildingSaveData(data.buildings);
            
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
    
    #region Building Save/Load
    
    /// <summary>
    /// Lấy data của tất cả buildings đã đặt
    /// </summary>
    private List<BuildingSaveData> GetBuildingSaveData()
    {
        var result = new List<BuildingSaveData>();
        
        if (BuildModePlacer.Instance == null)
            return result;
        
        foreach (var placedData in BuildModePlacer.Instance.PlacedObjects)
        {
            if (placedData.buildableObject == null || placedData.gameObject == null)
                continue;
            
            // Sử dụng objectId nếu có, fallback sang objectName
            string id = !string.IsNullOrEmpty(placedData.buildableObject.objectId) 
                ? placedData.buildableObject.objectId 
                : placedData.buildableObject.objectName;
            
            result.Add(new BuildingSaveData
            {
                objectId = id,
                posX = placedData.position.x,
                posY = placedData.position.y,
                posZ = placedData.position.z,
                rotX = placedData.rotation.x,
                rotY = placedData.rotation.y,
                rotZ = placedData.rotation.z,
                rotW = placedData.rotation.w
            });
        }
        
        Debug.Log($"💾 Saved {result.Count} buildings");
        return result;
    }
    
    /// <summary>
    /// Load và spawn lại các buildings từ save data
    /// </summary>
    private void LoadBuildingSaveData(List<BuildingSaveData> buildings)
    {
        if (buildings == null || buildings.Count == 0)
        {
            Debug.Log("📂 No buildings to load");
            return;
        }
        
        // Tìm BuildModeObjectSelector để lấy danh sách BuildableObjects
        var objectSelector = BuildModeObjectSelector.Instance;
        if (objectSelector == null)
        {
            objectSelector = FindFirstObjectByType<BuildModeObjectSelector>();
        }
        
        if (objectSelector == null)
        {
            Debug.LogWarning("[SaveLoadManager] BuildModeObjectSelector not found! Cannot load buildings.");
            return;
        }
        
        // Build dictionary để lookup nhanh
        var buildableDict = new Dictionary<string, BuildableObject>();
        foreach (var buildable in objectSelector.BuildableObjects)
        {
            string key = !string.IsNullOrEmpty(buildable.objectId) ? buildable.objectId : buildable.objectName;
            if (!buildableDict.ContainsKey(key))
            {
                buildableDict[key] = buildable;
            }
        }
        
        // Clear existing placed objects trước khi load
        ClearAllBuildings();
        
        // Spawn lại các buildings
        int loadedCount = 0;
        foreach (var buildingData in buildings)
        {
            if (!buildableDict.TryGetValue(buildingData.objectId, out BuildableObject buildable))
            {
                Debug.LogWarning($"[SaveLoadManager] BuildableObject not found: {buildingData.objectId}");
                continue;
            }
            
            Vector3 position = new Vector3(buildingData.posX, buildingData.posY, buildingData.posZ);
            Quaternion rotation = new Quaternion(buildingData.rotX, buildingData.rotY, buildingData.rotZ, buildingData.rotW);
            
            // Dùng BuildModePlacer để đặt object (không tốn tiền khi load)
            if (BuildModePlacer.Instance != null)
            {
                BuildModePlacer.Instance.PlaceObjectWithoutCost(buildable, position, rotation);
                loadedCount++;
            }
        }
        
        Debug.Log($"📂 Loaded {loadedCount}/{buildings.Count} buildings");
        
        // Refresh NPC display cache
        var npcManager = FindFirstObjectByType<NPCManager>();
        if (npcManager != null)
        {
            npcManager.RefreshDisplayCache();
        }
    }
    
    /// <summary>
    /// Xóa tất cả buildings đã đặt (không xóa save data)
    /// </summary>
    public void ClearAllBuildings()
    {
        if (BuildModePlacer.Instance != null)
        {
            BuildModePlacer.Instance.ClearAllPlacedObjects();
        }
        Debug.Log("🗑️ All buildings cleared from scene");
    }
    
    /// <summary>
    /// Xóa save data của buildings trong slot
    /// </summary>
    public void DeleteBuildingsInSlot(int slotIndex)
    {
        if (slotIndex < 1 || slotIndex > MAX_SAVE_SLOTS)
            return;
        
        string key = SAVE_SLOT_PREFIX + slotIndex;
        DeleteBuildingsFromKey(key);
    }
    
    /// <summary>
    /// Xóa save data của buildings trong Auto Save
    /// </summary>
    public void DeleteBuildingsInAutoSave()
    {
        DeleteBuildingsFromKey(AUTO_SAVE_KEY);
    }
    
    /// <summary>
    /// Xóa buildings từ save data (giữ lại money và meta info)
    /// </summary>
    private void DeleteBuildingsFromKey(string key)
    {
        if (!PlayerPrefs.HasKey(key))
            return;
        
        try
        {
            string json = PlayerPrefs.GetString(key);
            GameData data = JsonUtility.FromJson<GameData>(json);
            
            if (data != null)
            {
                data.buildings = new List<BuildingSaveData>(); // Clear buildings
                
                string newJson = JsonUtility.ToJson(data, true);
                PlayerPrefs.SetString(key, newJson);
                PlayerPrefs.Save();
                
                Debug.Log($"🗑️ Buildings deleted from save: {key}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLoadManager] Error deleting buildings: {e.Message}");
        }
    }
    
    /// <summary>
    /// Xóa tất cả save data (bao gồm buildings)
    /// </summary>
    public void DeleteAllSaveData()
    {
        // Xóa auto save
        PlayerPrefs.DeleteKey(AUTO_SAVE_KEY);
        
        // Xóa tất cả slots
        for (int i = 1; i <= MAX_SAVE_SLOTS; i++)
        {
            PlayerPrefs.DeleteKey(SAVE_SLOT_PREFIX + i);
        }
        
        PlayerPrefs.Save();
        Debug.Log("🗑️ All save data deleted!");
    }
    
    #endregion
}

[System.Serializable]
public class GameData
{
    // Game Progress
    public int totalMoney;
    
    // Buildings
    public List<BuildingSaveData> buildings = new List<BuildingSaveData>();
    
    // Meta Info (để hiển thị trên UI)
    public string saveDateTime;
    public float playTimeSeconds;
    public string gameVersion;
}

/// <summary>
/// Data cần thiết để save/load 1 building
/// </summary>
[System.Serializable]
public class BuildingSaveData
{
    public string objectId;  // ID của BuildableObject
    public float posX, posY, posZ;  // Position (Vector3 không serialize trực tiếp với JsonUtility)
    public float rotX, rotY, rotZ, rotW;  // Rotation (Quaternion)
}

/// <summary>
/// Thông tin slot để hiển thị trên UI
/// </summary>
public class SaveSlotInfo
{
    public int slotIndex;
    public int totalMoney;
    public int buildingCount;
    public string saveDateTime;
    public float playTime;
    public bool isEmpty;
}