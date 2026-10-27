using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace IdleGarden.Editor
{
    // Editor-only fallback sprites; the configured atlas supplies the active art.
    public static class GardenPlaceholderArt
    {
        private const int PixelBlockSize = 8;
        private static Texture2D canvas;
        private static void Box(int x, int y, int width, int height, string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            if (hex != "FFFFFF") color = GardenUiPalette.SnapToSharedColor((Color32)color);
            for (int row = y; row < y + height; row++)
                for (int col = x; col < x + width; col++)
                    if (col >= 0 && col < canvas.width && row >= 0 && row < canvas.height) canvas.SetPixel(col, row, color);
        }
        private static void Begin(int width = 32, int height = 32)
        {
            canvas = new Texture2D(width, height, TextureFormat.RGBA32, false);
            canvas.SetPixels(new Color[width * height]);
        }
        private static Sprite Save(string category, string name, Vector2 pivot, float ppu = 32)
        {
            string path = $"Assets/Game/Textures/{category}/{name}.png";
            int scaledWidth = canvas.width * PixelBlockSize;
            int scaledHeight = canvas.height * PixelBlockSize;
            var sourcePixels = canvas.GetPixels32();
            var scaledPixels = new Color32[scaledWidth * scaledHeight];
            for (int y = 0; y < canvas.height; y++)
                for (int x = 0; x < canvas.width; x++)
                {
                    Color32 color = sourcePixels[y * canvas.width + x];
                    for (int row = 0; row < PixelBlockSize; row++)
                        for (int col = 0; col < PixelBlockSize; col++)
                            scaledPixels[(y * PixelBlockSize + row) * scaledWidth + x * PixelBlockSize + col] = color;
                }
            var exportCanvas = new Texture2D(scaledWidth, scaledHeight, TextureFormat.RGBA32, false);
            exportCanvas.SetPixels32(scaledPixels);
            exportCanvas.Apply(false, false);
            File.WriteAllBytes(path, exportCanvas.EncodeToPNG());
            Object.DestroyImmediate(exportCanvas);
            Object.DestroyImmediate(canvas);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu * PixelBlockSize; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.alphaIsTransparency = true;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom; settings.spritePivot = pivot; importer.SetTextureSettings(settings);
            importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        public static Dictionary<string, Sprite> Generate()
        {
            var sprites = new Dictionary<string, Sprite>();
            const string atlasPath = "Assets/Game/Textures/Palette/garden-art-polished-v1.png";
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(atlasPath))
                if (asset is Sprite sprite) sprites[sprite.name] = sprite;
            if (sprites.Count == 19) return sprites;
            sprites.Clear();

            Begin(1, 1); Box(0, 0, 1, 1, "FFFFFF"); sprites["pixel"] = Save("Environment", "pixel", new Vector2(0, .5f), 1);
            Begin(40, 16);
            Box(0, 0, 40, 10, "513E35"); Box(1, 3, 38, 8, "8E6746"); Box(2, 6, 36, 5, "BB8857");
            for (int x = 4; x < 36; x += 7) { Box(x, 5, 3, 2, "73513D"); Box(x + 2, 9, 3, 1, "D5A36A"); }
            sprites["soil"] = Save("Environment", "soil", new Vector2(.5f, .7f));
            Begin(32, 16); Box(0, 0, 32, 10, "584739"); Box(0, 9, 32, 4, "567540"); Box(0, 12, 32, 2, "91B45C");
            for (int x = 1; x < 32; x += 5) { Box(x, 13, 2, 3, "729C4C"); Box(x + 1, 4, 2, 1, "75604A"); }
            sprites["grass"] = Save("Environment", "grass", new Vector2(.5f, 1));
            Begin(32, 40); Box(3, 0, 27, 17, "536977"); Box(5, 2, 23, 13, "8FA6AA"); Box(4, 16, 25, 4, "C6D1BD");
            Box(7, 17, 19, 2, "63A6BE"); Box(5, 20, 3, 14, "76573F"); Box(24, 20, 3, 14, "76573F");
            Box(1, 33, 30, 4, "6B4B36"); Box(4, 37, 24, 3, "A77D50"); Box(10, 4, 8, 2, "697D85");
            sprites["well"] = Save("Environment", "well", new Vector2(.5f, 0));
            for (int i = 0; i < 4; i++)
            {
                Begin(32, 48);
                Box(10, 0, 5, 8, "393E48"); Box(18, i == 1 ? 2 : 0, 5, 8, "393E48");
                Box(10, 6, 13, 13, "4C7080"); Box(9, 16, 15, 13, "8EAA69"); Box(13, 16, 7, 10, "B7C48B");
                Box(6, i == 3 ? 20 : 13, 4, 12, "F0C79D"); Box(24, i == 3 ? 20 : 13, 4, 12, "F0C79D");
                Box(10, 28, 15, 12, "F0C79D"); Box(10, 36, 15, 5, "73513D"); Box(12, 33, 2, 2, "363D3F"); Box(22, 33, 2, 2, "363D3F");
                Box(6, 39, 24, 4, "D6AD66"); Box(11, 42, 14, 5, "E5BF7D"); Box(11, 41, 14, 2, "826B43");
                if (i == 3) { Box(25, 17, 6, 7, "71A5B2"); Box(27, 24, 3, 3, "ABC9CE"); }
                sprites["worker" + i] = Save("Worker", "worker" + i, new Vector2(.5f, 0));
            }
            for (int stage = 0; stage < 3; stage++)
            {
                Begin(); Plant(stage, false); sprites["carrot" + stage] = Save("Crops", "carrot" + stage, new Vector2(.5f, 0));
                Begin(); Plant(stage, true); sprites["radish" + stage] = Save("Crops", "radish" + stage, new Vector2(.5f, 0));
            }
            return sprites;
        }
        private static void Plant(int stage, bool radish)
        {
            int height = 6 + stage * 6;
            Box(15, 1, 2, height, "668A46"); Box(9, height - 2, 7, 3, "8BB45C"); Box(17, height, 7, 3, "A5C86B");
            Box(11, height + 1, 4, 3, "648F43"); Box(17, height + 3, 4, 3, "8BB45C");
            if (stage == 0) return;
            Box(11, 1, 11, stage == 2 ? 12 : 6, radish ? "CE6C8E" : "D88C42");
            Box(13, 0, 7, 4, radish ? "EDD8C3" : "BE6B35");
            Box(13, stage == 2 ? 8 : 4, 6, 2, radish ? "E7A1B2" : "F1B25A");
        }
    }
}
