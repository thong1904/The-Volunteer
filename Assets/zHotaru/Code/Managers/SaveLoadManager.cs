using UnityEngine;

/// <summary>
/// Quản lý Save/Load/AutoSave
/// </summary>
public class SaveLoadManager : MonoBehaviour
{
    [Header("Auto Save Settings")]
    [SerializeField] private bool autoSaveEnabled = true;
    [SerializeField] private float autoSaveInterval = 300f; // 5 phút
    
    private float nextAutoSaveTime;
    
    public bool IsAutoSaveEnabled => autoSaveEnabled;
    
    void Update()
    {
        if (autoSaveEnabled && Time.time >= nextAutoSaveTime)
        {
            SaveGame();
            nextAutoSaveTime = Time.time + autoSaveInterval;
        }
    }
    
    public void SaveGame()
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
        };
        
        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString("GameData", json);
        PlayerPrefs.Save();
        
        Debug.Log("💾 Game Saved!");
    }
    
    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey("GameData"))
        {
            Debug.LogWarning("No save data found!");
            return;
        }
        
        var gm = GameManager.Instance;
        if (gm == null || gm.Upgrades == null)
        {
            Debug.LogWarning("[SaveLoadManager] GameManager or Upgrades is null!");
            return;
        }
        
        string json = PlayerPrefs.GetString("GameData");
        GameData data = JsonUtility.FromJson<GameData>(json);
        
        // Load upgrades
        gm.Upgrades.npcLimitLevel = data.npcLimitLevel;
        gm.Upgrades.playerSpeedLevel = data.playerSpeedLevel;
        gm.Upgrades.scoreMultiplierLevel = data.scoreMultiplierLevel;
        
        Debug.Log("📂 Game Loaded!");
    }
}

[System.Serializable]
public class GameData
{
    public int totalScore;
    public int npcLimitLevel;
    public int playerSpeedLevel;
    public int scoreMultiplierLevel;
    // Thêm các field khác khi cần
}