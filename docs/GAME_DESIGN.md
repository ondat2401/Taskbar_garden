# Idle Garden — design v2

Ngày 2026-10-19. Đây là hướng triển khai đã được user giao tự quyết, tự test và commit từng giai đoạn. Thay thế các ý tưởng cũ chưa triển khai khi có mâu thuẫn.

## Trải nghiệm

Vườn là một dải pixel art sát taskbar. Player chọn cây cho từng chậu, chọn công cụ và đầu tư; nhân vật tự trồng, chỉ đi tưới khi chậu cạn, lấy nước, thu hoạch, bán và trồng lại mãi. Khi không có việc, nhân vật nghỉ ở trạng thái Idle; có thể thêm hoạt động nghỉ sau này. Không có cây chết hoặc thao tác tưới bắt buộc. Đổi cây giữa vụ chỉ đổi vụ tiếp theo, không xóa cây đang lớn. Mỗi chậu có lựa chọn riêng, không ghi runtime lựa chọn vào profile thiết kế.

Vòng chính: cây → nông sản/vàng → nâng cấp hoặc mở theme → cây mới + rương mốc → công cụ có buff → chăm vườn tốt hơn → tái sinh → bonus vĩnh viễn. Rương theo thời gian và đồ trùng tạo vòng sưu tầm lâu dài. Không cần mua IAP để mở đủ tính năng gameplay.

## Cân bằng ban đầu (config được)

| Theme | Mở khóa | Cây demo | Tốc độ / giá bán / nước |
|---|---|---|---|
| Meadow | Có sẵn | Carrot, Radish | Giữ thông số prototype để bảo toàn baseline |
| Ember | 250 vàng | Fire pepper | 25 giây / 14 vàng / 1.1 nước mỗi giây |
| Frost | 500 vàng | Frost bloom | 35 giây / 20 vàng / .6 nước mỗi giây |

Theme không phải rarity hoặc skin. Cây lửa đòi nước nhiều hơn, cây băng chậm hơn nhưng ít phải chăm. Không thiết kế một cây luôn thắng ở mọi điều kiện. Giá/time chỉ là seed balance: đo đầu ra thực tế gồm thời gian đi/tưới để chỉnh sau.

- Vàng đầu30, giữ lại4 mỗi loại nông sản, bán dư tự động; có thể tắt để tích nguyên liệu.
- Nâng cấp tăng theo rank, chi phí vàng ×1.45/rank và nguyên liệu tăng mỗi5rank. Bonus cộng trên base, không nhân chồng vô hạn giữa từng nguồn.
- Không có độ bền công cụ hoặc phí hạt gây kẹt tài nguyên.
- Tái sinh cần vàng kiếm trong chu kỳ (không phải số dư), mục tiêu1000 ×1.6 mỗi lần. Nhận1seed tăng25% vàng/base mỗi seed. Reset vàng, rank, stock, cây đang trồng; giữ lựa chọn cây, theme, gear, hòm chưa mở, pity và cosmetic.
- Giới hạn double/int và clamp tốc độ/sản lượng là bảo vệ số học, không hứa vô hạn toán học. Không làm server kinh tế trong prototype.

## Công cụ: base khác rarity

`EquipmentDefinition` định nghĩa ID/chất liệu/slot/base stats. `GearInstance` lưu ID instance, definition ID, rarity và các affix đã roll. Roll chỉ khi nhận đồ; load/equip không roll lại.

Ví dụ base bình gỗ +4 dung tích, bình đồng +10. Gỗ Epic vẫn giữ base+4; Epic có nhiều affix và khoảng buff lớn hơn. Đồng Common giữ base+10 và không có affix. Gỗ Epic có thể tốt hơn về tốc độ hoặc hiệu suất nước, nhưng không tự đổi base thành đồng. Không tự xếp hạng item chỉ dựa trên rarity.

| Rarity | Tỉ lệ cơ bản | Số affix | Giá trị mỗi affix |
|---|---:|---:|---:|
| Common |55%|0|—|
| Uncommon |25%|1|3–7%|
| Rare |15%|2|5–12%|
| Epic |4%|3|8–18%|
| Legendary |1%|3|15–25%|

Affix chọn không trùng stat trên cùng item. Các stat: tốc độ lớn, yield, đi bộ, thao tác, dung tích, tiết kiệm nước. Gear base và affix là nguồn riêng với rank; cộng bonus vào hệ số cuối, không ghi đè WorkerConfig/CropDefinition.

## Rương miễn phí

- Rương đầu sau lần thu hoạch đầu tiên; mỗi theme mới mở lần đầu nhận1rương.
- Rương định kỳ mỗi600giây chơi thực, kể cả app ở background. Pause mô phỏng không tính thời gian; tốc độ ×5 của debug không làm timer nhanh hơn. Chưa cộng thời gian khi app đã tắt.
- Lần thứ20 không được Epic+ liên tiếp sẽ bảo đảm Epic+; tỉ lệ thông thường55/25/15/4/1%, lần pity Epic80%/Legendary20%. Hiển thị tỉ lệ và bộ đếm; không gọi đó là tỉ lệ cố định trên mọi lần mở.
- Mở rương nhận một instance và giảm1rương đúng một lần. Full inventory không tiêu rương. Có salvage đồ không mặc lấy essence; essence đổi rương qua giá cố định. Không xóa đồ player đang mặc.
- Inventory40slot, mỗi slot công cụ chỉ mặc1. Player có thể giữ hoặc salvage đồ trùng; không bắt auto-equip để tránh chọn sai build.
- Rương, gear và essence chỉ kiếm qua chơi trong prototype. Không có bán rương ngẫu nhiên bằng tiền thật.

## Cosmetic và IAP

- Skin cây/chậu/nhân vật chỉ đổi hình hoặc tint, không buff chỉ số. Theme nội dung kiếm qua chơi, không khóa gameplay bằng skin trả tiền.
- Catalog phân biệt cosmetic miễn phí, premium và gói quà nội dung cố định. Prototype cho trang bị cosmetic miễn phí và preview premium/bundle; không có giá thương mại cuối cùng hay trạng thái mua thành công giả.
- Không phát entitlement premium bằng local UI hoặc chỉ dựa trên callback client. Steam production cần AppID, cấu hình sản phẩm và receipt/entitlement được xác thực (Steam Inventory Service hoặc purchasing server).
- Giữ API mua thật ngoài prototype; không tạo checkout giả nhận tiền. Nguồn chính thức: https://partner.steamgames.com/doc/features/inventory và https://partner.steamgames.com/doc/features/microtransactions/implementation.
- Quyết định thương mại có thể đổi sau khi có dữ liệu chơi; paid randomized rewards không nằm trong bản thiết kế này.

## Data / lưu / giao diện

- ScriptableObject profile/catalog tham chiếu trực tiếp: crops, themes, upgrades, gear, drop rules, cosmetics. IDs ổn định là save key. Mọi weight, range, cost, interval chỉnh bằng Editor.
- Save plain data có schema version, migration, temp+flush+replace+backup; giữ file từ version tương lai, không ghi đè khi không đọc được. Schema hiện tại v3 migrate v1/v2. Lưu cả lựa chọn cây, công cụ/affix, thời gian dư, pity, milestone, theme và cosmetic.
- UI chính chỉ có tiền, Shop và menu. Shop chứa các thao tác vườn/nâng cấp. Inventory, thông tin item và màn phụ mở thành cửa sổ nhỏ nổi cùng kiểu khung game, nằm trong cùng Canvas/process chứ không tạo OS window riêng. Mỗi cửa sổ có thanh tiêu đề, nút đóng và kéo được trong vùng game.
- Dùng composition theo trách nhiệm: `GardenFloatingWindow` chỉ điều khiển khung/đóng/kéo; `GardenGearPanel` trình bày inventory và gọi progression; `GardenCosmeticPanel` trình bày catalog/preview và gọi `GardenCosmeticController`; controller áp tint và giữ loadout, còn catalog là ScriptableObject. `GardenHud` chỉ quản lý thanh chính và chuyển trạng thái giữa Shop/menu/cửa sổ. Các reference được serialize rõ ràng, không resolve dependency bằng lookup.
- Tất cả UI/plot GameObject được chuẩn bị bằng Editor; runtime không spawn/destroy UI hoặc lookup dependency. Dùng anchor và CanvasScaler để cửa sổ phụ ở gọn trong cửa sổ game; giữ idle30FPS.

## Giai đoạn và gate kiểm chứng

| Tag | Nội dung | Kiểm chứng bắt buộc |
|---|---|---|
| checkpoint/f02-pot-themes | Chọn cây từng chậu, theme unlock, rank upgrade | Không mất vụ hiện tại, riêng từng chậu, unlock/charge đúng1lần, rank không sửa base |
| checkpoint/f03-compact-shop | Dải cửa sổ, bốn trang và pot interaction | Play Mode, ảnh920×180/920×460, thao tác crop/upgrade/menu |
| checkpoint/f04-chests-gear | Rương thời gian/mốc, rarity affix, equip/salvage | RNG seeded, bounds/stat không trùng, pity, không double reward, effect simulation |
| checkpoint/f05-save-rebirth | Persistence hoàn chỉnh, migration, reset giữ đúng | File I/O, corrupt/future/backup, restart, giữ gear/theme/lựa chọn sau rebirth |
| checkpoint/f06-prototype | Cosmetic catalog/preview, regression + Windows build | Không phát premium giả, tests+Play+build, ghi giới hạn commerce rõ |

Sau mỗi gate: cập nhật CHECKPOINT.md, commit, annotated tag mới và push. Giữ các checkpoint F01/import cũ nguyên vẹn; các tên F04/F05 ở kế hoạch cũ được thay bằng bảng này trước khi chúng từng được tạo.
