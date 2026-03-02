using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Settings cho tất cả scene.
/// Không phải singleton - đặt trực tiếp vào Canvas của mỗi scene.
/// Quản lý: Mouse Sensitivity, BGM Volume, SFX Volume
/// </summary>
public class SettingsUI : MonoBehaviour
{
    [Header("=== Mouse Sensitivity ===")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_Text sensitivityValueText;
    [SerializeField] private float minSensitivity = 0.1f;
    [SerializeField] private float maxSensitivity = 5f;
    [SerializeField] private float defaultSensitivity = 0.5f;

    [Header("=== BGM Volume ===")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private TMP_Text bgmValueText;

    [Header("=== SFX Volume ===")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text sfxValueText;

    [Header("=== Display Format ===")]
    [Tooltip("Hiển thị dạng phần trăm (0-100%) hay giá trị thập phân (0.0-1.0)")]
    [SerializeField] private bool showAsPercentage = true;

    // PlayerPrefs Keys
    private const string SENSITIVITY_KEY = "MouseSensitivity";

    // Static để các script khác có thể truy cập mouse sensitivity
    public static float MouseSensitivity { get; private set; } = 1f;

    void Awake()
    {
        // Load mouse sensitivity từ PlayerPrefs
        MouseSensitivity = PlayerPrefs.GetFloat(SENSITIVITY_KEY, defaultSensitivity);
    }

    void Start()
    {
        SetupSensitivitySlider();
        SetupBGMSlider();
        SetupSFXSlider();
    }

    void OnEnable()
    {
        // Refresh giá trị mỗi khi UI được bật (đảm bảo đồng bộ)
        RefreshAllValues();
    }

    #region Setup Sliders

    private void SetupSensitivitySlider()
    {
        if (sensitivitySlider == null) return;

        sensitivitySlider.minValue = minSensitivity;
        sensitivitySlider.maxValue = maxSensitivity;
        sensitivitySlider.value = MouseSensitivity;

        UpdateSensitivityText(MouseSensitivity);


        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    private void SetupBGMSlider()
    {
        if (bgmSlider == null) return;

        bgmSlider.minValue = 0f;
        bgmSlider.maxValue = 1f;

        // Lấy giá trị từ SoundManager nếu có
        float currentBGM = SoundManager.Instance != null ? SoundManager.Instance.BGMVolume : 1f;
        bgmSlider.value = currentBGM;

        UpdateBGMText(currentBGM);

        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
    }

    private void SetupSFXSlider()
    {
        if (sfxSlider == null) return;

        sfxSlider.minValue = 0f;
        sfxSlider.maxValue = 1f;

        // Lấy giá trị từ SoundManager nếu có
        float currentSFX = SoundManager.Instance != null ? SoundManager.Instance.SFXVolume : 1f;
        sfxSlider.value = currentSFX;

        UpdateSFXText(currentSFX);

        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    #endregion

    #region Value Changed Callbacks

    private void OnSensitivityChanged(float value)
    {
        MouseSensitivity = value;
        PlayerPrefs.SetFloat(SENSITIVITY_KEY, value);
        PlayerPrefs.Save();

        UpdateSensitivityText(value);
    }

    private void OnBGMVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetBGMVolume(value);
        }

        UpdateBGMText(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(value);
        }

        UpdateSFXText(value);
    }

    #endregion

    #region Update Text Display

    private void UpdateSensitivityText(float value)
    {
        if (sensitivityValueText == null) return;

        // Sensitivity hiển thị dạng số thập phân
        sensitivityValueText.text = value.ToString("F1");
    }

    private void UpdateBGMText(float value)
    {
        if (bgmValueText == null) return;

        if (showAsPercentage)
        {
            int percent = Mathf.RoundToInt(value * 100);
            bgmValueText.text = percent + "%";
        }
        else
        {
            bgmValueText.text = value.ToString("F1");
        }
    }

    private void UpdateSFXText(float value)
    {
        if (sfxValueText == null) return;

        if (showAsPercentage)
        {
            int percent = Mathf.RoundToInt(value * 100);
            sfxValueText.text = percent + "%";
        }
        else
        {
            sfxValueText.text = value.ToString("F1");
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Refresh tất cả giá trị từ PlayerPrefs và SoundManager
    /// </summary>
    public void RefreshAllValues()
    {
        // Refresh Sensitivity
        MouseSensitivity = PlayerPrefs.GetFloat(SENSITIVITY_KEY, defaultSensitivity);
        if (sensitivitySlider != null)
        {
            sensitivitySlider.SetValueWithoutNotify(MouseSensitivity);
            UpdateSensitivityText(MouseSensitivity);
        }

        // Refresh BGM
        if (bgmSlider != null && SoundManager.Instance != null)
        {
            float bgmValue = SoundManager.Instance.BGMVolume;
            bgmSlider.SetValueWithoutNotify(bgmValue);
            UpdateBGMText(bgmValue);
        }

        // Refresh SFX
        if (sfxSlider != null && SoundManager.Instance != null)
        {
            float sfxValue = SoundManager.Instance.SFXVolume;
            sfxSlider.SetValueWithoutNotify(sfxValue);
            UpdateSFXText(sfxValue);
        }
    }

    /// <summary>
    /// Reset tất cả settings về mặc định
    /// </summary>
    public void ResetToDefault()
    {
        // Reset Sensitivity
        MouseSensitivity = defaultSensitivity;
        PlayerPrefs.SetFloat(SENSITIVITY_KEY, defaultSensitivity);
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = defaultSensitivity;
        }

        // Reset BGM
        if (bgmSlider != null)
        {
            bgmSlider.value = 1f;
        }

        // Reset SFX
        if (sfxSlider != null)
        {
            sfxSlider.value = 1f;
        }

        PlayerPrefs.Save();
    }

    #endregion

    void OnDestroy()
    {
        // Cleanup listeners
        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);

        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
    }
}
