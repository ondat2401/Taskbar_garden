using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IdleGarden.Editor
{
    public static class GardenCosmeticSetup
    {
        private const string CatalogPath = "Assets/Game/_Configs/Cosmetics/GardenCosmetics.asset";
        [MenuItem("Tools/Idle Garden/Setup F06 Cosmetic Preview")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Exit Play Mode first."); return; }
            var garden = Object.FindFirstObjectByType<GardenController>();
            var hud = garden ? garden.hud : null;
            if (!garden || !hud || !hud.menuPanel)
            { Debug.LogError("Open the prototype scene with its generated HUD before setting up F06."); return; }

            var catalog = EnsureCatalog();
            Undo.RecordObject(garden, "Connect cosmetic controller");
            var cosmeticController = garden.cosmetics;
            if (!cosmeticController) cosmeticController = Undo.AddComponent<GardenCosmeticController>(garden.gameObject);
            Undo.RecordObject(cosmeticController, "Configure cosmetic controller");
            cosmeticController.garden = garden;
            cosmeticController.catalog = catalog;
            garden.cosmetics = cosmeticController;

            var canvas = hud.transform;
            var canvasRect = hud.GetComponent<RectTransform>();
            var oldPanel = canvas.Find("Cosmetic Catalog Window");
            if (oldPanel) Undo.DestroyObjectImmediate(oldPanel.gameObject);
            var oldButton = hud.menuPanel.transform.Find("Cosmetics");
            if (oldButton) Undo.DestroyObjectImmediate(oldButton.gameObject);

            MakeButton("Cosmetics", hud.menuPanel.transform, new Vector2(0, 1), Vector2.one,
                new Vector2(6, -140), new Vector2(-6, -116), "Cosmetics", hud.ToggleCosmetics);
            BuildWindow(canvas, canvasRect, garden, hud);

            EditorUtility.SetDirty(garden);
            EditorUtility.SetDirty(hud);
            EditorUtility.SetDirty(cosmeticController);
            EditorSceneManager.MarkSceneDirty(garden.gameObject.scene);
            EditorSceneManager.SaveScene(garden.gameObject.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("F06 cosmetic catalog and floating preview window set up.");
        }

        private static GardenCosmeticCatalog EnsureCatalog()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CatalogPath));
            AssetDatabase.Refresh();
            var catalog = AssetDatabase.LoadAssetAtPath<GardenCosmeticCatalog>(CatalogPath);
            if (catalog) return catalog;

            catalog = ScriptableObject.CreateInstance<GardenCosmeticCatalog>();
            catalog.items = new[]
            {
                Cosmetic("mint-apron", "Mint Apron", "A fresh green outfit for the gardener.", GardenCosmeticTarget.Worker, GardenCosmeticOffer.Free,
                    new Color(.72f,.88f,.68f), Color.white, Color.white),
                Cosmetic("blue-glaze", "Blue Glaze", "A cool glaze for every pot.", GardenCosmeticTarget.Pot, GardenCosmeticOffer.Free,
                    Color.white, new Color(.60f,.80f,.91f), Color.white),
                Cosmetic("golden-sprouts", "Golden Sprouts", "Warm crop colors with no gameplay bonus.", GardenCosmeticTarget.Crop, GardenCosmeticOffer.Free,
                    Color.white, Color.white, new Color(1f,.78f,.42f)),
                Cosmetic("dusk-gardener", "Dusk Gardener", "Premium outfit preview; no purchase is available.", GardenCosmeticTarget.Worker, GardenCosmeticOffer.Premium,
                    new Color(.73f,.60f,.88f), Color.white, Color.white),
                Cosmetic("rose-bloom", "Rose Bloom", "Premium flower tint preview only.", GardenCosmeticTarget.Crop, GardenCosmeticOffer.Premium,
                    Color.white, Color.white, new Color(.93f,.62f,.72f)),
                Cosmetic("moonlit-garden", "Moonlit Garden", "Fixed-content set: gardener, blue pots, and soft green crops.", GardenCosmeticTarget.Set, GardenCosmeticOffer.FixedBundle,
                    new Color(.68f,.62f,.87f), new Color(.53f,.77f,.85f), new Color(.70f,.85f,.60f))
            };
            AssetDatabase.CreateAsset(catalog, CatalogPath);
            AssetDatabase.SaveAssetIfDirty(catalog);
            return catalog;
        }

        private static GardenCosmeticDefinition Cosmetic(string id, string title, string detail, GardenCosmeticTarget target,
            GardenCosmeticOffer offer, Color worker, Color pot, Color crop) => new GardenCosmeticDefinition
        {
            id = id, displayName = title, description = detail, target = target, offer = offer,
            workerTint = worker, potTint = pot, cropTint = crop
        };

        private static void BuildWindow(Transform canvas, RectTransform canvasRect, GardenController garden, GardenHud hud)
        {
            var root = Box("Cosmetic Catalog Window", canvas, new Vector2(.5f,.5f), new Vector2(.5f,.5f),
                new Vector2(-320,-210), new Vector2(320,210), GardenUiPalette.Panel);
            var header = Box("Cosmetic Window Header", root, new Vector2(0,1), Vector2.one,
                new Vector2(0,-32), Vector2.zero, GardenUiPalette.Header);
            hud.cosmeticWindow = header.gameObject.AddComponent<GardenFloatingWindow>();
            hud.cosmeticWindow.panel = root;
            hud.cosmeticWindow.canvasRect = canvasRect;
            MakeText("Window Title", header, "GARDEN COSMETICS  /  PREVIEW", new Vector2(12,-28), new Vector2(-54,-4), 13, TextAnchor.MiddleLeft, true);
            MakeButton("Close Cosmetics", header, new Vector2(1,1), Vector2.one,
                new Vector2(-42,-28), new Vector2(-6,-4), "×", hud.CloseCosmetics);

            hud.cosmeticPanel = root.gameObject.AddComponent<GardenCosmeticPanel>();
            hud.cosmeticPanel.garden = garden;
            hud.cosmeticPanel.equippedLabel = MakeText("Equipped Cosmetics", root, "Equipped", new Vector2(264,-58), new Vector2(-12,-36), 11, TextAnchor.MiddleLeft, true);
            hud.cosmeticPanel.selectedTitle = MakeText("Selected Cosmetic", root, "Choose a look", new Vector2(264,-88), new Vector2(-12,-64), 13, TextAnchor.MiddleLeft, true);
            hud.cosmeticPanel.description = MakeText("Cosmetic Description", root, "Preview a cosmetic on the live garden.", new Vector2(264,-144), new Vector2(-12,-94), 12, TextAnchor.UpperLeft, true);
            hud.cosmeticPanel.description.color = GardenUiPalette.TextMuted;
            hud.cosmeticPanel.description.horizontalOverflow = HorizontalWrapMode.Wrap;

            var viewport = Box("Cosmetic Catalog Viewport", root, new Vector2(0,1), new Vector2(0,1),
                new Vector2(12,-402), new Vector2(250,-66), GardenUiPalette.Surface);
            viewport.gameObject.AddComponent<RectMask2D>();
            int count = garden.cosmetics.catalog.items.Length;
            var content = Rect("Cosmetic Catalog Rows", viewport, new Vector2(0,1), Vector2.one,
                new Vector2(0,-count * 44), Vector2.zero);
            content.pivot = new Vector2(.5f,1);
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 36;
            hud.cosmeticPanel.rows = new GardenCosmeticRow[count];
            for (int i = 0; i < count; i++)
            {
                var row = Box("Cosmetic " + i, content, new Vector2(0,1), Vector2.one,
                    new Vector2(2,-(i + 1) * 44 + 2), new Vector2(-2,-i * 44 - 2), GardenUiPalette.Row);
                var button = row.gameObject.AddComponent<Button>(); button.targetGraphic = row.GetComponent<Image>();
                GardenUiPalette.StyleButton(button);
                var view = row.gameObject.AddComponent<GardenCosmeticRow>();
                view.owner = hud.cosmeticPanel; view.button = button; view.index = i;
                view.label = MakeText("Cosmetic Name", row, "Cosmetic", new Vector2(8,-40), new Vector2(-42,0), 11, TextAnchor.MiddleLeft, true);
                var swatch = Box("Color Swatch", row, new Vector2(1,.5f), new Vector2(1,.5f),
                    new Vector2(-30,-10), new Vector2(-8,10), Color.white);
                view.swatch = swatch.GetComponent<Image>();
                UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, view.Select);
                hud.cosmeticPanel.rows[i] = view;
            }

            hud.cosmeticPanel.previewButtonLabel = MakeButton("Preview Cosmetic", root, new Vector2(0,0), new Vector2(0,0),
                new Vector2(264,40), new Vector2(438,70), "Preview on garden", hud.cosmeticPanel.PreviewSelected);
            hud.cosmeticPanel.previewButton = hud.cosmeticPanel.previewButtonLabel.transform.parent.GetComponent<Button>();
            hud.cosmeticPanel.equipButtonLabel = MakeButton("Equip Cosmetic", root, new Vector2(1,0), new Vector2(1,0),
                new Vector2(-166,40), new Vector2(-12,70), "Equip free", hud.cosmeticPanel.EquipSelected);
            hud.cosmeticPanel.equipButton = hud.cosmeticPanel.equipButtonLabel.transform.parent.GetComponent<Button>();
            hud.cosmeticPanel.noticeLabel = MakeText("Cosmetic Notice", root,
                "Premium and fixed bundles are preview only. Checkout is not available.", new Vector2(264,78), new Vector2(-12,110), 10, TextAnchor.MiddleLeft, false);
            hud.cosmeticPanel.noticeLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            root.gameObject.SetActive(false);
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform; rect.SetParent(parent, false);
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
            return rect;
        }

        private static RectTransform Box(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var rect = Rect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var image = rect.gameObject.AddComponent<Image>(); image.color = color;
            GardenUiPalette.AddBorder(image);
            return rect;
        }

        private static Text MakeText(string name, Transform parent, string value, Vector2 offsetMin, Vector2 offsetMax,
            int size, TextAnchor alignment, bool top)
        {
            Vector2 anchorMin = top ? new Vector2(0,1) : Vector2.zero;
            Vector2 anchorMax = top ? Vector2.one : new Vector2(1,0);
            var rect = Rect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.text = value; text.fontSize = size;
            text.alignment = alignment; text.color = GardenUiPalette.TextPrimary;
            text.fontStyle = size >= 13 ? FontStyle.Bold : FontStyle.Normal; text.raycastTarget = false;
            return text;
        }

        private static Text MakeButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, string title, UnityAction action)
        {
            var rect = Box(name, parent, anchorMin, anchorMax, offsetMin, offsetMax, GardenUiPalette.Button);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = rect.GetComponent<Image>();
            GardenUiPalette.StyleButton(button);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, action);
            var label = MakeText("Label", rect, title, new Vector2(4,0), new Vector2(-4,0), 12, TextAnchor.MiddleCenter, false);
            label.fontStyle = FontStyle.Bold;
            label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(4,0); label.rectTransform.offsetMax = new Vector2(-4,0);
            return label;
        }
    }
}
