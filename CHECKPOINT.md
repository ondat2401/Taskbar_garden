# Checkpoint — progression + compact window

Cập nhật: 2026-10-19. **F01 hoàn tất**; checkpoint kế tiếp là F02. Chốt feature bằng Git commit + annotated tag theo yêu cầu user để có thể quay video.

## Trạng thái chính xác

- Source hiện tại **compile pass** trong Unity 6000.3.10f1.
- Scene đã nối **economy F01** qua `Prototype.asset → GardenEconomy.asset`, catalog 2 cây, starting gold30, reserve4, upgrades rỗng. Chưa gọi `GardenProgressionSetup.Apply()`, chưa thay HUD thành compact shop.
- HUD cũ hiển thị GOLD; tab Runtime trong Garden Config có stock/gold/earned, Toggle Auto-sell, Sell surplus và Sell 1 từng cây.
- `Builds/Windows/IdleGarden.exe` vẫn là **build cũ**. Không mô tả build này đã có shop/nâng cấp.
- Unity đã thoát Play Mode để compile. Không có process game test mới.
- Đã nối Git với https://github.com/ondat2401/Taskbar_garden, giữ lịch sử remote cũ. Tag `checkpoint/00-import-wip` lưu nguyên trạng source nháp trước khi hoàn tất F01; tag `checkpoint/f01-economy` chốt F01. Quy trình/shot list: `docs/GIT_CHECKPOINTS.md`.

## Yêu cầu đã được user chốt

- Game idle: các hệ thống kết nối để có tiến trình lâu dài, tham khảo tinh thần nâng cấp nhiều lớp của Hades II.
- Tiếp tục từ prototype; cửa sổ giống một dải taskbar, đơn giản và ít nút.
- Data chỉnh bằng Editor, số ô không hardcode; tuân thủ AGENTS.md.
- Có thể chuẩn bị asset trước. Đã chọn 4 icon AI cho shop, giữ sprite cây/nhân vật cũ.
- **Chia feature và checkpoint sau từng phần; không triển khai tất cả trong một lượt dài.**

## Các feature và thứ tự tiếp tục

| ID | Phần việc | Hiện trạng | Điều kiện checkpoint hoàn thành |
|---|---|---|---|
| F01 | Kho, vàng, bán nông sản | **Hoàn tất, có kiểm tra lặp lại + Play Mode** | 10.000 giao dịch, giới hạn số học, 1.200 giây sim, thu hoạch/bán/tích trữ trong scene pass |
| F02 | Nâng cấp cây/công cụ bằng vàng + nông sản | Đã có code + smoke test | Tạo config/6 upgrade asset, kiểm tra mua thành công/thất bại, hiệu ứng lên simulation, không sửa base asset |
| F03 | Cửa sổ gọn + shop + icon | Đã có code và PNG; chưa apply scene | Slice/import 4 sprite, áp dụng HUD, chụp/kiểm tra UI ở 920×180 và shop mở 920×460; sửa layout; test kéo/resize/pin/lock/menu |
| F04 | Lưu tiến trình | Đã có code; chưa test I/O | Test file round-trip, backup, file hỏng/newer version, thoát/mở app; không ghi vào save user khi test |
| F05 | Tái sinh lặp lại | Đã có code + smoke test | Xác nhận reset/giữ đúng, bấm xác nhận hai bước, reward không nhận hai lần, chu kỳ 2 tiếp tục và bonus còn sau reload |
| F06 | Bàn giao build mới | Chưa làm | Regression 4/8/12 ô + Play Mode + Windows build + cập nhật README/AI_README |

**Bước tiếp theo:** F02. Config economy đã tồn tại với `upgrades = []`; `GardenProgressionSetup.EnsureConfig` hiện return sớm khi profile có config, nên F02 cần thêm các upgrade vào config hiện hữu mà không ghi đè giá trị user chỉnh. Sau F02 ghi commit/tag riêng, rồi F03. F04/F05 chỉ mới là bản nháp.

## Code đã viết

Các đường dẫn dưới `Assets/Game/Scripts/`:

- `Progression/UpgradeDefinition.cs`: ID, stat, bonus/rank, giá vàng tăng theo cấp, nguyên liệu, crop mục tiêu, icon.
- `Progression/ProgressionConfig.cs`: catalog cây/upgrade, vàng đầu, auto-sell reserve, goal tái sinh và bonus hạt; validation.
- `Progression/GardenProgression.cs`: kho theo crop ID, cấp theo upgrade ID, bán, mua, bonus, tái sinh, capture/restore state.
- `Progression/GardenSaveStore.cs`: DTO version 1, JSON, temp+flush+File.Replace/backup; giữ file hỏng và chặn overwrite file từ version mới.
- `UI/GardenHud.cs`, `UI/ShopUpgradeRow.cs`: UI callbacks và refresh mỗi 0.25s qua controller, không tạo object runtime.
- `Editor/GardenConfig/GardenProgressionSetup.cs`: EnsureConfig, slice atlas, dựng HUD/shop bằng Editor, menu Apply Compact Garden and Progression.
- `Editor/GardenConfig/GardenEconomySetup.cs`: gắn/tạo config chỉ kho–vàng F01, không dựng lại HUD/scene hay tạo upgrade.
- `Editor/GardenConfig/GardenEconomyVerification.cs`: menu Verify Economy F01, self-check có cleanup asset tạm, chạy lại được.

Đã chỉnh:

- `CropDefinition`: thêm `sellPrice` (default 8).
- `GardenProfile`: optional `progression`, `showPlotMeters`, default window 920×180/min560×150; validate catalog.
- `GardenSimulation`: constructor giữ tương thích (`progression = null`); harvest chuyển vào kho, nâng cấp tác động chỉ số tính toán, CaptureInto/Restore.
- `GardenController`: tạo progression khi được gán, save autosave/quit trong standalone; Editor chủ ý không lưu; viewport dành phần dưới cho vườn khi shop mở.
- `DesktopWindow`: SetPanelExpanded mở cửa sổ cho shop, thu lại khi đóng; không thêm dependency lookup.
- `GardenPrototypeBuilder`: BuildHud chuyển sang builder mới; ẩn bar từng ô qua flag; build size lấy profile.
- `GardenConfigWindow`: tab Progression, giá cây, các trường config mới.

## Thiết kế hiện tại (được tự chọn trong phạm vi user cho phép)

- Auto-sell giữ 4 mỗi loại làm nguyên liệu, bán phần dư. Có toggle để tích trữ khi chi phí nguyên liệu vượt dự trữ.
- 6 upgrade: carrot growth, global yield, tank capacity, walking, action speed, water efficiency.
- Nâng cấp cộng bonus theo rank lên chỉ số base; chi phí vàng tăng mũ. Sản lượng nguyên được làm tròn; demo yield bonus đặt 100% để cấp đầu thấy ngay.
- Tái sinh: đủ tổng vàng kiếm trong chu kỳ, nhận 1 seed; reset vàng/kho/rank/cây, giữ seed. Seed tăng vàng bán; goal chu kỳ tăng.
- UI gọn: một thanh dưới chứa vàng/seeds, Shop, menu `:` và grip resize. Menu chứa pin/lock/pause/exit. Shop mở thì chiều cao native tăng lên ít nhất460; đóng khôi phục size.
- Chưa thêm chế biến, đơn hàng, bùa/Arcana loadout, mở giống hay multiplayer.

## Asset đã tạo

- `Assets/Game/Textures/UI/garden-icons-ai.png`: atlas 2×2, icon watering / seeds / harvest / rebirth. Đã copy vào project; **chưa slice/import qua setup**.
- Nguồn: built-in Imagegen, 2026-10-18. Prompt: 2×2 transparent inventory atlas, teal/brass watering can, tan carrot seed packet, produce crate, golden sprouting rebirth seed; cozy muted pixel-art style, no text, no scene.
- Bản gốc giữ ở `C:/Users/ADMIN/.codex/generated_images/01a14ecb-ae87-7340-8dbe-66407b5cc963/exec-293a83d0-3328-4876-bcb6-1d179bb57468.png`.
- Đã xem ảnh: bố cục 4 icon đúng. Cần kiểm tra alpha/padding và độ đọc khi hiển thị40px; đây là art draft AI, không khẳng định true pixel grid đã QA.

## Kiểm chứng đã thực sự chạy

- `unity command recompile_status --format json`: completed, failed=false.
- `GardenVerification.Run()` chạy lại pass (4/8/12 ô, refill, pause, dry growth, numeric config).
- Smoke test in-memory trong live Editor, dùng ScriptableObject tạm và xóa ở finally:
  - Starting gold30, giá cây10, harvest6 → stock4, gold50.
  - Mua upgrade giá10 + 1 cây → gold40, stock3, rank1.
  - Tái sinh đủ goal20 → seed1, rank0, stock0, bonus vàng>1.
  - Capture → GardenSaveStore.Valid → tạo progression từ DTO: seed/gold khớp.
- F01 hoàn tất 2026-10-19: compile pass; menu Verify Economy F01 và Verify Simulation pass. Kiểm tra null/ngoài catalog, bán số âm/quá số lượng/stock trống, reserve0 và reserve âm, 10.000 giao dịch, long.MaxValue stock và double.MaxValue gold. Giao dịch không biểu diễn được khoản cộng vàng bị từ chối nguyên vẹn, không trừ hàng.
- Simulation nối economy chạy 24.000 tick × .05s = 1.200 giây, bảo toàn lượng nông sản và doanh thu pass.
- Play Mode scene thật tại ×5: snapshot harvest16, gold94, earned64, stock[4,4], upgrades0; bán tay pass; tắt auto-sell rồi thu hoạch thêm giữ nguyên gold; bán phần tích trữ11 cây nhận88 vàng pass. Cuối lượt harvest38/gold270. Console không error trong lượt kiểm tra. Đã thoát Play và khôi phục speed ban đầu; không tạo save hay debug object trong scene.
- **Chưa test file I/O, simulation có upgrade trong scene, shop UI, native window mới hoặc build mới.** File exe cũ chưa có F01.

## Điểm cần rà soát trước khi coi là hoàn tất

- GardenProgressionSetup chỉ mới compile; chưa chạy slicing/UI generation. Kiểm tra Image/Text anchors, scroll, thông báo rebirth và màn hình hẹp.
- Editor chơi ở Game View cao: shop không tự đổi kích thước Editor như native; cần đặt resolution test 920×460 để QA.
- Khoảng cách/giá trị config giữ live, nhưng thay catalog/upgrade list phải ngoài Play và rebuild shop. Cần disable structural fields trong Play cho rõ.
- Save validation cần test NaN/Infinity các config float, numeric overflow, malformed/missing fields, future version và backup.
- Logic có giới hạn số học double/int và clamp tốc độ/sản lượng để tránh tràn; đây không phải lời hứa tiến trình toán học vô hạn.
- Pin/Lock state label có code nhưng chưa được nhìn/thao tác trong build mới.

## Lệnh tiếp tục

```powershell
unity status --format json
unity command recompile_status --format json
unity command eval 'IdleGarden.Editor.GardenVerification.Run(); return "PASS";' --format json
unity command eval 'IdleGarden.Editor.GardenEconomyVerification.Run(); return "PASS F01";' --format json
# Chỉ gọi sau khi đã kiểm tra scene/profile và muốn áp dụng F03:
unity command eval 'IdleGarden.Editor.GardenProgressionSetup.Apply(); return "applied";' --format json
```

Sửa scene/asset qua Editor API; không sửa YAML. Nếu script mới không vào compilation list, workaround đã dùng: AssetDatabase.MoveAsset file `.cs` sang tên tạm rồi về lại, Refresh; kiểm tra lại source và compile. Đừng reset Library hoặc package của user.
