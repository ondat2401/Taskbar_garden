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

**2026-10-19 · F03 đang làm · chưa có tag hoàn tất**

Thu gọn thanh chính; mở workshop khi chọn chậu, chia trang cây và nâng cấp. Chuẩn bị collider/callback cho chậu và nối atlas cây lửa/băng đã tạo bằng AI. Art hiện là bản nháp có glow, chưa đạt quy chuẩn pixel grid cuối cùng.

Gặp lỗi Unity fake-null khi thêm collider trong Editor; đổi sang kiểm tra null theo Unity. Có lượt chụp ảnh không khớp trạng thái workshop; kiểm tra lại xác nhận Editor đã dừng Play. Chạy Play mới cho ảnh workshop 920×540 đúng, tiền và chậu cập nhật. Chưa kết luận nguyên nhân của lượt dừng trước đó.

**Đã kiểm tra tiếp:** compile pass; Play Mode xác nhận callback đổi cây, chuyển chậu, đổi tab và đóng panel; xem trực tiếp ảnh workshop. Chưa kiểm tra click vật lý lên chậu và drag/resize native. Chưa chốt tag F03.

**Cảnh quay sau khi pass:** dải vườn gọn → bấm chậu → chọn cây → mở theme → xem nâng cấp. Chưa có bản quay/test native mới.

## Cách ghi tiếp

Mỗi mốc bổ sung: thay đổi người chơi nhìn thấy, lý do chọn thiết kế, vấn đề thật đã gặp và cách sửa, kiểm chứng đã chạy, commit/tag, gợi ý cảnh quay. Giữ lỗi và giới hạn đúng sự thật; không biến kế hoạch thành thành quả.

Các mốc sắp tới: F03 giao diện; F04 rương/trang bị; F05 save/tái sinh; F06 cosmetic preview và build. Cập nhật từng mốc khi xong, không tạo sẵn câu chuyện thành công.
