using UnityEngine;

public class BGMController : MonoBehaviour
{
    public static BGMController Instance;

    [Header("BGM Settings")]
    public AudioSource bgmSource;
    public AudioClip[] bgmList; // Size = 5

    private int currentBGMIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (bgmSource == null)
            bgmSource = GetComponent<AudioSource>();

        ApplyGlobalSettings();

        if (bgmList != null && bgmList.Length > 0)
        {
            PlayCurrentBGM();
        }
    }

    void Update()
    {
        // Nếu nhạc bật và bài hiện tại đã phát xong → sang bài tiếp theo
        if (SoundManager.Instance != null &&
            SoundManager.Instance.isBGMOn &&
            bgmSource != null &&
            !bgmSource.isPlaying)
        {
            PlayNextBGM();
        }
    }

    private void ApplyGlobalSettings()
    {
        if (SoundManager.Instance != null && bgmSource != null)
        {
            bgmSource.mute = !SoundManager.Instance.isBGMOn;
        }
    }

    public void SetBGMState(bool isOn)
    {
        if (bgmSource == null) return;

        bgmSource.mute = !isOn;

        if (!isOn)
        {
            bgmSource.Stop();
        }
        else
        {
            PlayCurrentBGM();
        }
    }

    private void PlayCurrentBGM()
    {
        if (bgmSource == null || bgmList.Length == 0)
            return;

        bgmSource.clip = bgmList[currentBGMIndex];
        bgmSource.Play();
    }

    private void PlayNextBGM()
    {
        currentBGMIndex++;

        if (currentBGMIndex >= bgmList.Length)
            currentBGMIndex = 0; // quay về BGM 1

        PlayCurrentBGM();
    }
}
