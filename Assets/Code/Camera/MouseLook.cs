using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    public Transform player;
    public float sensitivity = 1f;
    public float baseSensitivity = 0.1f;
    float xRotation;
    Vector2 lookInput;

    PlayerInputActions input;

    void Awake()
    {
        input = new PlayerInputActions();
    }

    void OnEnable()
    {
        input.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        input.Player.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    void OnDisable()
    {
        input.Player.Look.performed -= ctx => lookInput = ctx.ReadValue<Vector2>();
        input.Player.Look.canceled -= ctx => lookInput = Vector2.zero;
    }

    void Update()
    {
        float mouseX = lookInput.x * sensitivity * baseSensitivity;
        float mouseY = lookInput.y * sensitivity * baseSensitivity;


        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        player.Rotate(Vector3.up * mouseX);
    }

    public void EnableInput() => input.Player.Enable();
    public void DisableInput() => input.Player.Disable();
    public void SetSensitivity(float value)
    {
        sensitivity = value;
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
    }

    public float GetSensitivity()
    {
        return sensitivity;
    }

}
