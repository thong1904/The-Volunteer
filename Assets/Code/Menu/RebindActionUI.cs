using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class RebindKeyButton : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference action;
    public int bindingIndex;
    public InputActionAsset allActions; // Kéo PlayerInputActions vào

    [Header("UI")]
    public TMP_Text buttonText;

    InputActionRebindingExtensions.RebindingOperation rebindingOp;
    string oldBindingPath;

    void Start()
    {
        LoadBinding();
        UpdateButtonText();
    }

    public void StartRebind()
    {
        oldBindingPath = action.action.bindings[bindingIndex].effectivePath;
        buttonText.text = "Press any button";

        action.action.Disable();

        rebindingOp = action.action
            .PerformInteractiveRebinding(bindingIndex)
            .OnPotentialMatch(op =>
            {
                string newPath = op.selectedControl.path;

                if (IsDuplicateBinding(newPath))
                {
                    Debug.Log("Key already in use: " + newPath);
                    op.Cancel(); // HỦY REBIND
                }
            })
            .OnCancel(op =>
            {
                RestoreOldBinding();
            })
            .OnComplete(op =>
            {
                action.action.Enable();
                op.Dispose();

                UpdateButtonText();
                SaveBinding();
            });

        rebindingOp.Start();
    }

    bool IsDuplicateBinding(string newPath)
    {
        foreach (var map in allActions.actionMaps)
        {
            foreach (var act in map.actions)
            {
                foreach (var binding in act.bindings)
                {
                    if (binding.effectivePath == newPath)
                        return true;
                }
            }
        }
        return false;
    }

    void RestoreOldBinding()
    {
        action.action.ApplyBindingOverride(bindingIndex, oldBindingPath);
        action.action.Enable();
        UpdateButtonText();
    }

    void UpdateButtonText()
    {
        buttonText.text =
            action.action.GetBindingDisplayString(bindingIndex);
    }

    void SaveBinding()
    {
        string key = action.action.actionMap.name + "_" + action.action.name;
        PlayerPrefs.SetString(
            key,
            action.action.SaveBindingOverridesAsJson()
        );
    }

    void LoadBinding()
    {
        string key = action.action.actionMap.name + "_" + action.action.name;
        if (PlayerPrefs.HasKey(key))
        {
            action.action.LoadBindingOverridesFromJson(
                PlayerPrefs.GetString(key)
            );
        }
    }
}
