using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC")]
[TaskName("Wait At Display")]
[TaskDescription("NPC dừng lại để xem các vị trí trưng bày")]
public class NPCWaitAtDisplay : Action
{
    private NPCBehaviorTree npcBehavior;
    private float waitTime;
    private float startTime;
    private DisplayArea displayArea;
    
    [SerializeField] private float minWaitTime = 3f;
    [SerializeField] private float maxWaitTime = 8f;
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private float rotationSpeed = 5f; // Tốc độ xoay để nhìn vào vị trí trưng bày
    
    public override void OnAwake()
    {
        npcBehavior = GetComponent<NPCBehaviorTree>();
    }
    
    public override void OnStart()
    {
        // Lựa chọn ngẫu nhiên thời gian chờ
        waitTime = Random.Range(minWaitTime, maxWaitTime);
        startTime = Time.time;
        
        // Dừng di chuyển của NPC
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        
        // Phát animation idle
        npcBehavior.PlayAnimation(idleAnimationName);
        
        // Tìm DisplayArea gần nhất để lấy focus point
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);
        foreach (Collider collider in colliders)
        {
            displayArea = collider.GetComponent<DisplayArea>();
            if (displayArea != null)
                break;
        }
        
        Debug.Log($"{npcBehavior.NPCName} đang xem trưng bày trong {waitTime:F2} giây");
    }
    
    public override TaskStatus OnUpdate()
    {
        // Xoay NPC để nhìn vào focus point của display area
        if (displayArea != null)
        {
            Vector3 focusPoint = displayArea.GetFocusPoint();
            Vector3 directionToFocus = (focusPoint - transform.position).normalized;
            
            // Tính toán quaternion hướng
            Quaternion targetRotation = Quaternion.LookRotation(directionToFocus);
            
            // Xoay NPC mượt mà
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Kiểm tra thời gian chờ
        if (Time.time - startTime >= waitTime)
        {
            return TaskStatus.Success;
        }
        
        return TaskStatus.Running;
    }
    
    public override void OnEnd()
    {
        Debug.Log($"{npcBehavior.NPCName} kết thúc xem trưng bày");
    }
}
