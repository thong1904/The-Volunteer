using UnityEngine;
using System;

/// <summary>
/// Quản lý điểm số và thống kê. Là con của GameManager.
/// Truy cập qua: GameManager.Instance.Score
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Statistics")]
    private int totalScore = 0;
    private int correctAnswers = 0;
    private int wrongAnswers = 0;
    private int totalInteractions = 0;
    private float dayStartTime = 0f;
    private int currentDay = 1;

    // Events
    public event Action<int> OnScoreChanged;
    public event Action<DayStatistics> OnDayStatsUpdated;

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Thêm điểm vào tổng
    /// </summary>
    public void AddScore(int points)
    {
        totalScore += points;
        Debug.Log($"Score Added: {points} | Total Score: {totalScore}");
        OnScoreChanged?.Invoke(totalScore);
    }

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
    /// Ghi nhận câu trả lời sai
    /// </summary>
    public void RecordWrongAnswer()
    {
        wrongAnswers++;
        totalInteractions++;
        Debug.Log($"❌ Wrong! Total: {wrongAnswers}/{totalInteractions}");
        OnDayStatsUpdated?.Invoke(GetDayStatistics());
    }

    /// <summary>
    /// Ghi nhận 1 lượt tương tác (không phải câu hỏi, VD: nhặt rác)
    /// </summary>
    public void RecordInteraction()
    {
        totalInteractions++;
        OnDayStatsUpdated?.Invoke(GetDayStatistics());
    }

    /// <summary>
    /// Lấy tổng điểm hiện tại
    /// </summary>
    public int GetTotalScore() => totalScore;

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
            totalScore = totalScore
        };
    }

    /// <summary>
    /// Reset điểm và thống kê cho ngày mới
    /// </summary>
    public void ResetScore()
    {
        totalScore = 0;
        correctAnswers = 0;
        wrongAnswers = 0;
        totalInteractions = 0;
        dayStartTime = Time.time;
        
        Debug.Log($"📊 Stats Reset for Day {currentDay}");
        OnScoreChanged?.Invoke(totalScore);
    }

    /// <summary>
    /// Chuyển sang ngày mới (tăng day counter)
    /// </summary>
    public void NextDay()
    {
        currentDay++;
        ResetScore();
        Debug.Log($"📅 Advanced to Day {currentDay}");
    }

    /// <summary>
    /// Set score trực tiếp (dùng cho load game)
    /// </summary>
    public void SetScore(int score)
    {
        totalScore = score;
        OnScoreChanged?.Invoke(totalScore);
    }

    /// <summary>
    /// Set day trực tiếp (dùng cho load game)
    /// </summary>
    public void SetDay(int day)
    {
        currentDay = day;
    }
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
    public int totalScore;

    public string GetPlayTimeString()
    {
        int minutes = (int)(playTime / 60);
        int seconds = (int)(playTime % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }
}