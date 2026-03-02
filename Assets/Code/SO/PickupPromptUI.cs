using UnityEngine;
using TMPro;

public class PickupPromptUI : MonoBehaviour
{
    public static PickupPromptUI Instance;

    public GameObject root;
    public TMP_Text text;

    void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(string message)
    {
        root.SetActive(true);
        text.text = message;
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}
