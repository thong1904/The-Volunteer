using TMPro;
using UnityEngine;

public class MoneyUI : MonoBehaviour
{
    public TextMeshProUGUI text;

    void Update()
    {
        if (MoneyManager.Instance != null)
            text.text = MoneyManager.Instance.GetTotalMoney() + "$";
    }
}
