using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    public float jumpForce = 6f;
    public float walkSpeed = 5f;
    public float sprintSpeed = 7.5f;

    bool isSprinting;
    CharacterController controller;
    Vector2 moveInput;
    Vector3 velocity;

    PlayerInputActions input;

    void Awake()
    {
        input = new PlayerInputActions();
        controller = GetComponent<CharacterController>();
    }

    void OnEnable()
    {
        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        input.Player.Jump.performed += _ => Jump();

        input.Player.Sprint.performed += _ => isSprinting = true;
        input.Player.Sprint.canceled += _ => isSprinting = false;
    }

    void OnDisable()
    {
        input.Player.Move.performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled -= ctx => moveInput = Vector2.zero;

        input.Player.Jump.performed -= _ => Jump();

        input.Player.Sprint.performed -= _ => isSprinting = true;
        input.Player.Sprint.canceled -= _ => isSprinting = false;
    }

    void Update()
    {
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        controller.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Jump()
    {
        if (controller.isGrounded)
            velocity.y = jumpForce;
    }

    public void EnableInput() => input.Player.Enable();
    public void DisableInput() => input.Player.Disable();
}
