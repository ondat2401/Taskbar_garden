using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace IdleGarden.Editor
{
    public static class GardenThemeArt
    {
        public static void Apply()
        {
            const string path="Assets/Game/Textures/Crops/theme-growth-ai.png";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit=400; importer.filterMode=FilterMode.Point; importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed; importer.alphaIsTransparency=true; importer.maxTextureSize=2048;
            importer.SaveAndReimport();
            var factory=new SpriteDataProviderFactories(); factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            if(provider.GetSpriteRects().Length!=6)
            {
                importer.GetSourceTextureWidthAndHeight(out int width,out int height);
                var rects=new SpriteRect[6]; var pairs=new List<SpriteNameFileIdPair>();
                for(int i=0;i<6;i++)
                {
                    rects[i]=new SpriteRect {name=(i<3?"fire":"frost")+(i%3),rect=new Rect(i%3*width/3f,i<3?height/2f:0,width/3f,height/2f),pivot=new Vector2(.5f,.1f),alignment=SpriteAlignment.Custom,spriteID=GUID.Generate()};
                    pairs.Add(new SpriteNameFileIdPair(rects[i].name,rects[i].spriteID));
                }
                provider.SetSpriteRects(rects); provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(pairs); provider.Apply(); importer.SaveAndReimport();
            }
            var sprites=new Dictionary<string,Sprite>();
            foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(path)) if(asset is Sprite sprite) sprites[sprite.name]=sprite;
            foreach(string id in new[]{"fire-pepper","frost-bloom"})
            {
                var crop=AssetDatabase.LoadAssetAtPath<CropDefinition>("Assets/Game/_Configs/Crops/"+id+".asset");
                if(!crop) continue;
                string prefix=id=="fire-pepper"?"fire":"frost";
                crop.stages=new[]{sprites[prefix+0],sprites[prefix+1],sprites[prefix+2]}; crop.theme.cropTint=Color.white;
                EditorUtility.SetDirty(crop); EditorUtility.SetDirty(crop.theme); AssetDatabase.SaveAssetIfDirty(crop); AssetDatabase.SaveAssetIfDirty(crop.theme);
            }
        }
    }
}
