# Tiến độ design v2

Thiết kế chuẩn: [GAME_DESIGN.md](GAME_DESIGN.md). Kế hoạch cũ trong CHECKPOINT.md là lịch sử baseline F01; bảng này theo thay đổi mới của user.

| Checkpoint | Trạng thái | Bằng chứng |
|---|---|---|
| F01 economy | Đã push | 8cb0f89, transaction + simulation + Play Mode |
| F02 pot/themes/upgrades | Đã kiểm tra | Compile; Pots(), Verify Economy F01, Verify Simulation pass. Đổi giữa vụ giữ cây, không ảnh hưởng chậu khác, theme không trừ tiền/cho rương hai lần; mua rank tính đúng giá và không mutate base data. 6 rank upgrades, 3 theme, 4 crop. |
| F03 compact shop | Đang làm | Chưa được tính là pass |
| F04 chests/gear | Chờ | — |
| F05 save/rebirth | Chờ | — |
| F06 cosmetic/build | Chờ | — |

AI assets: `theme-growth-ai.png` (built-in Imagegen, 2026-10-19), 3×2 fire/frost growth stages, Point/uncompressed, PPU400, pivot(.5,.1). Draft có glow; chưa gọi là strict32px palette. `garden-icons-ai.png` là atlas4 icon đã có từ baseline.
