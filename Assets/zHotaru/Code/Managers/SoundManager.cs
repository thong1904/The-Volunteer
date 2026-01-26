using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Quản lý toàn bộ âm thanh: BGM + SFX.
/// Singleton độc lập, DontDestroyOnLoad.
/// Tự động thêm sound cho tất cả Button trong scene.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM Playlist")]
    [SerializeField] private AudioClip[] bgmList;
    private int currentBGMIndex = 0;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip buttonCancelSound;
    [SerializeField] private AudioClip maleSound;
    [SerializeField] private AudioClip femaleSound;
    [SerializeField] private AudioClip feYeahSound;
    [SerializeField] private AudioClip maYeahSound;
    [SerializeField] private AudioClip maHuhSound;
    [SerializeField] private AudioClip feHuhSound;
    [SerializeField] private AudioClip boostSound;

    [Header("Auto Button Sound")]
    [SerializeField] private bool autoAddButtonSound = true;

    [Header("Settings")]
    [SerializeField] private bool isBGMOn = true;
    [SerializeField] private bool isSFXOn = true;
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    // Public properties
    public bool IsBGMOn => isBGMOn;
    public bool IsSFXOn => isSFXOn;
    public float BGMVolume => bgmVolume;
    public float SFXVolume => sfxVolume;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupAudioSources();
        LoadSettings();
    }

    void Start()
    {
        if (isBGMOn && bgmList != null && bgmList.Length > 0)
        {
            PlayCurrentBGM();
        }

        // Đăng ký event khi chuyển scene
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Auto add sound cho scene hiện tại
        if (autoAddButtonSound)
            RegisterAllButtons();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Auto add sound cho scene mới
        if (autoAddButtonSound)
            RegisterAllButtons();
    }

    /// <summary>
    /// Tự động thêm sound click cho tất cả Button trong scene
    /// </summary>
    private void RegisterAllButtons()
    {
        // Tìm tất cả Button (kể cả inactive)
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Button btn in allButtons)
        {
            // Bỏ qua nếu button đã có UIButtonSound (custom sound)
            if (btn.GetComponent<UIButtonSound>() != null)
                continue;

            // Thêm listener (kiểm tra để không add trùng)
            btn.onClick.RemoveListener(PlayButtonClick);
            btn.onClick.AddListener(PlayButtonClick);
        }

        Debug.Log($"[SoundManager] Registered {allButtons.Length} buttons in scene {SceneManager.GetActiveScene().name}");
    }

    void Update()
    {
        // Auto-play next BGM khi hết bài
        if (isBGMOn && bgmSource != null && !bgmSource.isPlaying && bgmList != null && bgmList.Length > 0)
        {
            PlayNextBGM();
        }
    }

    private void SetupAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = false;
            bgmSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    #region BGM Controls

    public void PlayCurrentBGM()
    {
        if (bgmSource == null || bgmList == null || bgmList.Length == 0) return;

        bgmSource.clip = bgmList[currentBGMIndex];
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void PlayNextBGM()
    {
        currentBGMIndex = (currentBGMIndex + 1) % bgmList.Length;
        PlayCurrentBGM();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }

    #endregion

    #region SFX Play Methods

    public void PlayButtonClick() => PlaySFX(buttonClickSound);
    public void PlayButtonCancel() => PlaySFX(buttonCancelSound);
    public void PlayMale() => PlaySFX(maleSound);
    public void PlayFemale() => PlaySFX(femaleSound);
    public void PlayMaYeah() => PlaySFX(maYeahSound);
    public void PlayFeYeah() => PlaySFX(feYeahSound);
    public void PlayMaHuh() => PlaySFX(maHuhSound);
    public void PlayFeHuh() => PlaySFX(feHuhSound);
    public void PlayBoost() => PlaySFX(boostSound);

    /// <summary>
    /// Phát SFX theo tên (dùng cho UIButtonSound)
    /// </summary>
    public void PlaySFX(string soundName, float volumeScale = 1f)
    {
        AudioClip clip = GetSFXByName(soundName);
        if (clip != null && isSFXOn && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
        }
    }

    private AudioClip GetSFXByName(string name)
    {
        return name.ToLower() switch
        {
            "confirm" or "click" => buttonClickSound,
            "cancel" => buttonCancelSound,
            "mayeah" => maYeahSound,
            "feyeah" => feYeahSound,
            "mahuh" => maHuhSound,
            "fehuh" => feHuhSound,
            "boost" => boostSound,
            "male" => maleSound,
            "female" => femaleSound,
            _ => null
        };
    }

    /// <summary>
    /// Phát SFX bất kỳ (dùng cho clip không có sẵn)
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (!isSFXOn || clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    #endregion

    #region Settings Controls

    public void ToggleBGM()
    {
        SetBGM(!isBGMOn);
    }

    public void SetBGM(bool isOn)
    {
        isBGMOn = isOn;

        if (bgmSource != null)
        {
            if (isOn)
            {
                if (!bgmSource.isPlaying)
                    PlayCurrentBGM();
            }
            else
            {
                bgmSource.Stop();
            }
        }

        SaveSettings();
    }

    public void ToggleSFX()
    {
        SetSFX(!isSFXOn);
    }

    public void SetSFX(bool isOn)
    {
        isSFXOn = isOn;
        SaveSettings();
    }

    /// <summary>
    /// Set BGM volume (0-1). Dùng cho slider.
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
            bgmSource.volume = bgmVolume;
        SaveSettings();
    }

    /// <summary>
    /// Set SFX volume (0-1). Dùng cho slider.
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        SaveSettings();
    }

    #endregion

    #region Save/Load

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("BGM_On", isBGMOn ? 1 : 0);
        PlayerPrefs.SetInt("SFX_On", isSFXOn ? 1 : 0);
        PlayerPrefs.SetFloat("BGM_Volume", bgmVolume);
        PlayerPrefs.SetFloat("SFX_Volume", sfxVolume);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        isBGMOn = PlayerPrefs.GetInt("BGM_On", 1) == 1;
        isSFXOn = PlayerPrefs.GetInt("SFX_On", 1) == 1;
        bgmVolume = PlayerPrefs.GetFloat("BGM_Volume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFX_Volume", 1f);

        // Apply to audio sources
        if (bgmSource != null)
            bgmSource.volume = bgmVolume;
    }

    #endregion
}