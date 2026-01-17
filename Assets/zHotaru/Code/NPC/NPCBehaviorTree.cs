using UnityEngine;
using BehaviorDesigner.Runtime;

public class NPCBehaviorTree : MonoBehaviour
{
    [Header("NPC Info")]
    [SerializeField] private string npcName;
    [SerializeField] private int npcID;
    
    [Header("Museum Settings")]
    [SerializeField] private Vector3 museumEntrance;
    [SerializeField] private Vector3[] displayPositions; // Vị trí các khu trưng bày
    
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
    
    public string NPCName => npcName;
    public int NPCID => npcID;
    public Vector3 CurrentTarget => currentTarget;
    public bool IsInMuseum => isInMuseum;
    public float MovementSpeed => movementSpeed;
    
    void Start()
    {
        if (behaviorTree == null)
            behaviorTree = GetComponent<BehaviorTree>();
        
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
        if (displayPositions.Length > 0)
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
        currentTarget = transform.position + Vector3.forward * 20f; // Di chuyển ra khỏi bảo tàng
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
}
