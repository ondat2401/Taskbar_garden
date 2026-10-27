# AI handoff — Idle Garden

Đọc CHECKPOINT.md trước. F01–F08 và follow-up đã tích hợp; F09 polished placeholder atlas đang dùng `garden-art-polished-v1.png` với 19 slices/22 màu. Tiến độ ở docs/FEATURE_STATUS.md, hành trình ở docs/DEVLOG.md. Sau mỗi feature cập nhật hiện trạng, dev log và commit/tag; không nối nhật ký dài vào file này.

## Project

Unity 6000.3.10f1, URP17.3, InputSystem1.18, uGUI2; Windows desktop qua UniWindowController. Scene Assets/Game/Scenes/GardenPrototype.unity. Profile Assets/Game/_Configs/Garden/Prototype.asset → GardenEconomy.asset. Assets thuộc Assets/Game; scripts chia feature. Packages MCP của user giữ nguyên.

Player chọn cây cho từng chậu, nhân vật tự trồng/tưới/thu hoạch lặp lại. Đổi giữa vụ đợi cây cũ xong. Ba theme/bốn cây/sáu nâng cấp; rương timed, pity, gear/affix, equip/salvage và cosmetic preview đã tích hợp. Save schema v3 migrate v1/v2, ghi temp/replace/backup, chặn file lỗi/tương lai; rebirth giữ lựa chọn cây, cosmetic và tiến trình vĩnh viễn. Multiplayer/Steam commerce chưa làm; prototype không có checkout hoặc entitlement premium.

## Code liên quan

- Garden/GardenSimulation.cs: state/job, dry-only watering, luân phiên việc trồng/tưới và quay vòng chậu để tránh bỏ đói chậu xa, chọn cây, Capture/Restore. GardenController.cs: tick, camera, HUD, autosave standalone; Editor không ghi save.
- Progression/GardenProgression.cs: economy/theme/chests/gear/rebirth. GearConfig.cs và EquipmentDefinition.cs: luật drop và base stats. GardenSaveStore.cs: DTO version3, migrate v1/v2, atomic replacement và backup recovery.
- Cosmetics/GardenCosmeticCatalog.cs và GardenCosmeticController.cs: catalog, loadout miễn phí, tint preview tạm thời và Capture/Restore.
- Crops/CropDefinition.cs, GardenTheme.cs: cây và theme. Worker/WorkerConfig.cs: chỉ số gốc.
- UI/GardenHud.cs: callback chậu/cây/theme/tab/shop và chuyển cửa sổ. GardenFloatingWindow chỉ quản lý khung; GardenGearPanel và GardenCosmeticPanel giữ presentation riêng; row và cửa sổ được dựng sẵn bằng Editor.
- UI/GardenUiPalette.cs: palette 22 màu dùng chung cho HUD và sprite; atlas đang dùng là `Textures/Palette/garden-art-polished-v1.png`.
- Desktop/DesktopWindow.cs, WindowHandle.cs: native drag/resize/menu, Windows standalone.
- Editor/GardenConfig/: GardenPrototypeBuilder, GardenProgressionSetup dựng scene/HUD; GardenCosmeticSetup tạo catalog/cửa sổ; các *Verification kiểm tra feature.

## Verification

F01–F06 regression pass: 10k giao dịch, giới hạn số học, 1200s sim, pots/themes/upgrades, chest/gear, migration v1/v2, I/O, recovery và rebirth. Follow-up worker: compile, verifications F01–F06, Play Mode và Game View pass; worker/well chạm ground baseline, crop chạm mặt đất, worker sortingOrder 4 sau crop 5, và chỉ tưới khi chậu khô. F07/F08 verifications và UI pass đã ghi dưới các tag tương ứng. F09 atlas xác nhận 19 slices, 22 màu, alpha/silhouette gốc; profile validation, F01–F06 Editor verifications và Game View 920×180 pass. Windows QA build thành công trong profile cô lập; standalone Shop, Upgrades, Inventory, Cosmetics và restart/save được kiểm tra. Chưa kiểm DPI/multi-monitor hoặc gamepad.

## Workflow

unity status --format json; unity command recompile_status --format json. Eval qua live Editor để thao tác asset/scene, không sửa YAML. GardenVerification.Run(), GardenEconomyVerification.Run(), GardenFeatureVerification.Pots(), GardenGearVerification.Run(), GardenSaveVerification.Run() và GardenCosmeticVerification.Run() là entry test trong namespace IdleGarden.Editor. Nếu script mới không vào compile list, AssetDatabase.Refresh() rồi chờ recompile; không reset Library/package.

Giữ nguyên thay đổi NuGet meta/Assets/_Recovery ngoài feature. Assets/Temp chứa screenshot QA tạm, không stage. Không claim test chưa chạy. Build output Builds/Windows/IdleGarden.exe bị ignore; user muốn tag từng giai đoạn để quay video, không dời tag đã push.
