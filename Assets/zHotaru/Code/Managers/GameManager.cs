using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Time Settings")]
    [SerializeField] private float dayDuration = 300f; // 5 phút = 1 ngày (có thể thay đổi)
    
    [Header("Game State")]
    [SerializeField] private int playerScore = 0;
    
    private float dayStartTime;
    private bool isDayEnded = false;
    
    public bool IsDayEnded => isDayEnded;
    public int PlayerScore => playerScore;
    public float RemainingDayTime => Mathf.Max(0, dayDuration - (Time.time - dayStartTime));
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        dayStartTime = Time.time;
    }
    
    void Update()
    {
        // Kiểm tra xem ngày đã kết thúc chưa
        if (!isDayEnded && Time.time - dayStartTime >= dayDuration)
        {
            isDayEnded = true;
            OnDayEnded();
        }
    }
    
    public void AddScore(int points)
    {
        playerScore += points;
        Debug.Log($"Cộng {points} điểm! Tổng điểm: {playerScore}");
    }
    
    public void ResetDay()
    {
        dayStartTime = Time.time;
        isDayEnded = false;
        playerScore = 0;
    }
    
    private void OnDayEnded()
    {
        Debug.Log("Ngày đã kết thúc! Tổng điểm: " + playerScore);
        // TODO: Hiển thị màn hình kết thúc ngày
    }
}
