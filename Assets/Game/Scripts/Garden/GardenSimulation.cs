using System;
using UnityEngine;

namespace IdleGarden
{
    public enum GardenJob { Idle, Plant, Water, Harvest, Refill }

    public sealed class PlotState
    {
        public CropDefinition Crop { get; private set; }
        public CropDefinition SelectedCrop { get; private set; }
        public bool Planted;
        public float Growth;
        public float Water;
        public int Harvested;
        public PlotState(PlotConfig config) { Crop = SelectedCrop = config.crop; Water = Mathf.Clamp(config.startingWater, 0, Crop.waterCapacity); }
        public void Select(CropDefinition crop) { SelectedCrop = crop; if (!Planted) BeginNextCrop(); }
        public void BeginNextCrop() { Crop = SelectedCrop; Water = Mathf.Min(Water, Crop.waterCapacity); }
        public bool Ready => Planted && Growth >= 1;
    }

    // Pure state: no scene lookup, GameObject creation or animation dependencies.
    public sealed class GardenSimulation
    {
        public readonly PlotState[] Plots;
        public readonly GardenProfile Profile;
        public GardenJob Job { get; private set; }
        public int Target { get; private set; } = -1;
        public float WorkerX { get; private set; }
        public float Water { get; private set; }
        public bool Moving { get; private set; }
        public bool FacingLeft { get; private set; }
        public int HarvestCount { get; private set; }
        public float ActionProgress { get; private set; }
        public bool Paused;
        private float actionElapsed;
        private readonly GardenProgression progression;
        public float TankCapacity => Profile.worker.tankCapacity * (progression?.Multiplier(UpgradeStat.TankCapacity) ?? 1);
        private float WaterPerAction => Profile.worker.waterPerAction;

        public GardenSimulation(GardenProfile profile, GardenProgression progression = null)
        {
            if (!profile || !profile.Validate(out _)) throw new ArgumentException("Invalid garden profile.");
            Profile = profile;
            this.progression = progression;
            Plots = new PlotState[profile.plots.Count];
            for (int i = 0; i < Plots.Length; i++) Plots[i] = new PlotState(profile.plots[i]);
            Water = TankCapacity;
            WorkerX = WellX;
        }

        public float WellX => -Profile.wellDistance;
        public float PlotX(int index) => index * Profile.plotSpacing;
        public float RightEdge => PlotX(Plots.Length - 1) + 1;

        public void Tick(float dt)
        {
            if (Paused || dt <= 0) return;
            var worker = Profile.worker;
            Water = Mathf.Min(Water, TankCapacity);
            for (int i = 0; i < Plots.Length; i++)
            {
                var plot = Plots[i];
                if (!plot.Planted || plot.Ready) continue;
                float waterRate = plot.Crop.waterPerSecond / (progression?.Multiplier(UpgradeStat.WaterEfficiency) ?? 1);
                float availableTime = waterRate > 0 ? plot.Water / waterRate : dt;
                float growingTime = Mathf.Min(dt, availableTime);
                float growSpeed = progression?.Multiplier(UpgradeStat.GrowSpeed, plot.Crop) ?? 1;
                plot.Growth = Mathf.Min(1, plot.Growth + growingTime / Mathf.Max(.2f, plot.Crop.growSeconds / growSpeed));
                plot.Water = Mathf.Clamp(plot.Water - growingTime * waterRate, 0, plot.Crop.waterCapacity);
            }
            if (Job == GardenJob.Idle) ChooseJob();
            if (Job == GardenJob.Idle) { Moving = false; return; }
            float targetX = Job == GardenJob.Refill ? WellX : PlotX(Target);
            Moving = Mathf.Abs(targetX - WorkerX) > .001f;
            if (Moving)
            {
                FacingLeft = targetX < WorkerX;
                WorkerX = Mathf.MoveTowards(WorkerX, targetX, Mathf.Max(.01f, worker.moveSpeed) * (progression?.Multiplier(UpgradeStat.MoveSpeed) ?? 1) * dt);
                return;
            }
            float duration = Job switch
            {
                GardenJob.Plant => worker.plantSeconds,
                GardenJob.Water => worker.waterSeconds,
                GardenJob.Harvest => worker.harvestSeconds,
                _ => worker.refillSeconds
            };
            duration /= progression?.Multiplier(UpgradeStat.ActionSpeed) ?? 1;
            actionElapsed += dt;
            ActionProgress = Mathf.Clamp01(actionElapsed / Mathf.Max(.01f, duration));
            if (ActionProgress < 1) return;
            if (Job == GardenJob.Refill) Water = TankCapacity;
            else
            {
                var plot = Plots[Target];
                switch (Job)
                {
                    case GardenJob.Plant: plot.Planted = true; plot.Growth = 0; break;
                    case GardenJob.Water:
                        float amount = Mathf.Min(Mathf.Min(Water, WaterPerAction), Mathf.Max(0, plot.Crop.waterCapacity - plot.Water));
                        plot.Water += amount; Water -= amount; break;
                    case GardenJob.Harvest:
                        int amountHarvested = (int)Math.Min(1000000, Math.Max(1, Math.Round(plot.Crop.yield * (double)(progression?.Multiplier(UpgradeStat.CropYield, plot.Crop) ?? 1))));
                        plot.Harvested = (int)Math.Min(int.MaxValue, (long)plot.Harvested + amountHarvested);
                        HarvestCount = (int)Math.Min(int.MaxValue, (long)HarvestCount + amountHarvested);
                        progression?.AddHarvest(plot.Crop, amountHarvested);
                        plot.Planted = false; plot.Growth = 0; plot.BeginNextCrop(); break;
                }
            }
            Job = GardenJob.Idle; Target = -1; actionElapsed = 0; ActionProgress = 0;
        }

        private void ChooseJob()
        {
            for (int priority = 0; priority < 3; priority++)
            {
                int nearest = -1; float distance = float.MaxValue;
                for (int i = 0; i < Plots.Length; i++)
                {
                    var plot = Plots[i];
                    bool wanted = priority == 0 ? plot.Ready : priority == 1
                        ? plot.Planted && !plot.Ready && plot.Water <= plot.Crop.waterCapacity * Mathf.Clamp01(plot.Crop.waterThreshold)
                            && plot.Water < plot.Crop.waterCapacity && plot.Crop.waterPerSecond > 0
                        : !plot.Planted;
                    float candidateDistance = Mathf.Abs(PlotX(i) - WorkerX);
                    if (wanted && candidateDistance < distance) { nearest = i; distance = candidateDistance; }
                }
                if (nearest < 0) continue;
                Target = nearest;
                Job = priority == 0 ? GardenJob.Harvest : priority == 1 ? GardenJob.Water : GardenJob.Plant;
                if (Job == GardenJob.Water && Water < Mathf.Min(WaterPerAction, TankCapacity) - .001f)
                    Job = GardenJob.Refill;
                return;
            }
        }

        public void CaptureInto(GardenSaveData saved)
        {
            saved.plots = new PlotSave[Plots.Length];
            for (int i = 0; i < Plots.Length; i++)
            {
                var p = Plots[i]; saved.plots[i] = new PlotSave { cropId = p.Crop.id, nextCropId = p.SelectedCrop.id, planted = p.Planted, growth = p.Growth, water = p.Water, harvested = p.Harvested };
            }
            saved.workerX = WorkerX; saved.workerWater = Water; saved.harvested = HarvestCount;
        }

        public void Restore(GardenSaveData saved)
        {
            for (int i = 0; i < Math.Min(Plots.Length, saved.plots.Length); i++)
            {
                var p = saved.plots[i]; var target = Plots[i];
                var current = progression?.Config.FindCrop(p.cropId) ?? (p.cropId == target.Crop.id ? target.Crop : null);
                var selected = progression?.Config.FindCrop(p.nextCropId) ?? current;
                if (!current || (progression != null && (!progression.CropUnlocked(current) || !progression.CropUnlocked(selected)))) continue;
                target.Select(current);
                target.Planted = p.planted; target.Growth = p.growth; target.Water = Mathf.Min(target.Crop.waterCapacity, p.water); target.Harvested = p.harvested;
                target.Select(selected);
            }
            WorkerX = Mathf.Clamp(saved.workerX, WellX, RightEdge); Water = Mathf.Min(saved.workerWater, TankCapacity); HarvestCount = saved.harvested;
        }
        public bool SelectCrop(int index, CropDefinition crop)
        {
            if (index < 0 || index >= Plots.Length || !crop || (progression != null && !progression.CropUnlocked(crop))) return false;
            if (progression == null && crop != Profile.plots[index].crop) return false;
            Plots[index].Select(crop); return true;
        }
    }
}

