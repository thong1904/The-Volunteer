using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;

    PlayerInputActions input;

    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float crouchSpeed = 2f;

    [Header("Jump & Gravity")]
    public float jumpForce = 1.6f;
    public float gravity = -9.81f;

    [Header("Crouch")]
    public float standHeight = 2f;
    public float crouchHeight = 1f;
    public Vector3 standCenter = new Vector3(0, 1f, 0);
    public Vector3 crouchCenter = new Vector3(0, 0.5f, 0);

    Vector2 moveInput;
    Vector3 velocity;

    bool isGrounded;
    bool isSprinting;
    bool isCrouching;

    // ================= INIT =================
    void Awake()
    {
        input = new PlayerInputActions();
    }

    void OnEnable()
    {
        input.Player.Enable();

        input.Player.Move.performed += OnMove;
        input.Player.Move.canceled  += OnMoveCanceled;

        input.Player.Sprint.performed += OnSprint;
        input.Player.Sprint.canceled  += OnSprintCanceled;

        input.Player.Crouch.performed += OnCrouch;
        input.Player.Jump.performed   += OnJump;
    }

    void OnDisable()
    {
        input.Player.Move.performed -= OnMove;
        input.Player.Move.canceled  -= OnMoveCanceled;

        input.Player.Sprint.performed -= OnSprint;
        input.Player.Sprint.canceled  -= OnSprintCanceled;

        input.Player.Crouch.performed -= OnCrouch;
        input.Player.Jump.performed   -= OnJump;

        input.Player.Disable();
    }

    // ================= INPUT =================
    void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    void OnSprint(InputAction.CallbackContext ctx)
    {
        if (isCrouching) return;
        isSprinting = true;
    }

    void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        isSprinting = false;
    }

    void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (isCrouching)
            TryStandUp();
        else
            Crouch();
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        if (!isGrounded) return;
        if (isCrouching) return;

        velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
    }

    // ================= UPDATE =================
    void Update()
    {
        GroundCheck();
        Move();
        ApplyGravity();
    }

    // ================= LOGIC =================
    void GroundCheck()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;
    }

    void Move()
    {
        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        float speed = walkSpeed;

        if (isCrouching)
            speed = crouchSpeed;
        else if (isSprinting)
            speed = sprintSpeed;

        controller.Move(move * speed * Time.deltaTime);
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // ================= CROUCH =================
  void Crouch()
{
    controller.height = crouchHeight;

    // Pivot ở giữa → kéo collider xuống 1 chút
    float offset = (standHeight - crouchHeight) / 2f;
    controller.center = Vector3.down * offset;

    isCrouching = true;
}

void TryStandUp()
{
    float checkDistance = standHeight - crouchHeight;
    Vector3 origin = transform.position + Vector3.up * (crouchHeight / 2f);

    if (Physics.SphereCast(origin, controller.radius, Vector3.up, out _, checkDistance))
        return;

    controller.height = standHeight;
    controller.center = Vector3.zero; // QUAN TRỌNG
    isCrouching = false;
}

}
