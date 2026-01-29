using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance;

    InputActionAsset actions;
    InputActionMap playerMap;
    InputActionMap uiMap;
    InputAction interact;
    InputAction move, look, sprint, jump, crouch, scene;
    InputAction menuPlayer;
    InputAction menuUI;
    InputAction InventoryPlayer;
    InputAction InventoryUI;
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public bool Sprint { get; private set; }
    public bool InteractThisFrame { get; private set; }
    bool jumpThisFrame;
    bool crouchThisFrame;

    public bool Jump => jumpThisFrame;
    public bool CrouchToggle => crouchThisFrame;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        actions = GameSettingManager.Instance.inputActions;

        playerMap = actions.FindActionMap("Player");
        uiMap = actions.FindActionMap("UI");
        interact = playerMap.FindAction("Interact");
        move = playerMap.FindAction("Move");
        look = playerMap.FindAction("Look");
        sprint = playerMap.FindAction("Sprint");
        jump = playerMap.FindAction("Jump");
        crouch = playerMap.FindAction("Crouch");
        scene = playerMap.FindAction("Scenes");
        menuPlayer = playerMap.FindAction("Menu");
        menuUI = uiMap.FindAction("Menu");
        InventoryPlayer = playerMap.FindAction("Inventory");
        InventoryUI = uiMap.FindAction("Inventory");

        move.performed += ctx => Move = ctx.ReadValue<Vector2>();
        move.canceled += _ => Move = Vector2.zero;

        look.performed += ctx => Look = ctx.ReadValue<Vector2>();
        look.canceled += _ => Look = Vector2.zero;

        sprint.performed += _ => Sprint = true;
        sprint.canceled += _ => Sprint = false;

        jump.performed += _ => jumpThisFrame = true;
        crouch.performed += _ => crouchThisFrame = true;

        // 🔑 P mở / đóng menu ở CẢ 2 map
        menuPlayer.performed += _ => OnMenuPressed();
        menuUI.performed += _ => OnMenuPressed();
        InventoryPlayer.performed += _ => OnMenuPressed();
        InventoryUI.performed += _ => OnMenuPressed();
        scene.performed += _ => SceneManager.LoadScene("map");
        interact.performed += _ => InteractThisFrame = true;
        playerMap.Enable();
    }

    void LateUpdate()
    {
        InteractThisFrame = false;
        jumpThisFrame = false;
        crouchThisFrame = false;
    }

    public void ConsumeJump() => jumpThisFrame = false;
    public void ConsumeCrouch() => crouchThisFrame = false;

    public void EnablePlayer()
    {
        uiMap.Disable();
        playerMap.Enable();
    }

    public void EnableUI()
    {
        playerMap.Disable();
        uiMap.Enable();
    }
    void OnMenuPressed()
    {
        MenuManager.Instance.Toggle();

        if (MenuManager.Instance.IsOpen)
            EnableUI();
        else
            EnablePlayer();
    }
}
