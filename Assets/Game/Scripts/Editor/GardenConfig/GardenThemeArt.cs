using UnityEditor;
using UnityEngine;

namespace IdleGarden.Editor
{
    public static class GardenThemeArt
    {
        public static void Apply()
        {
            var sprites = GardenPlaceholderArt.Generate();
            foreach (string[] crop in new[] { new[] { "Carrot", "carrot" }, new[] { "Radish", "radish" }, new[] { "fire-pepper", "fire" }, new[] { "frost-bloom", "frost" } })
            {
                var definition = AssetDatabase.LoadAssetAtPath<CropDefinition>("Assets/Game/_Configs/Crops/" + crop[0] + ".asset");
                if (!definition) continue;
                string prefix = crop[1];
                definition.stages = new[] { sprites[prefix + "0"], sprites[prefix + "1"], sprites[prefix + "2"] };
                definition.swayStages = new[] { sprites[prefix + "0-sway"], sprites[prefix + "1-sway"], sprites[prefix + "2-sway"] };
                definition.animationFps = 2;
                if (definition.theme) { definition.theme.cropTint = Color.white; EditorUtility.SetDirty(definition.theme); AssetDatabase.SaveAssetIfDirty(definition.theme); }
                EditorUtility.SetDirty(definition); AssetDatabase.SaveAssetIfDirty(definition);
            }
        }
    }
}
