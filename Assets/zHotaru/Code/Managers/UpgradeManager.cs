using UnityEngine;
using System;

/// <summary>
/// Quản lý hệ thống nâng cấp
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    [Header("NPC Upgrades")]
    public int npcLimitLevel = 1; // Level 1 = 5 NPCs, Level 10 = 50 NPCs
    
    [Header("Player Upgrades")]
    public int playerSpeedLevel = 1;
    public int scoreMultiplierLevel = 1;
    
    [Header("Boost Items")]
    public bool hasSpeedBoost = false;
    public bool hasScoreBoost = false;
    public float boostDuration = 30f;
    
    private float speedBoostEndTime;
    private float scoreBoostEndTime;
    
    public event Action<int> OnNPCLimitUpgrade;
    public event Action<int> OnPlayerSpeedUpgrade;
    public event Action<int> OnScoreMultiplierUpgrade;
    
    // Getters
    public int GetMaxNPCs() => 5 + (npcLimitLevel - 1) * 5; // 5, 10, 15,..., 50
    public float GetPlayerSpeedMultiplier() => 1f + (playerSpeedLevel - 1) * 0.1f; // 1.0, 1.1, 1.2...
    public float GetScoreMultiplier()
    {
        float baseMultiplier = 1f + (scoreMultiplierLevel - 1) * 0.2f;
        return hasScoreBoost ? baseMultiplier * 2f : baseMultiplier;
    }
    
    void Update()
    {
        // Kiểm tra boost hết hạn
        if (hasSpeedBoost && Time.time >= speedBoostEndTime)
        {
            hasSpeedBoost = false;
            Debug.Log("⚡ Speed Boost hết hạn!");
        }
        
        if (hasScoreBoost && Time.time >= scoreBoostEndTime)
        {
            hasScoreBoost = false;
            Debug.Log("⭐ Score Boost hết hạn!");
        }
    }
    
    public void UpgradeNPCLimit()
    {
        if (npcLimitLevel >= 10) return; // Max level 10 (50 NPCs)
        
        npcLimitLevel++;
        int newMax = GetMaxNPCs();
        
        // Cập nhật NPCManager
        if (GameManager.Instance != null && GameManager.Instance.NPCs != null)
        {
            GameManager.Instance.NPCs.UpgradeMaxCustomers(newMax);
        }
        
        OnNPCLimitUpgrade?.Invoke(newMax);
        Debug.Log($"⬆️ NPC Limit: Level {npcLimitLevel} - Max: {newMax}");
    }
    
    public void UpgradePlayerSpeed()
    {
        playerSpeedLevel++;
        OnPlayerSpeedUpgrade?.Invoke(playerSpeedLevel);
        Debug.Log($"⬆️ Player Speed: Level {playerSpeedLevel} - x{GetPlayerSpeedMultiplier()}");
    }
    
    public void UpgradeScoreMultiplier()
    {
        scoreMultiplierLevel++;
        OnScoreMultiplierUpgrade?.Invoke(scoreMultiplierLevel);
        Debug.Log($"⬆️ Score Multiplier: Level {scoreMultiplierLevel} - x{GetScoreMultiplier()}");
    }
    
    public void ActivateSpeedBoost()
    {
        hasSpeedBoost = true;
        speedBoostEndTime = Time.time + boostDuration;
        Debug.Log($"⚡ Speed Boost activated for {boostDuration}s!");
    }
    
    public void ActivateScoreBoost()
    {
        hasScoreBoost = true;
        scoreBoostEndTime = Time.time + boostDuration;
        Debug.Log($"⭐ Score Boost (x2) activated for {boostDuration}s!");
    }
}