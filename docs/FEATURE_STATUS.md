# Tiến độ design v2

Thiết kế chuẩn: [GAME_DESIGN.md](GAME_DESIGN.md). Kế hoạch cũ trong CHECKPOINT.md là lịch sử baseline F01; bảng này theo thay đổi mới của user.

| Checkpoint | Trạng thái | Bằng chứng |
|---|---|---|
| F01 economy | Đã push | 8cb0f89, transaction + simulation + Play Mode |
| F02 pot/themes/upgrades | Đã kiểm tra | Compile; Pots(), Verify Economy F01, Verify Simulation pass. Đổi giữa vụ giữ cây, không ảnh hưởng chậu khác, theme không trừ tiền/cho rương hai lần; mua rank tính đúng giá và không mutate base data. 6 rank upgrades, 3 theme, 4 crop. |
| F03 compact shop | Đã push | `4e16846`, `checkpoint/f03-compact-shop`; Windows build: click Shop/menu, bung/thu 920×180 ↔ 920×540, pin, lock, drag và resize; Play Mode regression pass. Chưa kiểm DPI/multi-monitor. |
| F04 chests/gear | Đã kiểm tra | Timer/pity/drop/affix/equip/salvage/essence seeded checks pass; scene Play Mode kiểm listener và cửa sổ nổi. Chưa chạy standalone hoặc kiểm DPI. |
| F05 save/rebirth | Đã kiểm tra | `checkpoint/f05-save-rebirth`; schema v2 migrate v1, atomic repeated-save/backup recovery, chặn save tương lai/lỗi; isolated disk round-trip và Play Mode rebirth giữ gear/theme/rương/lựa chọn cây, reset vàng/stock/rank/vụ đang trồng. Chưa kiểm standalone restart. |
| F06 cosmetic/build | Tiếp theo | — |

AI assets: `theme-growth-ai.png` (built-in Imagegen, 2026-10-19), 3×2 fire/frost growth stages, Point/uncompressed, PPU400, pivot(.5,.1). Draft có glow; chưa gọi là strict32px palette. `garden-icons-ai.png` là atlas4 icon đã có từ baseline.
