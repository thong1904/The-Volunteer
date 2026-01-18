using UnityEngine;

public class SFXController : MonoBehaviour
{
    public static SFXController Instance { get; private set; }
    [Header("SFX Settings")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip buttonCancelSound;
    [SerializeField] private AudioClip newRankSound;
    [SerializeField] private AudioClip dragSound;
    [SerializeField] private AudioClip putSound;
    [SerializeField] private AudioClip maleSound;
    [SerializeField] private AudioClip femaleSound;
    [SerializeField] private AudioClip yeahSound;
    [SerializeField] private AudioClip nopeSound;
    [SerializeField] private AudioClip dingSound;
    [SerializeField] private AudioClip boostSound;

    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private bool isSceneLoading = false;
    
    public void SetIsSceneLoading(bool isLoading)
    {
        isSceneLoading = isLoading;
    }


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
        }
    }

    void Start()
    {
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
                sfxSource = gameObject.AddComponent<AudioSource>();
        }

        UpdateFromGlobalSettings();
    }

    public void UpdateFromGlobalSettings()
    {
        if (SoundManager.Instance != null && sfxSource != null)
        {
            sfxSource.mute = !SoundManager.Instance.isSFXOn;
        }
    }

    public void ConfirmButtonSound()
    {
        PlaySFX(buttonClickSound);
    }

    public void CancelButtonSound()
    {
        PlaySFX(buttonCancelSound);
    }

    public void NewRankSound()
    {
        PlaySFX(newRankSound);
    }
    public void DragSound()
    {
        if (isSceneLoading)
            return;
        PlaySFX(dragSound);
    }
    public void PutSound()
    {
        if (isSceneLoading)
            return;
        PlaySFX(putSound);
    }
    public void MaleSound()
    {
        if (isSceneLoading)
            return;
        PlaySFX(maleSound);
    }
    public void FemaleSound()
    {
        if (isSceneLoading)
            return;
        PlaySFX(femaleSound);
    }
    public void YeahSound()
    {
        if (isSceneLoading)
            return;
        PlaySFX(yeahSound);
    }
    public void NopeSound()
    {
        if (isSceneLoading)
            return;
        PlaySFX(nopeSound);
    }
    public void DingSound()
    {
        if (isSceneLoading)
            return;
        PlaySFX(dingSound);
    }
    public void BoostSound()
    {
        if (isSceneLoading)
            return;
        PlaySFX(boostSound);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            if (SoundManager.Instance != null && SoundManager.Instance.isSFXOn)
            {
                sfxSource.PlayOneShot(clip);
            }
        }
    }
}