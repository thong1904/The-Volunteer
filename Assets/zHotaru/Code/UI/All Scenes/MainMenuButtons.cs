using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{
    [Header("=== UI VFX References ===")]
    [SerializeField] private UIVFX mainMenuVFX;
    [SerializeField] private UIVFX playMenuVFX;
    [SerializeField] private UIVFX settingsMenuVFX;
    [SerializeField] private UIVFX loadMenuVFX;

    [Header("=== Scene Transition ===")]
    [SerializeField] private LogoSceneTransition logoTransition;
    [SerializeField] private string gameSceneName = "Game";

    #region Main Menu Buttons
    public void OnPlayButtonClicked()
    {
        mainMenuVFX?.FadeOutUI(() =>
        {
            playMenuVFX?.FadeInUI();
        });
    }

    public void OnNewGameClicked()
    {
        if (logoTransition != null)
        {
            logoTransition.TransitionToScene(gameSceneName);
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    public void OnSettingsButtonClicked()
    {
        settingsMenuVFX?.FadeInUI();
    }

    public void OnLoadButtonClicked()
    {
        loadMenuVFX?.FadeInUI();
    }

    public void OnQuitButtonClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    #endregion

    #region Back Buttons
    public void OnBackFromPlayMenuClicked()
    {
        playMenuVFX?.FadeOutUI(() =>
        {
            mainMenuVFX?.FadeInUI();
        });
    }

    public void OnCloseSettingsClicked()
    {
        settingsMenuVFX?.FadeOutUI();
    }

    public void OnCloseLoadClicked()
    {
        loadMenuVFX?.FadeOutUI();
    }
    #endregion

    #region Utility Methods
    public void TransitionMenus(UIVFX fromMenu, UIVFX toMenu)
    {
        if (fromMenu != null)
        {
            fromMenu.FadeOutUI(() =>
            {
                toMenu?.FadeInUI();
            });
        }
        else
        {
            toMenu?.FadeInUI();
        }
    }

    public void OpenOverlayMenu(UIVFX menu)
    {
        menu?.FadeInUI();
    }

    public void CloseMenu(UIVFX menu)
    {
        menu?.FadeOutUI();
    }
    #endregion
}
