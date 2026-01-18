// Sử dụng script này để tạo Behavior Tree trong Behavior Designer
// 
// Hướng dẫn tạo Behavior Tree:
// ================================
//
// 1. Tạo GameObject cho NPC:
//    - Add component NPCBehaviorTree
//    - Add component BehaviorTree (từ Behavior Designer)
//    - Add component Rigidbody (nếu cần di chuyển vật lý)
//    - Add component Animator (nếu cần animation)
//
// 2. Cấu hình NPCBehaviorTree:
//    - Đặt Museum Entrance (vị trí cửa bảo tàng)
//    - Đặt Display Positions (các vị trí trưng bày)
//    - Cấu hình Movement Speed, Stay Time, Museum Time
//    - Cấu hình Question Event Chance, Litter Chance
//
// 3. Tạo Behavior Tree trong Behavior Designer:
//    - Mở Behavior Designer
//    - Tạo Sequence chính (Root)
//    - Cấu trúc Tree như sau:
//
//    Root (Sequence)
//    │
//    ├─ Selector (Ưu tiên rời bảo tàng)
//    │  ├─ CheckDayEnded
//    │  ├─ CheckTimeToLeaveMuseum
//    │  └─ Sequence (Normal Behavior)
//    │     │
//    │     ├─ MoveToTarget (tới bảo tàng)
//    │     │
//    │     └─ Selector (Khi ở trong bảo tàng)
//    │        ├─ CheckQuestionEvent → NPCQuestionEvent
//    │        ├─ CheckLittering → NPCLitteringBehavior
//    │        └─ Sequence (Di chuyển xem trưng bày)
//    │           ├─ (Action) SetTargetDisplayPosition
//    │           ├─ MoveToTarget
//    │           └─ NPCWaitAtDisplay
//    │
//    └─ NPCLeaveMuseum (rời bảo tàng)
//
// 4. Các Action tùy chỉnh cần thêm:
//    - SetTargetDisplayPosition (action để chọn vị trí trưng bày)
//    - SetEntered (action để đánh dấu NPC đã vào bảo tàng)
//
// Diễn giải luồng hoạt động:
// =========================
// 1. NPC bắt đầu di chuyển tới bảo tàng
// 2. Khi vào bảo tàng, bắt đầu timer rời bảo tàng
// 3. Trong bảo tàng, NPC lặp lại:
//    a. Kiểm tra xem có event câu hỏi không (30%)
//    b. Kiểm tra xem có vứt rác không (15%)
//    c. Chọn vị trí trưng bày và đi xem
// 4. Khi hết thời gian hoặc ngày kết thúc, NPC rời bảo tàng
//
// Lưu ý:
// - Điều chỉnh xác suất trong Inspector
// - Xác suất kiểm tra mỗi frame, nên hạn chế bằng cooldown nếu cần
// - Cần implement các action SetTargetDisplayPosition và SetEntered
// - Cần có UI để hiển thị câu hỏi khi event xảy ra

using UnityEngine;

public class BehaviorTreeGuide : MonoBehaviour
{
    // Đây chỉ là file hướng dẫn, không cần code thực thi
}
