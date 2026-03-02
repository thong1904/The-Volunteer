using UnityEngine;
using UnityEngine.InputSystem;

public class InputTest : MonoBehaviour
{
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            Debug.Log("INTERACT WORKS");
    }
}
