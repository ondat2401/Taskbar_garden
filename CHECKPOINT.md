# Checkpoint — Idle Garden

Cập nhật 2026-10-19. Tiếp tục F03; user cho phép tự làm, test và commit từng giai đoạn.

## Đã chốt

- F01: `8cb0f89`, tag `checkpoint/f01-economy`. Kho/vàng/bán; transaction, simulation và Play Mode pass.
- F02: `7d50e26`, tag `checkpoint/f02-pot-themes`. Cây chọn riêng từng chậu, đổi giữa vụ đợi thu hoạch, ba theme/bốn cây/sáu nâng cấp. Pots(), economy và simulation regression pass.
- Thiết kế mới: `docs/GAME_DESIGN.md`. Tiến độ: `docs/FEATURE_STATUS.md`. Hành trình để dựng video: `docs/DEVLOG.md`; quy trình tag: `docs/GIT_CHECKPOINTS.md`.

## Đang làm F03

Scene đã áp dụng compact HUD/workshop, tab chậu/nâng cấp, collider và callback chậu; sprite lửa/băng đã nối. Source compile pass. Play mới ngày 2026-10-19: ảnh workshop 920×540 đúng; callback chọn cây, chuyển chậu, đổi tab, đóng panel pass. Chưa kiểm chứng click chuột vật lý lên chậu, native drag/resize/pin/lock và standalone mới. Chưa tag F03.

Bước tiếp: kiểm tra UI hẹp, click chậu và native window; chốt F03 theo đúng phạm vi đã verify. Sau đó F04 rương/trang bị, F05 save/tái sinh, F06 cosmetic preview và build. Không coi code save/rebirth nháp là đã test I/O. Exe hiện có là build khung cũ.

## Giữ nguyên

Không stage/revert NuGet importer meta và Assets/_Recovery của user. Screenshot QA trong Assets/Temp là file tạm; không đưa vào commit. Không sửa YAML scene; dùng Unity CLI/Pipeline. Editor không ghi save người chơi. Không khẳng định thao tác native pass từ compile/ảnh Editor.
