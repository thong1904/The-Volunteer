using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Crouch")]
    public float standHeight = 1.8f;
    public float crouchHeight = 1.0f;

    CharacterController controller;
    Vector3 velocity;
    bool canRequestJump = true;

    bool isCrouching;
    bool jumpRequested;
    bool hasJumped;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        controller.height = standHeight;
    }

    void Update()
    {
        HandleJump();      // đọc input trước
        HandleCrouch();
        ApplyGravity();    // cập nhật velocity.y
        HandleMovement();  // Move DUY NHẤT 1 LẦN
    }

    /* ================= MOVEMENT ================= */

   void HandleMovement()
{
    Vector2 input = GameInputManager.Instance.Move;

    float speed;

    if (isCrouching)
    {
        speed = walkSpeed * 0.5f; // crouch speed
    }
    else if (GameInputManager.Instance.Sprint)
    {
        speed = sprintSpeed;
    }
    else
    {
        speed = walkSpeed;
    }

    Vector3 move =
        transform.right * input.x +
        transform.forward * input.y;

    Vector3 finalMove = move * speed;
    finalMove.y = velocity.y;

    controller.Move(finalMove * Time.deltaTime);
}

    /* ================= JUMP ================= */

   void HandleJump()
{
    // ❌ nếu đang crouch → hủy jump ngay
    if (isCrouching)
    {
        GameInputManager.Instance.ConsumeJump();
        jumpRequested = false;
        return;
    }

    if (GameInputManager.Instance.Jump && canRequestJump)
    {
        jumpRequested = true;
        canRequestJump = false;
        GameInputManager.Instance.ConsumeJump();
    }

    if (hasJumped)
        return;

    if (jumpRequested && controller.isGrounded)
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        hasJumped = true;
        jumpRequested = false;
    }
}



    /* ================= CROUCH ================= */

   void HandleCrouch()
{
    if (!GameInputManager.Instance.CrouchToggle)
        return;

    isCrouching = !isCrouching;
    controller.height = isCrouching ? crouchHeight : standHeight;

    // 🔥 CLEAR JUMP KHI ĐỔI TRẠNG THÁI
    jumpRequested = false;
    canRequestJump = true;

    GameInputManager.Instance.ConsumeCrouch();
}

    /* ================= GRAVITY ================= */

   void ApplyGravity()
{
    if (controller.isGrounded && velocity.y < 0)
    {
        velocity.y = -2f;
        hasJumped = false;
        canRequestJump = true; // 🔓 cho phép nhận jump mới
    }

    velocity.y += gravity * Time.deltaTime;
}

}
