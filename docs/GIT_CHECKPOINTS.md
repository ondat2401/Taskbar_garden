# Git checkpoint cho quay video

Remote: https://github.com/ondat2401/Taskbar_garden

Mỗi feature được kiểm tra và chốt bằng một commit cùng annotated tag. Tag đã dùng cho video không được dời/ghi đè; sửa tiếp thì thêm commit/checkpoint mới.

| Tag | Nội dung | Trạng thái |
|---|---|---|
| `checkpoint/00-import-wip` | Nhập project Unity hiện có, gồm bản nháp F01–F05 và art AI | Lưu nguyên trạng; không phải bản hoàn thiện các feature |
| `checkpoint/f01-economy` | Kho, vàng, bán tay/tự động; nối thu hoạch trong scene | Chỉ tạo sau khi F01 kiểm tra pass |

Các tag `checkpoint/f02-upgrades`, `checkpoint/f03-compact-shop`, `checkpoint/f04-save`, `checkpoint/f05-rebirth`, `checkpoint/f06-build` chỉ được tạo khi feature tương ứng hoàn tất. Xem `CHECKPOINT.md` để biết phần nào đã tích hợp và kiểm chứng.

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
