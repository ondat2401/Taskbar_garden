using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace IdleGarden.Editor
{
    public sealed class GardenConfigWindow : EditorWindow
    {
        private GardenProfile profile;
        private GardenController controller;
        private VisualElement fields;
        private Label runtime;
        private HelpBox validation;
        private int tab;

        [MenuItem("Tools/Idle Garden/Garden Config")]
        public static void Open() => GetWindow<GardenConfigWindow>("Garden Config");

        public void CreateGUI()
        {
            minSize = new Vector2(470, 540);
            profile = AssetDatabase.LoadAssetAtPath<GardenProfile>(GardenPrototypeBuilder.ProfilePath);
            var root = rootVisualElement;
            root.style.paddingLeft = root.style.paddingRight = 14;
            root.style.paddingTop = 12;
            var title = new Label("IDLE GARDEN / PROTOTYPE");
            title.style.fontSize = 20; title.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(title);
            root.Add(new Label("Configure • Preview • Observe"));
            var picker = new ObjectField("Garden Profile") { objectType = typeof(GardenProfile), value = profile };
            picker.RegisterValueChangedCallback(e => { profile = e.newValue as GardenProfile; DrawFields(); });
            root.Add(picker);
            root.Add(new HelpBox("Numbers and art can be edited during Play. Plot count, crop assignment and spacing apply outside Play via Apply Layout. Runtime state never writes into config.", HelpBoxMessageType.Info));
            var actions = new VisualElement(); actions.style.flexDirection = FlexDirection.Row;
            actions.Add(new Button(() => { GardenPrototypeBuilder.CreatePrototype(); profile = AssetDatabase.LoadAssetAtPath<GardenProfile>(GardenPrototypeBuilder.ProfilePath); picker.value = profile; DrawFields(); }) { text = "Open / Create Prototype" });
            actions.Add(new Button(() => { if (profile) GardenPrototypeBuilder.ApplyLayout(profile); }) { text = "Apply Layout" });
            actions.Add(new Button(AssetDatabase.SaveAssets) { text = "Save Config" });
            root.Add(actions);
            var tabs = new Toolbar();
            string[] names = { "Garden", "Crops", "Worker", "Runtime", "Progression" };
            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                tabs.Add(new ToolbarButton(() => { tab = index; DrawFields(); }) { text = names[i] });
            }
            root.Add(tabs);
            validation = new HelpBox("", HelpBoxMessageType.Warning); root.Add(validation);
            fields = new ScrollView(); fields.style.flexGrow = 1; root.Add(fields);
            root.schedule.Execute(RefreshRuntime).Every(250);
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            DrawFields();
        }

        private void OnDisable() { EditorApplication.playModeStateChanged -= OnPlayModeChanged; }
        private void OnPlayModeChanged(PlayModeStateChange state) { controller = null; DrawFields(); }
        private void DrawFields()
        {
            if (fields == null) return;
            fields.Unbind(); fields.Clear(); runtime = null;
            if (!profile) { fields.Add(new Label("Select a profile, or create the prototype.")); return; }
            var serialized = new SerializedObject(profile);
            if (tab == 0)
            {
                Add(serialized, "simulationSpeed");
                var layout = new VisualElement(); layout.SetEnabled(!EditorApplication.isPlaying);
                fields.Add(layout);
                foreach (string property in new[] { "worker", "progression", "plots", "plotSpacing", "wellDistance", "showPlotMeters", "soilSprite", "wellSprite", "grassSprite", "windowSize", "minimumWindowSize" })
                    Add(serialized, property, layout);
                fields.Add(new Label("Add plots with + in the list, assign a crop, then Apply Layout."));
            }
            else if (tab == 1)
            {
                var seen = new System.Collections.Generic.HashSet<CropDefinition>();
                foreach (var plot in profile.plots)
                {
                    if (plot == null || !plot.crop || !seen.Add(plot.crop)) continue;
                    var crop = plot.crop;
                    var foldout = new Foldout { text = crop.displayName, value = true }; fields.Add(foldout);
                    var cropData = new SerializedObject(crop);
                    foreach (string property in new[] { "id", "displayName", "growSeconds", "waterPerSecond", "waterCapacity", "waterThreshold", "yield", "sellPrice", "stages" })
                        Add(cropData, property, foldout);
                    var preview = new VisualElement(); preview.style.flexDirection = FlexDirection.Row;
                    if (crop.stages != null)
                        foreach (var sprite in crop.stages)
                            if (sprite) preview.Add(new Image { sprite = sprite, scaleMode = ScaleMode.ScaleToFit, style = { width = 72, height = 72 } });
                    foldout.Add(preview);
                }
                fields.Add(new Button(() =>
                {
                    string path = EditorUtility.SaveFilePanelInProject("New crop", "NewCrop", "asset", "Create a crop definition", "Assets/Game/_Configs/Crops");
                    if (string.IsNullOrEmpty(path)) return;
                    var crop = CreateInstance<CropDefinition>(); crop.id = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                    crop.displayName = crop.name = System.IO.Path.GetFileNameWithoutExtension(path);
                    AssetDatabase.CreateAsset(crop, path); Selection.activeObject = crop;
                }) { text = "+ Create Crop Asset" });
            }
            else if (tab == 2 && profile.worker)
            {
                var worker = new SerializedObject(profile.worker);
                foreach (string property in new[] { "moveSpeed", "plantSeconds", "waterSeconds", "harvestSeconds", "refillSeconds", "tankCapacity", "waterPerAction", "animationFps", "idleFrames", "walkFrames", "workFrames" })
                    Add(worker, property);
            }
            else if (tab == 3)
            {
                var row = new VisualElement(); row.style.flexDirection = FlexDirection.Row;
                row.Add(new Button(() => { if (controller && controller.Simulation != null) controller.TogglePause(); }) { text = "Pause / Resume" });
                foreach (float speed in new[] { 1f, 2f, 5f })
                    row.Add(new Button(() => { Undo.RecordObject(profile, "Simulation speed"); profile.simulationSpeed = speed; EditorUtility.SetDirty(profile); }) { text = $"×{speed}" });
                fields.Add(row);
                var running = EditorApplication.isPlaying ? FindFirstObjectByType<GardenController>() : null;
                if (running && running.Progression != null)
                {
                    var progress = running.Progression;
                    fields.Add(new Button(() => { progress.AutoSell = !progress.AutoSell; RefreshRuntime(); }) { text = "Toggle Auto-sell" });
                    fields.Add(new Button(() => { progress.SellSurplus(); RefreshRuntime(); }) { text = "Sell surplus (keep reserves)" });
                    foreach (var crop in progress.Config.crops)
                    {
                        var selectedCrop = crop;
                        fields.Add(new Button(() => { progress.Sell(selectedCrop, 1); RefreshRuntime(); }) { text = "Sell 1 " + crop.displayName });
                    }
                }
                runtime = new Label(); runtime.style.whiteSpace = WhiteSpace.Normal; runtime.style.fontSize = 13; fields.Add(runtime);
                RefreshRuntime();
            }
            else if (tab == 4)
            {
                if (!profile.progression)
                {
                    fields.Add(new Button(() => { GardenEconomySetup.EnsureConfig(profile); DrawFields(); }) { text = "Create Economy Config (F01)" });
                    return;
                }
                fields.Add(new HelpBox("Stable IDs are save keys. Do not rename IDs on shipped content. After changing the upgrade list, exit Play and Apply Compact Garden and Progression to rebuild shop rows.", HelpBoxMessageType.Info));
                var config = new SerializedObject(profile.progression);
                foreach (string property in new[] { "crops", "upgrades", "startingGold", "autoSellReserve", "rebirthBaseGoal", "rebirthGoalGrowth", "goldBonusPerSeed" }) Add(config, property);
                foreach (var upgrade in profile.progression.upgrades)
                {
                    if (!upgrade) continue;
                    var foldout = new Foldout { text = upgrade.displayName, value = false }; fields.Add(foldout);
                    var data = new SerializedObject(upgrade);
                    foreach (string property in new[] { "id", "displayName", "description", "icon", "stat", "targetCrop", "bonusPerLevel", "baseGoldCost", "costGrowth", "material", "baseMaterialCost", "materialStepLevels" }) Add(data, property, foldout);
                }
                fields.Add(new Button(GardenProgressionSetup.Apply) { text = "Apply Compact Garden and Progression" });
            }
        }

        private void Add(SerializedObject data, string property, VisualElement parent = null)
        {
            var field = new PropertyField(data.FindProperty(property));
            (parent ?? fields).Add(field); field.Bind(data);
        }

        private void RefreshRuntime()
        {
            if (!profile || validation == null) return;
            bool valid = profile.Validate(out string error) && profile.ValidateVisuals(out error);
            validation.text = valid ? "Config valid. Layout changes require Apply Layout before Play." : error;
            validation.messageType = valid ? HelpBoxMessageType.Info : HelpBoxMessageType.Error;
            if (runtime == null) return;
            if (!EditorApplication.isPlaying) { runtime.text = "Enter Play Mode to inspect runtime."; return; }
            if (!controller) controller = FindFirstObjectByType<GardenController>(); // Editor-only inspection.
            var sim = controller ? controller.Simulation : null;
            if (sim == null) { runtime.text = "No running garden in this scene."; return; }
            var text = new System.Text.StringBuilder();
            var progress = controller.Progression;
            if (progress != null)
            {
                text.AppendLine($"Gold: {GardenProgression.Number(progress.Gold)} | Earned: {GardenProgression.Number(progress.CycleGold)} | Auto-sell: {progress.AutoSell} | Reserve: {progress.Config.autoSellReserve}");
                foreach (var crop in progress.Config.crops) text.AppendLine($"Stock {crop.displayName}: {progress.Count(crop)} | Price: {crop.sellPrice}");
                text.AppendLine();
            }
            text.AppendLine($"Job: {sim.Job}  |  Moving: {sim.Moving}  |  Target: {sim.Target + 1}");
            text.AppendLine($"Water: {sim.Water:0.0}  |  Harvest: {sim.HarvestCount}  |  X: {sim.WorkerX:0.00}\n");
            for (int i = 0; i < sim.Plots.Length; i++)
            {
                var plot = sim.Plots[i];
                text.AppendLine($"{i + 1:00}  {plot.Crop.displayName}  |  {(plot.Planted ? plot.Ready ? "Ready" : "Growing" : "Empty")}  |  {plot.Growth:P0}  |  Water {plot.Water:0.0}  |  Yield {plot.Harvested}");
            }
            runtime.text = text.ToString();
        }
    }
}
