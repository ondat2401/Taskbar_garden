# AI agent entry point

Đọc `AI_README.md` trước để biết hiện trạng, đường dẫn và verification; chỉ đọc code liên quan tới task. Sau mỗi thay đổi, cập nhật bản hiện trạng này, không nối nhật ký dài hoặc giữ kế hoạch đã lỗi thời.

Nếu có `CHECKPOINT.md`, đọc checkpoint đang làm trước khi tiếp tục. User yêu cầu chia công việc theo feature và ghi checkpoint sau mỗi phần; phân biệt code nháp, scene đã tích hợp và build đã kiểm chứng.

## Quy tắc dự án

- Trả lời tiếng Việt, ngắn gọn. Giữ diff nhỏ, đọc caller và tái sử dụng pattern hiện có.
- Không refactor lớn, xóa tính năng hay đổi API công khai khi chưa hỏi user. Không ghi đè thay đổi của user.
- Không resolve dependency runtime bằng GetComponent/TryGetComponent, FindObjectOfType/FindObjectsOfType, GameObject.Find hoặc Transform.Find. Serialize/inject reference rõ ràng.
- Không Instantiate/Destroy trong flow gameplay runtime; dùng scene/prefab được chuẩn bị hoặc pool/factory có thiết kế rõ ràng.
- Không tạo/mutate List, LINQ hay allocation lặp lại trong Update/LateUpdate/FixedUpdate.
- Editor tooling được phép tạo asset/scene, lookup và dựng layout. Không nhầm code Editor với runtime.
- Giữ config, runtime state và presentation riêng. Không tạo abstraction/framework cho tương lai.
- Asset game thuộc `Assets/Game/`; Scripts chia theo feature, asset chia theo loại. Package bên thứ ba ở Packages.
- Dùng Unity CLI/Pipeline để thao tác scene/asset trong Editor đang mở; không sửa YAML thủ công.
- Chạy compile và kiểm tra hành vi Unity nếu thay gameplay. Không nói đã test phần chưa thực sự chạy. Xóa object/code debug tạm sau khi xong.
