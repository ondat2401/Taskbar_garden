using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace IdleGarden
{
    [Serializable] public sealed class InventorySave { public string id; public long count = -1; }
    [Serializable] public sealed class UpgradeSave { public string id; public int level = -1; }
    [Serializable] public sealed class PlotSave { public string cropId, nextCropId; public bool planted; public float growth = float.NaN, water = float.NaN; public int harvested = -1; }
    [Serializable]
    public sealed class GardenSaveData
    {
        public const int CurrentVersion = 2;
        public int version = CurrentVersion;
        public double gold, cycleGold;
        public long seeds;
        public int rebirths;
        public bool autoSell = true;
        public InventorySave[] inventory = Array.Empty<InventorySave>();
        public UpgradeSave[] upgrades = Array.Empty<UpgradeSave>();
        public PlotSave[] plots = Array.Empty<PlotSave>();
        public float workerX, workerWater;
        public int harvested;
        public string[] themes = Array.Empty<string>();
        public int chests;
        public bool firstHarvestReward;
        public GearInstance[] gearInventory = Array.Empty<GearInstance>();
        public EquippedGearSave[] equippedGear = Array.Empty<EquippedGearSave>();
        public long essence;
        public int chestPityMisses;
        public double chestTimerSeconds;
    }

    public sealed class GardenSaveStore
    {
        public readonly string Path;
        public string Warning { get; private set; }
        private bool blocked;
        private bool recovered;
        public GardenSaveStore(string path) { Path = path; }

        public GardenSaveData Load()
        {
            if (!File.Exists(Path) && !File.Exists(Path + ".bak")) return null;
            try
            {
                if (TryRead(Path, out var saved, out bool newer)) return saved;
                if (newer) { blocked = true; Warning = "Save belongs to a newer build. Saving disabled to preserve it."; return null; }
                if (TryRead(Path + ".bak", out saved, out newer))
                { recovered = true; Warning = "Recovered progress from backup."; return saved; }
                blocked = true; Warning = "Save is unreadable; preserved on disk. Saving disabled.";
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is ArgumentException)
            { blocked = true; Warning = "Cannot load save: " + error.Message; }
            return null;
        }

        public bool Save(GardenSaveData saved)
        {
            if (blocked) return false;
            try
            {
                if (!Valid(saved)) { Warning = "Invalid runtime state; save skipped."; return false; }
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
                byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(saved, true));
                using (var file = new FileStream(Path + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
                { file.Write(bytes, 0, bytes.Length); file.Flush(true); }
                if (recovered && File.Exists(Path)) File.Move(Path, Path + ".corrupt-" + Guid.NewGuid().ToString("N"));
                recovered = false;
                if (File.Exists(Path)) File.Replace(Path + ".tmp", Path, Path + ".bak");
                else File.Move(Path + ".tmp", Path);
                Warning = null; return true;
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is ArgumentException)
            { Warning = "Cannot save progress: " + error.Message; return false; }
        }

        private static bool TryRead(string path, out GardenSaveData saved, out bool newer)
        {
            saved = null; newer = false;
            if (!File.Exists(path)) return false;
            try
            {
                saved = new GardenSaveData
                {
                    version = int.MinValue, gold = double.NaN, cycleGold = double.NaN, seeds = long.MinValue,
                    rebirths = int.MinValue, inventory = null, upgrades = null, plots = null,
                    workerX = float.NaN, workerWater = float.NaN, harvested = int.MinValue, themes = null,
                    chests = int.MinValue, gearInventory = null, equippedGear = null, essence = long.MinValue,
                    chestPityMisses = int.MinValue, chestTimerSeconds = double.NaN
                };
                JsonUtility.FromJsonOverwrite(File.ReadAllText(path), saved);
                newer = saved.version > GardenSaveData.CurrentVersion;
                if (newer) return false;
                if (saved.version == 1) MigrateV1(saved);
                return Valid(saved);
            }
            catch (ArgumentException) { return false; }
        }

        private static void MigrateV1(GardenSaveData saved)
        {
            if (saved.themes == null) saved.themes = Array.Empty<string>();
            if (saved.gearInventory == null) saved.gearInventory = Array.Empty<GearInstance>();
            if (saved.equippedGear == null) saved.equippedGear = Array.Empty<EquippedGearSave>();
            if (saved.chests == int.MinValue) saved.chests = 0;
            if (saved.essence == long.MinValue) saved.essence = 0;
            if (saved.chestPityMisses == int.MinValue) saved.chestPityMisses = 0;
            if (double.IsNaN(saved.chestTimerSeconds)) saved.chestTimerSeconds = 0;
            if (saved.plots != null)
                for (int i = 0; i < saved.plots.Length; i++)
                    if (saved.plots[i] != null && string.IsNullOrWhiteSpace(saved.plots[i].nextCropId))
                        saved.plots[i].nextCropId = saved.plots[i].cropId;
            saved.version = GardenSaveData.CurrentVersion;
        }

        public static bool Valid(GardenSaveData data)
        {
            if (data == null || data.version != GardenSaveData.CurrentVersion || !GardenProgression.Finite(data.gold) || data.gold < 0 || !GardenProgression.Finite(data.cycleGold) || data.cycleGold < 0 ||
                data.seeds < 0 || data.rebirths < 0 || data.inventory == null || data.upgrades == null || data.plots == null || data.themes == null ||
                data.chests < 0 || data.essence < 0 || data.chestPityMisses < 0 || !GardenProgression.Finite(data.chestTimerSeconds) || data.chestTimerSeconds < 0 ||
                data.gearInventory == null || data.equippedGear == null ||
                !GardenProgression.Finite(data.workerX) || !GardenProgression.Finite(data.workerWater) || data.workerWater < 0 || data.harvested < 0) return false;
            var ids = new HashSet<string>();
            foreach (var item in data.inventory)
                if (item == null || string.IsNullOrWhiteSpace(item.id) || item.count < 0 || !ids.Add(item.id)) return false;
            ids.Clear();
            foreach (var item in data.upgrades)
                if (item == null || string.IsNullOrWhiteSpace(item.id) || item.level < 0 || !ids.Add(item.id)) return false;
            ids.Clear();
            foreach (var item in data.gearInventory)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.id) || string.IsNullOrWhiteSpace(item.equipmentId) ||
                    !Enum.IsDefined(typeof(GearRarity), item.rarity) || item.affixes == null || item.affixes.Length > 6 || !ids.Add(item.id)) return false;
                for (int i = 0; i < item.affixes.Length; i++)
                {
                    var affix = item.affixes[i];
                    if (!Enum.IsDefined(typeof(UpgradeStat), affix.stat) || !GardenProgression.Finite(affix.percentBonus) || affix.percentBonus < 0 || affix.percentBonus > 1) return false;
                    for (int j = 0; j < i; j++) if (item.affixes[j].stat == affix.stat) return false;
                }
            }
            var equippedSlots = new HashSet<GearSlot>();
            foreach (var item in data.equippedGear)
                if (item == null || !Enum.IsDefined(typeof(GearSlot), item.slot) || string.IsNullOrWhiteSpace(item.instanceId) ||
                    !equippedSlots.Add(item.slot) || !ids.Contains(item.instanceId)) return false;
            foreach (var plot in data.plots)
                if (plot == null || string.IsNullOrWhiteSpace(plot.cropId) || string.IsNullOrWhiteSpace(plot.nextCropId) || !GardenProgression.Finite(plot.growth) || plot.growth < 0 || plot.growth > 1 ||
                    !GardenProgression.Finite(plot.water) || plot.water < 0 || plot.harvested < 0) return false;
            return true;
        }
    }
}
