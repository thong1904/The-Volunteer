using UnityEngine;
using System;

public class TrashCollisionManager : MonoBehaviour
{
    public static TrashCollisionManager Instance { get; private set; }

    // Event khi trash được thu thập - TODO: Dùng để trigger hiệu ứng, âm thanh, v.v
    public event Action<TrashData, BinData, int> OnTrashCollected;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CollectTrash(TrashData trash, BinData bin, int score)
    {
        Debug.Log($"Trash [{trash.trashType}] hit Bin [{bin.binType}] - Score: {score}");

        // Phát hành event
        OnTrashCollected?.Invoke(trash, bin, score);

        // Gọi ScoreManager để cộng điểm
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(score);
        }
    }
}
