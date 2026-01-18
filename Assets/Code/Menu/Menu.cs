using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public GameObject pausePanel;
    public MouseLook mouseLook;
    public PlayerMovement playerMovement;
    public Slider sensitivitySlider;
    PlayerInputActions input;

    public GameObject crosshair;

    bool isPaused;
    void Start()
    {
        pausePanel.SetActive(false);

        float savedSen = PlayerPrefs.GetFloat("MouseSensitivity", 1f);

        sensitivitySlider.minValue = 0f;
        sensitivitySlider.maxValue = 5f;
        sensitivitySlider.value = savedSen;

        mouseLook.SetSensitivity(savedSen);

        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);

        ResumeGame();
    }
    void Awake()
    {
        input = new PlayerInputActions();
        DontDestroyOnLoad(gameObject);
    }

    

    void OnEnable()
    {
        input.Player.Enable();
        input.Player.Menu.performed += OnMenu;
    }

    void OnDisable()
    {
        input.Player.Menu.performed -= OnMenu;
        input.Player.Disable();
    }

    void OnMenu(InputAction.CallbackContext ctx)
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

   void PauseGame()
{
    isPaused = true;
    pausePanel.SetActive(true);
    crosshair.SetActive(false); 

    Time.timeScale = 0f;

    mouseLook.DisableInput();
    playerMovement.DisableInput();

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
}


   void ResumeGame()
{
    isPaused = false;
    pausePanel.SetActive(false);
    crosshair.SetActive(true); 

    Time.timeScale = 1f;

    mouseLook.EnableInput();
    playerMovement.EnableInput();

    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
}

    void OnSensitivityChanged(float value)
{
    mouseLook.SetSensitivity(value);
}

}
