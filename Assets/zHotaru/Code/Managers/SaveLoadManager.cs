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
        GameData data = new GameData
        {
            // Score
            totalScore = ScoreManager.Instance?.GetTotalScore() ?? 0,
            
            // Upgrades
            npcLimitLevel = GameManager.Instance.Upgrades.npcLimitLevel,
            playerSpeedLevel = GameManager.Instance.Upgrades.playerSpeedLevel,
            scoreMultiplierLevel = GameManager.Instance.Upgrades.scoreMultiplierLevel,
            
            // TODO: Thêm các data khác
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
        
        string json = PlayerPrefs.GetString("GameData");
        GameData data = JsonUtility.FromJson<GameData>(json);
        
        // Load upgrades
        var upgrades = GameManager.Instance.Upgrades;
        upgrades.npcLimitLevel = data.npcLimitLevel;
        upgrades.playerSpeedLevel = data.playerSpeedLevel;
        upgrades.scoreMultiplierLevel = data.scoreMultiplierLevel;
        
        // TODO: Load các data khác
        
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