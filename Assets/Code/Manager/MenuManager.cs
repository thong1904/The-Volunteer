using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    public GameObject menuUI;
    PlayerLook look;

    bool isOpen;
    bool isTransitioning; // 🔑 khóa double trigger

    [System.Obsolete]
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        look = FindObjectOfType<PlayerLook>();
        menuUI.SetActive(false);
    }

    public void Toggle()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        isOpen = !isOpen;

        if (isOpen)
            Open();
        else
            Close();

        // mở khóa ở cuối frame
        Invoke(nameof(Unlock), 0f);
    }

    void Unlock() => isTransitioning = false;

    void Open()
    {
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (look != null)
            look.allowLook = false;

        menuUI.SetActive(true);
    }

    void Close()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (look != null)
            look.allowLook = true;

        menuUI.SetActive(false);
    }

    public bool IsOpen => isOpen;
}
