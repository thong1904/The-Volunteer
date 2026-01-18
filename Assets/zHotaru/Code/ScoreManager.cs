using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int totalScore = 0;

    // Event khi điểm thay đổi - để sau này kết nối UI hiển thị
    // TODO: Tạo UI script để subscribe vào event này và cập nhật hiển thị
    public event Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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
}