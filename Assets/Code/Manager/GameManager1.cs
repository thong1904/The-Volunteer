using UnityEngine;

public class GameManager1 : MonoBehaviour
{
    public static GameManager1 instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
}
