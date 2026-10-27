using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleGarden
{
    [Serializable]
    public sealed class PlotConfig
    {
        public CropDefinition crop;
        [Min(0)] public float startingWater = 8;
    }

    [CreateAssetMenu(menuName = "Idle Garden/Garden Profile")]
    public sealed class GardenProfile : ScriptableObject
    {
        // The crop's lower alpha edge sits on the upper alpha edge of soil.png.
        public const float GroundY = -.2f;
        public const float CropSurfaceY = .53125f;
        public const float CropScale = 2f / 3f;
        public const float WellVisualOffset = 1f;
        public const float WellVisualScale = 2f / 3f;

        public WorkerConfig worker;
        public ProgressionConfig progression;
        public bool showPlotMeters;
        public List<PlotConfig> plots = new List<PlotConfig>();
        [Min(.8f)] public float plotSpacing = 1.6f;
        [Min(.5f)] public float wellDistance = 1.5f;
        [Range(0, 10)] public float simulationSpeed = 1;
        public Sprite soilSprite;
        public Sprite wellSprite;
        public Sprite grassSprite;
        public Vector2Int windowSize = new Vector2Int(920, 180);
        public Vector2Int minimumWindowSize = new Vector2Int(560, 150);

        public bool Validate(out string error)
        {
            if (!worker) { error = "Assign a Worker Config."; return false; }
            if (progression && !progression.Validate(out error)) return false;
            if (plots == null || plots.Count == 0) { error = "Add at least one plot."; return false; }
            if (plotSpacing < .8f || wellDistance < .5f) { error = "Plot spacing / well distance is too small."; return false; }
            if (worker.moveSpeed <= 0 || worker.tankCapacity <= 0 || worker.waterPerAction <= 0 ||
                worker.plantSeconds <= 0 || worker.waterSeconds <= 0 || worker.harvestSeconds <= 0 || worker.refillSeconds <= 0)
            { error = "Worker speed, action times and water settings must be positive."; return false; }
            for (int i = 0; i < plots.Count; i++)
            {
                var crop = plots[i]?.crop;
                if (!crop || string.IsNullOrWhiteSpace(crop.id) || crop.growSeconds <= 0 || crop.waterCapacity <= 0 || crop.waterPerSecond < 0)
                { error = $"Plot {i + 1}: assign a valid crop and positive growth / water capacity."; return false; }
                if (progression && !progression.Contains(crop))
                { error = $"Plot {i + 1}: crop must also exist in Progression catalog."; return false; }
                for (int j = 0; j < i; j++)
                    if (plots[j].crop != crop && plots[j].crop.id == crop.id)
                    { error = $"Duplicate crop ID: {crop.id}."; return false; }
            }
            error = null;
            return true;
        }

        public bool ValidateVisuals(out string error)
        {
            if (!soilSprite || !wellSprite || !grassSprite)
            { error = "Assign soil, well and grass sprites in Garden."; return false; }
            if (!worker || !HasFrames(worker.idleFrames) || !HasFrames(worker.walkFrames) || !HasFrames(worker.workFrames))
            { error = "Worker requires non-empty Idle / Walk / Work sprite arrays."; return false; }
            for (int i = 0; i < plots.Count; i++)
                if (plots[i] == null || !plots[i].crop || !HasFrames(plots[i].crop.stages) ||
                    plots[i].crop.swayStages == null || plots[i].crop.swayStages.Length != plots[i].crop.stages.Length || !HasFrames(plots[i].crop.swayStages))
                { error = $"Plot {i + 1}: assign matching crop stage and sway sprites before applying layout."; return false; }
            if (minimumWindowSize.x < 400 || minimumWindowSize.y < 140 || windowSize.x < minimumWindowSize.x || windowSize.y < minimumWindowSize.y)
            { error = "Window size must fit minimum size (at least 400 × 140)."; return false; }
            error = null; return true;
        }

        private static bool HasFrames(Sprite[] frames)
        {
            if (frames == null || frames.Length == 0) return false;
            for (int i = 0; i < frames.Length; i++) if (!frames[i]) return false;
            return true;
        }
    }
}

