using UnityEngine;

public class PlayerMoney : MonoBehaviour
{
    public int currentMoney;

    public void Add(int value)
    {
        currentMoney += value;
    }

    public bool Spend(int value)
    {
        if (currentMoney < value) return false;
        currentMoney -= value;
        return true;
    }
}
