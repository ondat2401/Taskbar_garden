# Hành trình làm Idle Garden

Tư liệu cho dev log/video, cập nhật theo feature. Các mốc đầu được tổng hợp từ tài liệu và Git hiện có; không phải nhật ký được ghi trực tiếp ở thời điểm đó. Chỉ dùng tag đã tạo để quay lại phiên bản. Hướng dẫn: [GIT_CHECKPOINTS.md](GIT_CHECKPOINTS.md).

## 01 — Từ ý tưởng đến khung game

Ý tưởng: một vườn pixel art nằm trên taskbar, nhân vật tự đi trồng, tưới và thu hoạch trong lúc người chơi làm việc khác. Người chơi quản lý lựa chọn và mua sắm. Multiplayer hai vườn đồng bộ là hướng mở rộng, chưa triển khai.

Chọn Unity 6000.3.10f1 và UniWindowController cho cửa sổ desktop. Tách config, simulation và hiển thị để chỉnh dữ liệu bằng Editor; danh sách chậu không khóa ở bốn ô. Chuẩn bị scene/prefab bằng tool Editor để dễ inspect.

Đã kiểm tra simulation 4/8/12 ô, refill, pause, cây khô và build Windows ban đầu. Build chạy được; thao tác drag/resize native và DPI nhiều màn hình chưa được xác nhận. Khi đưa project lên Git, source đã chứa các bản nháp progression; tag `checkpoint/00-import-wip` (`5a34436`) ghi đúng trạng thái này, không đại diện cho một game đã hoàn thiện.

**Cảnh quay:** dải vườn tự hoạt động; Editor đổi số chậu và Apply Layout. Build cũ chỉ minh họa khung game.

## 02 — Thu hoạch bắt đầu có ý nghĩa

**2026-10-19 · F01 · `checkpoint/f01-economy` · `8cb0f89`**

Thêm kho và vàng: nông sản đi vào kho, tự bán phần vượt dự trữ bốn cây mỗi loại. Có thể tắt bán tự động để tích nguyên liệu rồi bán tay. Đây là nền kinh tế cho nâng cấp về sau.

Một vấn đề ít thấy trên màn hình: số lượng và vàng rất lớn có thể gây tràn hoặc mất độ chính xác. Giao dịch không biểu diễn được phải bị từ chối nguyên vẹn, tránh mất hàng mà không nhận vàng.

**Đã kiểm tra:** 10.000 giao dịch, giới hạn số học, 1.200 giây simulation; Play Mode xác nhận thu hoạch → kho → vàng, tích trữ và bán tay. Chưa có executable mới ở mốc này.

**Cảnh quay:** vàng tăng khi bán dư; tắt auto-sell để kho tăng; bấm bán để thấy vàng đổi ngay.

## 03 — Chọn một lần, vườn tự vận hành

**2026-10-19 · F02 · `checkpoint/f02-pot-themes` · `7d50e26`**

Chốt hướng chơi: người chơi chọn cây cho từng chậu, nhân vật lặp lại việc chăm cây. Đổi lựa chọn giữa vụ sẽ hoàn thành cây cũ rồi trồng cây mới, tránh mất tiến trình đang có. Mỗi chậu có lựa chọn riêng.

Thêm ba theme Meadow/Ember/Frost, bốn loại cây và sáu nâng cấp. Theme mới dùng vàng mở khóa; phần thưởng rương mỗi lần mở chỉ cấp một lần. Tách chỉ số gốc khỏi bonus để nâng cấp không sửa trực tiếp asset cây/công cụ.

Thiết kế tiếp: vật liệu của công cụ quyết định chỉ số gốc, độ hiếm quyết định các buff ngẫu nhiên. Rương nhận qua thời gian chơi/mốc mở khóa; IAP dự kiến dành cho ngoại hình và quà có nội dung cố định. Đây là thiết kế trong [GAME_DESIGN.md](GAME_DESIGN.md), chưa phải các tính năng đã chạy.

**Đã kiểm tra:** từ chối cây chưa mở khóa, đổi giữa vụ, độc lập từng chậu, chi phí và hiệu ứng sáu nâng cấp, không mutate base asset; regression economy/simulation pass. Atlas AI lửa/băng có ở tag, nhưng việc gán sprite hoàn chỉnh tiếp tục trong F03.

**Cảnh quay:** so sánh hai chậu trồng cây khác nhau; đổi cây khi đang lớn và chờ vụ tiếp theo. Giao diện chọn cây hoàn chỉnh thuộc F03, không mô tả là đã có ở tag F02.

## 04 — Thu gọn để sống trên desktop

**2026-10-20 · F03 · `checkpoint/f03-compact-shop`**

Thu gọn thanh chính; mở workshop khi chọn chậu, chia trang cây và nâng cấp. Chuẩn bị collider/callback cho chậu và nối atlas cây lửa/băng đã tạo bằng AI. Art hiện là bản nháp có glow, chưa đạt quy chuẩn pixel grid cuối cùng.

Gặp lỗi Unity fake-null khi thêm collider trong Editor; đổi sang kiểm tra null theo Unity. Có lượt chụp ảnh không khớp trạng thái workshop; kiểm tra lại xác nhận Editor đã dừng Play. Chạy Play mới cho ảnh workshop 920×540 đúng, tiền và chậu cập nhật. Menu từng bị workshop che; mở menu nay tự đóng shop.

**Đã kiểm tra:** compile và F01/F02 regression pass. Windows build nhận click Shop/menu; bung/thu 920×180 ↔ 920×540; pin, lock, drag khi mở khóa và resize phản hồi. Play Mode xác nhận chọn cây, chuyển chậu/tab và menu/shop loại trừ nhau; ảnh cửa sổ và workshop đã xem trực tiếp. Chưa kiểm DPI/multi-monitor. Native test dùng save hiện có; vườn tiếp tục idle và save hiện tại được giữ nguyên.

**Cảnh quay:** dải vườn gọn → bấm chậu → chọn cây → mở theme → xem nâng cấp; mở menu rồi thử pin/lock và kéo cửa sổ.

## 05 — Rương và trang bị

**2026-10-20 · F04 · `checkpoint/f04-chests-gear`**

Thêm rương miễn phí mỗi600giây chơi thật, không tăng theo tốc độ simulation và dừng khi pause. Lần mở thứ20 sau chuỗi miss bảo đảm Epic+; gear giữ base stat theo vật liệu, rarity thêm affix riêng. Inventory40 slot; gear mặc được thay theo slot, đồ mặc không salvage, essence đổi lấy rương.

Inventory mở trong cửa sổ nổi cùng Canvas. `GardenFloatingWindow` phụ trách khung/đóng/kéo; `GardenGearPanel` hiển thị và gọi progression; các row đã dựng sẵn trong Editor. Sau khi scene báo dirty, đã sao lưu bằng Editor API trước khi Apply rồi xóa bản sao sau khi scene lưu thành công, giữ nguyên phần scene chưa lưu ban đầu.

**Đã kiểm tra:** compile, F01–F03 regression, seeded timer/pity/drop/affix/capacity/equip/salvage/essence/capture checks; Play Mode mở rương và thao tác qua listener, kiểm tra panel transitions và drag bounds. Đã xem Game View ở510×287. Chưa chạy Windows standalone F04, chưa kiểm DPI/multi-monitor hoặc file I/O save.

## 06 — Lưu tiến trình và tái sinh

**2026-10-20 · F05 · `checkpoint/f05-save-rebirth`**

Nâng schema lên v2 và migrate save v1: lựa chọn cây cũ lấy từ cây đã lưu, các trường theme/gear mới được khởi tạo an toàn. Save tiếp tục ghi file tạm, flush, replace và giữ bản backup. Nếu file primary lỗi thì khôi phục backup mà vẫn giữ file lỗi; nếu save thuộc build mới hơn hoặc không đọc được thì chặn ghi để tránh mất dữ liệu.

Rebirth reset vàng chu kỳ, kho cây, rank và cây đang trồng nhưng giữ bonus seed, theme, gear, rương/pity/essence cùng lựa chọn riêng của từng chậu. Controller tạo simulation mới rồi áp lại lựa chọn đã mở khóa.

**Đã kiểm tra:** compile; F01–F05 Editor verifications; I/O trên thư mục tạm với migration v1, save/replace nhiều lần, fallback backup, file tương lai và file hỏng; đọc lại save sau rebirth bằng store mới. Play Mode xác nhận `GardenController.Rebirth()` giữ lựa chọn cây, theme, gear/rương và reset vụ đang trồng. Chưa restart Windows standalone hoặc chạm save thật.

## 07 — Chọn ngoại hình, chưa mở giao dịch

**2026-10-20 · F06 · `checkpoint/f06-prototype`**

Thêm catalog cosmetic theo slot Worker/Pot/Crop và bundle, với ba lựa chọn miễn phí để trang bị, hai premium và một bundle nội dung cố định để preview. Preview premium/bundle đổi tint trực tiếp trên vườn nhưng không ghi vào loadout; đóng cửa sổ trả lại màu đã trang bị. Không có nút mua hoặc cấp entitlement. Cửa sổ mở từ Window Menu, cùng Canvas và `GardenFloatingWindow` với Inventory. Catalog, áp tint/loadout và presentation nằm ở các lớp riêng.

Save schema v3 migrate cả v1 và v2; lựa chọn free được lưu. F01–F06 Editor verifications và Play Mode UnityEvent/tint/rollback/save-capture pass. Game View đã xem ở 510×287. Windows `BuildReport` Succeeded; standalone smoke dùng Company/Product cô lập và tạo save schema v3 trong profile QA, giữ nguyên save người chơi. Chưa restart sau khi equip cosmetic, chưa kiểm DPI/multi-monitor hoặc gamepad.

**Cảnh quay:** mở Menu → Cosmetics; chọn Moonlit Garden → Preview on garden; xem tint người/chậu/cây; đóng để thấy loadout trở lại; equip Mint Apron và mở lại panel để thấy đã lưu.

### Follow-up F06 — chạm đất và chăm vườn công bằng hơn

**2026-10-20 · `checkpoint/f06-worker-grounding`**

Hạ worker và giếng về cùng ground baseline; đặt cây trên mặt đất của chậu và vẽ worker sau cây. Nước không còn kích hoạt lượt tưới sớm: nhân vật chỉ đi tưới khi chậu cạn. Khi không có việc, simulation ở `Idle` và dùng animation nghỉ đã có.

Regression 12 chậu phát hiện ưu tiên tuyệt đối khiến một số chậu bị bỏ đói. Scheduler nay luân phiên trồng/tưới và quay vòng vị trí, vẫn giữ thu hoạch ưu tiên cao. Compile, F01–F06 verifications, Play Mode và Game View 510×287 pass; chưa build standalone lại.

### F07 — polish UI

**2026-10-20 · `checkpoint/f07-ui-polish`**

Chuẩn hóa workshop, inventory và cosmetics bằng palette xanh rêu tối, chữ parchment, accent brass, viền mảnh và trạng thái nút hover/pressed/disabled/selected. Tab và dòng đang chọn được nhấn rõ hơn; giá dùng accent, mô tả phụ giảm độ sáng. Giữ transparency của nền game để desktop click-through; không đổi world art hay thêm asset.

Compile, sáu F01–F06 Editor verifications, Play Mode và Game View 510×287 pass. Đã xem trạng thái Garden, Upgrades, Inventory và Cosmetics; ép Canvas repaint sau chuyển panel để ảnh phản ánh đúng trạng thái. Chưa build standalone riêng, chưa kiểm DPI/multi-monitor/gamepad.

**Cảnh quay:** mở Shop và đổi Pots/Upgrades, mở Inventory, sau đó Menu → Cosmetics; rê chuột qua nút để thấy trạng thái hover và chọn cosmetic để thấy selected.

## Cách ghi tiếp

Mỗi mốc bổ sung: thay đổi người chơi nhìn thấy, lý do chọn thiết kế, vấn đề thật đã gặp và cách sửa, kiểm chứng đã chạy, commit/tag, gợi ý cảnh quay. Giữ lỗi và giới hạn đúng sự thật; không biến kế hoạch thành thành quả.

Checkpoint tiếp theo chưa xác định; cập nhật sau khi chốt feature kế tiếp, không tạo sẵn câu chuyện thành công.
