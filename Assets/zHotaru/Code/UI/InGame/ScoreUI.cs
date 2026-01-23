using UnityEngine;
using TMPro;

/// <summary>
/// UI hiển thị điểm số. Tự động subscribe vào ScoreManager.OnScoreChanged.
/// </summary>
public class ScoreUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text scoreText;
    
    [Header("Display Format")]
    [SerializeField] private string scoreFormat = "Score: {0}";
    
    [Header("Animation (optional)")]
    [SerializeField] private bool animateOnChange = true;
    [SerializeField] private float punchScale = 1.2f;
    [SerializeField] private float animationSpeed = 5f;
    
    private Vector3 originalScale;
    private bool isAnimating = false;

    void Start()
    {
        if (scoreText != null)
            originalScale = scoreText.transform.localScale;
        
        // Subscribe vào event
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
            // Hiển thị điểm ban đầu
            UpdateScoreDisplay(ScoreManager.Instance.GetTotalScore());
        }
        else
        {
            Debug.LogWarning("[ScoreUI] ScoreManager.Instance is null on Start. Will retry in Update.");
        }
    }

    void Update()
    {
        // Retry subscribe nếu ScoreManager chưa sẵn sàng lúc Start
        if (ScoreManager.Instance != null && !isSubscribed)
        {
            ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
            UpdateScoreDisplay(ScoreManager.Instance.GetTotalScore());
            isSubscribed = true;
        }
        
        // Animation scale
        if (isAnimating && scoreText != null)
        {
            scoreText.transform.localScale = Vector3.Lerp(
                scoreText.transform.localScale, 
                originalScale, 
                animationSpeed * Time.deltaTime
            );
            
            if (Vector3.Distance(scoreText.transform.localScale, originalScale) < 0.01f)
            {
                scoreText.transform.localScale = originalScale;
                isAnimating = false;
            }
        }
    }

    private bool isSubscribed = false;

    void OnDestroy()
    {
        // Unsubscribe để tránh memory leak
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= OnScoreChanged;
        }
    }

    private void OnScoreChanged(int newScore)
    {
        UpdateScoreDisplay(newScore);
        
        if (animateOnChange)
        {
            PlayScoreAnimation();
        }
    }

    private void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = string.Format(scoreFormat, score);
        }
    }

    private void PlayScoreAnimation()
    {
        if (scoreText != null)
        {
            scoreText.transform.localScale = originalScale * punchScale;
            isAnimating = true;
        }
    }
}
