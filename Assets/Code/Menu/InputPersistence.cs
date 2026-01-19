using UnityEngine;
using UnityEngine.InputSystem;
public class InputPersistence : MonoBehaviour
{
    public InputActionAsset actions;

    void Awake()
    {
        

        foreach (var map in actions.actionMaps)
        {
            string key = map.name + "_" + map.name;
            if (PlayerPrefs.HasKey(key))
                map.LoadBindingOverridesFromJson(PlayerPrefs.GetString(key));
        }
    }
}
