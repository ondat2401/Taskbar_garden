using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace IdleGarden.Editor
{
    public static class GardenContentSetup
    {
        [MenuItem("Tools/Idle Garden/Setup Themes and Upgrades F02")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) return;
            var profile = AssetDatabase.LoadAssetAtPath<GardenProfile>(GardenPrototypeBuilder.ProfilePath);
            GardenProgressionSetup.EnsureConfig(profile);
            Directory.CreateDirectory("Assets/Game/_Configs/Themes"); AssetDatabase.Refresh();
            var meadow = Theme("meadow", "Meadow", 0, Color.white);
            var ember = Theme("ember", "Ember", 250, new Color(1,.6f,.4f));
            var frost = Theme("frost", "Frost", 500, new Color(.45f,.8f,1));
            var config = profile.progression;
            var themes = new List<GardenTheme>(config.themes);
            foreach (var theme in new[] { meadow, ember, frost }) if (!themes.Contains(theme)) themes.Add(theme);
            config.themes = themes.ToArray();
            var crops = new List<CropDefinition>(config.crops);
            var fire = Crop("fire-pepper", "Fire pepper", ember, 25, 14, 1.1f, config.crops[0].stages);
            var ice = Crop("frost-bloom", "Frost bloom", frost, 35, 20, .6f, config.crops[0].stages);
            foreach (var crop in new[] { fire, ice }) if (!crops.Contains(crop)) crops.Add(crop);
            config.crops = crops.ToArray(); EditorUtility.SetDirty(config); AssetDatabase.SaveAssetIfDirty(config);
        }
        private static GardenTheme Theme(string id, string title, double cost, Color tint)
        {
            string path = "Assets/Game/_Configs/Themes/" + id + ".asset";
            var theme = AssetDatabase.LoadAssetAtPath<GardenTheme>(path); if (theme) return theme;
            theme = ScriptableObject.CreateInstance<GardenTheme>(); theme.id=id; theme.displayName=title; theme.unlockGold=cost; theme.cropTint=tint;
            AssetDatabase.CreateAsset(theme,path); return theme;
        }
        private static CropDefinition Crop(string id,string title,GardenTheme theme,float seconds,int price,float water,Sprite[] sprites)
        {
            string path = "Assets/Game/_Configs/Crops/" + id + ".asset";
            var crop = AssetDatabase.LoadAssetAtPath<CropDefinition>(path); if (crop) return crop;
            crop=ScriptableObject.CreateInstance<CropDefinition>(); crop.id=id; crop.displayName=title; crop.theme=theme;
            crop.growSeconds=seconds; crop.sellPrice=price; crop.waterPerSecond=water; crop.stages=sprites;
            AssetDatabase.CreateAsset(crop,path); return crop;
        }
    }
}
