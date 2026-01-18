using UnityEngine;

public class ItemGenerator : MonoBehaviour
{
    public Transform player; // Player
    public GameObject[] itemPrefabs; // Danh sách prefab
    public float spawnRadius = 10f; // Bán kính spawn
    public int itemCount = 12; // Số lượng vật thể spawn
    // Thời gian kiểm tra (giảm delay)
    public float minDistance = 1.5f; // Khoảng cách tối thiểu giữa các vật thể

    private GameObject[] spawnedItems;

    void Start()
    {
        spawnedItems = new GameObject[itemCount];
        SpawnAllItems();
    }
    void Update()
    {
        CheckItemsInRange();
    }

    void SpawnAllItems()
    {
        for (int i = 0; i < itemCount; i++)
        {
            SpawnItem(i);
        }
    }

    void SpawnItem(int index)
    {
        if (spawnedItems[index] != null)
        {
            Destroy(spawnedItems[index]);
        }
        Vector3 randomPos = Vector3.zero;
        bool validPos = false;
        int tryCount = 0;
        while (!validPos && tryCount < 30)
        {
            randomPos = GetRandomPositionAroundPlayer();
            validPos = true;
            for (int i = 0; i < spawnedItems.Length; i++)
            {
                if (i == index) continue;
                if (spawnedItems[i] != null)
                {
                    float dist = Vector3.Distance(randomPos, spawnedItems[i].transform.position);
                    if (dist < minDistance)
                    {
                        validPos = false;
                        break;
                    }
                }
            }
            tryCount++;
        }
        int prefabIdx = Random.Range(0, itemPrefabs.Length);
        spawnedItems[index] = Instantiate(itemPrefabs[prefabIdx], randomPos, Quaternion.identity);
    }

    Vector3 GetRandomPositionAroundPlayer()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        return new Vector3(
            player.position.x + randomCircle.x,
            player.position.y,
            player.position.z + randomCircle.y
        );
    }

    void CheckItemsInRange()
    {
        for (int i = 0; i < spawnedItems.Length; i++)
        {
            if (spawnedItems[i] == null) continue;
            float dist = Vector3.Distance(player.position, spawnedItems[i].transform.position);
            if (dist > spawnRadius)
            {
                Destroy(spawnedItems[i]);
                SpawnItem(i);
            }
        }
    }
}
