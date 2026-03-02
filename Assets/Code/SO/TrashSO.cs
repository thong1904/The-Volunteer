using UnityEngine;

[CreateAssetMenu(menuName = "Trash/TrashSO")]
public class TrashSO : ScriptableObject
{
    public string trashName;
    public Sprite icon;

    [HideInInspector] public int price;

    public void RollPrice()
    {
        price = Random.Range(10, 21); // 10–20
    }
}
