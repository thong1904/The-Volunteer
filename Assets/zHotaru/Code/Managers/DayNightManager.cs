using UnityEngine;
using System;

/// <summary>
/// Quản lý chu kỳ ngày/đêm. Là con của GameManager.
/// Truy cập qua: GameManager.Instance.DayNight
/// </summary>
public class DayNightManager : MonoBehaviour
{
    // Giữ static để backward compatible
    public static DayNightManager Instance { get; private set; }

    [Header("Thời gian")]
    [SerializeField] private float startHour = 9f;
    [SerializeField] private float endHour = 18f;
    [SerializeField] private float minutesPerHour = 1f;

    [Header("Skybox & Ánh sáng")]
    [SerializeField] private Material skyboxDay;
    [SerializeField] private Material skyboxSunset;
    [SerializeField] private Light sunLight;
    [SerializeField] private Color colorDay = Color.white;
    [SerializeField] private Color colorSunset = new Color(1f, 0.7f, 0.3f);

    private float currentHour;
    private bool isRunning = true;

    public event Action<float> OnTimeChanged;
    public event Action OnSunset;
    public event Action OnNewDay;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentHour = startHour;
        UpdateEnvironment();
    }

    private void Update()
    {
        if (!isRunning) return;

        // Tăng giờ dựa vào thời gian thực
        currentHour += (Time.deltaTime / 60f) / minutesPerHour;

        // Nếu đến 6PM thì dừng
        if (currentHour >= endHour)
        {
            currentHour = endHour;
            isRunning = false;
            OnSunset?.Invoke();
            Debug.Log("Đã đến 6PM - Dừng thời gian. Hãy về giường!");
        }

        UpdateEnvironment();
        OnTimeChanged?.Invoke(currentHour);
    }

    private void UpdateEnvironment()
    {
        // Lấy % thời gian từ sáng đến tối (0 = sáng, 1 = tối)
        float timeProgress = (currentHour - startHour) / (endHour - startHour);
        timeProgress = Mathf.Clamp01(timeProgress);

        // Chuyển skybox từ ngày sang hoàng hôn
        Color skyboxColor = Color.Lerp(Color.white, new Color(0.8f, 0.8f, 0.8f), timeProgress);
        RenderSettings.skybox.SetColor("_Tint", skyboxColor);

        // Thay đổi màu ánh sáng mặt trời
        if (sunLight != null)
        {
            sunLight.color = Color.Lerp(colorDay, colorSunset, timeProgress);
            sunLight.intensity = Mathf.Lerp(1f, 0.7f, timeProgress);
        }

        // Thay đổi góc xoay của mặt trời (từ góc sáng -> góc chiều)
        if (sunLight != null)
        {
            float angle = Mathf.Lerp(30f, -30f, timeProgress); // 30 độ sáng, -30 độ tối
            sunLight.transform.rotation = Quaternion.Euler(angle, 0f, 0f);
        }
    }

    /// <summary>
    /// Lấy giờ hiện tại (VD: 14.5 = 2:30 PM)
    /// </summary>
    public float GetCurrentHour()
    {
        return currentHour;
    }

    /// <summary>
    /// Lấy giờ hiện tại dạng string (VD: "14:30")
    /// </summary>
    public string GetTimeString()
    {
        int hours = (int)currentHour;
        int minutes = (int)((currentHour - hours) * 60);
        return $"{hours:D2}:{minutes:D2}";
    }

    /// <summary>
    /// Kiểm tra có còn time không
    /// </summary>
    public bool IsTimeRunning()
    {
        return isRunning;
    }

    /// <summary>
    /// Kiểm tra xem đã vào buổi tối hay ngày đã kết thúc
    /// </summary>
    public bool IsNighttime()
    {
        return currentHour >= endHour || !isRunning;
    }

    /// <summary>
    /// Bắt đầu ngày mới
    /// </summary>
    public void StartNewDay()
    {
        currentHour = startHour;
        isRunning = true;
        OnNewDay?.Invoke();
        Debug.Log("Ngày mới bắt đầu!");
    }
}