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
| F07 UI polish | Đã kiểm tra | `checkpoint/f07-ui-polish`; palette dùng chung, viền mảnh, chữ/accent phân cấp, hover/pressed/disabled/selected; áp dụng workshop, inventory, cosmetics. Compile, F01–F06 Editor verifications và Play Mode/Game View bốn UI pass. Chưa build standalone riêng hoặc kiểm DPI/multi-monitor/gamepad. |
| F08 shared art palette | Đã kiểm tra | `checkpoint/f08-shared-art-palette`; atlas 19 sprite cho worker, giếng, ground và 4 cây/3 giai đoạn, UI icon và world art dùng chung 22 màu; scene/prefab/config đã nối sprite, player 4 > soil 2, crop 5. Compile, mapping/theme setup, F01–F06 Editor verifications và Play Mode/Game View Upgrades pass. Chưa build standalone hoặc kiểm DPI/multi-monitor/gamepad. |

Checkpoint tiếp theo chưa được chọn.

AI assets: `theme-growth-ai.png` còn trong project nhưng prototype dùng `Textures/Palette/garden-placeholder-atlas.png`; nguồn và prompt ở [PLACEHOLDER_ART.md](PLACEHOLDER_ART.md). `garden-icons-ai.png` được remap về tập màu chung.
