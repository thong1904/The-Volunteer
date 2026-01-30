using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class RebindActionUI : MonoBehaviour
{
    public InputActionReference action;
    public int bindingIndex;
    public TMP_Text buttonText;

    InputActionRebindingExtensions.RebindingOperation operation;

    void Start()
    {
        Refresh();
    }

   public void StartRebind()
{
    if (action == null) return;

    buttonText.text = "Press key...";
    action.action.Disable();

    // 🔥 QUAN TRỌNG: xóa override cũ
    action.action.RemoveBindingOverride(bindingIndex);

    operation = action.action
        .PerformInteractiveRebinding(bindingIndex)
        .OnComplete(op =>
        {
            op.Dispose();
            action.action.Enable();

            GameSettingManager.Instance.SaveRebinds();
            Refresh();
        });

    operation.Start();
}


    public void Refresh()
    {
        if (action == null) return;

        buttonText.text =
            action.action.GetBindingDisplayString(bindingIndex);
    }
}
