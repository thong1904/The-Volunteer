using UnityEngine;
using BehaviorDesigner.Runtime;

public class NPCBehaviorTree : MonoBehaviour
{
    [Header("NPC Info")]
    [SerializeField] private string npcName;
    [SerializeField] private int npcID;
    
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
    [SerializeField] [Range(0, 100)] private float questionEventChance = 30f; // % xác suất event câu hỏi
    [SerializeField] [Range(0, 100)] private float litterChance = 15f; // % xác suất vứt rác
    
    [Header("References")]
    [SerializeField] private BehaviorTree behaviorTree;
    
    private Vector3 currentTarget;
    private bool isInMuseum = false;
    private bool isDayEnded = false;
    private float museumExitTime;
    private Animator animator;
    
    public string NPCName => npcName;
    public int NPCID => npcID;
    public Vector3 CurrentTarget => currentTarget;
    public bool IsInMuseum => isInMuseum;
    public float MovementSpeed => movementSpeed;
    
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
        // Ưu tiên dùng Transform nếu có
        if (displayTransforms.Length > 0)
        {
            Transform randomDisplay = displayTransforms[Random.Range(0, displayTransforms.Length)];
            
            // Nếu là DisplayArea, lấy vị trí ngẫu nhiên trong vùng
            DisplayArea displayArea = randomDisplay.GetComponent<DisplayArea>();
            if (displayArea != null)
            {
                currentTarget = displayArea.GetRandomPosition();
            }
            else
            {
                currentTarget = randomDisplay.position;
            }
        }
        else if (displayPositions.Length > 0)
        {
            // Fallback to Vector3 system
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
        // Vô hiệu hóa NPC sau khi rời bảo tàng
        gameObject.SetActive(false);
        // Hoặc nếu muốn xóa hoàn toàn:
        // Destroy(gameObject);
    }
    
    public bool ShouldTriggerQuestionEvent()
    {
        return Random.Range(0f, 100f) < questionEventChance;
    }
    
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
}
