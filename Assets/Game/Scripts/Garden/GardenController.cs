using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace IdleGarden
{
    public sealed class GardenController : MonoBehaviour
    {
        public GardenProfile profile;
        public PlotView[] plots;
        public Transform workerTransform;
        public SpriteRenderer workerRenderer;
        public Camera gardenCamera;
        public Text statusLabel;
        public Text harvestLabel;
        public GardenHud hud;
        public GardenSimulation Simulation { get; private set; }
        public GardenProgression Progression { get; private set; }
        private GardenSaveStore saveStore;
        private float animationTime;
        private float viewX;
        private static readonly string[] JobLabels = { "Resting", "Planting", "Watering", "Harvesting", "Refilling" };

        private void Start()
        {
            if (!profile || !profile.Validate(out _) || plots.Length != profile.plots.Count)
            { Debug.LogError("Invalid layout. Open Garden Config and Apply Layout before Play.", this); enabled = false; return; }
            GardenSaveData saved = null;
            if (profile.progression)
            {
#if !UNITY_EDITOR
                saveStore = new GardenSaveStore(System.IO.Path.Combine(Application.persistentDataPath, "garden-v1.json"));
                saved = saveStore.Load();
                if (saveStore.Warning != null) Debug.LogWarning(saveStore.Warning);
#endif
                Progression = new GardenProgression(profile.progression, saved);
            }
            Simulation = new GardenSimulation(profile, Progression);
            if (saved != null) Simulation.Restore(saved);
            viewX = (Simulation.WellX + Simulation.RightEdge) * .5f;
            Application.runInBackground = true;
            Application.targetFrameRate = 30;
            InvokeRepeating(nameof(RefreshHud), 0, .25f);
            InvokeRepeating(nameof(SaveGame), 15, 15);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            Simulation.Tick(dt * Mathf.Max(0, profile.simulationSpeed));
            animationTime += dt * profile.simulationSpeed * (Simulation.Paused ? 0 : 1);
            workerTransform.position = new Vector3(Mathf.Round(Simulation.WorkerX * 32) / 32, .09375f, 0);
            workerRenderer.flipX = Simulation.FacingLeft;
            var config = profile.worker;
            var frames = Simulation.Moving ? config.walkFrames : Simulation.Job == GardenJob.Idle ? config.idleFrames : config.workFrames;
            if (frames != null && frames.Length > 0)
                workerRenderer.sprite = frames[(int)(animationTime * config.animationFps) % frames.Length];
            for (int i = 0; i < plots.Length; i++) plots[i].Render(Simulation.Plots[i]);
            if (Mouse.current != null)
            {
                Vector2 pointer = Mouse.current.position.ReadValue();
                if (pointer.x >= 0 && pointer.x <= Screen.width && pointer.y >= 30 && pointer.y < gardenCamera.pixelHeight)
                    viewX += Mouse.current.scroll.ReadValue().y * -.003f;
            }
            float gardenHeight = hud && hud.ShopOpen ? Mathf.Min(180, Screen.height) : Screen.height;
            gardenCamera.rect = new Rect(0, 0, 1, gardenHeight / Screen.height);
            int pixelScale = Mathf.Max(1, Mathf.FloorToInt(Screen.width / 450f));
            gardenCamera.orthographicSize = gardenHeight / (64f * pixelScale);
            float halfWidth = gardenCamera.orthographicSize * gardenCamera.aspect;
            float left = Simulation.WellX - 1, right = Simulation.RightEdge;
            viewX = right - left <= halfWidth * 2 ? (left + right) * .5f : Mathf.Clamp(viewX, left + halfWidth, right - halfWidth);
            float cameraY = gardenCamera.orthographicSize - .375f;
            gardenCamera.transform.position = new Vector3(Mathf.Round(viewX * 32) / 32, Mathf.Round(cameraY * 32) / 32, -10);
        }

        // Text formatting is throttled outside the frame loop.
        private void RefreshHud()
        {
            if (Simulation == null) return;
            if (hud) { hud.Refresh(); return; }
            statusLabel.text = $"{(Simulation.Paused ? "Paused" : Simulation.Moving ? "Walking" : JobLabels[(int)Simulation.Job])}  |  Water {Simulation.Water:0}/{profile.worker.tankCapacity:0}";
            harvestLabel.text = $"HARVEST  {Simulation.HarvestCount:000}";
        }

        public void TogglePause() { if (Simulation != null) Simulation.Paused = !Simulation.Paused; }
        public void PanLeft() { viewX -= 1.5f; }
        public void PanRight() { viewX += 1.5f; }

        public void SaveGame()
        {
            if (saveStore == null || Simulation == null || Progression == null) return;
            var saved = Progression.Capture(); Simulation.CaptureInto(saved);
            if (!saveStore.Save(saved) && saveStore.Warning != null) Debug.LogWarning(saveStore.Warning);
        }

        public bool Rebirth()
        {
            if (Progression == null || !Progression.TryRebirth()) return false;
            Simulation = new GardenSimulation(profile, Progression);
            SaveGame(); return true;
        }
        private void OnApplicationQuit() { SaveGame(); }
        private void OnApplicationPause(bool paused) { if (paused) SaveGame(); }
    }
}
