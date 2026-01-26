using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Transform playerBody;
    public float sensitivity = 0.5f;
   
    float xRotation;
    public bool allowLook = true;

    void Update()
    {
        if (!allowLook) return;

        Vector2 look = GameInputManager.Instance.Look;
        if (look == Vector2.zero) return;

        float mouseX = look.x * GameSettingManager.Instance.mouseSensitivity * sensitivity * 80f * Time.deltaTime;
        float mouseY = look.y * GameSettingManager.Instance.mouseSensitivity * sensitivity * 80f * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
