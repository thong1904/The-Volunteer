using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public PlayerInput playerInput;
    public PlayerLook mouseLook;

    bool isOpen = false;

    public void OnToggleMenu(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        isOpen = !isOpen;

        settingsPanel.SetActive(isOpen);

        if (isOpen)
            OpenMenu();
        else
            CloseMenu();
    }
   void Start()
{
    isOpen = false;
    settingsPanel.SetActive(false);

    playerInput.enabled = false;
    playerInput.enabled = true;

    playerInput.SwitchCurrentActionMap("Player");

    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
}

    void OpenMenu()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0f;

        playerInput.SwitchCurrentActionMap("UI");
        mouseLook.enabled = false;
    }

    void CloseMenu()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Time.timeScale = 1f;

        playerInput.SwitchCurrentActionMap("Player");
        mouseLook.enabled = true;
    }
}
