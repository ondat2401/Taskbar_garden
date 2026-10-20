# Checkpoint — Idle Garden

Cập nhật 2026-10-20. F05 save/rebirth đã tích hợp và kiểm tra; tiếp tục F06 cosmetic preview/build. User cho phép tự làm, test và commit từng giai đoạn.

## Đã chốt

- F01: `8cb0f89`, tag `checkpoint/f01-economy`. Kho/vàng/bán; transaction, simulation và Play Mode pass.
- F02: `7d50e26`, tag `checkpoint/f02-pot-themes`. Cây chọn riêng từng chậu, đổi giữa vụ đợi thu hoạch, ba theme/bốn cây/sáu nâng cấp. Pots(), economy và simulation regression pass.
- F03: `4e16846`, tag `checkpoint/f03-compact-shop`. Windows build nhận click Shop/menu, bung/thu 920×180 ↔ 920×540, pin/lock, kéo khi mở khóa và resize. Play Mode xác nhận menu đóng Shop trước khi mở; F01/F02 regression pass.
- F04: tag `checkpoint/f04-chests-gear`. Rương thời gian/pity, rarity/affix, inventory/equip/salvage/essence và cửa sổ nổi đã vào scene. Seeded verification, F01–F03 regression và Play Mode UI pass. Chưa chạy standalone F04; chưa kiểm DPI/multi-monitor.
- F05: tag `checkpoint/f05-save-rebirth`. Save schema v2/migration v1, atomic replace/backup recovery, bảo toàn file tương lai/lỗi và rebirth giữ lựa chọn cây cùng tiến trình vĩnh viễn. F01–F05 verifier và Play Mode rebirth pass; I/O dùng thư mục tạm cô lập, chưa restart standalone.
- Thiết kế mới: `docs/GAME_DESIGN.md`. Tiến độ: `docs/FEATURE_STATUS.md`. Hành trình để dựng video: `docs/DEVLOG.md`; quy trình tag: `docs/GIT_CHECKPOINTS.md`.

## Đã kiểm tra F03

Garden Bar ở mép trên; menu mở xuống dưới, workshop chừa khoảng thanh. Windows build nhận click thật; Shop bung/thu cửa sổ 920×180 ↔ 920×540. Pin/lock, drag khi mở khóa và resize phản hồi. Menu đóng Shop khi mở; Play Mode xác nhận state và ảnh menu sau sửa. F01–F02 verification cũng chạy lại pass. Chưa kiểm DPI hoặc nhiều màn hình.

Native test dùng save hiện có. UI ban đầu hiển thị 536 vàng; vườn chạy idle trong lúc test và save hiện tại được giữ nguyên. Build kiểm tra: `Builds/Windows/F03-native-test/`.

## Đã kiểm tra F04

Rương mỗi600 giây unpaused real-time (không theo simulation speed/offline), pity lần mở thứ20, 40 slot, gear base + affix không trùng, equip theo slot, salvage không cho đồ đang mặc, essence đổi rương. HUD có 40 row dựng sẵn bằng Editor; `GardenFloatingWindow` giữ khung/đóng/kéo, `GardenGearPanel` gọi API progression và `GardenProgression` sở hữu luật/state. Shop/menu/gear loại trừ nhau.

Đã chạy compile; `GardenVerification.Run()`, `GardenEconomyVerification.Run()`, `GardenFeatureVerification.Pots()` và `GardenGearVerification.Run()` pass. Play Mode kiểm tra mở rương qua UnityEvent, chọn row, equip, chặn salvage đồ đang mặc, đóng cửa sổ, Shop/menu chuyển trạng thái và drag không vượt Canvas. Game View 510×287 đã xem. F04 không ghi persistent save trong Editor; chưa kiểm file I/O hoặc native Windows build.

Sau F05 là F06 cosmetic preview và build.

## Đã kiểm tra F05

`GardenSaveVerification.Run()` xác nhận migration save F01 v1 sang v2 (cây chọn kế tiếp mặc định cây đã lưu), save/replace nhiều lần, backup của bản hợp lệ trước, fallback khi primary lỗi và giữ bản lỗi khi phục hồi. File version tương lai hoặc save lỗi không thể bị ghi đè. Save sau rebirth được đọc lại bằng store mới từ đĩa tạm.

Đã chạy compile; `GardenVerification.Run()`, `GardenEconomyVerification.Run()`, `GardenFeatureVerification.Pots()`, `GardenGearVerification.Run()` và `GardenSaveVerification.Run()` pass. Play Mode gọi `GardenController.Rebirth()` thật: seed tăng, cây đang trồng reset; lựa chọn từng chậu, theme, gear đã mặc và rương được giữ. Chưa chạy standalone restart hoặc ghi vào save người chơi.

## Giữ nguyên

Không stage/revert NuGet importer meta và Assets/_Recovery của user. Screenshot QA trong Assets/Temp là file tạm; không đưa vào commit. Không sửa YAML scene; dùng Unity CLI/Pipeline. Editor không ghi save người chơi. Không khẳng định thao tác native pass từ compile/ảnh Editor.
