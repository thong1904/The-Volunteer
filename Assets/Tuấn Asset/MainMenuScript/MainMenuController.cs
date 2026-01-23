using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup canvasGroup;

    [Header("Fade Settings")]
    public float fadeInSpeed = 1.5f;
    public float fadeOutSpeed = 2.0f;

    bool isTransitioning = false;

    void Start()
    {
        // Chuẩn bị trạng thái ban đầu
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * fadeInSpeed;
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    // ===================== BUTTON EVENTS =====================

    public void OnPlayPressed()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        StartCoroutine(FadeOutAndLoad("Hotaru Test"));
    }

    public void OnQuitPressed()
    {
        if (isTransitioning) return;

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ===================== SCENE TRANSITION =====================

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeOutSpeed;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        SceneManager.LoadScene(sceneName);
    }
}
