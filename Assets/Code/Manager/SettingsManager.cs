using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public PlayerLook mouseLook;
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityText;

    void Start()
    {
        float sens = PlayerPrefs.GetFloat("MouseSens", 1f);
        sensitivitySlider.value = sens;
        ApplySensitivity(sens);
    }

    public void OnSensitivityChanged(float value)
    {
        ApplySensitivity(value);
        PlayerPrefs.SetFloat("MouseSens", value);
    }

    void ApplySensitivity(float value)
    {
        mouseLook.sensitivity = value;
        sensitivityText.text = value.ToString("F1");
    }
}
