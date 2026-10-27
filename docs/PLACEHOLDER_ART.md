# Placeholder art palette

Bộ art đang là placeholder. Rule mới cho asset pixel art dùng palette 8×8 gồm 64 màu và pixel logic phóng đúng 8×8 texel; nền ngoài silhouette trong suốt. Palette ảnh 128×128 ở [garden-art-palette-8x8.png](../Assets/Game/Textures/Palette/garden-art-palette-8x8.png), mỗi swatch 16×16; màu máy đọc nằm trong `GardenUiPalette.SharedColors`.

| Hàng | Màu theo thứ tự trái sang phải |
|---|---|
| 1 | `#101A15` `#17241E` `#1C2D23` `#243B2C` `#293F30` `#3A453B` `#45362E` `#755139` |
| 2 | `#35503A` `#395A42` `#466946` `#567850` `#7CA05E` `#A07B4F` `#7098A0` `#B8C5A3` |
| 3 | `#CD7D3D` `#BE6C7A` `#DCD0B0` `#E5B85C` `#F3E9C9` `#ABC17A` `#FF714A` `#F7B5E3` |
| 4 | `#8F2E48` `#B83B52` `#D94D5D` `#F26B62` `#FF8A70` `#F4518B` `#E93C6F` `#FFB2C4` |
| 5 | `#A84C27` `#D55D26` `#EF7428` `#FF8F2E` `#FFAA36` `#FFC34B` `#FFDB5C` `#FFF0A0` |
| 6 | `#32652E` `#458833` `#5AAE3C` `#73C94B` `#8CDE57` `#ABE96C` `#C8F58A` `#E6FFB8` |
| 7 | `#126A68` `#168D7A` `#20AE90` `#39D3B0` `#5BCBE3` `#3A9DDF` `#4678D4` `#A0D8FF` |
| 8 | `#5E398B` `#764CA8` `#925CC7` `#AF6CDE` `#CA83EF` `#E2A0FF` `#EB75C5` `#FFFFFF` |

## Rule tạo asset mới

- Chọn màu trực tiếp từ đúng một ô trong palette; không thêm màu trung gian. Alpha chỉ được trong suốt hoàn toàn hoặc đục hoàn toàn.
- Vẽ trên grid pixel logic rồi phóng bằng nearest-neighbor hệ số nguyên 8. Kích thước texture chia hết cho 8; không kéo méo, blur, anti-alias, gradient, glow, hạt/nhiễu hoặc lem màu.
- Mỗi khối 8×8 texel phải phẳng một màu. Nếu có nguồn Imagegen, xem đó là draft rồi lượng tử hóa về grid và 64 màu trước khi nhập Unity.
- Import sprite bằng Point, không mipmap, không compression; giữ aspect ratio và chỉ scale theo bội số nguyên.
- Trước khi duyệt, xác nhận kích thước, alpha, tập màu con của 64 swatch và tính đồng nhất từng khối 8×8.

Texture mẫu chờ đánh giá: [garden-flower-palette8-preview.png](../Assets/Game/Textures/Palette/garden-flower-palette8-preview.png), 256×256 px, canvas logic 32×32, mỗi điểm 8×8 px. Đây là sibling asset chưa nối vào scene/config. Atlas đang chạy vẫn là [garden-art-polished-v1.png](../Assets/Game/Textures/Palette/garden-art-polished-v1.png); chưa rebuild atlas hoặc thay sprite trước khi duyệt mẫu. Palette mới giữ nguyên đủ 22 màu RGB cũ của atlas và UI, đồng thời thêm màu tươi sáng hơn. Editor fallback cũng snap màu về palette, xuất block 8×8 và nhân PPU lên 8× để giữ nguyên kích thước world.

Nguồn mẫu: built-in Imagegen ngày 2026-10-27, không dùng ảnh tham chiếu; draft 1254×1254 được alpha-crop, snap màu và upscale nearest-neighbor. Prompt: “one small, readable, cheerful flowering garden crop with leaves and one bright orange flower, side-view/front-facing orthographic; transparent background, centered, no ground or pot; chunky square pixel art, vivid greens/orange/cream; no anti-aliasing, gradients, shadows, texture, noise, grid, text, blur or glow.”

| Vai trò | Màu |
|---|---|
| Outline/panel | `#101A15` `#17241E` `#1C2D23` `#243B2C` `#293F30` |
| Moss/UI | `#35503A` `#395A42` `#3A453B` `#466946` `#567850` |
| Lá | `#7CA05E` `#ABC17A` `#B8C5A3` |
| Đất | `#45362E` `#755139` `#A07B4F` |
| Nước/thu hoạch/hoa | `#7098A0` `#CD7D3D` `#BE6C7A` `#DCD0B0` `#E5B85C` `#F3E9C9` |

Atlas đang dùng là [garden-art-polished-v1.png](../Assets/Game/Textures/Palette/garden-art-polished-v1.png). Atlas ban đầu vẫn được giữ tại [garden-placeholder-atlas.png](../Assets/Game/Textures/Palette/garden-placeholder-atlas.png), cùng ảnh nguồn [garden-placeholder-atlas-source.png](../Assets/Game/Textures/Palette/garden-placeholder-atlas-source.png).

F09 tạo bản art ứng viên 1536×1024 rồi chuẩn hóa trong Unity Editor: giữ alpha/silhouette của atlas cũ, snap pixel nhìn thấy về 22 màu `GardenUiPalette.SharedColors`, sao chép 19 custom slices và giữ importer Point, không mipmap, không compression, PPU 170. Bản raw có glow/nền và màu ngoài palette nên không được nhập trực tiếp. Atlas mới vẫn là placeholder có thể thay tiếp, không phải art cuối.

Sprite names trong atlas được tham chiếu trực tiếp bởi ScriptableObject: hàng 1 `worker0` idle, `worker1`/`worker2` walk, `worker3` work, `well`, `grass`; hàng 2 `soil`, `carrot0`–`carrot2`, `radish0`–`radish1`; hàng 3 `radish2`, `fire0`–`fire2`, `frost0`–`frost1`; hàng 4 `frost2`. Sprite rectangles là custom vì nội dung atlas tràn khỏi hàng lưới đều; giữ nguyên tên/slice IDs khi sửa sheet. Player sortingOrder 4, soil 2, crop 5.

Nguồn atlas gốc: built-in Imagegen ngày 2026-10-20, không dùng ảnh tham chiếu hay asset bên ngoài. Bản polished v1 tạo ngày 2026-10-27 từ atlas đó làm reference; alpha gốc và custom slices được giữ để bảo toàn reference. Khi thay tiếp, giữ kích thước 1536×1024, 19 tên sprite, custom rectangles, alpha trong suốt và palette chung.

## Prompt F09 tạo atlas (lưu lịch sử; không dùng cho asset mới)

```text
Create a transparent PNG sprite atlas for a small cozy desktop idle-garden game, as replaceable placeholder art. Landscape image 1536x1024 pixels, an EXACT regular 6-column by 4-row grid of equal 256x256 pixel cells, no gutters, no grid lines, no labels or text. Fully transparent background in every unused area and between silhouettes. Each occupied cell has one isolated sprite, centered horizontally and bottom-aligned to the same baseline at 224 px. The artwork itself is crisp low-resolution pixel art (design on a 32x48 pixel grid, then enlarge each pixel uniformly), hard square edges, flat fills, simple readable silhouettes, no anti-aliasing, no gradients, no glow, no noise, no 3D, no perspective. Side-view/front-facing orthographic game sprites.

Use ONLY these exact opaque colors plus transparency: #101A15 outline, #17241E deepest shadow, #1C2D23 deep green, #243B2C foliage shadow, #293F30 dark green, #35503A forest green, #395A42 leaf green, #3A453B neutral dark, #466946 mid moss, #567850 bright moss, #B8C5A3 sage, #E5B85C brass gold, #F3E9C9 warm cream. This must harmonize with a moss-green, parchment and brass game UI. Keep shadow/highlight direction consistent. Do not add any colors.

Cell contents in reading order, top row first, left to right:
Row 1: (1) gardener idle, (2) same gardener walking left stride, (3) same gardener walking right stride, (4) same gardener watering with a small tilted watering can; maintain identical head, cream face, brass straw hat, moss tunic, dark trousers and body proportions in all 4 frames; (5) a compact rustic well with a roof, wooden supports, pale sage stone basin and visible water; (6) a short horizontal seamless grass ground strip, green turf over dark soil.
Row 2: (1) a short horizontal cultivated soil bed, dark outline, warm dark-green soil texture with a slim mossy top edge; (2) tiny carrot seedling; (3) medium carrot plant; (4) mature leafy carrot with a small brass-gold carrot body partly visible; (5) tiny radish seedling; (6) medium radish plant.
Row 3: (1) mature leafy radish with a cream root; (2) tiny fire-pepper seedling; (3) medium fire-pepper plant; (4) mature fire-pepper plant with brass-gold peppers; (5) tiny frost-bloom seedling; (6) medium frost-bloom plant.
Row 4: (1) mature frost-bloom with pale-cream/sage petals; cells (2) through (6) completely transparent and empty.
Keep all three frames of each crop as the exact same plant species and silhouette language, becoming larger in later stages; keep every character animation frame recognizably the same gardener. Soil and grass are flat side-view ground assets with no cast shadow outside the tile. No pot, because pots are a separate game object. No full scene, backdrop, border, checkerboard, UI, symbols, watermark, text, or extra objects. Preserve generous transparent margins so sprites can be sliced from equal grid cells.
```

## Prompt F09 chỉnh atlas (lưu lịch sử; không dùng cho asset mới)

```text
Edit this existing sprite atlas as a precise cleanup pass. Keep the same 1536x1024 canvas, exact 6 columns x 4 rows of 256x256 cells, same cell order, same character/plant silhouettes and game theme. Preserve only these sprite objects in their assigned cells: row 1 has gardener idle, two walking frames, watering frame, rustic well, grass strip; row 2 has soil bed, carrot seedling/growing/mature, radish seedling/growing; row 3 has mature radish, fire-pepper seedling/growing/mature, frost-bloom seedling/growing; row 4 has mature frost-bloom in cell 1 and all other cells empty. Crucial: scale every sprite so the complete shape sits fully inside its own cell with at least 24 transparent pixels of padding on all four sides; nothing may cross a cell boundary. For ground assets, fit the whole strip inside its cell. Keep the 4 gardener frames identical in design and size, and all crop stages consistent.

Remove the dark backdrop, haze, vignette, glow, cast shadows, and every stray pixel outside the sprites so all unused pixels are truly fully transparent. Convert the art to clean strict pixel art: hard square pixel clusters, crisp edges, no anti-aliasing, no gradients or texture noise. Use only these exact colors for visible pixels plus full transparency: #101A15, #17241E, #1C2D23, #243B2C, #293F30, #35503A, #395A42, #3A453B, #466946, #567850, #B8C5A3, #E5B85C, #F3E9C9. Remove the unintended pink/red color from radish roots and express them with warm cream and brass from this palette. Do not add new assets, text, borders, grid lines, backdrop, pot, UI, or watermark.
```
