# Idle Garden — prototype

Game trồng cây pixel art nằm trên desktop, phía trên taskbar. Nhân vật tự trồng, tưới, lấy nước và thu hoạch. Chưa có kinh tế, nâng cấp, save game hoặc multiplayer.

## Chạy trong Unity

1. Dùng Unity **6000.3.10f1**.
2. Mở `Tools > Idle Garden > Garden Config`.
3. Bấm **Open / Create Prototype**, sau đó Play.
4. Tab **Runtime** hiển thị công việc, nước, sản lượng và từng ô; có Pause/Resume và ×1/×2/×5.

## Chỉnh data

- Profile: `Assets/Game/_Configs/Garden/Prototype.asset`.
- **Garden**: thêm/xóa/đổi thứ tự ô trong danh sách, chọn cây, chỉnh khoảng cách và kích thước cửa sổ.
- Thoát Play rồi bấm **Apply Layout** sau khi thay số ô, loại cây từng ô, khoảng cách hoặc vị trí bồn.
- **Crops**: thông số cây, sprite từng giai đoạn và preview. Tạo asset cây mới bằng nút Create Crop Asset, sau đó gán vào ô ở tab Garden.
- **Worker**: tốc độ, thời gian hành động, bình nước, sprite animation.
- Thông số số học và sprite được đọc trực tiếp khi chạy. Thay đổi asset trong Play là chỉnh config thật; bấm Save Config nếu muốn lưu. Trạng thái tăng trưởng/nước/sản lượng không ghi ngược vào asset.
- Apply Layout chỉ dựng lại nhánh `Generated Layout` do công cụ quản lý. Không đặt object thủ công vào nhánh này; sửa prefab `Assets/Game/Prefabs/Garden/Plot.prefab` để đổi hình thức ô.

## Gameplay

Ưu tiên thu hoạch → tưới → trồng. Trong cùng mức ưu tiên chọn ô gần nhất; hoàn tất việc rồi mới chọn việc khác. Bình không đủ cho lần tưới thì về bồn. Cây thiếu nước dừng lớn, không chết. Hạt không giới hạn; thu hoạch cộng bộ đếm, sau đó tự trồng lại.

Vườn dài có thể cuộn bằng bánh xe chuột hoặc nút `<` / `>`. UI và pixel placeholder có thể thay độc lập với simulation.

## Windows desktop

Chọn `Tools > Idle Garden > Build Windows Prototype`. File chạy: `Builds/Windows/IdleGarden.exe`.

- Kéo thanh tiêu đề để di chuyển.
- Kéo tay nắm `/` phía dưới bên phải để đổi kích thước.
- Pin bật/tắt luôn nổi; Lock khóa kéo/resize; Pause dừng mô phỏng; Close thoát.
- Vùng alpha trong suốt cho bấm xuyên. Chức năng desktop chỉ bật trong Windows standalone, không sửa cửa sổ Unity Editor.
- Build dùng Direct3D 11, tắt flip-model swapchain và HDR để hỗ trợ UniWindowController. Thao tác build có cập nhật các Player Settings này và alpha output của URP asset đang dùng.
- Prototype không lưu vị trí/kích thước giữa các lần chạy; kích thước khởi tạo lấy từ profile. Chưa hỗ trợ kéo resize từ mọi cạnh, chỉ tay nắm.

## Kiểm tra

`Tools > Idle Garden > Verify Simulation` chạy kiểm tra trạng thái cho 4/8/12 ô, nhiều vòng thu hoạch, lấy nước, pause, cây khô và chỉnh số học khi chạy. Kết quả verification thực tế xem `AI_README.md`.

## Dành cho AI agent

Đọc `AGENTS.md` rồi `AI_README.md` trước khi tìm code. Cập nhật bản ghi hiện trạng sau mỗi lần thêm/sửa/xóa để tiết kiệm context cho agent tiếp theo.
