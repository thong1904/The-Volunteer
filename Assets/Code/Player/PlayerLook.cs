using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    public Transform playerBody;
    public float sensitivity = 1f;

    Vector2 lookInput;
    float xRotation = 0f;

    public void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    void Update()
    {
        float mouseX = lookInput.x * sensitivity * 80 * Time.deltaTime;
        float mouseY = lookInput.y * sensitivity * 80 * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
