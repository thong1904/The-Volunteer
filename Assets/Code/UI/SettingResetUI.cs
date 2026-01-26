using UnityEngine;

public class SettingResetUI : MonoBehaviour
{
    public void ResetKeybind()
    {
        GameSettingManager.Instance.ResetAllKeybinds();
    }
}
