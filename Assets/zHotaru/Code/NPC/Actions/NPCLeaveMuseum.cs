using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NPC")]
[TaskName("Leave Museum")]
[TaskDescription("NPC rời bỏ bảo tàng")]
public class NPCLeaveMuseum : Action
{
    private NPCBehaviorTree npcBehavior;
    private Vector3 exitPosition;
    
    [SerializeField] private float stoppingDistance = 1f;
    [SerializeField] private float rotationSpeed = 5f;
    
    public override void OnAwake()
    {
        npcBehavior = GetComponent<NPCBehaviorTree>();
    }
    
    public override void OnStart()
    {
        // Thiết lập vị trí thoát ra khỏi bảo tàng (xa ra phía trước)
        exitPosition = transform.position + transform.forward * 50f;
        npcBehavior.SetExiting();
        
        Debug.Log($"{npcBehavior.NPCName} đang rời bảo tàng");
    }
    
    public override TaskStatus OnUpdate()
    {
        Vector3 targetPosition = npcBehavior.CurrentTarget;
        
        // Tính khoảng cách tới mục tiêu
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        // Nếu đã rời xa bảo tàng
        if (distance < stoppingDistance || transform.position.z > exitPosition.z)
        {
            Rigidbody rb = transform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
            
            Debug.Log($"{npcBehavior.NPCName} đã rời bảo tàng");
            return TaskStatus.Success;
        }
        
        // Hướng tới mục tiêu
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        // Xoay hướng NPC
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        
        // Di chuyển NPC
        Vector3 movement = direction * npcBehavior.MovementSpeed;
        
        Rigidbody rb2 = transform.GetComponent<Rigidbody>();
        if (rb2 != null)
        {
            rb2.linearVelocity = new Vector3(movement.x, rb2.linearVelocity.y, movement.z);
        }
        else
        {
            transform.Translate(movement * Time.deltaTime, Space.World);
        }
        
        return TaskStatus.Running;
    }
    
    public override void OnEnd()
    {
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }
}
