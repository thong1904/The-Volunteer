using UnityEngine;
using UnityEngine.InputSystem;

public class GameSettingManager : MonoBehaviour
{
    public static GameSettingManager Instance;

    public InputActionAsset inputActions;
    public float mouseSensitivity = 1f;

    const string SENS_KEY = "MouseSensitivity";
    const string REBIND_KEY = "InputRebinds";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSettings();
    }

    void LoadSettings()
    {
        mouseSensitivity = PlayerPrefs.GetFloat(SENS_KEY, 1f);

        if (PlayerPrefs.HasKey(REBIND_KEY))
        {
            inputActions.LoadBindingOverridesFromJson(
                PlayerPrefs.GetString(REBIND_KEY)
            );
        }
    }

    public void SetMouseSensitivity(float value)
    {
        mouseSensitivity = value;
        PlayerPrefs.SetFloat(SENS_KEY, value);
    }

    public void SaveRebinds()
    {
        PlayerPrefs.SetString(
            REBIND_KEY,
            inputActions.SaveBindingOverridesAsJson()
        );
        PlayerPrefs.Save();
    }

    public void ResetAllKeybinds()
    {
        foreach (var map in inputActions.actionMaps)
            map.RemoveAllBindingOverrides();

        PlayerPrefs.DeleteKey(REBIND_KEY);
    }
}
