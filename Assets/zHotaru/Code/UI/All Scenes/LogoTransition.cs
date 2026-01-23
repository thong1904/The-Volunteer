using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Component chuyển cảnh với Logo animation.
/// Đặt trong mỗi scene, gắn vào Canvas chứa Logo Image.
/// </summary>
public class LogoSceneTransition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup transitionCanvasGroup;
    [SerializeField] private Image logoImage;

    [Header("Transition Settings")]
    [SerializeField] private float scaleUp = 10f;
    [SerializeField] private float transitionDuration = 1.5f;
    [SerializeField] private float reverseTransitionDuration = 1.5f;
    [SerializeField] private float holdDuration = 0.3f; // Thời gian giữ logo ở giữa
    
    [Header("Auto Play")]
    [SerializeField] private bool autoPlayReverseOnStart = false;
    
    // Callbacks
    public System.Action OnTransitionStart;
    public System.Action OnTransitionComplete;
    public System.Action OnReverseComplete;
    
    // State
    private bool isTransitioning = false;
    public bool IsTransitioning => isTransitioning;

    private void Awake()
    {
        InitializeUI();
    }

    private void Start()
    {
        if (autoPlayReverseOnStart && IsReady())
        {
            PlayReverseTransition();
        }
    }

    private void InitializeUI()
    {
        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.interactable = false;
            transitionCanvasGroup.blocksRaycasts = false;
        }
        
        if (logoImage != null)
        {
            logoImage.transform.localScale = new Vector3(scaleUp, scaleUp, scaleUp);
        }
    }

    /// <summary>
    /// Kiểm tra references đã sẵn sàng chưa
    /// </summary>
    public bool IsReady()
    {
        return transitionCanvasGroup != null && logoImage != null;
    }

    /// <summary>
    /// Chuyển đến scene khác với transition animation
    /// </summary>
    public void TransitionToScene(string sceneName)
    {
        if (!IsReady() || string.IsNullOrEmpty(sceneName) || isTransitioning) return;
        StartCoroutine(DoTransitionToScene(sceneName));
    }

    /// <summary>
    /// Phát animation reverse (logo thu nhỏ rồi biến mất)
    /// Thường dùng khi vào scene mới
    /// </summary>
    public void PlayReverseTransition(System.Action onComplete = null)
    {
        if (!IsReady() || isTransitioning) return;
        StartCoroutine(DoReverseTransition(onComplete));
    }

    /// <summary>
    /// Phát animation forward rồi reverse (không chuyển scene)
    /// Dùng cho loading, ending, etc.
    /// </summary>
    public void PlayFullTransition(System.Action onComplete = null)
    {
        if (!IsReady() || isTransitioning) return;
        StartCoroutine(DoFullTransition(onComplete));
    }

    private IEnumerator DoTransitionToScene(string sceneName)
    {
        isTransitioning = true;
        OnTransitionStart?.Invoke();

        // Show canvas
        transitionCanvasGroup.alpha = 1f;
        transitionCanvasGroup.interactable = true;
        transitionCanvasGroup.blocksRaycasts = true;
        logoImage.transform.localScale = new Vector3(scaleUp, scaleUp, scaleUp);

        // Logo zoom in
        yield return logoImage.transform
            .DOScale(Vector3.one, transitionDuration)
            .SetEase(Ease.OutExpo)
            .SetUpdate(true)
            .WaitForCompletion();

        // Hold
        yield return new WaitForSecondsRealtime(holdDuration);

        // Load scene
        SceneManager.LoadScene(sceneName);
        
        // Note: Sau khi load scene, object này sẽ bị destroy
        // Scene mới cần có LogoSceneTransition với autoPlayReverseOnStart = true
    }

    private IEnumerator DoReverseTransition(System.Action onComplete)
    {
        isTransitioning = true;

        // Show canvas với logo ở giữa
        transitionCanvasGroup.alpha = 1f;
        transitionCanvasGroup.interactable = true;
        transitionCanvasGroup.blocksRaycasts = true;
        logoImage.transform.localScale = Vector3.one;

        // Logo zoom out
        yield return logoImage.transform
            .DOScale(Vector3.zero, reverseTransitionDuration)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .WaitForCompletion();

        // Hide canvas
        transitionCanvasGroup.alpha = 0f;
        transitionCanvasGroup.interactable = false;
        transitionCanvasGroup.blocksRaycasts = false;

        isTransitioning = false;
        
        OnReverseComplete?.Invoke();
        onComplete?.Invoke();
    }

    private IEnumerator DoFullTransition(System.Action onComplete)
    {
        isTransitioning = true;
        OnTransitionStart?.Invoke();

        // Show canvas
        transitionCanvasGroup.alpha = 1f;
        transitionCanvasGroup.interactable = true;
        transitionCanvasGroup.blocksRaycasts = true;
        logoImage.transform.localScale = new Vector3(scaleUp, scaleUp, scaleUp);

        // Logo zoom in
        yield return logoImage.transform
            .DOScale(Vector3.one, transitionDuration)
            .SetEase(Ease.OutExpo)
            .SetUpdate(true)
            .WaitForCompletion();

        // Hold
        yield return new WaitForSecondsRealtime(holdDuration);

        // Logo zoom out
        yield return logoImage.transform
            .DOScale(Vector3.zero, reverseTransitionDuration)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .WaitForCompletion();

        // Hide canvas
        transitionCanvasGroup.alpha = 0f;
        transitionCanvasGroup.interactable = false;
        transitionCanvasGroup.blocksRaycasts = false;

        isTransitioning = false;
        
        OnTransitionComplete?.Invoke();
        onComplete?.Invoke();
    }

    /// <summary>
    /// Dừng tất cả transition đang chạy
    /// </summary>
    public void StopTransition()
    {
        StopAllCoroutines();
        
        if (logoImage != null)
            logoImage.transform.DOKill();

        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.interactable = false;
            transitionCanvasGroup.blocksRaycasts = false;
        }
        
        isTransitioning = false;
    }

    private void OnDestroy()
    {
        if (logoImage != null)
            logoImage.transform.DOKill();
    }
}