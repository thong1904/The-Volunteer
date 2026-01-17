using UnityEngine;

/// <summary>
/// Hướng dẫn chi tiết để tạo và cấu hình Behavior Tree trong Behavior Designer
/// </summary>
/// 
/*
 * ========================================
 * HƯỚNG DẪN CẤU HÌNH BEHAVIOR TREE
 * ========================================
 * 
 * ## BƯỚC 1: Chuẩn bị GameObject NPC
 * 
 * 1.1. Tạo hoặc chọn NPC GameObject
 * 1.2. Thêm các component bắt buộc:
 *      - NPCBehaviorTree (script chính)
 *      - BehaviorTree (từ Behavior Designer plugin)
 *      - Rigidbody (sử dụng Kinematic hoặc Dynamic)
 *      - Capsule Collider (hoặc Collider phù hợp)
 *      - Animator (nếu có animation)
 * 
 * 1.3. Cấu hình trong Inspector:
 *      NPCBehaviorTree:
 *      - NPC Name: Tên của NPC (ví dụ "NPC_Guide_01")
 *      - NPC ID: ID duy nhất (0, 1, 2, ...)
 *      - Museum Entrance: Vị trí cửa bảo tàng (ví dụ 0, 5, -10)
 *      - Display Positions: Mảng các vị trí trưng bày (tối thiểu 3-5 vị trí)
 *      - Movement Speed: 3-5 m/s
 *      - Min Stay Time: 2-3 giây
 *      - Max Stay Time: 5-8 giây
 *      - Min Museum Time: 30 giây
 *      - Max Museum Time: 120 giây
 *      - Question Event Chance: 30% (0-100)
 *      - Litter Chance: 15% (0-100)
 * 
 * ## BƯỚC 2: Tạo Behavior Tree trong Behavior Designer
 * 
 * 2.1. Mở cửa sổ Behavior Designer
 *      - Window → Behavior Designer
 * 
 * 2.2. Tạo Behavior Tree mới
 *      - Nhấp vào BehaviorTree component
 *      - Chọn "Create Behavior Tree" nếu chưa có
 * 
 * 2.3. Cấu trúc cây (Copy cấu trúc này):
 * 
 *      ROOT (Sequence)
 *      │
 *      ├─── SELECTOR "Daily Check"
 *      │    ├─── CheckDayEnded
 *      │    └─── Sequence "Museum Behavior"
 *      │         ├─── MoveToTarget (tới bảo tàng)
 *      │         ├─── SetEnteredMuseum
 *      │         │
 *      │         └─── REPEATER (hoặc Parallel)
 *      │              └─── SELECTOR "Inside Museum"
 *      │                   ├─── CheckTimeToLeaveMuseum (rời bảo tàng nếu đủ thời gian)
 *      │                   │    └─── NPCLeaveMuseum
 *      │                   │
 *      │                   ├─── Sequence "Question Event"
 *      │                   │    ├─── CheckQuestionEvent
 *      │                   │    └─── NPCQuestionEvent
 *      │                   │
 *      │                   ├─── Sequence "Littering"
 *      │                   │    ├─── CheckLittering
 *      │                   │    └─── NPCLitteringBehavior
 *      │                   │
 *      │                   └─── Sequence "View Display"
 *      │                        ├─── SetTargetDisplay
 *      │                        ├─── MoveToTarget
 *      │                        └─── NPCWaitAtDisplay
 * 
 * ## BƯỚC 3: Chi tiết từng Node
 * 
 * 3.1. CheckDayEnded
 *      - Task Type: Conditional
 *      - Script: CheckDayEnded
 *      - Chức năng: Kiểm tra xem ngày đã kết thúc (trong Selector, priority cao)
 * 
 * 3.2. CheckTimeToLeaveMuseum
 *      - Task Type: Conditional
 *      - Script: CheckTimeToLeaveMuseum
 *      - Chức năng: Kiểm tra thời gian ở bảo tàng (nếu đủ thời gian → Success)
 *      - Lưu ý: PHẢI nằm TRONG REPEATER ở trong bảo tàng, chứ không phải ở ngoài!
 * 
 * 3.3. MoveToTarget (xuất hiện 2 lần)
 *      - Task Type: Action
 *      - Script: NPCMoveToTargetNavMesh (hoặc NPCMoveToTarget)
 *      - Cài đặt:
 *        * Stopping Distance: 0.5 (NavMesh) hoặc 1 (Manual)
 *      - Chức năng: Di chuyển tới target (tới bảo tàng hoặc vị trí trưng bày)
 *      - LƯU Ý: File có tên NPCMoveToTarget.cs nhưng class là NPCMoveToTargetNavMesh
 * 
 * 3.4. SetEnteredMuseum
 *      - Task Type: Action
 *      - Script: SetEnteredMuseum
 *      - Chức năng: Đánh dấu NPC đã vào bảo tàng (kích hoạt timer)
 *      - Điều kiện thành công: Nếu khoảng cách < 2f thì Success
 * 
 * 3.5. SetTargetDisplay
 *      - Task Type: Action
 *      - Script: SetTargetDisplay
 *      - Chức năng: Chọn ngẫu nhiên 1 vị trí trưng bày từ danh sách
 * 
 * 3.6. NPCWaitAtDisplay
 *      - Task Type: Action
 *      - Script: NPCWaitAtDisplay
 *      - Cài đặt:
 *        * Min Wait Time: 3
 *        * Max Wait Time: 8
 *      - Chức năng: NPC dừng lại để xem trưng bày
 * 
 * 3.7. CheckQuestionEvent
 *      - Task Type: Conditional
 *      - Script: CheckQuestionEvent
 *      - Chức năng: 30% chance để kích hoạt event câu hỏi (mỗi frame)
 * 
 * 3.8. NPCQuestionEvent
 *      - Task Type: Action
 *      - Script: NPCQuestionEvent
 *      - Cài đặt:
 *        * Questions: Thêm danh sách các câu hỏi
 *          - Question Text: "Nội dung câu hỏi?"
 *          - Answers: ["Đáp án 1", "Đáp án 2", "Đáp án 3"]
 *          - Correct Answer Index: (0, 1 hoặc 2)
 *          - Points Reward: 10
 *      - Chức năng: Hiển thị câu hỏi và xử lý câu trả lời
 * 
 * 3.9. CheckLittering
 *      - Task Type: Conditional
 *      - Script: CheckLittering
 *      - Chức năng: 15% chance để vứt rác (mỗi frame)
 * 
 * 3.10. NPCLitteringBehavior
 *       - Task Type: Action
 *       - Script: NPCLitteringBehavior
 *       - Cài đặt:
 *         * Trash Prefab: Gán prefab rác
 *         * Throw Force: 5
 *       - Chức năng: NPC vứt rác
 * 
 * 3.11. NPCLeaveMuseum
 *       - Task Type: Action
 *       - Script: NPCLeaveMuseum
 *       - Cài đặt:
 *         * Stopping Distance: 1
 *         * Rotation Speed: 5
 *       - Chức năng: NPC rời bảo tàng, di chuyển ra khỏi (100 đơn vị), sau đó despawn
 *       - LƯU Ý: Phải là Sequence bên trong SELECTOR "Inside Museum", không phải ở ngoài!
 * 
 * ## BƯỚC 4: Cấu hình các Composite Node
 * 
 * 4.1. SEQUENCE
 *      - Loại: BehaviorDesigner.Runtime.Tasks.Sequence
 *      - Hành động: Chạy các child lần lượt, dừng nếu một child fail
 *      - Dùng cho: Các tác vụ cần xảy ra theo thứ tự (ví dụ: kiểm tra → di chuyển → chờ)
 * 
 * 4.2. SELECTOR
 *      - Loại: BehaviorDesigner.Runtime.Tasks.Selector
 *      - Hành động: Chạy các child lần lượt, dừng nếu một child success
 *      - Dùng cho: Lựa chọn hành động ưu tiên (ví dụ: rời nếu hết hạn, nếu không thì tiếp tục)
 * 
 * 4.3. PARALLEL (tùy chọn)
 *      - Loại: BehaviorDesigner.Runtime.Tasks.Parallel
 *      - Hành động: Chạy tất cả child cùng lúc
 *      - Dùng cho: Thực hiện nhiều tác vụ song song
 * 
 * 4.4. REPEATER (tùy chọn)
 *      - Loại: BehaviorDesigner.Runtime.Tasks.Repeater
 *      - Cài đặt: Unlimited hoặc số lần cụ thể
 *      - Dùng cho: Lặp lại các tác vụ
 * 
 * ## BƯỚC 5: Cầu hình GameManager và UIManager
 * 
 * 5.1. Tạo GameObject "GameManager"
 *      - Thêm component GameManager
 *      - Cài đặt: Day Duration = 300 (5 phút = 1 ngày)
 * 
 * 5.2. Tạo GameObject "UIManager"
 *      - Thêm component UIManager
 *      - Gán các element UI:
 *        * Question Panel: Canvas/QuestionPanel
 *        * Question Text: Canvas/QuestionPanel/QuestionText
 *        * NPC Name Text: Canvas/QuestionPanel/NPCNameText
 *        * Answers Container: Canvas/QuestionPanel/AnswerButtonsContainer
 *        * Answer Button Prefab: Prefab của nút câu trả lời
 * 
 * ## BƯỚC 6: Chuẩn bị Prefab rác
 * 
 * 6.1. Tạo hoặc import mô hình rác
 * 6.2. Thêm Rigidbody để rác có thể rơi
 * 6.3. Lưu làm Prefab
 * 6.4. Gán vào NPCLitteringBehavior → Trash Prefab
 * 
 * ## BƯỚC 7: Test
 * 
 * 7.1. Chạy Scene
 * 7.2. Theo dõi trong Console để xem các sự kiện
 * 7.3. Điều chỉnh tham số nếu cần
 * 
 * ## NHỮNG ĐIỀU CẦN LƯU Ý
 * 
 * ⚠️ CẤU TRÚC BEHAVIOR TREE ĐÚNG:
 *    ROOT (Sequence)
 *    └─ SELECTOR "Daily Check"
 *       ├─ CheckDayEnded (nếu đúng → despawn)
 *       └─ Sequence "Museum Behavior"
 *          ├─ MoveToTarget (tới bảo tàng)
 *          ├─ SetEnteredMuseum (đánh dấu đã vào)
 *          └─ REPEATER
 *             └─ SELECTOR "Inside Museum"
 *                ├─ Sequence: CheckTimeToLeaveMuseum → NPCLeaveMuseum (RỜI và DESPAWN)
 *                ├─ Sequence: CheckQuestionEvent → NPCQuestionEvent
 *                ├─ Sequence: CheckLittering → NPCLitteringBehavior
 *                └─ Sequence: SetTargetDisplay → MoveToTarget → NPCWaitAtDisplay
 *
 * ⚠️ LỖICHIARA CỦA BẢN CŨ: 
 *    - Nếu NPCLeaveMuseum ở NGOÀI, NPC sẽ rời bảo tàng ngay lập tức
 *    - Cần phải ở BÊN TRONG REPEATER với priority cao (đầu Selector)
 * 
 * ⚠️ Xác suất (30% câu hỏi, 15% rác) xảy ra mỗi frame
 *    Nên thêm cooldown nếu không muốn sự kiện xảy ra quá thường xuyên
 * 
 * ⚠️ UIManager cần các component UI:
 *    - TextMeshPro/TextMeshProUGUI
 *    - Button
 * 
 * ⚠️ Đảm bảo có đủ Display Positions trong NPCBehaviorTree
 *    Nếu mảng trống, NPC sẽ không di chuyển trong bảo tàng
 * 
 * ⚠️ Museum Entrance phải nằm trong phạm vi map và accessible
 * 
 * ⚠️ Nếu sử dụng Rigidbody, hãy dùng Kinematic mode để kiểm soát vị trí
 * 
 * ⚠️ NPCLeaveMuseum sẽ gọi DespawnNPC() khi NPC rời khỏi bảo tàng
 *    SetActive(false) làm NPC ẩn đi, hoặc có thể thay bằng Destroy()
 */

public class BehaviorTreeDetailedGuide : MonoBehaviour
{
    // Hướng dẫn chi tiết - không cần code thực thi
}
