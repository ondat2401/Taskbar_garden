using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace IdleGarden.Editor
{
    public static class GardenSaveVerification
    {
        [MenuItem("Tools/Idle Garden/Verify F05 Save and Rebirth")]
        public static void Run()
        {
            var profile = AssetDatabase.LoadAssetAtPath<GardenProfile>(GardenPrototypeBuilder.ProfilePath);
            var config = profile ? profile.progression : null;
            Check(config && config.Validate(out _), "Invalid progression config.");

            string directory = Path.Combine(Path.GetTempPath(), "IdleGarden-F05-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                VerifyMigration(directory);
                VerifyV2Migration(directory);
                VerifySaveAndRecovery(directory);
                VerifyFutureAndCorruptFiles(directory);
                VerifyRebirth(profile, config, directory);
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
            Debug.Log("PASS F05: v1 migration, atomic repeated save, backup recovery, future/corrupt file preservation and rebirth retention/reset.");
        }

        private static void VerifyMigration(string directory)
        {
            string path = Path.Combine(directory, "legacy.json");
            File.WriteAllText(path, "{\"version\":1,\"gold\":321,\"cycleGold\":321,\"seeds\":2,\"rebirths\":1," +
                "\"inventory\":[{\"id\":\"carrot\",\"count\":7}],\"upgrades\":[]," +
                "\"plots\":[{\"cropId\":\"carrot\",\"planted\":false,\"growth\":0,\"water\":2,\"harvested\":4}]," +
                "\"workerX\":0,\"workerWater\":4,\"harvested\":4}");
            var store = new GardenSaveStore(path);
            var migrated = store.Load();
            Check(migrated != null && migrated.version == GardenSaveData.CurrentVersion && migrated.plots[0].nextCropId == "carrot",
                "F01 save did not migrate to the current schema.");
            Check(migrated.themes != null && migrated.gearInventory != null && migrated.equippedGear != null,
                "Migration did not initialize fields introduced after F01.");
            Check(migrated.cosmetics != null && migrated.cosmetics.workerId == string.Empty,
                "Migration did not initialize the cosmetic loadout.");
            Check(store.Save(migrated) && JsonUtility.FromJson<GardenSaveData>(File.ReadAllText(path)).version == GardenSaveData.CurrentVersion,
                "Migrated save was not persisted at the current schema.");
        }

        private static void VerifyV2Migration(string directory)
        {
            string path = Path.Combine(directory, "version2.json");
            var legacy = Save(); legacy.version = 2; legacy.cosmetics = null;
            File.WriteAllText(path, JsonUtility.ToJson(legacy));
            var migrated = new GardenSaveStore(path).Load();
            Check(migrated != null && migrated.version == GardenSaveData.CurrentVersion && migrated.cosmetics != null &&
                migrated.cosmetics.workerId == string.Empty && migrated.cosmetics.potId == string.Empty && migrated.cosmetics.cropId == string.Empty,
                "F05 save did not migrate to the cosmetic schema.");
        }

        private static void VerifySaveAndRecovery(string directory)
        {
            string path = Path.Combine(directory, "roundtrip.json");
            var store = new GardenSaveStore(path);
            Check(store.Save(Save(10)), "Initial save failed: " + store.Warning);
            Check(store.Save(Save(20)), "Replacement save failed: " + store.Warning);
            Check(new GardenSaveStore(path).Load().gold == 20, "Replacement did not load.");
            Check(JsonUtility.FromJson<GardenSaveData>(File.ReadAllText(path + ".bak")).gold == 10,
                "Atomic replacement did not preserve the previous save as backup.");

            File.WriteAllText(path, "{corrupt");
            var recoveredStore = new GardenSaveStore(path);
            var recovered = recoveredStore.Load();
            Check(recovered != null && recovered.gold == 10 && recoveredStore.Warning != null && recoveredStore.Warning.Contains("backup"),
                "Corrupt primary did not recover from the last good backup.");
            Check(recoveredStore.Save(recovered), "Saving recovered backup failed: " + recoveredStore.Warning);
            Check(new GardenSaveStore(path).Load().gold == 10 && Directory.GetFiles(directory, "roundtrip.json.corrupt-*").Length == 1,
                "Recovery did not preserve the corrupt primary while restoring the backup.");
        }

        private static void VerifyFutureAndCorruptFiles(string directory)
        {
            string futurePath = Path.Combine(directory, "future.json");
            File.WriteAllText(futurePath, "{\"version\":99,\"gold\":9000}");
            File.WriteAllText(futurePath + ".bak", JsonUtility.ToJson(Save(5)));
            string futureBytes = File.ReadAllText(futurePath);
            var futureStore = new GardenSaveStore(futurePath);
            Check(futureStore.Load() == null && futureStore.Warning != null && futureStore.Warning.Contains("newer"),
                "Future-version save was not refused.");
            Check(!futureStore.Save(Save(1)) && File.ReadAllText(futurePath) == futureBytes,
                "A future-version save was overwritten.");

            string corruptPath = Path.Combine(directory, "unreadable.json");
            const string corruptBytes = "{broken-json";
            File.WriteAllText(corruptPath, corruptBytes);
            var corruptStore = new GardenSaveStore(corruptPath);
            Check(corruptStore.Load() == null && !corruptStore.Save(Save(1)), "Unrecoverable corrupt save was overwritten.");
            Check(File.ReadAllText(corruptPath) == corruptBytes, "Corrupt save bytes changed after a refused write.");

            string emptyPath = Path.Combine(directory, "empty.json");
            const string emptyBytes = "{}";
            File.WriteAllText(emptyPath, emptyBytes);
            var emptyStore = new GardenSaveStore(emptyPath);
            Check(emptyStore.Load() == null && !emptyStore.Save(Save(1)) && File.ReadAllText(emptyPath) == emptyBytes,
                "An empty JSON object was accepted as a save or overwritten.");

            string partialPath = Path.Combine(directory, "partial.json");
            const string partialBytes = "{\"version\":2,\"gold\":1,\"cycleGold\":0,\"seeds\":0,\"rebirths\":0,\"autoSell\":true," +
                "\"inventory\":[{\"id\":\"carrot\"}],\"upgrades\":[],\"plots\":[],\"workerX\":0,\"workerWater\":0,\"harvested\":0," +
                "\"themes\":[],\"chests\":0,\"firstHarvestReward\":false,\"gearInventory\":[],\"equippedGear\":[]," +
                "\"essence\":0,\"chestPityMisses\":0,\"chestTimerSeconds\":0}";
            File.WriteAllText(partialPath, partialBytes);
            var partialStore = new GardenSaveStore(partialPath);
            Check(partialStore.Load() == null && !partialStore.Save(Save(1)) && File.ReadAllText(partialPath) == partialBytes,
                "A save with a missing inventory count was accepted or overwritten.");
        }

        private static void VerifyRebirth(GardenProfile profile, ProgressionConfig config, string directory)
        {
            var saved = Save();
            saved.gold = 9000;
            saved.cycleGold = config.rebirthBaseGoal * Math.Pow(config.rebirthGoalGrowth, 2);
            saved.seeds = 3;
            saved.rebirths = 2;
            saved.autoSell = false;
            saved.chests = 4;
            saved.firstHarvestReward = true;
            saved.essence = 7;
            saved.chestPityMisses = 6;
            saved.chestTimerSeconds = config.gear.chestIntervalSeconds * .4;
            saved.inventory = new InventorySave[config.crops.Length];
            for (int i = 0; i < config.crops.Length; i++) saved.inventory[i] = new InventorySave { id = config.crops[i].id, count = 19 };
            saved.upgrades = new UpgradeSave[config.upgrades.Length];
            for (int i = 0; i < config.upgrades.Length; i++) saved.upgrades[i] = new UpgradeSave { id = config.upgrades[i].id, level = 2 };
            saved.themes = new string[config.themes.Length];
            for (int i = 0; i < config.themes.Length; i++) saved.themes[i] = config.themes[i].id;
            var can = new GearInstance { id = "f05-can", equipmentId = "wooden-can", rarity = GearRarity.Common, affixes = Array.Empty<GearAffix>() };
            saved.gearInventory = new[] { can };
            saved.equippedGear = new[] { new EquippedGearSave { slot = GearSlot.WateringCan, instanceId = can.id } };

            var progression = new GardenProgression(config, saved, new System.Random(29));
            var before = new GardenSimulation(profile, progression);
            var fire = config.FindCrop("fire-pepper");
            var frost = config.FindCrop("frost-bloom");
            Check(fire && before.SelectCrop(0, fire), "Could not set an unlocked crop selection for the rebirth check.");
            before.Plots[0].Planted = true;
            before.Plots[0].Growth = .75f;
            if (frost && before.Plots.Length > 1) Check(before.SelectCrop(1, frost), "Could not set a second pot selection.");
            var choices = new CropDefinition[before.Plots.Length];
            for (int i = 0; i < choices.Length; i++) choices[i] = before.Plots[i].SelectedCrop;

            Check(GardenSaveStore.Valid(progression.Capture()), "Rebirth fixture is not valid save data.");
            Check(progression.TryRebirth(), "Eligible garden could not rebirth.");
            Check(progression.Seeds == 4 && progression.Rebirths == 3 && progression.CycleGold == 0 && progression.Gold == config.startingGold,
                "Rebirth did not reset cycle gold or award exactly one permanent seed.");
            Check(progression.AutoSell == false && progression.Chests == 4 && progression.Essence == 7 && progression.ChestPityMisses == 6,
                "Rebirth reset retained settings or chest progression.");
            Check(progression.GearInventory.Count == 1 && progression.Equipped(GearSlot.WateringCan)?.id == can.id,
                "Rebirth removed gear or its equipped state.");
            for (int i = 0; i < config.crops.Length; i++) Check(progression.Count(config.crops[i]) == 0, "Rebirth retained crop stock.");
            for (int i = 0; i < config.upgrades.Length; i++) Check(progression.Level(i) == 0, "Rebirth retained an upgrade rank.");
            for (int i = 0; i < config.themes.Length; i++) Check(progression.ThemeUnlocked(config.themes[i]), "Rebirth locked a previously unlocked theme.");

            var after = new GardenSimulation(profile, progression);
            after.CopyCropSelectionsFrom(before);
            for (int i = 0; i < after.Plots.Length; i++)
                Check(after.Plots[i].SelectedCrop == choices[i] && !after.Plots[i].Planted && after.Plots[i].Growth == 0,
                    "Rebirth lost a pot selection or kept a planted crop.");
            string path = Path.Combine(directory, "rebirth.json");
            var snapshot = progression.Capture();
            after.CaptureInto(snapshot);
            Check(new GardenSaveStore(path).Save(snapshot), "Could not write post-rebirth save.");
            var disk = new GardenSaveStore(path).Load();
            Check(disk != null, "Could not reload post-rebirth save.");
            var restored = new GardenProgression(config, disk);
            var restoredSimulation = new GardenSimulation(profile, restored);
            restoredSimulation.Restore(disk);
            for (int i = 0; i < restoredSimulation.Plots.Length; i++)
                Check(restoredSimulation.Plots[i].SelectedCrop == choices[i], "Pot selection did not survive a post-rebirth save/restart.");
            Check(restored.GearInventory.Count == 1 && restored.Equipped(GearSlot.WateringCan)?.id == can.id &&
                restored.Chests == 4 && restored.Essence == 7 && restored.ChestPityMisses == 6 && !restored.AutoSell && restored.ThemeUnlocked(fire.theme),
                "Post-rebirth save/restart lost retained progression.");
            Check(Math.Abs(restored.ChestSecondsRemaining - config.gear.chestIntervalSeconds * .6) < .01,
                "Rebirth/save capture lost the chest timer remainder.");
        }

        private static GardenSaveData Save(double gold = 0) => new GardenSaveData
        {
            gold = gold, cycleGold = 0, inventory = Array.Empty<InventorySave>(), upgrades = Array.Empty<UpgradeSave>(),
            plots = Array.Empty<PlotSave>(), themes = Array.Empty<string>(), gearInventory = Array.Empty<GearInstance>(),
            equippedGear = Array.Empty<EquippedGearSave>()
        };

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }
    }
}
