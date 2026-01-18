using UnityEngine;

public class ButtonHover : MonoBehaviour
{
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnHoverEnter()
    {
        transform.localScale = hoverScale;
    }

    public void OnHoverExit()
    {
        transform.localScale = originalScale;
    }
}
