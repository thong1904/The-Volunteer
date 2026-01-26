using UnityEngine;

public class MenuUI : MonoBehaviour
{
    static MenuUI instance;

    void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    public static void Notify(bool show)
    {
        if (instance != null)
            instance.gameObject.SetActive(show);
    }
}
