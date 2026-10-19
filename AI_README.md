# AI handoff — Idle Garden

Đọc CHECKPOINT.md trước. Trạng thái F01/F02 đã chốt, F03 đang kiểm tra; thiết kế chuẩn docs/GAME_DESIGN.md, tiến độ docs/FEATURE_STATUS.md, hành trình docs/DEVLOG.md. Sau mỗi feature cập nhật hiện trạng và ghi dev log + commit/tag. Không nối nhật ký dài vào file này.

## Project

Unity 6000.3.10f1, URP17.3, InputSystem1.18, uGUI2; Windows desktop qua UniWindowController. Scene Assets/Game/Scenes/GardenPrototype.unity. Profile Assets/Game/_Configs/Garden/Prototype.asset → GardenEconomy.asset. Assets thuộc Assets/Game; scripts chia feature. Packages MCP của user giữ nguyên.

Player chọn cây cho từng chậu, nhân vật tự trồng/tưới/thu hoạch lặp lại. Đổi giữa vụ đợi cây cũ xong. Ba theme/bốn cây/sáu nâng cấp hiện có. Kho giữ reserve4 và tự bán dư, mở theme thưởng một rương. Rương timed/rarity affix chưa triển khai; save/rebirth còn cần hoàn thiện và kiểm tra. Multiplayer/Steam commerce chưa làm. Thiết kế IAP cosmetic, chưa có purchase thật.

## Code liên quan

- Garden/GardenSimulation.cs: state/job, chọn cây, Capture/Restore. GardenController.cs: tick, camera, HUD, autosave standalone; Editor không ghi save.
- Progression/GardenProgression.cs: inventory/gold/rank/theme/chests/rebirth. ProgressionConfig.cs và UpgradeDefinition.cs: data. GardenSaveStore.cs: DTO/version1, I/O nháp chưa test đầy đủ.
- Crops/CropDefinition.cs, GardenTheme.cs: cây và theme. Worker/WorkerConfig.cs: chỉ số gốc.
- UI/GardenHud.cs: callback chậu/cây/theme/tab/shop. ShopUpgradeRow.cs: nâng cấp. Garden/PlotView.cs: sprite và click chậu.
- Desktop/DesktopWindow.cs, WindowHandle.cs: native drag/resize/menu, Windows standalone.
- Editor/GardenConfig/: GardenPrototypeBuilder, GardenProgressionSetup dựng scene/HUD; GardenContentSetup tạo catalog; GardenThemeArt nối atlas; GardenConfigWindow chỉnh data; GardenVerification/GardenEconomyVerification/GardenFeatureVerification kiểm tra.

## Verification

F01: 10k giao dịch, giới hạn số học, 1200s sim và Play Mode pass. F02: Pots() + economy + simulation regression pass, mua sáu upgrade không mutate base. Regression 4/8/12 ô từng chạy pass. F03: compile pass; ảnh workshop 920×540 đúng; callback chọn cây/chuyển chậu/tab/đóng pass trong Play mới. Native và build mới chưa verify; executable cũ chỉ có khung.

## Workflow

unity status --format json; unity command recompile_status --format json. Eval qua live Editor để thao tác asset/scene, không sửa YAML. GardenVerification.Run(), GardenEconomyVerification.Run(), GardenFeatureVerification.Pots() là entry test hiện có trong namespace IdleGarden.Editor. Nếu script mới không vào compile list, AssetDatabase MoveAsset sang tên tạm rồi về có thể đăng ký lại; không reset Library/package.

Giữ nguyên thay đổi NuGet meta/Assets/_Recovery ngoài feature. Assets/Temp chứa screenshot QA tạm, không stage. Không claim test chưa chạy. Build output Builds/Windows/IdleGarden.exe bị ignore; user muốn tag từng giai đoạn để quay video, không dời tag đã push.
