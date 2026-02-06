using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopUpgradeButton : MonoBehaviour
{
    public ShopUpgradeType upgradeType;

    [Header("Price")]
    public int basePrice = 100;
    public int priceIncrease = 50;

    [Header("UI")]
    public TextMeshProUGUI priceText;
    public Button button;

    PlayerScanSkill scanSkill;
    InventoryManager inventory;

    int currentPrice;

    void Start()
    {
        scanSkill = FindFirstObjectByType<PlayerScanSkill>();
        inventory = InventoryManager.Instance;

        UpdatePrice();
        UpdateUI();
    }

    void UpdatePrice()
    {
        int level = 1;

        if (upgradeType != ShopUpgradeType.InventorySlot)
            level = scanSkill.level;

        currentPrice = basePrice + priceIncrease * (level - 1);
        priceText.text = currentPrice + "$";
    }

    void UpdateUI()
    {
        if (upgradeType != ShopUpgradeType.InventorySlot)
        {
            button.interactable = scanSkill.CanUpgrade();
        }
        else
        {
            button.interactable =
                inventory.unlockedSlots < inventory.slots.Count;
        }
    }

    public void OnClick()
    {
        if (PlayerMoney.Instance.money < currentPrice)
        {
            Debug.Log("Not enough money");
            return;
        }

        bool success = false;

        switch (upgradeType)
        {
            case ShopUpgradeType.ScanRadius:
            case ShopUpgradeType.ScanDuration:
            case ShopUpgradeType.ScanCooldown:
                success = scanSkill.Upgrade();
                break;

            case ShopUpgradeType.InventorySlot:
                inventory.UnlockNextSlot();
                success = true;
                break;
        }

        if (!success) return;

        PlayerMoney.Instance.AddMoney(-currentPrice);

        UpdatePrice();
        UpdateUI();
    }
}
