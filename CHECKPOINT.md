# Checkpoint — Idle Garden

Cập nhật 2026-10-27. F01–F14 và follow-up đã tích hợp/kiểm tra theo checkpoint. F13 art palette 8×8/64 màu và animation sheet vẫn là placeholder, chờ đánh giá. F14 đồng bộ thời tiết hiện tại lên HUD, chưa ảnh hưởng tưới/trồng; Play Mode lấy được thời tiết thật và Windows QA build hiển thị trạng thái. Người dùng cho phép tự làm, kiểm tra và commit từng giai đoạn.

## F14 — đồng bộ thời tiết (đã tích hợp)

- Palette board: `Assets/Game/Textures/Palette/garden-art-palette-8x8.png`; texture hoa mẫu đã được duyệt và giữ làm chuẩn.
- Texture tĩnh tách PNG riêng; `worker-animation.png` là sheet 4 frame; mỗi loài cây có sheet 6 frame (3 stage + 3 sway). Atlas cũ và sprite trùng được lưu ở `Assets/Game/Textures/Legacy/`.
- Raster check xác nhận palette, alpha 0/255 và block 8×8 phẳng trên texture mới. Unity compile/profile validation pass; Game View 920×180 đã xem; runtime kiểm tra sway đổi từ `carrot1` sang `carrot1-sway`.
- Dùng Open-Meteo Forecast API với current temperature/weather code; không cần API key. `Weather.asset` mặc định Ho Chi Minh City (10.8231, 106.6297), có thể đổi tọa độ; đồng bộ lần đầu khi chạy rồi mỗi 15 phút, timeout 8 giây.
- Máy có Microsoft Weather; phần tài liệu Windows tìm được chỉ chỉ cách mở app bằng URI `msnweather:` và API Geolocator lấy vị trí riêng, không thấy API công khai đọc report đã chọn trong app. Google Weather API có current conditions nhưng cần API key/billing cho dùng production; chọn Open-Meteo cho prototype.
- HUD hiển thị trạng thái + °C + attribution. WMO được ánh xạ thành nắng, mây rải rác, âm u, sương mù, mưa, tuyết, giông hoặc không rõ; lỗi đầu tiên hiện unavailable, lỗi sau giữ dữ liệu gần nhất và đánh dấu stale.
- Không yêu cầu GPS và không thay đổi gameplay/tưới cây. Mưa/nắng, VFX, dự báo và chọn vị trí trong game chưa làm; F13 vẫn là placeholder chờ đánh giá.
- Unity mapping/parser/URL self-check pass; Play Mode nhận API thật, Game View hiển thị trạng thái, Windows QA build `IdleGarden-QA-F14` thành công. QA dùng profile riêng, save schema v3 đọc được; Player.log không ghi lỗi.
- Open-Meteo attribution: [Forecast API](https://open-meteo.com/en/docs). Free tier chỉ cho mục đích phi thương mại, yêu cầu attribution CC BY 4.0 và không cam kết uptime; dùng license/provider thương mại trước khi phát hành có doanh thu: [pricing/usage](https://open-meteo.com/en/pricing).

## Đã chốt

- F01: `8cb0f89`, tag `checkpoint/f01-economy`. Kho/vàng/bán; transaction, simulation và Play Mode pass.
- F02: `7d50e26`, tag `checkpoint/f02-pot-themes`. Cây chọn riêng từng chậu, đổi giữa vụ đợi thu hoạch, ba theme/bốn cây/sáu nâng cấp. Pots(), economy và simulation regression pass.
- F03: `4e16846`, tag `checkpoint/f03-compact-shop`. Windows build nhận click Shop/menu, bung/thu 920×180 ↔ 920×540, pin/lock, kéo khi mở khóa và resize. Play Mode xác nhận menu đóng Shop trước khi mở; F01/F02 regression pass.
- F04: tag `checkpoint/f04-chests-gear`. Rương thời gian/pity, rarity/affix, inventory/equip/salvage/essence và cửa sổ nổi đã vào scene. Seeded verification, F01–F03 regression và Play Mode UI pass. Chưa chạy standalone F04; chưa kiểm DPI/multi-monitor.
- F05: tag `checkpoint/f05-save-rebirth`. Save schema v2/migration v1, atomic replace/backup recovery, bảo toàn file tương lai/lỗi và rebirth giữ lựa chọn cây cùng tiến trình vĩnh viễn. F01–F05 verifier và Play Mode rebirth pass; I/O dùng thư mục tạm cô lập, chưa restart standalone.
- F06: tag `checkpoint/f06-prototype`. Catalog có 3 cosmetic miễn phí, 2 premium và 1 bundle cố định; miễn phí equip/lưu, premium/bundle preview tint tạm thời, không checkout hoặc entitlement giả. Save schema v3 migrate v1/v2. F01–F06 verifier, Play Mode UI và Windows BuildReport pass; standalone QA ghi save v3 vào profile cô lập.
- Follow-up F06: tag `checkpoint/f06-worker-grounding`. Worker và well đặt đúng ground baseline; crop đặt lên mặt đất, worker vẽ sau crop. Chậu chỉ được tưới khi nước cạn; scheduler luân phiên trồng/tưới và quay vòng chậu để tránh bỏ đói vị trí. F01–F06 verifications, Play Mode và Game View pass; chưa build standalone lại.
- F07 UI polish: tag `checkpoint/f07-ui-polish`. Giao diện dùng palette xanh rêu tối, chữ parchment, accent brass, viền mảnh và trạng thái hover/pressed/disabled/selected. Áp dụng cho workshop, inventory và cosmetics; giữ nền game trong suốt để cửa sổ desktop tiếp tục click-through. Compile, sáu F01–F06 Editor verifications và Play Mode/Game View cho bốn cửa sổ pass. Chưa build standalone riêng hoặc kiểm DPI/multi-monitor/gamepad.
- F08 shared art palette: tag `checkpoint/f08-shared-art-palette`. Tạo sprite atlas placeholder cho worker, giếng, nền cỏ/đất và ba giai đoạn của bốn cây; áp 22 màu từ `GardenUiPalette.SharedColors` cho world art và icon UI. Config, prefab và scene trỏ đến sprite mới; player vẫn sortingOrder 4, soil 2, crop 5. Compile, 19-sprite mapping, F01–F06 verifications và Play Mode/Game View Upgrades pass. Chưa build standalone riêng hoặc kiểm DPI/multi-monitor/gamepad.
- F09 art pass: `checkpoint/f09-art-pass`. Atlas polished v1 được chuẩn hóa bằng alpha/slice gốc và palette 22 màu; 19 sprite mới đã nối vào worker/crop/garden config, prefab và scene. `GardenPlaceholderArt.Generate()` trỏ vào atlas mới để Apply Layout không quay lại art cũ. Profile validation, F01–F06 verifications, Game View và Windows build pass; atlas vẫn là placeholder.
- F10 game feel: nhãn vàng scale lên 12% trong 0.24 giây khi currency tăng, sau đó trả về scale gốc. Play Mode self-check tạo thu hoạch, xác nhận vàng 30→158 và scale 1→1.12→1; không tạo object runtime.
- F11 final UI: menu gọn 5 hàng 24px trong panel 140px, Cosmetics nằm trọn trong 920×180. Standalone mở/đóng Cosmetics, Shop/Upgrades và Inventory; Gear đã mở thành công trong standalone smoke trước đó.
- F12 final QA: Windows QA build thành công; F01–F06 Editor verifications pass. Standalone 920×180 hiển thị đủ 5 mục, mở/đóng Cosmetics và mở Shop/Upgrades; Inventory/Gear đã smoke-test trong các lượt standalone trước. Save schema v3 trong profile QA riêng còn sau restart (gold 86), không dùng save người chơi. Chưa kiểm DPI, multi-monitor hoặc gamepad.
- Follow-up QA F07: tag `checkpoint/f07-cosmetic-label-fix`. Sáu tên offer bị vô hình do RectTransform cao 0px; sửa cấu hình Editor và scene thành 40px. Play Mode xác nhận đủ tên, chọn offer/đóng cửa sổ, 78 nút đúng palette; F01–F06 Editor verifications pass. Chưa build standalone hoặc kiểm DPI/multi-monitor/gamepad.
- Thiết kế mới: `docs/GAME_DESIGN.md`. Tiến độ: `docs/FEATURE_STATUS.md`. Hành trình để dựng video: `docs/DEVLOG.md`; quy trình tag: `docs/GIT_CHECKPOINTS.md`.

## Đã kiểm tra F03

Garden Bar ở mép trên; menu mở xuống dưới, workshop chừa khoảng thanh. Windows build nhận click thật; Shop bung/thu cửa sổ 920×180 ↔ 920×540. Pin/lock, drag khi mở khóa và resize phản hồi. Menu đóng Shop khi mở; Play Mode xác nhận state và ảnh menu sau sửa. F01–F02 verification cũng chạy lại pass. Chưa kiểm DPI hoặc nhiều màn hình.

Native test dùng save hiện có. UI ban đầu hiển thị 536 vàng; vườn chạy idle trong lúc test và save hiện tại được giữ nguyên. Build kiểm tra: `Builds/Windows/F03-native-test/`.

## Đã kiểm tra F04

Rương mỗi600 giây unpaused real-time (không theo simulation speed/offline), pity lần mở thứ20, 40 slot, gear base + affix không trùng, equip theo slot, salvage không cho đồ đang mặc, essence đổi rương. HUD có 40 row dựng sẵn bằng Editor; `GardenFloatingWindow` giữ khung/đóng/kéo, `GardenGearPanel` gọi API progression và `GardenProgression` sở hữu luật/state. Shop/menu/gear loại trừ nhau.

Đã chạy compile; `GardenVerification.Run()`, `GardenEconomyVerification.Run()`, `GardenFeatureVerification.Pots()` và `GardenGearVerification.Run()` pass. Play Mode kiểm tra mở rương qua UnityEvent, chọn row, equip, chặn salvage đồ đang mặc, đóng cửa sổ, Shop/menu chuyển trạng thái và drag không vượt Canvas. Game View 510×287 đã xem. F04 không ghi persistent save trong Editor; chưa kiểm file I/O hoặc native Windows build.

F07 UI polish đã hoàn tất; chưa chốt checkpoint kế tiếp.

## Đã kiểm tra F05

`GardenSaveVerification.Run()` xác nhận migration save F01 v1 sang v2 (cây chọn kế tiếp mặc định cây đã lưu), save/replace nhiều lần, backup của bản hợp lệ trước, fallback khi primary lỗi và giữ bản lỗi khi phục hồi. File version tương lai hoặc save lỗi không thể bị ghi đè. Save sau rebirth được đọc lại bằng store mới từ đĩa tạm.

Đã chạy compile; `GardenVerification.Run()`, `GardenEconomyVerification.Run()`, `GardenFeatureVerification.Pots()`, `GardenGearVerification.Run()` và `GardenSaveVerification.Run()` pass. Play Mode gọi `GardenController.Rebirth()` thật: seed tăng, cây đang trồng reset; lựa chọn từng chậu, theme, gear đã mặc và rương được giữ. Chưa chạy standalone restart hoặc ghi vào save người chơi.

## Đã kiểm tra F06

Catalog ScriptableObject phân loại từng cosmetic theo slot và offer. `GardenCosmeticController` giữ loadout, áp tint và hoàn tác preview; `GardenCosmeticPanel` chỉ trình bày/lựa chọn; `GardenFloatingWindow` vẫn phụ trách khung, đóng và kéo. Free item được trang bị và lưu; premium/bundle chỉ xem thử, không có purchase API hay cấp entitlement. Cửa sổ Cosmetics mở từ menu trong Canvas game, dùng cùng kiểu khung với Inventory.

Save schema v3 migrate dữ liệu v1/v2 và giữ cosmetic miễn phí. Compile cùng F01–F06 Editor verifications pass. Play Mode gọi các UnityEvent thật: mở cửa sổ, preview bundle đổi tint worker/pot/crop, đóng cửa sổ phục hồi loadout, equip free và kiểm tra snapshot save hợp lệ. Game View đã xem ở 510×287. Windows BuildReport qua `GardenPrototypeBuilder.BuildWindows()` là Succeeded; standalone QA dùng Company/Product riêng và ghi save v3 ở profile cô lập, không chạm save người chơi. Chưa restart standalone sau khi equip, chưa kiểm DPI/multi-monitor hoặc gamepad.

## Đã kiểm tra follow-up grounding/watering

Worker và well dùng baseline `y=-0.2`, crop dùng mặt soil `y=-0.04`; sorting order worker 4 sau crop 5. `GardenSimulation` chỉ gọi job tưới khi nước chậu bằng 0, luân phiên trồng/tưới và duyệt chậu theo vòng để không bỏ đói chậu xa. Khi không còn việc, `GardenJob.Idle` phát animation Idle hiện có.

Compile pass; `GardenVerification.Run()` kiểm tra vườn 4/8/12 chậu, refill, pause, dry-only watering/rest và cây khô; F01–F06 verifications pass. Play Mode/Game View 510×287 xác nhận baseline, sorting và simulation chạy; scene không dirty sau khi dừng. Chưa build Windows standalone lại cho follow-up, chưa kiểm DPI/multi-monitor hoặc gamepad.

## Đã kiểm tra F07 UI polish

`GardenUiPalette` gom palette, màu nút và viền cho các UI được dựng bằng Editor. Active tab và dòng đang chọn dùng accent; text phụ và giá có màu riêng. Nền thế giới vẫn trong suốt để giữ click-through; không thay sprite thế giới hoặc tạo asset mới.

Compile pass; `GardenVerification.Run()`, `GardenEconomyVerification.Run()`, `GardenFeatureVerification.Pots()`, `GardenGearVerification.Run()`, `GardenSaveVerification.Run()` và `GardenCosmeticVerification.Run()` chạy xong. Play Mode mở Garden, Upgrades, Inventory và Cosmetics; Game View 510×287 đã xem sau khi ép Canvas repaint. Chưa build standalone riêng hoặc kiểm DPI/multi-monitor/gamepad.

## Giữ nguyên

Không stage/revert NuGet importer meta, Assets/_Recovery, Assets/Temp, `node_modules` hoặc `package.json` của user. Không sửa YAML scene; dùng Unity CLI/Pipeline. Editor không ghi save người chơi. Không khẳng định thao tác native pass từ compile/ảnh Editor.
