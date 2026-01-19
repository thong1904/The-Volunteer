using UnityEngine;
using UnityEngine.InputSystem;

public class InputLoader : MonoBehaviour
{
    public InputActionAsset actions;

    void Awake()
    {
        foreach (var map in actions.actionMaps)
        {
            string json = PlayerPrefs.GetString(map.name, "");
            if (!string.IsNullOrEmpty(json))
                map.LoadBindingOverridesFromJson(json);
        }
    }
}
    