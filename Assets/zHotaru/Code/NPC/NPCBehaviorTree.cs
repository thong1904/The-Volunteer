using UnityEngine;
using BehaviorDesigner.Runtime;

public enum NPCGender
{
    Male,
    Female
}

public class NPCBehaviorTree : MonoBehaviour
{
    [Header("NPC Info")]
    [SerializeField] private string npcName;
    [SerializeField] private NPCGender gender = NPCGender.Male;
    
    [Header("Museum Settings")]
    [SerializeField] private Transform museumEntranceTransform;
    [SerializeField] private Transform[] displayTransforms; // Transform các khu trưng bày
    
    // Fallback for old Vector3 system (kept for backward compatibility)
    [SerializeField] private Vector3 museumEntrance;
    [SerializeField] private Vector3[] displayPositions;
    
    [Header("Behavior Settings")]
    [SerializeField] private float movementSpeed = 3f;
    //[SerializeField] private float minStayTime = 3f;
    //[SerializeField] private float maxStayTime = 8f;
    [SerializeField] private float minMuseumTime = 30f;
    [SerializeField] private float maxMuseumTime = 120f;
    
    [Header("Probability")]
    [SerializeField] [Range(0, 100)] private float litterChance = 15f; // % xác suất vứt rác
    
    [Header("Question Event Settings")]
    [SerializeField] private float questionTriggerRadius = 5f; // Bán kính kích hoạt câu hỏi (theo NPC)
    [SerializeField, Range(0f, 100f)] private float questionChancePercent = 50f; // Xác suất bổ sung cho CheckQuestionEvent
    [SerializeField] private bool showQuestionRadiusGizmo = true; // Hiển thị Gizmo bán kính câu hỏi
    
    [Header("References")]
    [SerializeField] private BehaviorTree behaviorTree;
    
    private Vector3 currentTarget;
    private bool isInMuseum = false;
    private bool isDayEnded = false;
    private bool isAtDisplay = false;
    private float museumExitTime;
    private Animator animator;
    private Coroutine questionSoundCoroutine;
    
    public string NPCName => npcName;
    public NPCGender Gender => gender;
    public bool IsAtDisplay { get => isAtDisplay; set => isAtDisplay = value; }
    public Vector3 CurrentTarget => currentTarget;
    public bool IsInMuseum => isInMuseum;
    public float MovementSpeed => movementSpeed;
    public float QuestionTriggerRadius => questionTriggerRadius;
    public float QuestionChancePercent => questionChancePercent;
    public bool ShowQuestionRadiusGizmo => showQuestionRadiusGizmo;
    
    void Start()
    {
        if (behaviorTree == null)
            behaviorTree = GetComponent<BehaviorTree>();
        
        animator = GetComponent<Animator>();
        
        // Lấy vị trí entrance từ Transform, nếu không có thì dùng Vector3
        if (museumEntranceTransform != null)
            currentTarget = museumEntranceTransform.position;
        else
            currentTarget = museumEntrance;
            
        museumExitTime = Random.Range(minMuseumTime, maxMuseumTime);
    }

    void Update()
    {
        // Kiểm tra nếu ngày kết thúc
        if (DayNightManager.Instance != null && DayNightManager.Instance.IsNighttime())
        {
            isDayEnded = true;
        }
    }
    
    public void SetTargetDisplayPosition()
    {
        if (displayTransforms != null && displayTransforms.Length > 0)
        {
            Transform randomDisplay = displayTransforms[Random.Range(0, displayTransforms.Length)];
            var displayArea = randomDisplay.GetComponent<DisplayArea>();
            currentTarget = displayArea != null ? displayArea.GetRandomPosition() : randomDisplay.position;
        }
        else if (displayPositions != null && displayPositions.Length > 0)
        {
            currentTarget = displayPositions[Random.Range(0, displayPositions.Length)];
        }
    }
    
    public void SetEntered()
    {
        isInMuseum = true;
        museumExitTime = Time.time + Random.Range(minMuseumTime, maxMuseumTime);
    }
    
    public void SetExiting()
    {
        isInMuseum = false;
        
        // Lấy vị trí entrance
        Vector3 entrancePos = museumEntranceTransform != null 
            ? museumEntranceTransform.position 
            : museumEntrance;
        
        // Tính vị trí exit phía ngoài entrance
        Vector3 exitDirection = (entrancePos - transform.position).normalized;
        currentTarget = entrancePos + exitDirection * 20f;
    }
    
    public void DespawnNPC()
    {
        // Gọi NPCManager thay vì tự SetActive
        if (GameManager.Instance != null && GameManager.Instance.NPCs != null)
        {
            GameManager.Instance.NPCs.DespawnNPC(gameObject);
        }
        else
        {
            // Fallback
            gameObject.SetActive(false);
        }
    }
    
    // Đã hợp nhất xác suất hỏi thành QuestionChancePercent để tránh chồng xác suất
    
    public bool ShouldLitter()
    {
        return isInMuseum && Random.Range(0f, 100f) < litterChance;
    }
    
    public bool IsTimeToLeaveMuseum()
    {
        return isInMuseum && Time.time >= museumExitTime;
    }
    
    public bool IsDayEnded()
    {
        return isDayEnded;
    }
    
    public void PlayAnimation(string animationName)
    {
        if (animator != null && !string.IsNullOrEmpty(animationName))
        {
            animator.SetTrigger(animationName);
        }
    }

    // Inject displays from NPCManager (scene-level)
    public void SetDisplays(Transform[] displays)
    {
        if (displays != null && displays.Length > 0)
            displayTransforms = displays;
    }

    // Inject entrance point from NPCManager
    public void SetEntrancePoint(Transform entrance)
    {
        museumEntranceTransform = entrance;
        if (entrance != null)
            museumEntrance = entrance.position;
    }

    // Reset all runtime state on (re)spawn
    public void ResetState()
    {
        isInMuseum = false;
        isDayEnded = false;
        museumExitTime = Random.Range(minMuseumTime, maxMuseumTime);

        // Reset target back to entrance so NPC walks in
        currentTarget = museumEntranceTransform != null
            ? museumEntranceTransform.position
            : museumEntrance;

        // Restart Behavior Designer tree cleanly
        if (behaviorTree == null)
            behaviorTree = GetComponent<BehaviorTree>();
        if (behaviorTree != null)
        {
            behaviorTree.DisableBehavior();
            behaviorTree.EnableBehavior();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showQuestionRadiusGizmo) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, questionTriggerRadius);
    }

    #region NPC Sound Methods

    /// <summary>
    /// Bắt đầu phát âm thanh hỏi lặp lại (male/female sound)
    /// </summary>
    public void StartQuestionSound()
    {
        StopQuestionSound(); // Dừng nếu đang chạy
        questionSoundCoroutine = StartCoroutine(QuestionSoundLoop());
    }

    /// <summary>
    /// Dừng phát âm thanh hỏi
    /// </summary>
    public void StopQuestionSound()
    {
        if (questionSoundCoroutine != null)
        {
            StopCoroutine(questionSoundCoroutine);
            questionSoundCoroutine = null;
        }
    }

    private System.Collections.IEnumerator QuestionSoundLoop()
    {
        while (true)
        {
            PlayQuestionSound();
            yield return new WaitForSeconds(2f); // Lặp lại mỗi 2 giây
        }
    }

    /// <summary>
    /// Phát âm thanh hỏi một lần (male/female)
    /// </summary>
    public void PlayQuestionSound()
    {
        if (SoundManager.Instance == null) return;

        if (gender == NPCGender.Male)
            SoundManager.Instance.PlayMale();
        else
            SoundManager.Instance.PlayFemale();
    }

    /// <summary>
    /// Phát âm thanh khi trả lời đúng
    /// </summary>
    public void PlayCorrectAnswerSound()
    {
        if (SoundManager.Instance == null) return;

        if (gender == NPCGender.Male)
            SoundManager.Instance.PlayMaYeah();
        else
            SoundManager.Instance.PlayFeYeah();
    }

    /// <summary>
    /// Phát âm thanh khi trả lời sai
    /// </summary>
    public void PlayWrongAnswerSound()
    {
        if (SoundManager.Instance == null) return;

        if (gender == NPCGender.Male)
            SoundManager.Instance.PlayMaHuh();
        else
            SoundManager.Instance.PlayFeHuh();
    }

    #endregion
}
