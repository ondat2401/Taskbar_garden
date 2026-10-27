# Tiến độ design v2

Thiết kế chuẩn: [GAME_DESIGN.md](GAME_DESIGN.md). Kế hoạch cũ trong CHECKPOINT.md là lịch sử baseline F01; bảng này theo thay đổi mới của user.

| Checkpoint | Trạng thái | Bằng chứng |
|---|---|---|
| F01 economy | Đã push | 8cb0f89, transaction + simulation + Play Mode |
| F02 pot/themes/upgrades | Đã kiểm tra | Compile; Pots(), Verify Economy F01, Verify Simulation pass. Đổi giữa vụ giữ cây, không ảnh hưởng chậu khác, theme không trừ tiền/cho rương hai lần; mua rank tính đúng giá và không mutate base data. 6 rank upgrades, 3 theme, 4 crop. |
| F03 compact shop | Đã push | `4e16846`, `checkpoint/f03-compact-shop`; Windows build: click Shop/menu, bung/thu 920×180 ↔ 920×540, pin, lock, drag và resize; Play Mode regression pass. Chưa kiểm DPI/multi-monitor. |
| F04 chests/gear | Đã kiểm tra | Timer/pity/drop/affix/equip/salvage/essence seeded checks pass; scene Play Mode kiểm listener và cửa sổ nổi. Chưa chạy standalone hoặc kiểm DPI. |
| F05 save/rebirth | Đã kiểm tra | `checkpoint/f05-save-rebirth`; schema v2 migrate v1, atomic repeated-save/backup recovery, chặn save tương lai/lỗi; isolated disk round-trip và Play Mode rebirth giữ gear/theme/rương/lựa chọn cây, reset vàng/stock/rank/vụ đang trồng. Chưa kiểm standalone restart. |
| F06 cosmetic/build | Đã push | `checkpoint/f06-prototype`; schema v3 migrate v1/v2; ba cosmetic miễn phí equip/lưu, hai premium và một bundle chỉ preview; F01–F06 verification và Play Mode tint/rollback/save capture pass; Windows BuildReport Succeeded, standalone QA ghi save v3 trong profile cô lập. Chưa kiểm restart sau equip, DPI/multi-monitor hoặc gamepad. |
| F06 follow-up: grounding/watering | Đã kiểm tra | `checkpoint/f06-worker-grounding`; worker/well/crop đặt đúng baseline, worker vẽ sau cây; tưới khi khô, trồng/tưới luân phiên và quay vòng chậu; F01–F06 Editor verification, Play Mode và Game View pass. Chưa build standalone lại. |
| F07 UI polish | Đã kiểm tra | `checkpoint/f07-ui-polish`; QA follow-up `checkpoint/f07-cosmetic-label-fix` sửa sáu nhãn offer cao 0px thành 40px. Play Mode xác nhận nhãn, chọn dòng/đóng cửa sổ và 78 nút theo palette; F01–F06 Editor verifications pass. Chưa build standalone hoặc kiểm DPI/multi-monitor/gamepad. |
| F08 shared art palette | Đã kiểm tra | `checkpoint/f08-shared-art-palette`; atlas 19 sprite cho worker, giếng, ground và 4 cây/3 giai đoạn, UI icon và world art dùng chung 22 màu; scene/prefab/config đã nối sprite, player 4 > soil 2, crop 5. Compile, mapping/theme setup, F01–F06 Editor verifications và Play Mode/Game View Upgrades pass. Chưa build standalone hoặc kiểm DPI/multi-monitor/gamepad. |
| F09 art pass | Đã tích hợp | `checkpoint/f09-art-pass`; sibling atlas polished v1, 19 slice, 22 màu và alpha/silhouette gốc; config/prefab/scene và Garden Config lookup đã trỏ art mới. Profile validation, F01–F06 verifications, Game View 920×180 và Windows build pass. Vẫn là placeholder. |
| F10 game feel | Đã kiểm tra | Nhãn vàng pulse 12% trong 0.24 giây khi vàng tăng, rồi về scale ban đầu. Play Mode self-check: 30→158 vàng, scale 1→1.12→1; không thêm object hay allocation mỗi frame. |
| F11 final UI | Đã kiểm tra | Năm mục menu (Always on top, Position lock, Pause, Exit, Cosmetics) co về 24px trong panel cao 140px; Cosmetics không còn nằm dưới mép ở cửa sổ 920×180. Standalone mở/đóng Cosmetics, Shop/Upgrades và Inventory. |
| F12 standalone QA | Đã kiểm tra | Windows QA build thành công; F01–F06 Editor verifications pass. Standalone 920×180 hiển thị đủ menu, mở/đóng Cosmetics và mở Shop/Upgrades; Inventory/Gear đã smoke-test trước. Save v3 trong profile QA riêng còn sau restart (gold 86). |
| F13 art assets | Đã tích hợp | `checkpoint/f13-art-assets`; palette 8×8/64 màu; texture tĩnh tách riêng, worker 4 frame và bốn crop sheet 6 frame; scene/prefab/config đã remap. Raster assert, compile, profile validation, Game View 920×180 và runtime sway pass. |
| F14 weather sync | Đã tích hợp, kiểm tra | Open-Meteo current temperature + WMO weather code; cấu hình tọa độ trong `Assets/Game/_Configs/Garden/Weather.asset` (mặc định HCMC), refresh 15 phút, attribution trên HUD. Mapper/parser/URL self-check, Play Mode API thật, Game View và Windows QA build pass. Chỉ hiển thị trạng thái; chưa ảnh hưởng gameplay. API miễn phí phi thương mại, cần license cho bản thương mại. |
| F15 art layout | Đã tích hợp, kiểm tra | Crop giảm còn 2/3, alpha base căn đúng mặt đất qua QA follow-up `checkpoint/f15-crop-grounding-fix`; giếng giảm 2/3, đứng cạnh worker; camera giữ giếng trong khung. Sorting grass 0, soil 2, well 3, worker 4, crop 5. Compile và Play Mode/Game View pass; chưa build Windows riêng. |

F01–F15 đã được tích hợp và kiểm tra theo phạm vi ghi ở đây. F13 art là placeholder pixel đã được duyệt; weather chưa có hiệu ứng hay ảnh hưởng gameplay. DPI, multi-monitor và gamepad chưa được kiểm tra.

AI draft/source texture cũ được giữ trong `Textures/Legacy/`; runtime/config hiện dùng từng PNG tĩnh riêng hoặc sheet animation riêng cho từng asset có chuyển động. Nguồn, chuẩn hóa và prompt ở [PLACEHOLDER_ART.md](PLACEHOLDER_ART.md).
