using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Interact Ray")]
    public float interactDistance = 2.5f;
    public LayerMask interactLayer;

    [Header("Gameplay")]
    public PlayerScanSkill scanSkill;

    [Header("References")]
    public Transform cameraTransform;

    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float crouchSpeed = 2f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Look")]
    public float mouseSensitivity = 1.2f;
    public float gamepadSensitivity = 90f;
    public float minLookX = -80f;
    public float maxLookX = 80f;

    [Header("Crouch")]
    public float standHeight = 1.8f;
    public float crouchHeight = 1.0f;

    CharacterController controller;
    TrashPickup currentTrash;
    TrashBin currentBin;
    IInteractable currentInteract;
    Vector2 moveInput;
    Vector2 lookInput;
    Vector3 velocity;

    float xRotation;
    bool isGrounded;
    bool isSprinting;
    bool isCrouching;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        controller.height = standHeight;
    }

    void Update()
    {
        HandleGroundCheck();
        HandleMove();
        HandleLook();
        ApplyGravity();
    }

    void LateUpdate()
    {
        CheckInteractRay();
    }

    #region INPUT

    public void OnMove(InputAction.CallbackContext ctx)
    {

        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || isCrouching) return;

        if (isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    public void OnSprint(InputAction.CallbackContext ctx)
    {
        if (isCrouching)
        {
            isSprinting = false;
            return;
        }

        isSprinting = ctx.ReadValueAsButton();
    }

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        isCrouching = !isCrouching;
        controller.height = isCrouching ? crouchHeight : standHeight;
        isSprinting = false;
    }

    public void OnScan(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || scanSkill == null) return;
        scanSkill.TryScan();
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || currentInteract == null) return;

        currentInteract.Interact();
        ClearInteractState();
    }

    #endregion

    #region MOVEMENT

    void HandleMove()
    {
        float speed = walkSpeed;

        if (isCrouching) speed = crouchSpeed;
        else if (isSprinting) speed = sprintSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * speed * Time.deltaTime);
    }

    void HandleLook()
    {
        if (lookInput == Vector2.zero) return;

        float sensitivity =
            Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero
            ? mouseSensitivity
            : gamepadSensitivity * Time.deltaTime;

        float mouseX = lookInput.x * sensitivity;
        float mouseY = lookInput.y * sensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minLookX, maxLookX);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleGroundCheck()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    #endregion

    #region INTERACT & OUTLINE

    void CheckInteractRay()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            // ===== TRASH PICKUP =====
            TrashPickup trash = hit.collider.GetComponentInParent<TrashPickup>();
            if (trash != null)
            {
                ClearInteractState();

                currentTrash = trash;
                currentInteract = trash;

                trash.RequestOutline(OutlineState.Interact);
                PickupPromptUI.Instance.Show("E to pick up");
                return;
            }

            // ===== TRASH BIN =====
            TrashBin bin = hit.collider.GetComponentInParent<TrashBin>();
            if (bin != null)
            {
                ClearInteractState();

                currentBin = bin;
                currentInteract = bin;

                bin.ShowOutline();
                PickupPromptUI.Instance.Show("E to sell");
                return;
            }
        }

        ClearInteractState();
    }



    void ClearInteractState()
    {
        if (currentTrash != null)
        {
            currentTrash.ReleaseOutline(OutlineState.Interact);
            currentTrash = null;
        }

        if (currentBin != null)
        {
            currentBin.HideOutline();
            currentBin = null;
        }

        currentInteract = null;
        PickupPromptUI.Instance.Hide();
    }

    #endregion
}
