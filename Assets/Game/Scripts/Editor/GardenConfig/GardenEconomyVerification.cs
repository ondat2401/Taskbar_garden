using System;
using UnityEditor;
using UnityEngine;

namespace IdleGarden.Editor
{
    public static class GardenEconomyVerification
    {
        private static void Check(bool condition, string message)
        {
            if (!condition) throw new Exception("F01: " + message);
        }

        [MenuItem("Tools/Idle Garden/Verify Economy F01")]
        public static void Run()
        {
            var carrot = ScriptableObject.CreateInstance<CropDefinition>();
            var radish = ScriptableObject.CreateInstance<CropDefinition>();
            var impostor = ScriptableObject.CreateInstance<CropDefinition>();
            var config = ScriptableObject.CreateInstance<ProgressionConfig>();
            var worker = ScriptableObject.CreateInstance<WorkerConfig>();
            var profile = ScriptableObject.CreateInstance<GardenProfile>();
            try
            {
                carrot.id = "f01-carrot"; carrot.sellPrice = 10;
                radish.id = "f01-radish"; radish.sellPrice = 20;
                impostor.id = carrot.id; impostor.sellPrice = 1000000;
                config.crops = new[] { carrot, radish };
                config.upgrades = Array.Empty<UpgradeDefinition>();
                var p = new GardenProgression(config);
                Check(p.Sell(null, 1) == 0, "Null crop sale must be rejected.");
                p.AddHarvest(null, 1); p.AddHarvest(impostor, 10); p.AddHarvest(carrot, -1);
                Check(p.Count(carrot) == 0 && p.Gold == 30, "Invalid harvest mutated economy.");
                p.AddHarvest(carrot, 4);
                Check(p.Count(carrot) == 4 && p.Gold == 30, "Reserve was sold.");
                p.AddHarvest(carrot, 2);
                Check(p.Count(carrot) == 4 && p.Gold == 50 && p.CycleGold == 20, "Surplus sale mismatch.");
                Check(p.Sell(impostor, 1) == 0 && p.Count(carrot) == 4, "Unregistered asset sold using another crop ID.");
                p.AutoSell = false; p.AddHarvest(radish, 10);
                Check(p.Count(radish) == 10 && p.Gold == 50, "Auto-sell OFF did not stockpile.");
                Check(p.Sell(radish, 3) == 60 && p.Count(radish) == 7 && p.Gold == 110, "Manual sale mismatch.");
                Check(p.Sell(radish, -3) == 0 && p.Count(radish) == 7, "Negative sale mutated stock.");
                Check(p.SellSurplus() == 60 && p.Count(radish) == 4 && p.Count(carrot) == 4, "Sell surplus did not retain reserves.");
                Check(p.Sell(radish, long.MaxValue) == 80 && p.Count(radish) == 0, "Oversell did not clamp to stock.");
                Check(p.Sell(radish, 1) == 0, "Empty stock generated gold.");

                p = new GardenProgression(config); p.AutoSell = false;
                long harvested = 0; double expectedSales = 0;
                for (int i = 0; i < 10000; i++)
                {
                    p.AddHarvest(carrot, 3); harvested += 3;
                    long sold = Math.Min(2, p.Count(carrot));
                    expectedSales += sold * 10;
                    p.Sell(carrot, 2);
                }
                Check(p.Count(carrot) == harvested - 20000 && p.Gold == 30 + expectedSales && p.CycleGold == expectedSales,
                    "Repeated transactions broke conservation of crops/gold.");

                var saved = p.Capture(); saved.gold = double.MaxValue; saved.cycleGold = double.MaxValue;
                var fullGold = new GardenProgression(config, saved);
                long before = fullGold.Count(carrot);
                Check(fullGold.Sell(carrot, 1) == 0 && fullGold.Count(carrot) == before && fullGold.Gold == double.MaxValue,
                    "Sale consumed stock without representable gold credit.");
                saved = p.Capture(); saved.inventory[0].count = long.MaxValue;
                var fullStock = new GardenProgression(config, saved); fullStock.AutoSell = false;
                fullStock.AddHarvest(carrot, 1);
                Check(fullStock.Count(carrot) == long.MaxValue, "Stock overflow wrapped negative.");
                fullStock.Sell(carrot, long.MaxValue);
                Check(fullStock.Count(carrot) == 0 && GardenProgression.Finite(fullStock.Gold), "Large valid sale failed.");

                config.autoSellReserve = 0;
                var zeroReserve = new GardenProgression(config); zeroReserve.AddHarvest(carrot, 3);
                Check(zeroReserve.Count(carrot) == 0 && zeroReserve.Gold == 60, "Zero reserve did not sell everything.");
                config.autoSellReserve = 4;
                Check(config.Validate(out _), "Economy-only config should be valid without upgrades.");
                config.autoSellReserve = -1;
                Check(!config.Validate(out _), "Negative reserve should be invalid."); config.autoSellReserve = 4;

                carrot.growSeconds = radish.growSeconds = 1;
                carrot.waterPerSecond = radish.waterPerSecond = .2f;
                worker.moveSpeed = 8;
                worker.plantSeconds = worker.waterSeconds = worker.harvestSeconds = worker.refillSeconds = .1f;
                profile.worker = worker; profile.progression = config;
                for (int i = 0; i < 4; i++) profile.plots.Add(new PlotConfig { crop = i % 2 == 0 ? carrot : radish });
                var linked = new GardenProgression(config);
                var sim = new GardenSimulation(profile, linked);
                for (int i = 0; i < 24000; i++) sim.Tick(.05f);
                long carrots = (long)sim.Plots[0].Harvested + sim.Plots[2].Harvested;
                long radishes = (long)sim.Plots[1].Harvested + sim.Plots[3].Harvested;
                double revenue = Math.Max(0, carrots - 4) * 10d + Math.Max(0, radishes - 4) * 20d;
                Check(carrots > 4 && radishes > 4 && linked.Count(carrot) == 4 && linked.Count(radish) == 4,
                    "Simulation harvests did not feed crop inventory.");
                Check(linked.Gold == 30 + revenue && linked.CycleGold == revenue, "Simulation revenue mismatch.");
                Debug.Log("PASS F01: reserves, auto-sell OFF/ON, manual/surplus/invalid sales, 10,000 transactions, numeric limits, catalog validation and 1,200 simulated seconds of harvest-to-economy.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile); UnityEngine.Object.DestroyImmediate(worker);
                UnityEngine.Object.DestroyImmediate(config); UnityEngine.Object.DestroyImmediate(impostor);
                UnityEngine.Object.DestroyImmediate(radish); UnityEngine.Object.DestroyImmediate(carrot);
            }
        }
    }
}
