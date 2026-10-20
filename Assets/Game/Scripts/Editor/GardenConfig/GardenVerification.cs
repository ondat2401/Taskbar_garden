using System;
using UnityEditor;
using UnityEngine;

namespace IdleGarden.Editor
{
    public static class GardenVerification
    {
        [MenuItem("Tools/Idle Garden/Verify Simulation")]
        public static void Run()
        {
            var crop = ScriptableObject.CreateInstance<CropDefinition>();
            var worker = ScriptableObject.CreateInstance<WorkerConfig>();
            var profile = ScriptableObject.CreateInstance<GardenProfile>();
            try
            {
                crop.growSeconds = 4; crop.waterCapacity = 3; crop.waterPerSecond = .5f;
                worker.moveSpeed = 8; worker.waterPerAction = 2; worker.tankCapacity = 5;
                worker.plantSeconds = worker.waterSeconds = worker.harvestSeconds = worker.refillSeconds = .1f;
                profile.worker = worker; profile.plotSpacing = 1;
                foreach (int count in new[] { 4, 8, 12 })
                {
                    profile.plots.Clear();
                    for (int i = 0; i < count; i++) profile.plots.Add(new PlotConfig { crop = crop, startingWater = 0 });
                    var sim = new GardenSimulation(profile); bool refilled = false;
                    for (int step = 0; step < 24000; step++)
                    {
                        sim.Tick(.05f);
                        refilled |= sim.Job == GardenJob.Refill;
                        if (sim.Water < -.001f || sim.Water > worker.tankCapacity + .001f) throw new Exception("Tank out of range.");
                        for (int i = 0; i < count; i++)
                            if (sim.Plots[i].Water < -.001f || sim.Plots[i].Growth < 0 || sim.Plots[i].Growth > 1)
                                throw new Exception("Plot state out of range.");
                    }
                    if (!refilled) throw new Exception("Refill never happened.");
                    for (int i = 0; i < count; i++)
                        if (sim.Plots[i].Harvested < 3) throw new Exception($"Plot {i} starved with {count} plots.");
                    float x = sim.WorkerX, water = sim.Water; int harvested = sim.HarvestCount;
                    sim.Paused = true; sim.Tick(10);
                    if (sim.WorkerX != x || sim.Water != water || sim.HarvestCount != harvested) throw new Exception("Pause mutated simulation.");
                    sim.Paused = false;
                }
                profile.plots.Clear(); profile.plots.Add(new PlotConfig { crop = crop, startingWater = .8f });
                var resting = new GardenSimulation(profile); resting.Plots[0].Planted = true;
                resting.Tick(.1f);
                if (resting.Job != GardenJob.Idle || resting.Moving)
                    throw new Exception("Worker should rest while a crop still has water.");
                resting.Plots[0].Water = .02f; resting.Tick(.05f);
                if (resting.Job != GardenJob.Water || resting.Target != 0)
                    throw new Exception("Worker did not dispatch watering after the plot ran dry.");

                profile.plots[0].startingWater = 0;
                var dry = new GardenSimulation(profile); dry.Plots[0].Planted = true;
                dry.Tick(.1f);
                if (dry.Plots[0].Growth != 0) throw new Exception("Dry crop grew.");
                dry.Plots[0].Water = 2; crop.growSeconds = 10; dry.Tick(.1f);
                if (Mathf.Abs(dry.Plots[0].Growth - .01f) > .0001f) throw new Exception("Live crop config did not apply.");
                Debug.Log("PASS: 4/8/12 plots, repeated harvests, refill, bounds, pause, dry-only watering/rest, dry growth and live numeric config.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile); UnityEngine.Object.DestroyImmediate(worker); UnityEngine.Object.DestroyImmediate(crop);
            }
        }
    }
}

