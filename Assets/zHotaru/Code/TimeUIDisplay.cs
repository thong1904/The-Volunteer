using UnityEngine;
using TMPro;

public class TimeUIDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;

    private void Start()
    {
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.OnTimeChanged += UpdateTimeDisplay;
            DayNightManager.Instance.OnSunset += OnSunsetUI;
        }
    }

    private void UpdateTimeDisplay(float hour)
    {
        if (timeText != null)
        {
            timeText.text = DayNightManager.Instance.GetTimeString();
        }
    }

    private void OnSunsetUI()
    {
        if (timeText != null)
        {
            timeText.text = "Đã tối - Về giường!";
            timeText.color = new Color(1f, 0.7f, 0.3f); // Màu cam
        }
    }

    private void OnDestroy()
    {
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.OnTimeChanged -= UpdateTimeDisplay;
        }
    }
}