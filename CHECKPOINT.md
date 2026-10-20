# Checkpoint — Idle Garden

Cập nhật 2026-10-20. F03 đã kiểm tra; tiếp tục F04. User cho phép tự làm, test và commit từng giai đoạn.

## Đã chốt

- F01: `8cb0f89`, tag `checkpoint/f01-economy`. Kho/vàng/bán; transaction, simulation và Play Mode pass.
- F02: `7d50e26`, tag `checkpoint/f02-pot-themes`. Cây chọn riêng từng chậu, đổi giữa vụ đợi thu hoạch, ba theme/bốn cây/sáu nâng cấp. Pots(), economy và simulation regression pass.
- F03: tag `checkpoint/f03-compact-shop` sẽ ghi sau commit. Windows build nhận click Shop/menu, bung/thu 920×180 ↔ 920×540, pin/lock, kéo khi mở khóa và resize. Play Mode xác nhận menu đóng Shop trước khi mở; F01/F02 regression pass.
- Thiết kế mới: `docs/GAME_DESIGN.md`. Tiến độ: `docs/FEATURE_STATUS.md`. Hành trình để dựng video: `docs/DEVLOG.md`; quy trình tag: `docs/GIT_CHECKPOINTS.md`.

## Đã kiểm tra F03

Garden Bar ở mép trên; menu mở xuống dưới, workshop chừa khoảng thanh. Windows build nhận click thật; Shop bung/thu cửa sổ 920×180 ↔ 920×540. Pin/lock, drag khi mở khóa và resize phản hồi. Menu đóng Shop khi mở; Play Mode xác nhận state và ảnh menu sau sửa. F01–F02 verification cũng chạy lại pass. Chưa kiểm DPI hoặc nhiều màn hình.

Native test dùng save hiện có. UI ban đầu hiển thị 536 vàng; vườn chạy idle trong lúc test và save hiện tại được giữ nguyên. Build kiểm tra: `Builds/Windows/F03-native-test/`.

## Đang làm F04

Rương theo thời gian/mốc, rarity affix, inventory/equip/salvage/essence. Cửa sổ inventory nổi được tạo sẵn trong Canvas qua Editor tooling, có tiêu đề/đóng/kéo và reference rõ ràng; không spawn trong runtime. Cập nhật docs sau khi tích hợp. Sau F04 là F05 save/rebirth, F06 cosmetic preview và build. Không coi code save/rebirth nháp là đã test I/O.

## Giữ nguyên

Không stage/revert NuGet importer meta và Assets/_Recovery của user. Screenshot QA trong Assets/Temp là file tạm; không đưa vào commit. Không sửa YAML scene; dùng Unity CLI/Pipeline. Editor không ghi save người chơi. Không khẳng định thao tác native pass từ compile/ảnh Editor.
