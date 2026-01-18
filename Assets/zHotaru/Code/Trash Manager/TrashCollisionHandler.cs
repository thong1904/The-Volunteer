using UnityEngine;

public class TrashCollisionHandler : MonoBehaviour
{
    [SerializeField] private TrashData trashData;
    private bool hasCollided = false;

    private void Start()
    {
        // Đảm bảo Rigidbody có collider
        if (GetComponent<Collider>() == null)
        {
            Debug.LogWarning("TrashCollisionHandler requires a Collider component!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollided) return; // Chỉ xử lý va chạm một lần

        BinCollisionHandler binHandler = other.GetComponent<BinCollisionHandler>();
        if (binHandler == null)
        {
            // Thử tìm BinCollisionHandler từ parent object
            binHandler = other.GetComponentInParent<BinCollisionHandler>();
        }

        if (binHandler != null && trashData != null)
        {
            HandleCollisionWithBin(binHandler.GetBinData());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;

        BinCollisionHandler binHandler = collision.gameObject.GetComponent<BinCollisionHandler>();
        if (binHandler == null)
        {
            binHandler = collision.gameObject.GetComponentInParent<BinCollisionHandler>();
        }

        if (binHandler != null && trashData != null)
        {
            HandleCollisionWithBin(binHandler.GetBinData());
        }
    }

    private void HandleCollisionWithBin(BinData binData)
    {
        int score = binData.GetScore(trashData);
        
        Debug.Log($"Trash [{trashData.trashType}] hit Bin [{binData.binType}] - Score: {score}");

        // Gọi callback nếu có
        TrashCollisionManager.Instance?.CollectTrash(trashData, binData, score);

        // Thêm hiệu ứng hoặc xóa object
        hasCollided = true;
        Destroy(gameObject);
    }

    public TrashData GetTrashData()
    {
        return trashData;
    }

    public void SetTrashData(TrashData data)
    {
        trashData = data;
    }
}
