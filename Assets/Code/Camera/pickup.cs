using UnityEngine;

public class InteractionRaycaster : MonoBehaviour
{
    public float interactDistance = 3f;

    GameObject currentTarget;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider.CompareTag("Pickup"))
            {
                if (currentTarget != hit.collider.gameObject)
                {
                    ClearHighlight();
                    currentTarget = hit.collider.gameObject;
                    SetHighlight(currentTarget, true);
                }
                return;
            }
        }

        ClearHighlight();
    }

    void ClearHighlight()
    {
        if (currentTarget != null)
        {
            SetHighlight(currentTarget, false);
            currentTarget = null;
        }
    }

    void SetHighlight(GameObject obj, bool state)
    {
        Outline outline = obj.GetComponent<Outline>();
        if (outline != null)
            outline.enabled = state;
    }
}
