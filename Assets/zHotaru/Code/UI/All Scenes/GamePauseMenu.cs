using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý Pause Menu trong gameplay scenes.
/// Đặt vào Canvas chứa Pause Menu.
/// Nhấn Esc để mở/đóng Pause Menu.
/// </summary>
public class GamePauseMenu : MonoBehaviour
{
    [Header("=== Menu References ===")]
    [SerializeField] private UIVFX pauseMenuVFX;
    [SerializeField] private UIVFX settingsMenuVFX;

    [Header("=== Scene Transition ===")]
    [SerializeField] private LogoSceneTransition logoTransition;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("=== Input Settings ===")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    [Header("=== Camera Control ===")]
    [Tooltip("Kéo Camera hoặc GameObject chứa script điều khiển camera vào đây")]
    [SerializeField] private MonoBehaviour cameraController;

    // State
    private bool isPaused = false;
    private bool isSettingsOpen = false;
    private bool isTransitioning = false;

    public bool IsPaused => isPaused;
    public bool IsSettingsOpen => isSettingsOpen;

    // Static property để các script khác check (VD: Camera, Player Movement)
    public static bool IsGamePaused { get; private set; } = false;

    void Update()
    {
        // Không xử lý input khi đang chuyển cảnh
        if (isTransitioning) return;

        if (Input.GetKeyDown(pauseKey))
        {
            HandleEscapePress();
        }
    }

    private void HandleEscapePress()
    {
        // Nếu Shop đang mở -> ShopInteract tự xử lý ESC, không làm gì ở đây
        if (ShopInteract.IsShopOpen)
        {
            return;
        }
        
        // Nếu Settings đang mở -> đóng Settings
        if (isSettingsOpen)
        {
            CloseSettings();
            return;
        }

        // Toggle Pause Menu
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    #region Pause/Resume

    /// <summary>
    /// Tạm dừng game và hiện Pause Menu
    /// </summary>
    public void PauseGame()
    {
        if (isPaused || isTransitioning) return;

        isPaused = true;
        IsGamePaused = true;
        Time.timeScale = 0f;

        // Disable camera movement
        SetCameraEnabled(false);

        // UIVFX đã có SetUpdate(true) nên vẫn hoạt động khi timeScale = 0
        pauseMenuVFX?.FadeInUI();

        // Ẩn và unlock cursor cho UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[GamePauseMenu] Game Paused");
    }

    /// <summary>
    /// Tiếp tục game và ẩn Pause Menu
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused || isTransitioning) return;

        pauseMenuVFX?.FadeOutUI(() =>
        {
            isPaused = false;
            IsGamePaused = false;
            Time.timeScale = 1f;

            // Enable camera movement
            SetCameraEnabled(true);

            // Lock cursor lại cho gameplay (FPS game)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("[GamePauseMenu] Game Resumed");
        });
    }

    #endregion

    #region Settings

    /// <summary>
    /// Mở Settings Menu (overlay trên Pause Menu)
    /// </summary>
    public void OpenSettings()
    {
        if (isSettingsOpen) return;

        isSettingsOpen = true;
        settingsMenuVFX?.FadeInUI();

        Debug.Log("[GamePauseMenu] Settings Opened");
    }

    /// <summary>
    /// Đóng Settings Menu
    /// </summary>
    public void CloseSettings()
    {
        if (!isSettingsOpen) return;

        settingsMenuVFX?.FadeOutUI(() =>
        {
            isSettingsOpen = false;
            Debug.Log("[GamePauseMenu] Settings Closed");
        });
    }

    #endregion

    #region Back to Menu

    /// <summary>
    /// Quay về Main Menu
    /// TODO: Thêm SaveGame trước khi chuyển scene
    /// </summary>
    public void BackToMainMenu()
    {
        if (isTransitioning) return;

        isTransitioning = true;

        // Đóng các menu trước
        settingsMenuVFX?.FadeOutUI();
        pauseMenuVFX?.FadeOutUI();

        // === TODO: SAVE GAME TRƯỚC KHI QUAY VỀ MENU ===
        // if (GameManager.Instance?.SaveLoad != null)
        // {
        //     GameManager.Instance.SaveLoad.SaveGame();
        // }
        // ==============================================

        // Reset time scale trước khi chuyển scene
        Time.timeScale = 1f;
        isPaused = false;

        // Reset cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Chuyển scene với transition
        if (logoTransition != null)
        {
            logoTransition.TransitionToScene(mainMenuSceneName);
        }
        else
        {
            // Fallback nếu không có logoTransition
            SceneManager.LoadScene(mainMenuSceneName);
        }

        Debug.Log("[GamePauseMenu] Returning to Main Menu");
    }

    #endregion

    #region Quit Game

    /// <summary>
    /// Thoát game hoàn toàn
    /// TODO: Thêm SaveGame trước khi thoát
    /// </summary>
    public void QuitGame()
    {
        if (isTransitioning) return;

        isTransitioning = true;

        // Đóng các menu trước
        settingsMenuVFX?.FadeOutUI();
        pauseMenuVFX?.FadeOutUI();

        // === TODO: SAVE GAME TRƯỚC KHI THOÁT ===
        // if (GameManager.Instance?.SaveLoad != null)
        // {
        //     GameManager.Instance.SaveLoad.SaveGame();
        // }
        // ========================================

        // Reset time scale
        Time.timeScale = 1f;
        isPaused = false;

        // Quit với transition hoặc trực tiếp
        if (logoTransition != null)
        {
            // Phát transition rồi quit
            logoTransition.OnTransitionComplete += DoQuit;
            logoTransition.PlayFullTransition();
        }
        else
        {
            DoQuit();
        }

        Debug.Log("[GamePauseMenu] Quitting Game");
    }

    private void DoQuit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    #endregion

    #region Utility

    /// <summary>
    /// Enable/Disable camera controller
    /// </summary>
    private void SetCameraEnabled(bool enabled)
    {
        if (cameraController != null)
        {
            cameraController.enabled = enabled;
        }
    }

    /// <summary>
    /// Force close tất cả menu (dùng khi cần reset state)
    /// </summary>
    public void ForceCloseAllMenus()
    {
        isPaused = false;
        isSettingsOpen = false;
        isTransitioning = false;
        IsGamePaused = false;
        Time.timeScale = 1f;

        if (pauseMenuVFX != null)
        {
            var canvasGroup = pauseMenuVFX.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        if (settingsMenuVFX != null)
        {
            var canvasGroup = settingsMenuVFX.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
    }

    #endregion

    void OnDestroy()
    {
        // Cleanup: đảm bảo timeScale được reset
        if (isPaused)
        {
            Time.timeScale = 1f;
        }

        // Unsubscribe event
        if (logoTransition != null)
        {
            logoTransition.OnTransitionComplete -= DoQuit;
        }
    }
}
