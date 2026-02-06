using UnityEngine;

public class PlayerMoney : MonoBehaviour
{
    public static PlayerMoney Instance;
    public int money;

    void Awake()
    {
        Instance = this;
    }

    public void AddMoney(int amount)
    {
        money += amount;
        Debug.Log("Earned: " + amount + " | Total: " + money);
    }

    // ✅ THÊM HÀM NÀY
    public bool Spend(int amount)
    {
        if (money < amount)
        {
            Debug.Log("Not enough money!");
            return false;
        }

        money -= amount;
        Debug.Log("Spent: " + amount + " | Left: " + money);
        return true;
    }
}
