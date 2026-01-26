using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingSensitivityUI : MonoBehaviour
{
    public Slider slider;
    public TMP_Text valueText;

    void Start()
    {
        slider.value = GameSettingManager.Instance.mouseSensitivity;
        UpdateText(slider.value);

        slider.onValueChanged.AddListener(OnValueChanged);
    }

    void OnValueChanged(float value)
    {
        GameSettingManager.Instance.SetMouseSensitivity(value);
        UpdateText(value);
    }

    void UpdateText(float value)
    {
        valueText.text = value.ToString("F1");
    }
}
