using System.IO;
using Kirurobo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace IdleGarden.Editor
{
    public static class GardenPrototypeBuilder
    {
        public const string ProfilePath = "Assets/Game/_Configs/Garden/Prototype.asset";
        public const string ScenePath = "Assets/Game/Scenes/GardenPrototype.unity";
        private const string RootName = "Idle Garden";

        [MenuItem("Tools/Idle Garden/Open or Create Prototype")]
        public static void CreatePrototype()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Exit Play Mode before creating a scene."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath)) { EditorSceneManager.OpenScene(ScenePath); return; }
            foreach (string folder in new[] { "_Configs/Garden", "_Configs/Crops", "_Configs/Worker", "Scenes", "Prefabs/Garden", "Textures/Crops", "Textures/Worker", "Textures/Environment", "Materials" })
                Directory.CreateDirectory("Assets/Game/" + folder);
            AssetDatabase.Refresh();
            var profile = CreateDefaults();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject(RootName);
            var controller = root.AddComponent<GardenController>(); controller.profile = profile;
            var cameraObject = new GameObject("Garden Camera"); cameraObject.transform.SetParent(root.transform);
            var camera = cameraObject.AddComponent<Camera>(); cameraObject.tag = "MainCamera";
            camera.orthographic = true; camera.orthographicSize = 2.4f;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.clear; camera.allowHDR = false;
            cameraObject.AddComponent<AudioListener>();
            var cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = false; cameraData.allowHDROutput = false;
            camera.transform.position = new Vector3(2, 1.15f, -10); controller.gardenCamera = camera;
            var native = root.AddComponent<UniWindowController>();
            native.isHitTestEnabled = true; native.hitTestType = UniWindowController.HitTestType.Opacity;
            native.SetCamera(camera);
            var desktop = root.AddComponent<DesktopWindow>(); desktop.profile = profile; desktop.window = native; desktop.targetCamera = camera;
            // Native window properties are only changed by DesktopWindow in Windows standalone.
            var worker = SpriteObject("Worker", root.transform, profile.worker.idleFrames[0], new Vector3(-1.5f, GardenProfile.GroundY, 0), 4);
            controller.workerTransform = worker.transform; controller.workerRenderer = worker;
            SpriteObject("Well", root.transform, profile.wellSprite, new Vector3(-profile.wellDistance, GardenProfile.GroundY, 0), 3);
            var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)); events.transform.SetParent(root.transform);
            BuildHud(root.transform, controller, desktop);
            ApplyLayout(profile);
            Undo.FlushUndoRecordObjects();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = root;
        }

        public static void ApplyLayout(GardenProfile profile)
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Layout can only be changed outside Play Mode."); return; }
            if (!profile.Validate(out string error) || !profile.ValidateVisuals(out error)) { Debug.LogError(error); return; }
            var controller = Object.FindFirstObjectByType<GardenController>();
            if (!controller) { Debug.LogWarning("Open the prototype scene first."); return; }
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Apply garden layout");
            Undo.RecordObject(controller, "Update profile"); controller.profile = profile;
            // Only this tool-owned subtree is rebuilt. Other scene objects remain untouched.
            var old = controller.transform.Find("Generated Layout");
            if (old) Undo.DestroyObjectImmediate(old.gameObject);
            var layout = new GameObject("Generated Layout"); Undo.RegisterCreatedObjectUndo(layout, "Create layout"); layout.transform.SetParent(controller.transform);
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Materials/PixelUnlit.mat");
            if (!material)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
                AssetDatabase.CreateAsset(material, "Assets/Game/Materials/PixelUnlit.mat");
            }
            var white = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Textures/Environment/pixel.png");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Garden/Plot.prefab");
            if (!prefab)
            {
                var template = new GameObject("Plot");
                var view = template.AddComponent<PlotView>();
                view.soilRenderer = SpriteObject("Soil", template.transform, profile.soilSprite, Vector3.zero, 2);
                view.cropRenderer = SpriteObject("Crop", template.transform, profile.plots[0].crop.stages[0], new Vector3(0, GardenProfile.CropSurfaceY, 0), 5);
                for (int i = 0; i < 2; i++)
                {
                    float y = -.48f - i * .13f;
                    var background = SpriteObject(i == 0 ? "Growth Track" : "Water Track", template.transform, white, new Vector3(-.5f, y, 0), 12);
                    background.transform.localScale = new Vector3(1, .065f, 1); background.color = new Color(.15f, .2f, .18f);
                    var pivot = new GameObject(i == 0 ? "Growth Fill" : "Water Fill"); pivot.transform.SetParent(template.transform); pivot.transform.localPosition = new Vector3(-.5f, y, 0);
                    var fill = SpriteObject("Bar", pivot.transform, white, Vector3.zero, 13);
                    fill.transform.localScale = new Vector3(1, .065f, 1); fill.color = i == 0 ? new Color(.67f, .83f, .36f) : new Color(.35f, .72f, .85f);
                    if (i == 0) view.growthFill = pivot.transform; else view.waterFill = pivot.transform;
                }
                foreach (var renderer in template.GetComponentsInChildren<SpriteRenderer>()) renderer.sharedMaterial = material;
                prefab = PrefabUtility.SaveAsPrefabAsset(template, "Assets/Game/Prefabs/Garden/Plot.prefab"); Object.DestroyImmediate(template);
            }
            controller.plots = new PlotView[profile.plots.Count];
            for (int i = 0; i < profile.plots.Count; i++)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, layout.transform);
                instance.name = $"Plot {i + 1:00}"; instance.transform.localPosition = new Vector3(i * profile.plotSpacing, 0, 0);
                var view = instance.GetComponent<PlotView>(); view.soilRenderer.sprite = profile.soilSprite;
                view.cropRenderer.sprite = profile.plots[i].crop.stages[0]; controller.plots[i] = view;
                view.cropRenderer.transform.localPosition = new Vector3(0, GardenProfile.CropSurfaceY, 0);
                view.garden=controller; view.plotIndex=i;
                var collider=instance.GetComponent<BoxCollider2D>(); if(!collider) collider=instance.AddComponent<BoxCollider2D>();
                collider.offset=new Vector2(0,.35f); collider.size=new Vector2(1.2f,1.5f);
                view.growthFill.gameObject.SetActive(profile.showPlotMeters);
                view.waterFill.gameObject.SetActive(profile.showPlotMeters);
                var growthTrack = instance.transform.Find("Growth Track"); if (growthTrack) growthTrack.gameObject.SetActive(profile.showPlotMeters);
                var waterTrack = instance.transform.Find("Water Track"); if (waterTrack) waterTrack.gameObject.SetActive(profile.showPlotMeters);
            }
            float left = -profile.wellDistance - 1;
            float right = (profile.plots.Count - 1) * profile.plotSpacing + 1;
            for (float x = left; x < right; x += 1)
                SpriteObject("Grass", layout.transform, profile.grassSprite, new Vector3(x, GardenProfile.GroundY, 0), 0).sharedMaterial = material;
            var well = controller.transform.Find("Well");
            if (well) { Undo.RecordObject(well, "Ground well"); well.localPosition = new Vector3(-profile.wellDistance, GardenProfile.GroundY, 0); }
            Undo.RecordObject(controller.workerTransform, "Move worker");
            Undo.RecordObject(controller.gardenCamera.transform, "Move garden camera");
            controller.workerTransform.localPosition = new Vector3(-profile.wellDistance, GardenProfile.GroundY, 0);
            if (controller.workerRenderer) { Undo.RecordObject(controller.workerRenderer, "Sort worker behind crops"); controller.workerRenderer.sortingOrder = 4; }
            controller.gardenCamera.transform.position = new Vector3((left + right) * .5f, 1.15f, -10);
            foreach (var renderer in controller.GetComponentsInChildren<SpriteRenderer>()) renderer.sharedMaterial = material;
            EditorUtility.SetDirty(controller); EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            Undo.FlushUndoRecordObjects();
            Undo.CollapseUndoOperations(group);
        }

        private static GardenProfile CreateDefaults()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GardenProfile>(ProfilePath); if (existing) return existing;
            var art = GardenPlaceholderArt.Generate();
            var worker = ScriptableObject.CreateInstance<WorkerConfig>();
            worker.idleFrames = new[] { art["worker0"] }; worker.walkFrames = new[] { art["worker1"], art["worker2"] }; worker.workFrames = new[] { art["worker3"], art["worker0"] };
            AssetDatabase.CreateAsset(worker, "Assets/Game/_Configs/Worker/Gardener.asset");
            var carrot = ScriptableObject.CreateInstance<CropDefinition>();
            carrot.stages = new[] { art["carrot0"], art["carrot1"], art["carrot2"] };
            AssetDatabase.CreateAsset(carrot, "Assets/Game/_Configs/Crops/Carrot.asset");
            var radish = ScriptableObject.CreateInstance<CropDefinition>(); radish.id = "radish"; radish.displayName = "Radish";
            radish.growSeconds = 30; radish.waterCapacity = 16; radish.yield = 2;
            radish.stages = new[] { art["radish0"], art["radish1"], art["radish2"] };
            AssetDatabase.CreateAsset(radish, "Assets/Game/_Configs/Crops/Radish.asset");
            var profile = ScriptableObject.CreateInstance<GardenProfile>(); profile.worker = worker;
            profile.soilSprite = art["soil"]; profile.wellSprite = art["well"]; profile.grassSprite = art["grass"];
            for (int i = 0; i < 4; i++) profile.plots.Add(new PlotConfig { crop = i % 2 == 0 ? carrot : radish });
            AssetDatabase.CreateAsset(profile, ProfilePath); return profile;
        }

        private static SpriteRenderer SpriteObject(string name, Transform parent, Sprite sprite, Vector3 position, int order)
        {
            var go = new GameObject(name); go.transform.SetParent(parent); go.transform.localPosition = position;
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingOrder = order; return renderer;
        }

        private static void BuildHud(Transform root, GardenController controller, DesktopWindow desktop)
        {
            GardenProgressionSetup.BuildHud(root, controller, desktop);
        }
        [MenuItem("Tools/Idle Garden/Build Windows Prototype")]
        public static void BuildWindows()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play Mode before building.");
            if (!File.Exists(ScenePath)) CreatePrototype();
            var profile = AssetDatabase.LoadAssetAtPath<GardenProfile>(ProfilePath);
            PlayerSettings.defaultIsNativeResolution = false; PlayerSettings.defaultScreenWidth = profile.windowSize.x; PlayerSettings.defaultScreenHeight = profile.windowSize.y;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed; PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true; PlayerSettings.useFlipModelSwapchain = false;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { GraphicsDeviceType.Direct3D11 });
            var pipeline = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if (pipeline)
            {
                var data = new SerializedObject(pipeline);
                var hdr = data.FindProperty("m_SupportsHDR"); if (hdr != null) hdr.boolValue = false;
                var alpha = data.FindProperty("m_AllowPostProcessAlphaOutput"); if (alpha != null) alpha.boolValue = true;
                data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(pipeline);
            }
            AssetDatabase.SaveAssets(); Directory.CreateDirectory("Builds/Windows");
            var report = BuildPipeline.BuildPlayer(new[] { ScenePath }, "Builds/Windows/IdleGarden.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new System.Exception("Windows build failed: " + report.summary.result);
            Debug.Log("Idle Garden build succeeded: Builds/Windows/IdleGarden.exe");
        }
    }
}
