# Git checkpoint cho quay video

Remote: https://github.com/ondat2401/Taskbar_garden

Mỗi feature được kiểm tra và chốt bằng một commit cùng annotated tag. Tag đã dùng cho video không được dời/ghi đè; sửa tiếp thì thêm commit/checkpoint mới.

| Tag | Nội dung | Trạng thái |
|---|---|---|
| `checkpoint/00-import-wip` | Nhập project Unity hiện có, gồm bản nháp F01–F05 và art AI | Lưu nguyên trạng; không phải bản hoàn thiện các feature |
| `checkpoint/f01-economy` | Kho, vàng, bán tay/tự động; nối thu hoạch trong scene | Hoàn tất 2026-10-19; compile, self-check và Play Mode pass; chưa có standalone build mới |
| `checkpoint/f02-pot-themes` | Chọn cây riêng từng chậu, theme, sáu nâng cấp | `7d50e26`; kiểm tra logic và regression pass; UI chọn cây tiếp tục ở F03 |
| `checkpoint/f03-compact-shop` | Cửa sổ compact, workshop và tương tác chậu | `4e16846`; click thật, Shop resize, drag/pin/lock và regression Play Mode pass |
| `checkpoint/f04-chests-gear` | Rương theo thời gian, pity, gear và inventory nổi | Seeded checks + F01–F03 regression + Play Mode UI pass; chưa build standalone F04 |
| `checkpoint/f05-save-rebirth` | Save schema v2, migration v1, recovery và rebirth | Migration/I/O recovery verification và Play Mode rebirth pass; chưa kiểm standalone restart |
| `checkpoint/f06-prototype` | Cosmetic catalog, free equip, preview và Windows build | F01–F06 verifications, Play Mode tint/rollback và Windows BuildReport pass; standalone QA lưu schema v3 trong profile cô lập |
| `checkpoint/f06-worker-grounding` | Follow-up: grounding sprites, dry-only watering và lịch việc công bằng | F01–F06 Editor verifications, Play Mode và Game View pass; chưa build standalone lại |
| `checkpoint/f07-ui-polish` | Palette, viền, typography và trạng thái UI cho workshop/inventory/cosmetics | Compile, F01–F06 Editor verifications, Play Mode và Game View bốn UI pass; chưa build standalone riêng |

Thiết kế hiện tại: `docs/GAME_DESIGN.md`; tư liệu kể hành trình và gợi ý cảnh quay: [DEVLOG.md](DEVLOG.md). Checkpoint tiếp theo chưa được chọn.

## Mở checkpoint để quay mà không đụng thư mục đang làm

```powershell
git fetch origin --tags
git worktree add --detach "../Taskbar-garden-video-f01" checkpoint/f01-economy
```

Mở thư mục worktree mới trong Unity 6000.3.10f1. Scene: `Assets/Game/Scenes/GardenPrototype.unity`. Mỗi worktree có Library riêng nên lần import đầu mất thời gian. Không checkout/reset trong project Unity đang mở để quay phiên bản khác.

Tag import chứa source nháp của các feature sau nhưng chưa kích hoạt vào scene. Đừng giới thiệu nó như lịch sử từng feature đã hoàn thành. Repo không có source snapshot cũ hơn lúc bắt đầu đưa project lên Git; không dựng lại lịch sử giả từ build cũ.

## Sau mỗi feature

1. Hoàn tất kiểm tra phù hợp; ghi kết quả và hạn chế vào CHECKPOINT.md/AI_README.md.
2. Commit code, scene, prefab, config và `.meta` liên quan; giữ những thay đổi không thuộc feature riêng.
3. Tạo annotated tag `checkpoint/fNN-ten-feature`, ghi nội dung và kiểm chứng.
4. Push commit và đúng tag đó lên origin. Không force-push hay dời tag.

Không commit Library, Temp, Logs, Builds, UserSettings, file kết nối MCP, credential hoặc dữ liệu save cá nhân. Build executable không nằm trong tag; khi cần quay standalone hãy build từ đúng worktree/tag, đọc hạn chế trong checkpoint trước.

## Gợi ý cảnh quay F01

1. Mở scene GardenPrototype, mở `Tools > Idle Garden > Garden Config`, chọn tab Runtime sau khi Play.
2. Cho chạy ×5 để quay nhanh; chỉ chỉnh runtime cho buổi quay, không commit speed đã tăng.
3. Quay nhân vật tự chăm cây; kho giữ 4 mỗi loại, GOLD trên HUD tăng khi bán phần dư.
4. Bấm Toggle Auto-sell: quay hàng tăng trong kho, vàng không tăng.
5. Bấm Sell 1 để bán một cây hoặc Sell surplus để bán dư và giữ dự trữ. Quay thay đổi kho/vàng.
6. Dừng Play. F01 không có UI shop/upgrade; chúng thuộc checkpoint sau. Không dùng exe cũ để quay minh họa F01.

Project được đưa vào Git từ source nháp đang có (tag import). Các package MCP/NuGet đã có trong project được giữ lại; thay đổi tự sinh khi Unity import/recovery không thuộc commit feature nếu không cần cho feature đó.
