using UnityEngine;
using System;

/// <summary>
/// Quản lý tiền và thống kê trong game.
/// Thay thế ScoreManager - sử dụng Money thay vì Score.
/// </summary>
public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [Header("Money Settings")]
    [SerializeField] private int startingMoney = 0;
    
    [Header("Statistics")]
    private int totalMoney = 0;
    private int correctAnswers = 0;
    private int wrongAnswers = 0;
    private int totalInteractions = 0;
    private float dayStartTime = 0f;
    private int currentDay = 1;
    
    // Tiền kiếm được trong ngày (để hiển thị cuối ngày)
    private int moneyEarnedToday = 0;

    // Events
    public event Action<int> OnMoneyChanged;
    public event Action<DayStatistics> OnDayStatsUpdated;

    void Awake()
    {
        Instance = this;
        totalMoney = startingMoney;
    }

    #region Money Methods

    /// <summary>
    /// Thêm tiền (kiếm được từ câu hỏi, bán hàng, v.v.)
    /// </summary>
    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        
        totalMoney += amount;
        moneyEarnedToday += amount;
        Debug.Log($"💰 Money Added: +{amount} | Total: {totalMoney}");
        OnMoneyChanged?.Invoke(totalMoney);
    }

    /// <summary>
    /// Chi tiêu tiền (mua hàng, nâng cấp, v.v.)
    /// </summary>
    public bool SpendMoney(int amount)
    {
        if (amount <= 0) return true;
        
        if (totalMoney < amount)
        {
            Debug.Log($"❌ Not enough money! Need: {amount}, Have: {totalMoney}");
            return false;
        }

        totalMoney -= amount;
        Debug.Log($"💸 Money Spent: -{amount} | Left: {totalMoney}");
        OnMoneyChanged?.Invoke(totalMoney);
        return true;
    }

    /// <summary>
    /// Lấy tổng tiền hiện tại
    /// </summary>
    public int GetTotalMoney() => totalMoney;
    
    /// <summary>
    /// Lấy tiền kiếm được trong ngày
    /// </summary>
    public int GetMoneyEarnedToday() => moneyEarnedToday;

    #endregion

    #region Statistics Methods

    /// <summary>
    /// Ghi nhận câu trả lời đúng
    /// </summary>
    public void RecordCorrectAnswer()
    {
        correctAnswers++;
        totalInteractions++;
        Debug.Log($"✅ Correct! Total: {correctAnswers}/{totalInteractions}");
        OnDayStatsUpdated?.Invoke(GetDayStatistics());
    }

    /// <summary>
    /// Ghi nhận câu trả lời sai (không trừ tiền)
    /// </summary>
    public void RecordWrongAnswer()
    {
        wrongAnswers++;
        totalInteractions++;
        Debug.Log($"❌ Wrong! Total: {wrongAnswers}/{totalInteractions}");
        OnDayStatsUpdated?.Invoke(GetDayStatistics());
    }

    /// <summary>
    /// Ghi nhận 1 lượt tương tác
    /// </summary>
    public void RecordInteraction()
    {
        totalInteractions++;
        OnDayStatsUpdated?.Invoke(GetDayStatistics());
    }

    /// <summary>
    /// Lấy số câu trả lời đúng
    /// </summary>
    public int GetCorrectAnswers() => correctAnswers;

    /// <summary>
    /// Lấy số câu trả lời sai
    /// </summary>
    public int GetWrongAnswers() => wrongAnswers;

    /// <summary>
    /// Lấy tổng số lượt tương tác
    /// </summary>
    public int GetTotalInteractions() => totalInteractions;

    /// <summary>
    /// Lấy thời gian chơi (giây)
    /// </summary>
    public float GetPlayTime() => Time.time - dayStartTime;

    /// <summary>
    /// Lấy ngày hiện tại
    /// </summary>
    public int GetCurrentDay() => currentDay;

    /// <summary>
    /// Lấy toàn bộ thống kê ngày
    /// </summary>
    public DayStatistics GetDayStatistics()
    {
        return new DayStatistics
        {
            day = currentDay,
            playTime = GetPlayTime(),
            totalInteractions = totalInteractions,
            correctAnswers = correctAnswers,
            wrongAnswers = wrongAnswers,
            totalMoney = totalMoney,
            moneyEarnedToday = moneyEarnedToday
        };
    }

    #endregion

    #region Day Management

    /// <summary>
    /// Reset thống kê cho ngày mới (giữ lại tiền)
    /// </summary>
    public void ResetDayStats()
    {
        correctAnswers = 0;
        wrongAnswers = 0;
        totalInteractions = 0;
        moneyEarnedToday = 0;
        dayStartTime = Time.time;
        
        Debug.Log($"📊 Day Stats Reset for Day {currentDay}. Total Money: {totalMoney}");
        OnDayStatsUpdated?.Invoke(GetDayStatistics());
    }

    /// <summary>
    /// Chuyển sang ngày mới
    /// </summary>
    public void NextDay()
    {
        currentDay++;
        ResetDayStats();
        Debug.Log($"📅 Advanced to Day {currentDay}");
    }

    #endregion

    #region Save/Load Support

    /// <summary>
    /// Set money trực tiếp (dùng cho load game)
    /// </summary>
    public void SetMoney(int money)
    {
        totalMoney = money;
        OnMoneyChanged?.Invoke(totalMoney);
    }

    /// <summary>
    /// Set day trực tiếp (dùng cho load game)
    /// </summary>
    public void SetDay(int day)
    {
        currentDay = day;
    }

    #endregion

    #region Debug

    /// <summary>
    /// Debug: Cộng tiền (dùng để test)
    /// </summary>
    [ContextMenu("Debug: Add 100 Money")]
    public void DebugAddMoney100()
    {
        AddMoney(100);
    }

    /// <summary>
    /// Debug: Cộng tiền (dùng để test)
    /// </summary>
    [ContextMenu("Debug: Add 1000 Money")]
    public void DebugAddMoney1000()
    {
        AddMoney(1000);
    }

    /// <summary>
    /// Debug: Thêm tiền tùy ý
    /// </summary>
    public void DebugAddMoney(int amount)
    {
        AddMoney(amount);
        Debug.Log($"[DEBUG] Added {amount} money. Total: {totalMoney}");
    }

    #endregion
}

/// <summary>
/// Struct chứa thống kê của 1 ngày
/// </summary>
[System.Serializable]
public struct DayStatistics
{
    public int day;
    public float playTime;
    public int totalInteractions;
    public int correctAnswers;
    public int wrongAnswers;
    public int totalMoney;
    public int moneyEarnedToday;

    public string GetPlayTimeString()
    {
        int minutes = (int)(playTime / 60);
        int seconds = (int)(playTime % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }
}
