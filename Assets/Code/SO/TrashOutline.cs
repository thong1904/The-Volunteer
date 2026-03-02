using UnityEngine;

public class TrashOutline : MonoBehaviour
{
    Outline outline;

    void Awake()
    {
        outline = GetComponent<Outline>();
        outline.enabled = false; // Mặc định tắt
    }

    public void SetScan(bool value)
    {
        outline.enabled = value;
    }
}
