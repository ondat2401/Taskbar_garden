using UnityEditor;
using UnityEngine;

namespace IdleGarden.Editor
{
    public static class GardenThemeArt
    {
        public static void Apply()
        {
            var sprites = GardenPlaceholderArt.Generate();
            foreach(string id in new[]{"fire-pepper","frost-bloom"})
            {
                var crop=AssetDatabase.LoadAssetAtPath<CropDefinition>("Assets/Game/_Configs/Crops/"+id+".asset");
                if(!crop) continue;
                string prefix=id=="fire-pepper"?"fire":"frost";
                crop.stages=new[]{sprites[prefix+"0"],sprites[prefix+"1"],sprites[prefix+"2"]}; crop.theme.cropTint=Color.white;
                EditorUtility.SetDirty(crop); EditorUtility.SetDirty(crop.theme); AssetDatabase.SaveAssetIfDirty(crop); AssetDatabase.SaveAssetIfDirty(crop.theme);
            }
        }
    }
}
