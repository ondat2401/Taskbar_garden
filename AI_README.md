# AI handoff — Idle Garden

## Checkpoint đang làm — đọc trước

**Đọc `CHECKPOINT.md` để tiếp tục feature mới.** User yêu cầu chia feature và checkpoint vì chuyển context. Đã viết bản nháp tiền/kho/nâng cấp/tái sinh/save và compact HUD, compile + smoke test in-memory pass; **chưa apply setup vào scene và chưa build mới**. Scene/build cũ vẫn là baseline bên dưới.

Thứ tự: F01 kho/vàng → F02 upgrade → F03 compact window/shop/art → F04 save I/O → F05 rebirth end-to-end → F06 build. Hoàn tất và ghi checkpoint từng feature, không coi toàn bộ bản nháp là đã kiểm chứng.

## Đọc trước

- Đây là bản nhớ hiện trạng, không phải lịch sử hội thoại. Cập nhật trực tiếp khi thêm/xóa/sửa; xóa thông tin đã lỗi thời.
- User muốn prototype trồng cây 2D trên taskbar, cửa sổ kéo/resize được, data dễ chỉnh bằng Editor và tăng được số ô. Multiplayer hai vườn đồng bộ là hướng sau, chưa triển khai.
- Unity 6000.3.10f1, URP 17.3.0, Input System 1.18.0, uGUI 2.0.0. Giữ package sẵn có của user.
- Đã thêm Unity Pipeline 0.8.0-exp.1 để điều khiển Editor qua `unity command`, và UniWindowController qua UPM, pin commit `304f9ba2aa4a8fae7f3c71f38118c44722a2f6cc`.

## Phạm vi và quyết định

- Một nhân vật đi ngang; 4 ô mặc định, danh sách profile không hardcode số ô.
- Tự trồng/tưới/lấy nước/thu hoạch, hạt vô hạn, chỉ đếm sản lượng. Cây khô dừng lớn, không chết.
- Mỗi việc chạy đến hết; ưu tiên Harvest > Water > Plant, chọn ô gần nhất trong cùng nhóm. Refill là bước khi thiếu nước.
- Thông số số học / sprite đọc live. Số ô, gán loại cây, spacing đổi ngoài Play rồi Apply Layout.
- Baseline scene/build chưa có kinh tế/nâng cấp/save. Source mới và atlas AI đã được thêm nhưng chưa apply vào scene; chi tiết nằm trong CHECKPOINT.md. Multiplayer, Steam SDK và offline rewards chưa làm.
- Chỉ Windows standalone mới điều khiển cửa sổ native. Tay nắm resize góc phải dưới; chưa resize mọi cạnh, chưa lưu vị trí qua lần chạy.

## Bản đồ code

| Đường dẫn dưới Assets/Game | Vai trò |
|---|---|
| Scripts/Crops/CropDefinition.cs | Asset định nghĩa cây |
| Scripts/Worker/WorkerConfig.cs | Asset tốc độ, thao tác, animation |
| Scripts/Garden/GardenProfile.cs | Profile tổng, danh sách ô, layout, validation |
| Scripts/Garden/GardenSimulation.cs | State thuần và chọn/thực hiện công việc; mảng cấp phát lúc tạo simulation |
| Scripts/Garden/GardenController.cs | Tick, animation, camera pan và HUD |
| Scripts/Garden/PlotView.cs | Hiển thị sprite và bar từng ô |
| Scripts/Desktop/DesktopWindow.cs | Native window, drag/resize/pin/lock, chỉ bật Windows build |
| Scripts/Desktop/WindowHandle.cs | Pointer event cho vùng kéo/resize |
| Scripts/Editor/GardenConfig/GardenConfigWindow.cs | Editor UI Toolkit, 4 tab, serialized fields và runtime monitor |
| Scripts/Editor/GardenConfig/GardenPrototypeBuilder.cs | Tạo default asset/scene/prefab, Apply Layout, build Windows |
| Scripts/Editor/GardenConfig/GardenPlaceholderArt.cs | Sinh PNG placeholder offline trong Editor |
| Scripts/Editor/GardenConfig/GardenVerification.cs | Kiểm tra simulation bằng assert/exception, có menu chạy lại |

## Asset và đường vào

- Scene: `Assets/Game/Scenes/GardenPrototype.unity`; không sửa scene mẫu của user.
- Config chính: `Assets/Game/_Configs/Garden/Prototype.asset` → worker + danh sách crop của từng ô.
- Prefab ô: `Assets/Game/Prefabs/Garden/Plot.prefab`.
- Material: `Assets/Game/Materials/PixelUnlit.mat`; art PNG ở Textures/{Crops,Worker,Environment}, Point/None, PPU 32 (pixel trắng cho bar có PPU 1).
- Menu: `Tools > Idle Garden > Garden Config`, `Open or Create Prototype`, `Verify Simulation`, `Build Windows Prototype`.
- Apply Layout thay nhánh `Idle Garden/Generated Layout` bằng Undo; không để custom object trong đó. Các asset đã tồn tại không bị Create Defaults ghi đè.
- Build: `Builds/Windows/IdleGarden.exe`; build function đặt D3D11, windowed, resizable, background, tắt flip model/HDR, bật alpha URP nếu property có.

## Verification hiện tại

- 2026-10-18: Editor compile pass trên Unity 6000.3.10f1.
- `GardenVerification.Run()` pass: 4/8/12 ô, mỗi ô thu hoạch nhiều vòng, có refill, nước/growth trong giới hạn, pause không mutate state, cây khô không lớn, chỉnh growSeconds áp dụng live.
- Apply Layout trên scene thật: 8 → 12 → 4 ô pass; đã khôi phục 4 ô và simulationSpeed = 1, lưu asset/scene.
- Kiểm tra tạo scene mới từ tool rồi lưu/mở lại: 4 ô và references còn nguyên. Đã xóa scene test và khôi phục scene gốc cùng GUID.
- Play Mode thực tế đã chạy: tổng sản lượng 16, từng ô [3,6,3,4]; Console không warning/error trong lượt chạy sau khi sửa wiring. Đã xem ảnh Game View, sprite/bar/nhân vật hiển thị.
- Lần compile đầu Unity bỏ sót một số source trong compilation list; refresh/reimport không đủ. MoveAsset `.cs` → tên tạm → tên cũ bằng AssetDatabase đăng ký lại script và compile đã pass. Đây là lỗi import phiên làm việc, không phải dependency code còn thiếu.
- Build Windows x64 đã thành công; đã chạy exe, log xác nhận D3D11 và không có exception, process phản hồi bình thường. Đã tắt process test.
- Chưa kiểm chứng thao tác chuột native drag/resize/click-through hoặc DPI/nhiều màn hình; không ghi các mục này là pass chỉ vì build thành công.
- Pipeline bị tắt trong Player (không có RuntimePipelineConfig) là chủ ý; package chỉ phục vụ Editor. Build có warning tương ứng, không phải lỗi gameplay.
- Runtime dùng integer pixel scale theo chiều cao và snap vị trí hiển thị 1/32 world unit; simulation vẫn giữ tọa độ float. HUD text cập nhật mỗi 0.25 giây ngoài Update.

## Quy trình cho agent tiếp theo

1. Đọc file này + AGENTS.md; xem task hiện tại thay vì khôi phục toàn bộ chat. Giới hạn tìm kiếm vào Assets/Game và Packages/manifest.json; không quét Library/Temp/Logs/Builds để lấy context.
2. `unity status --format json`; dùng command discovery khi cần, luôn chọn đúng project.
3. Sửa source rồi refresh/compile qua Editor; sửa scene/asset qua API Editor.
4. Chạy Verify Simulation nếu đổi logic; Play Mode để xem wiring/hiển thị; build Windows nếu đổi desktop.
5. Cập nhật file này: quyết định mới, file thêm/xóa/đổi vai trò, kiểm chứng và phần còn thiếu. Ghi kết quả ngắn, không lưu log thô.
