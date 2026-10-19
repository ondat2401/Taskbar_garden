using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace IdleGarden.Editor
{
    public static class GardenEconomySetup
    {
        [MenuItem("Tools/Idle Garden/Enable Economy F01")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Exit Play Mode before changing the economy catalog."); return; }
            var garden = UnityEngine.Object.FindFirstObjectByType<GardenController>();
            if (!garden) { Debug.LogWarning("Open the garden scene first."); return; }
            EnsureConfig(garden.profile);
        }

        public static void EnsureConfig(GardenProfile profile)
        {
            if (!profile || EditorApplication.isPlaying || profile.progression) return;
            const string path = "Assets/Game/_Configs/Progression/GardenEconomy.asset";
            var config = AssetDatabase.LoadAssetAtPath<ProgressionConfig>(path);
            if (!config)
            {
                var crops = new List<CropDefinition>();
                foreach (var plot in profile.plots) if (plot?.crop && !crops.Contains(plot.crop)) crops.Add(plot.crop);
                if (crops.Count == 0) throw new InvalidOperationException("Assign plot crops first.");
                Directory.CreateDirectory("Assets/Game/_Configs/Progression"); AssetDatabase.Refresh();
                config = ScriptableObject.CreateInstance<ProgressionConfig>();
                config.crops = crops.ToArray(); config.upgrades = Array.Empty<UpgradeDefinition>();
                AssetDatabase.CreateAsset(config, path);
            }
            if (!config.Validate(out string error)) throw new InvalidOperationException(error);
            Undo.RecordObject(profile, "Enable garden economy"); profile.progression = config;
            EditorUtility.SetDirty(profile); AssetDatabase.SaveAssetIfDirty(profile); AssetDatabase.SaveAssetIfDirty(config);
        }
    }
}
