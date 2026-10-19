using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IdleGarden.Editor
{
    public static class GardenProgressionSetup
    {
        private static readonly Color Panel = new Color(.075f, .12f, .1f, .98f);
        private static readonly Color ButtonColor = new Color(.22f, .32f, .23f);
        private const string AtlasPath = "Assets/Game/Textures/UI/garden-icons-ai.png";

        [MenuItem("Tools/Idle Garden/Apply Compact Garden and Progression")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Exit Play Mode first."); return; }
            var garden = Object.FindFirstObjectByType<GardenController>();
            if (!garden) { Debug.LogWarning("Open the prototype scene first."); return; }
            EnsureConfig(garden.profile);
            Undo.RecordObject(garden.profile, "Use compact window");
            garden.profile.windowSize = new Vector2Int(920, 180);
            garden.profile.minimumWindowSize = new Vector2Int(560, 150);
            garden.profile.showPlotMeters = false;
            EditorUtility.SetDirty(garden.profile);
            var desktop = garden.GetComponent<DesktopWindow>();
            BuildHud(garden.transform, garden, desktop);
            GardenPrototypeBuilder.ApplyLayout(garden.profile);
            Undo.FlushUndoRecordObjects();
            EditorSceneManager.MarkSceneDirty(garden.gameObject.scene);
            EditorSceneManager.SaveScene(garden.gameObject.scene);
            AssetDatabase.SaveAssets();
        }

        public static void EnsureConfig(GardenProfile profile)
        {
            if (profile.progression) return;
            Directory.CreateDirectory("Assets/Game/_Configs/Progression");
            Directory.CreateDirectory("Assets/Game/_Configs/Upgrades"); AssetDatabase.Refresh();
            var config = AssetDatabase.LoadAssetAtPath<ProgressionConfig>("Assets/Game/_Configs/Progression/GardenEconomy.asset");
            if (!config)
            {
                var icons = ImportIcons();
                var crops = new List<CropDefinition>();
                foreach (var plot in profile.plots) if (plot.crop && !crops.Contains(plot.crop)) crops.Add(plot.crop);
                if (crops.Count == 0) throw new System.InvalidOperationException("Assign crops before setting up progression.");
                config = ScriptableObject.CreateInstance<ProgressionConfig>(); config.crops = crops.ToArray();
                CropDefinition carrot = crops[0], radish = crops.Count > 1 ? crops[1] : crops[0];
                config.upgrades = new[]
                {
                    Upgrade("carrot-growth", "Seed selection", "Carrot growth speed +25% of base per rank.", UpgradeStat.GrowSpeed, .25f, 30, carrot, carrot, Icon(icons,"seeds")),
                    Upgrade("harvest-yield", "Abundant harvest", "Harvest yield +100% of base per rank.", UpgradeStat.CropYield, 1, 50, radish, null, Icon(icons,"harvest")),
                    Upgrade("watering-can", "Larger watering can", "Water capacity +35% of base per rank.", UpgradeStat.TankCapacity, .35f, 35, radish, null, Icon(icons,"watering")),
                    Upgrade("walking", "Garden boots", "Walking speed +20% of base per rank.", UpgradeStat.MoveSpeed, .2f, 40, carrot, null, Icon(icons,"seeds")),
                    Upgrade("tools", "Better tools", "All action speeds +20% of base per rank.", UpgradeStat.ActionSpeed, .2f, 55, radish, null, Icon(icons,"watering")),
                    Upgrade("mulch", "Water-saving mulch", "Water efficiency +20% of base per rank.", UpgradeStat.WaterEfficiency, .2f, 45, carrot, null, Icon(icons,"harvest"))
                };
                AssetDatabase.CreateAsset(config, "Assets/Game/_Configs/Progression/GardenEconomy.asset");
            }
            profile.progression = config; EditorUtility.SetDirty(profile);
        }

        private static UpgradeDefinition Upgrade(string id, string title, string detail, UpgradeStat stat, float bonus, double cost, CropDefinition material, CropDefinition target, Sprite icon)
        {
            string path = "Assets/Game/_Configs/Upgrades/" + id + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(path); if (existing) return existing;
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.id = id; upgrade.displayName = title; upgrade.description = detail; upgrade.stat = stat;
            upgrade.bonusPerLevel = bonus; upgrade.baseGoldCost = cost; upgrade.material = material; upgrade.targetCrop = target; upgrade.icon = icon;
            AssetDatabase.CreateAsset(upgrade, path); return upgrade;
        }

        private static Sprite Icon(Dictionary<string, Sprite> icons, string name) => icons.TryGetValue(name, out var sprite) ? sprite : null;

        public static Dictionary<string, Sprite> ImportIcons()
        {
            var icons = new Dictionary<string, Sprite>();
            if (!File.Exists(AtlasPath)) return icons;
            AssetDatabase.ImportAsset(AtlasPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(AtlasPath);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var old = provider.GetSpriteRects();
            if (old.Length != 4)
            {
                importer.GetSourceTextureWidthAndHeight(out int width, out int height);
                string[] names = { "watering", "seeds", "harvest", "rebirth" };
                var rects = new SpriteRect[4];
                var pairs = new List<SpriteNameFileIdPair>();
                for (int i = 0; i < 4; i++)
                {
                    rects[i] = new SpriteRect { name = names[i], rect = new Rect(i % 2 * width / 2f, i < 2 ? height / 2f : 0, width / 2f, height / 2f),
                        pivot = new Vector2(.5f,.5f), alignment = SpriteAlignment.Center, spriteID = GUID.Generate() };
                    pairs.Add(new SpriteNameFileIdPair(rects[i].name, rects[i].spriteID));
                }
                provider.SetSpriteRects(rects);
                provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(pairs);
                provider.Apply(); importer.SaveAndReimport();
            }
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(AtlasPath)) if (asset is Sprite sprite) icons[sprite.name] = sprite;
            return icons;
        }

        public static void BuildHud(Transform root, GardenController controller, DesktopWindow desktop)
        {
            EnsureConfig(controller.profile);
            var old = root.Find("HUD"); if (old) Undo.DestroyObjectImmediate(old.gameObject);
            var canvasObject = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create compact HUD"); canvasObject.transform.SetParent(root, false);
            var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var hud = canvasObject.AddComponent<GardenHud>(); hud.garden = controller; hud.desktop = desktop; controller.hud = hud;
            var bar = Box("Garden Bar", canvas.transform, new Vector2(0,0), new Vector2(1,0), new Vector2(6,4), new Vector2(-6,32), Panel);
            var drag = bar.gameObject.AddComponent<WindowHandle>(); drag.window = desktop;
            hud.moneyLabel = Text("Balance", bar, "IDLE GARDEN", new Vector2(9,0), new Vector2(-120,0), 12);
            hud.moneyLabel.rectTransform.anchorMax = Vector2.one;
            hud.shopButtonLabel = Button("Shop", bar, new Vector2(1,0), Vector2.one, new Vector2(-112,2), new Vector2(-40,-2), "Shop", hud.ToggleShop);
            Button("Menu", bar, new Vector2(1,0), Vector2.one, new Vector2(-36,2), new Vector2(-18,-2), ":", hud.ToggleMenu);
            var grip = Box("Resize", bar, new Vector2(1,0), Vector2.one, new Vector2(-16,1), new Vector2(-1,-1), new Color(.32f,.42f,.28f));
            Text("Grip", grip, "/", Vector2.zero, Vector2.zero, 12, TextAnchor.MiddleCenter).Stretch();
            var handle = grip.gameObject.AddComponent<WindowHandle>(); handle.window = desktop; handle.resize = true;
            var menu = Box("Window Menu", canvas.transform, new Vector2(1,0), new Vector2(1,0), new Vector2(-210,36), new Vector2(-8,176), Panel);
            hud.menuPanel = menu.gameObject;
            hud.pinLabel = Button("Pin", menu, new Vector2(0,1), Vector2.one, new Vector2(6,-32), new Vector2(-6,-5), "Always on top", hud.TogglePin);
            hud.lockLabel = Button("Lock", menu, new Vector2(0,1), Vector2.one, new Vector2(6,-66), new Vector2(-6,-36), "Position lock", hud.ToggleLock);
            hud.pauseLabel = Button("Pause", menu, new Vector2(0,1), Vector2.one, new Vector2(6,-100), new Vector2(-6,-70), "Pause garden", hud.TogglePause);
            Button("Exit", menu, new Vector2(0,1), Vector2.one, new Vector2(6,-134), new Vector2(-6,-104), "Exit", desktop.Quit);
            menu.gameObject.SetActive(false);
            var shop = Box("Workshop", canvas.transform, Vector2.zero, Vector2.one, new Vector2(8,184), new Vector2(-8,-8), Panel);
            hud.shopPanel = shop.gameObject;
            hud.shopSummary = Text("Heading", shop, "GARDEN WORKSHOP", new Vector2(14,-32), new Vector2(-14,-5), 14, TextAnchor.MiddleLeft, true);
            hud.inventoryLabel = Text("Inventory", shop, "Stock", new Vector2(14,-57), new Vector2(-285,-33), 11, TextAnchor.MiddleLeft, true);
            hud.autoSellLabel = Button("Auto Sell", shop, new Vector2(1,1), Vector2.one, new Vector2(-280,-57), new Vector2(-144,-33), "Auto-sell", hud.ToggleAutoSell);
            Button("Sell", shop, new Vector2(1,1), Vector2.one, new Vector2(-138,-57), new Vector2(-10,-33), "Sell surplus", hud.SellSurplus);
            var viewport = Box("Upgrade Viewport", shop, Vector2.zero, Vector2.one, new Vector2(10,78), new Vector2(-10,-64), new Color(.1f,.15f,.12f));
            viewport.gameObject.AddComponent<RectMask2D>();
            int count = controller.profile.progression.upgrades.Length;
            var content = Rect("Upgrade List", viewport, new Vector2(0,1), Vector2.one, new Vector2(0,-count*64), Vector2.zero);
            content.pivot = new Vector2(.5f,1);
            var scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 24;
            hud.rows = new ShopUpgradeRow[count];
            for (int i = 0; i < count; i++)
            {
                var row = Box("Upgrade " + i, content, new Vector2(0,1), Vector2.one, new Vector2(4,-(i+1)*64+3), new Vector2(-4,-i*64-3), new Color(.14f,.21f,.16f));
                var view = row.gameObject.AddComponent<ShopUpgradeRow>(); view.owner = hud; view.upgradeIndex = i; hud.rows[i] = view;
                var icon = Rect("Icon", row, Vector2.zero, new Vector2(0,1), new Vector2(6,6), new Vector2(50,-6));
                view.icon = icon.gameObject.AddComponent<Image>(); view.icon.preserveAspect = true; view.icon.raycastTarget = false;
                view.title = Text("Title", row, "Upgrade", new Vector2(58,-27), new Vector2(-180,-3), 13, TextAnchor.MiddleLeft, true);
                view.description = Text("Effect", row, "Effect", new Vector2(58,3), new Vector2(-180,29), 11);
                view.price = Text("Price", row, "Price", new Vector2(-182,-23), new Vector2(-8,-2), 11, TextAnchor.MiddleRight, true, true);
                var buyLabel = Button("Buy", row, new Vector2(1,0), new Vector2(1,0), new Vector2(-170,5), new Vector2(-8,29), "Upgrade", view.Buy);
                view.buyButton = buyLabel.transform.parent.GetComponent<Button>();
            }
            hud.noticeLabel = Text("Notice", shop, "", new Vector2(14,40), new Vector2(-14,72), 11);
            hud.noticeLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            hud.rebirthLabel = Button("Rebirth", shop, Vector2.zero, new Vector2(1,0), new Vector2(12,8), new Vector2(-12,36), "Rebirth", hud.Rebirth);
            hud.rebirthButton = hud.rebirthLabel.transform.parent.GetComponent<Button>();
            shop.gameObject.SetActive(false);
            controller.statusLabel = hud.moneyLabel; controller.harvestLabel = hud.moneyLabel;
            EditorUtility.SetDirty(controller);
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 a, Vector2 b, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform)); var rect = (RectTransform)go.transform; rect.SetParent(parent,false);
            rect.anchorMin=a; rect.anchorMax=b; rect.offsetMin=min; rect.offsetMax=max; return rect;
        }
        private static RectTransform Box(string name, Transform parent, Vector2 a, Vector2 b, Vector2 min, Vector2 max, Color color)
        {
            var rect = Rect(name,parent,a,b,min,max); rect.gameObject.AddComponent<Image>().color=color; return rect;
        }
        private static Text Text(string name, Transform parent, string value, Vector2 min, Vector2 max, int size, TextAnchor alignment=TextAnchor.MiddleLeft, bool top=false, bool right=false)
        {
            Vector2 a = top ? new Vector2(right ? 1 : 0,1) : Vector2.zero;
            Vector2 b = top ? Vector2.one : new Vector2(1,0);
            var rect = Rect(name,parent,a,b,min,max);
            var text=rect.gameObject.AddComponent<Text>(); text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text=value; text.fontSize=size; text.alignment=alignment; text.color=new Color(.9f,.93f,.81f); text.raycastTarget=false;
            return text;
        }
        private static Text Button(string name, Transform parent, Vector2 a, Vector2 b, Vector2 min, Vector2 max, string title, UnityAction action)
        {
            var rect=Box(name,parent,a,b,min,max,ButtonColor); var button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=rect.GetComponent<Image>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick,action);
            return Text("Label",rect,title,new Vector2(4,0),new Vector2(-4,0),12,TextAnchor.MiddleCenter,false,false).Stretch();
        }
        private static Text Stretch(this Text text)
        {
            text.rectTransform.anchorMin=Vector2.zero; text.rectTransform.anchorMax=Vector2.one;
            text.rectTransform.offsetMin=new Vector2(4,0); text.rectTransform.offsetMax=new Vector2(-4,0); return text;
        }
    }
}
