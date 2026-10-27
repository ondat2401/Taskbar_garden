# AI handoff — Idle Garden

Đọc CHECKPOINT.md trước. F01–F12 đã tích hợp/QA; F13 đang chờ đánh giá texture mẫu theo palette 8×8 (64 màu), chưa nối vào scene; atlas active vẫn là `garden-art-polished-v1.png` (19 slices/22 màu). F14 weather sync được xếp sau phase asset. Tiến độ ở docs/FEATURE_STATUS.md, hành trình ở docs/DEVLOG.md. Sau mỗi feature cập nhật hiện trạng, dev log và commit/tag; không nối nhật ký dài vào file này.

## Project

Unity 6000.3.10f1, URP17.3, InputSystem1.18, uGUI2; Windows desktop qua UniWindowController. Scene Assets/Game/Scenes/GardenPrototype.unity. Profile Assets/Game/_Configs/Garden/Prototype.asset → GardenEconomy.asset. Assets thuộc Assets/Game; scripts chia feature. Packages MCP của user giữ nguyên.

Player chọn cây cho từng chậu, nhân vật tự trồng/tưới/thu hoạch lặp lại. Đổi giữa vụ đợi cây cũ xong. Ba theme/bốn cây/sáu nâng cấp; rương timed, pity, gear/affix, equip/salvage và cosmetic preview đã tích hợp. Save schema v3 migrate v1/v2, ghi temp/replace/backup, chặn file lỗi/tương lai; rebirth giữ lựa chọn cây, cosmetic và tiến trình vĩnh viễn. Multiplayer/Steam commerce chưa làm; prototype không có checkout hoặc entitlement premium.

## Code liên quan

- Garden/GardenSimulation.cs: state/job, dry-only watering, luân phiên việc trồng/tưới và quay vòng chậu để tránh bỏ đói chậu xa, chọn cây, Capture/Restore. GardenController.cs: tick, camera, HUD, autosave standalone; Editor không ghi save.
- Progression/GardenProgression.cs: economy/theme/chests/gear/rebirth. GearConfig.cs và EquipmentDefinition.cs: luật drop và base stats. GardenSaveStore.cs: DTO version3, migrate v1/v2, atomic replacement và backup recovery.
- Cosmetics/GardenCosmeticCatalog.cs và GardenCosmeticController.cs: catalog, loadout miễn phí, tint preview tạm thời và Capture/Restore.
- Crops/CropDefinition.cs, GardenTheme.cs: cây và theme. Worker/WorkerConfig.cs: chỉ số gốc.
- UI/GardenHud.cs: callback chậu/cây/theme/tab/shop và chuyển cửa sổ. GardenFloatingWindow chỉ quản lý khung; GardenGearPanel và GardenCosmeticPanel giữ presentation riêng; row và cửa sổ được dựng sẵn bằng Editor.
- UI/GardenUiPalette.cs: 64 swatch màu cho snapping art; UI style colors giữ nguyên. `GardenPlaceholderArt` fallback xuất block 8×8 và giữ world size qua PPU; atlas đang dùng là `Textures/Palette/garden-art-polished-v1.png`.
- Desktop/DesktopWindow.cs, WindowHandle.cs: native drag/resize/menu, Windows standalone.
- Editor/GardenConfig/: GardenPrototypeBuilder, GardenProgressionSetup dựng scene/HUD; GardenCosmeticSetup tạo catalog/cửa sổ; các *Verification kiểm tra feature.

## Verification

F01–F06 regression pass: 10k giao dịch, giới hạn số học, 1200s sim, pots/themes/upgrades, chest/gear, migration v1/v2, I/O, recovery và rebirth. Follow-up worker: compile, verifications F01–F06, Play Mode và Game View pass; worker/well chạm ground baseline, crop chạm mặt đất, worker sortingOrder 4 sau crop 5, và chỉ tưới khi chậu khô. F07/F08 verifications và UI pass đã ghi dưới các tag tương ứng. F09 atlas xác nhận 19 slices, 22 màu, alpha/silhouette gốc; profile validation, F01–F06 Editor verifications và Game View 920×180 pass. F10 Play Mode xác nhận vàng 30→158 và nhãn scale 1→1.12→1. F12: Windows QA build thành công; F01–F06 Editor verifications pass; standalone 920×180 hiển thị đủ 5 mục menu, Cosmetics mở/đóng, Shop/Upgrades hoạt động; Inventory/Gear đã smoke-test ở các lượt trước. Save v3 trong profile QA riêng còn nguyên sau restart (đọc lại gold 86). F13: palette 64 màu và preview 256×256 dùng 19 màu; mọi khối 8×8 phẳng, alpha nhị phân. Unity compile pass; importer Point/no mipmaps/uncompressed đã kiểm tra. Mẫu chưa nối vào scene. Chưa kiểm DPI/multi-monitor hoặc gamepad.

## Workflow

unity status --format json; unity command recompile_status --format json. Eval qua live Editor để thao tác asset/scene, không sửa YAML. GardenVerification.Run(), GardenEconomyVerification.Run(), GardenFeatureVerification.Pots(), GardenGearVerification.Run(), GardenSaveVerification.Run() và GardenCosmeticVerification.Run() là entry test trong namespace IdleGarden.Editor. Nếu script mới không vào compile list, AssetDatabase.Refresh() rồi chờ recompile; không reset Library/package.

Giữ nguyên thay đổi NuGet meta/Assets/_Recovery ngoài feature. Assets/Temp chứa screenshot QA tạm, không stage. Không claim test chưa chạy. Build output Builds/Windows/IdleGarden.exe bị ignore; user muốn tag từng giai đoạn để quay video, không dời tag đã push.
