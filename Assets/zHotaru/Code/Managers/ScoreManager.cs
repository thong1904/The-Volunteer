using UnityEngine;
using System;

/// <summary>
/// Quản lý điểm số. Là con của GameManager.
/// Truy cập qua: GameManager.Instance.Score
/// </summary>
public class ScoreManager : MonoBehaviour
{
    // Giữ static để backward compatible
    public static ScoreManager Instance { get; private set; }

    private int totalScore = 0;

    // Event khi điểm thay đổi
    public event Action<int> OnScoreChanged;

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
    /// Lấy tổng điểm hiện tại
    /// </summary>
    public int GetTotalScore()
    {
        return totalScore;
    }

    /// <summary>
    /// Reset điểm về 0
    /// </summary>
    public void ResetScore()
    {
        totalScore = 0;
        Debug.Log("Score Reset to 0");
        OnScoreChanged?.Invoke(totalScore);
    }

    /// <summary>
    /// Set score trực tiếp (dùng cho load game)
    /// </summary>
    public void SetScore(int score)
    {
        totalScore = score;
        // Cập nhật UI nếu cần
        OnScoreChanged?.Invoke(totalScore);
    }
}