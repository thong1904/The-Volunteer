using UnityEngine;
using TMPro;

public class ScoreUIDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Start()
    {
        if (scoreText == null)
        {
            Debug.LogError("scoreText chưa được gán!");
            return;
        }

        // Hiển thị điểm ban đầu
        if (ScoreManager.Instance != null)
        {
            scoreText.text = "Score: " + ScoreManager.Instance.GetTotalScore();

            // Subscribe vào event thay đổi điểm
            ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
        }
    }

    private void UpdateScoreDisplay(int newScore)
    {
        scoreText.text = "Score: " + newScore;
    }

    private void OnDestroy()
    {
        // Unsubscribe khi destroy để tránh lỗi
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
        }
    }
}