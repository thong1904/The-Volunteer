using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Crouch")]
    public float crouchHeight = 1.0f;
    public float standHeight = 1.8f;

    CharacterController controller;
    Vector2 moveInput;
    Vector3 velocity;

    bool isSprinting;
    bool isCrouching;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    /* ===== INPUT EVENTS ===== */

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext ctx)
    {
        isSprinting = ctx.ReadValueAsButton();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            ToggleCrouch();
    }

    /* ===== LOGIC ===== */

    void Update()
    {
        bool grounded = controller.isGrounded;
        if (grounded && velocity.y < 0)
            velocity.y = -2f;

        float speed = isSprinting ? sprintSpeed : walkSpeed;
        if (isCrouching) speed *= 0.5f;

        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        controller.height = isCrouching ? crouchHeight : standHeight;
    }
}
