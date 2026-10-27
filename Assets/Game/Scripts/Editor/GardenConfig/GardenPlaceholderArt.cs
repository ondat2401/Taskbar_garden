using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IdleGarden.Editor
{
    public static class GardenPlaceholderArt
    {
        private const string Root = "Assets/Game/Textures/";

        public static Dictionary<string, Sprite> Generate()
        {
            var sprites = new Dictionary<string, Sprite>();
            AddSingle(sprites, "pixel", "Environment/pixel.png");
            AddSingle(sprites, "soil", "Environment/soil.png");
            AddSingle(sprites, "grass", "Environment/grass.png");
            AddSingle(sprites, "well", "Environment/well.png");
            AddSheet(sprites, "Worker/worker-animation.png");
            foreach (string crop in new[] { "carrot", "radish", "fire", "frost" })
                AddSheet(sprites, "Crops/" + crop + "-animation.png");
            foreach (string frame in new[] { "worker0", "worker1", "worker2", "worker3" })
                if (!sprites.ContainsKey(frame)) throw new InvalidOperationException("Missing worker frame: " + frame);
            foreach (string crop in new[] { "carrot", "radish", "fire", "frost" })
                for (int stage = 0; stage < 3; stage++)
                    foreach (string suffix in new[] { "", "-sway" })
                        if (!sprites.ContainsKey(crop + stage + suffix)) throw new InvalidOperationException("Missing crop frame: " + crop + stage + suffix);
            if (sprites.Count != 32) throw new InvalidOperationException($"Expected 32 separate/animated garden sprites, loaded {sprites.Count}.");
            return sprites;
        }

        private static void AddSingle(Dictionary<string, Sprite> sprites, string name, string relativePath)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + relativePath);
            if (!sprite) throw new InvalidOperationException("Missing sprite: " + relativePath);
            sprites.Add(name, sprite);
        }

        private static void AddSheet(Dictionary<string, Sprite> sprites, string relativePath)
        {
            string path = Root + relativePath;
            int before = sprites.Count;
            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
                if (asset is Sprite sprite) sprites.Add(sprite.name, sprite);
            if (sprites.Count == before) throw new InvalidOperationException("Missing animation sprites: " + relativePath);
        }
    }
}
