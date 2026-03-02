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
    
    [Header("Entrance Area (để NPC không bị kẹt khi rời đi)")]
    [SerializeField] private float entranceAreaWidth = 10f;  // Chiều rộng vùng entrance
    [SerializeField] private float entranceAreaDepth = 3f;   // Chiều sâu vùng entrance
    
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
    private bool hasTriggeredExit = false;  // Đảm bảo chỉ reset BT 1 lần
    private float museumExitTime;
    private Animator animator;
    
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
            
        // Đặt thời gian rời bảo tàng là thời điểm tương lai (Time.time + duration)
        museumExitTime = Time.time + Random.Range(minMuseumTime, maxMuseumTime);
        
        // Subscribe vào event OnSunset để tự động rời bảo tàng khi hết ngày
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.OnSunset += OnSunsetTriggered;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe event khi NPC bị destroy
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.OnSunset -= OnSunsetTriggered;
        }
    }
    
    /// <summary>
    /// Được gọi khi DayNightManager fire event OnSunset
    /// </summary>
    private void OnSunsetTriggered()
    {
        // Tất cả NPC đều rời đi khi sunset, bất kể đã vào museum hay chưa
        if (!hasTriggeredExit)
        {
            Debug.Log($"[NPC] {npcName}: Sunset triggered - bắt đầu rời bảo tàng");
            SetExiting();
        }
    }

    void Update()
    {
        // Bỏ phần check DayNightManager - không cần nữa
        // NPC sẽ tự rời đi theo behavior tree của mình
    }
    
    public void SetTargetDisplayPosition()
    {
        if (displayTransforms != null && displayTransforms.Length > 0)
        {
            // Lọc chỉ lấy display đã available (preset hoặc đã build)
            var availableDisplays = new System.Collections.Generic.List<Transform>();
            foreach (var display in displayTransforms)
            {
                var displayArea = display.GetComponent<DisplayArea>();
                if (displayArea != null && displayArea.IsAvailableForNPC)
                {
                    availableDisplays.Add(display);
                }
            }
            
            if (availableDisplays.Count == 0)
            {
                Debug.LogWarning($"[NPC] {npcName}: Không có display nào khả dụng");
                return;
            }
            
            // Thử nhiều display để tìm vị trí hợp lệ
            int maxAttempts = availableDisplays.Count * 3;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Transform randomDisplay = availableDisplays[Random.Range(0, availableDisplays.Count)];
                var displayArea = randomDisplay.GetComponent<DisplayArea>();
                
                Vector3 targetPos;
                if (displayArea != null)
                {
                    // Truyền transform của NPC để kiểm tra path hợp lệ
                    targetPos = displayArea.GetRandomPosition(transform);
                }
                else
                {
                    targetPos = randomDisplay.position;
                }
                
                // Kiểm tra path có hợp lệ không
                UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
                if (UnityEngine.AI.NavMesh.CalculatePath(transform.position, targetPos, UnityEngine.AI.NavMesh.AllAreas, path))
                {
                    if (path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                    {
                        currentTarget = targetPos;
                        Debug.Log($"[NPC] {npcName}: Chọn display tại {targetPos}");
                        return;
                    }
                }
            }
            
            // Fallback: tìm vị trí gần nhất có thể đến được trong các display available
            Debug.LogWarning($"[NPC] {npcName}: Không tìm được display hợp lệ, thử tìm vị trí gần nhất");
            Vector3 bestTarget = transform.position;
            float bestDist = float.MaxValue;
            
            foreach (Transform display in availableDisplays)
            {
                var area = display.GetComponent<DisplayArea>();
                if (area == null || !area.IsAvailableForNPC) continue;
                
                Vector3 pos = area.GetRandomPosition(transform);
                
                UnityEngine.AI.NavMeshPath testPath = new UnityEngine.AI.NavMeshPath();
                if (UnityEngine.AI.NavMesh.CalculatePath(transform.position, pos, UnityEngine.AI.NavMesh.AllAreas, testPath))
                {
                    if (testPath.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                    {
                        float dist = Vector3.Distance(transform.position, pos);
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            bestTarget = pos;
                        }
                    }
                }
            }
            
            currentTarget = bestTarget;
            Debug.Log($"[NPC] {npcName}: Fallback - chọn display tại {currentTarget}");
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
        // Chỉ thực hiện 1 lần
        if (hasTriggeredExit) return;
        hasTriggeredExit = true;
        
        isInMuseum = false;
        isDayEnded = true;
        
        // Lấy vị trí entrance center
        Vector3 entranceCenter = museumEntranceTransform != null 
            ? museumEntranceTransform.position 
            : museumEntrance;
        
        // Tính vị trí ngẫu nhiên trong vùng entrance để NPC không bị kẹt
        Vector3 randomOffset = new Vector3(
            Random.Range(-entranceAreaWidth / 2f, entranceAreaWidth / 2f),
            0f,
            Random.Range(-entranceAreaDepth / 2f, entranceAreaDepth / 2f)
        );
        
        // Nếu có transform, dùng local direction
        Vector3 targetPos;
        if (museumEntranceTransform != null)
        {
            targetPos = entranceCenter 
                + museumEntranceTransform.right * randomOffset.x 
                + museumEntranceTransform.forward * randomOffset.z;
        }
        else
        {
            targetPos = entranceCenter + randomOffset;
        }
        
        // Sample vị trí lên NavMesh
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(targetPos, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            currentTarget = hit.position;
        }
        else if (UnityEngine.AI.NavMesh.SamplePosition(entranceCenter, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            // Fallback về center nếu random position không valid
            currentTarget = hit.position;
        }
        else
        {
            currentTarget = entranceCenter;
        }
        
        // Restart behavior tree 1 lần duy nhất
        RestartBehaviorTree();
    }
    
    /// <summary>
    /// Restart behavior tree để buộc NPC đánh giá lại từ đầu
    /// </summary>
    private void RestartBehaviorTree()
    {
        if (behaviorTree == null)
            behaviorTree = GetComponent<BehaviorTree>();
        
        if (behaviorTree != null)
        {
            behaviorTree.DisableBehavior();
            behaviorTree.EnableBehavior();
        }
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
        // Không vứt rác nếu ngày đã kết thúc hoặc không ở trong bảo tàng
        if (isDayEnded || !isInMuseum) return false;
        return Random.Range(0f, 100f) < litterChance;
    }
    
    public bool IsTimeToLeaveMuseum()
    {
        return isInMuseum && Time.time >= museumExitTime;
    }
    
    public bool IsDayEnded()
    {
        // Check khi đến endHour (21h) - NPC sẽ rời đi
        if (DayNightManager.Instance != null && DayNightManager.Instance.IsNighttime())
        {
            return true;
        }
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
        hasTriggeredExit = false;  // Reset flag để NPC có thể exit lại ở ngày mới
        
        // Đặt thời gian rời bảo tàng là thời điểm tương lai (Time.time + duration)
        museumExitTime = Time.time + Random.Range(minMuseumTime, maxMuseumTime);

        // Reset target back to entrance so NPC walks in
        currentTarget = museumEntranceTransform != null
            ? museumEntranceTransform.position
            : museumEntrance;

        // Subscribe lại vào event OnSunset (đề phòng trường hợp bị unsubscribe)
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.OnSunset -= OnSunsetTriggered; // Tránh duplicate
            DayNightManager.Instance.OnSunset += OnSunsetTriggered;
        }

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
    /// Phát âm thanh hỏi một lần (male/female) - 3D tại vị trí NPC
    /// </summary>
    public void PlayQuestionSound()
    {
        if (SoundManager.Instance == null) return;

        if (gender == NPCGender.Male)
            SoundManager.Instance.PlayMale3D(transform.position);
        else
            SoundManager.Instance.PlayFemale3D(transform.position);
    }

    /// <summary>
    /// Phát âm thanh khi trả lời đúng - 3D tại vị trí NPC
    /// </summary>
    public void PlayCorrectAnswerSound()
    {
        if (SoundManager.Instance == null) return;

        if (gender == NPCGender.Male)
            SoundManager.Instance.PlayMaYeah3D(transform.position);
        else
            SoundManager.Instance.PlayFeYeah3D(transform.position);
    }

    /// <summary>
    /// Phát âm thanh khi trả lời sai - 3D tại vị trí NPC
    /// </summary>
    public void PlayWrongAnswerSound()
    {
        if (SoundManager.Instance == null) return;

        if (gender == NPCGender.Male)
            SoundManager.Instance.PlayMaHuh3D(transform.position);
        else
            SoundManager.Instance.PlayFeHuh3D(transform.position);
    }

    #endregion
}
