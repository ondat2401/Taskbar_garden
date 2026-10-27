# Placeholder art palette

Bộ art này là placeholder để tiếp tục sửa. Sprite hiện dùng pixel art side-view; nền ngoài silhouette phải trong suốt để giữ click-through. Palette duy nhất nằm trong `Assets/Game/Scripts/UI/GardenUiPalette.cs`, field `SharedColors`; UI dùng tập màu nền/chữ/accent, sprite dùng thêm các tông môi trường trong cùng danh sách.

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

## Prompt tạo atlas

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

## Prompt chỉnh atlas

```text
Edit this existing sprite atlas as a precise cleanup pass. Keep the same 1536x1024 canvas, exact 6 columns x 4 rows of 256x256 cells, same cell order, same character/plant silhouettes and game theme. Preserve only these sprite objects in their assigned cells: row 1 has gardener idle, two walking frames, watering frame, rustic well, grass strip; row 2 has soil bed, carrot seedling/growing/mature, radish seedling/growing; row 3 has mature radish, fire-pepper seedling/growing/mature, frost-bloom seedling/growing; row 4 has mature frost-bloom in cell 1 and all other cells empty. Crucial: scale every sprite so the complete shape sits fully inside its own cell with at least 24 transparent pixels of padding on all four sides; nothing may cross a cell boundary. For ground assets, fit the whole strip inside its cell. Keep the 4 gardener frames identical in design and size, and all crop stages consistent.

Remove the dark backdrop, haze, vignette, glow, cast shadows, and every stray pixel outside the sprites so all unused pixels are truly fully transparent. Convert the art to clean strict pixel art: hard square pixel clusters, crisp edges, no anti-aliasing, no gradients or texture noise. Use only these exact colors for visible pixels plus full transparency: #101A15, #17241E, #1C2D23, #243B2C, #293F30, #35503A, #395A42, #3A453B, #466946, #567850, #B8C5A3, #E5B85C, #F3E9C9. Remove the unintended pink/red color from radish roots and express them with warm cream and brass from this palette. Do not add new assets, text, borders, grid lines, backdrop, pot, UI, or watermark.
```
