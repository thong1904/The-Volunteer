using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    
    [Header("Global Audio Settings")]
    public bool isBGMOn = true;
    public bool isSFXOn = true;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Load saved settings
            LoadAudioSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadAudioSettings()
    {
        isBGMOn = PlayerPrefs.GetInt("BGM_On", 1) == 1;
        isSFXOn = PlayerPrefs.GetInt("SFX_On", 1) == 1;
    }

    private void SaveAudioSettings()
    {
        PlayerPrefs.SetInt("BGM_On", isBGMOn ? 1 : 0);
        PlayerPrefs.SetInt("SFX_On", isSFXOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Global BGM Controls - CHỈ GỌI TỪ SETTINGSMANAGER
    public void ToggleBGM()
    {
        isBGMOn = !isBGMOn;
        SaveAudioSettings();
        
        // Update BGMController if exists
        if (BGMController.Instance != null)
        {
            BGMController.Instance.SetBGMState(isBGMOn);
        }
    }

    // Global SFX Controls - CHỈ GỌI TỪ SETTINGSMANAGER
    public void ToggleSFX()
    {
        isSFXOn = !isSFXOn;
        SaveAudioSettings();
        
        // Update all SFXControllers in current scene
        UpdateAllSFXControllers();
    }

    private void UpdateAllSFXControllers()
    {
        SFXController[] allSFXControllers = FindObjectsByType<SFXController>(FindObjectsSortMode.None);
        foreach (var sfxController in allSFXControllers)
        {
            sfxController.UpdateFromGlobalSettings();
        }
    }

    // Called when scene loads - để sync SFX khi chuyển scene
    public void OnSceneLoaded()
    {
        UpdateAllSFXControllers();
    }
}