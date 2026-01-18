using UnityEngine;

public class BinCollisionHandler : MonoBehaviour
{
    [SerializeField] private BinData binData;

    private void Start()
    {
        if (GetComponent<Collider>() == null)
        {
            Debug.LogWarning("BinCollisionHandler requires a Collider component!");
        }
    }

    public BinData GetBinData()
    {
        return binData;
    }

    public void SetBinData(BinData data)
    {
        binData = data;
    }
}
