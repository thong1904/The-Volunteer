using UnityEngine;

public class CursorManager : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        LockCursor();
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
