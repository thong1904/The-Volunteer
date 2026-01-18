using UnityEngine;

[CreateAssetMenu(menuName = "Trash/Bin Data")]
public class BinData : ScriptableObject
{
    public TrashType binType;

    public int GetScore(TrashData trashData)
    {
        if (trashData.trashType == binType)
        {
            return trashData.scoreValue;
        }
        return 0;
    }
}