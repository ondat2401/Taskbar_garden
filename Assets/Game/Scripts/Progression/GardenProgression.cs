using System;
using System.Collections.Generic;
using System.Globalization;

namespace IdleGarden
{
    public sealed class GardenProgression
    {
        public readonly ProgressionConfig Config;
        private readonly Dictionary<string, long> inventory = new Dictionary<string, long>();
        private readonly Dictionary<string, int> levels = new Dictionary<string, int>();
        private readonly HashSet<string> unlockedThemes = new HashSet<string>();
        public int Chests { get; private set; }
        private bool firstHarvestReward;
        public double Gold { get; private set; }
        public double CycleGold { get; private set; }
        public long Seeds { get; private set; }
        public int Rebirths { get; private set; }
        public bool AutoSell = true;
        public double RebirthGoal => Config.rebirthBaseGoal * Math.Pow(Config.rebirthGoalGrowth, Rebirths);
        public double GoldMultiplier => 1 + Seeds * (double)Config.goldBonusPerSeed;
        public bool CanRebirth => Finite(RebirthGoal) && CycleGold >= RebirthGoal && Seeds < long.MaxValue && Rebirths < int.MaxValue;

        public GardenProgression(ProgressionConfig config, GardenSaveData saved = null)
        {
            if (!config || !config.Validate(out _)) throw new ArgumentException("Invalid progression config.");
            Config = config; Gold = config.startingGold;
            if (saved != null)
            {
                Gold = saved.gold; CycleGold = saved.cycleGold; Seeds = saved.seeds; Rebirths = saved.rebirths; AutoSell = saved.autoSell;
                foreach (var entry in saved.inventory) inventory[entry.id] = entry.count;
                foreach (var entry in saved.upgrades) levels[entry.id] = entry.level;
                if (saved.themes != null) foreach (string id in saved.themes) unlockedThemes.Add(id);
                Chests = saved.chests; firstHarvestReward = saved.firstHarvestReward;
            }
            foreach (var crop in config.crops) if (!inventory.ContainsKey(crop.id)) inventory.Add(crop.id, 0);
            foreach (var upgrade in config.upgrades) if (!levels.ContainsKey(upgrade.id)) levels.Add(upgrade.id, 0);
        }

        public long Count(CropDefinition crop) => crop && inventory.TryGetValue(crop.id, out long count) ? count : 0;
        public bool ThemeUnlocked(GardenTheme theme) => !theme || theme.unlockGold == 0 || unlockedThemes.Contains(theme.id);
        public bool CropUnlocked(CropDefinition crop) => crop && Config.Contains(crop) && ThemeUnlocked(crop.theme);
        public bool UnlockTheme(GardenTheme theme)
        {
            if (!theme || !Config.ContainsTheme(theme) || ThemeUnlocked(theme) || Gold < theme.unlockGold || !Finite(theme.unlockGold) || theme.unlockGold <= 0) return false;
            Gold -= theme.unlockGold; unlockedThemes.Add(theme.id); GrantChest(); return true;
        }
        public void GrantChest() { if (Chests < int.MaxValue) Chests++; }
        public int Level(int index) => levels.TryGetValue(Config.upgrades[index].id, out int level) ? level : 0;
        public double GoldCost(int index) => Config.upgrades[index].baseGoldCost * Math.Pow(Config.upgrades[index].costGrowth, Level(index));
        public long MaterialCost(int index)
        {
            var upgrade = Config.upgrades[index];
            return upgrade.material ? (long)upgrade.baseMaterialCost * (1L + Level(index) / Math.Max(1, upgrade.materialStepLevels)) : 0;
        }

        public float Multiplier(UpgradeStat stat, CropDefinition crop = null)
        {
            double multiplier = 1;
            for (int i = 0; i < Config.upgrades.Length; i++)
            {
                var upgrade = Config.upgrades[i];
                if (upgrade.stat == stat && (!upgrade.targetCrop || upgrade.targetCrop == crop))
                    multiplier += Level(i) * (double)upgrade.bonusPerLevel;
            }
            return (float)Math.Min(1000000, multiplier);
        }

        public void AddHarvest(CropDefinition crop, int quantity)
        {
            if (!crop || !Config.Contains(crop) || quantity <= 0 || !inventory.TryGetValue(crop.id, out long count) || count > long.MaxValue - quantity) return;
            inventory[crop.id] = count + quantity;
            if (!firstHarvestReward) { firstHarvestReward = true; GrantChest(); }
            if (AutoSell) Sell(crop, Math.Max(0, Count(crop) - Math.Max(0, Config.autoSellReserve)));
        }

        public double Sell(CropDefinition crop, long quantity)
        {
            if (!crop || !Config.Contains(crop) || crop.sellPrice <= 0 || quantity <= 0) return 0;
            quantity = Math.Min(quantity, Count(crop));
            double value = quantity * (double)crop.sellPrice * GoldMultiplier;
            double nextGold = Gold + value, nextCycleGold = CycleGold + value;
            // Reject the whole sale when a double can no longer represent its credit.
            if (quantity == 0 || value <= 0 || !Finite(value) || !Finite(nextGold) || !Finite(nextCycleGold) || nextGold <= Gold || nextCycleGold <= CycleGold) return 0;
            inventory[crop.id] -= quantity; Gold = nextGold; CycleGold = nextCycleGold; return value;
        }

        public double SellSurplus()
        {
            double total = 0;
            foreach (var crop in Config.crops) total += Sell(crop, Math.Max(0, Count(crop) - Math.Max(0, Config.autoSellReserve)));
            return total;
        }

        public bool CanBuy(int index) => index >= 0 && index < Config.upgrades.Length && Finite(GoldCost(index)) &&
            Level(index) < int.MaxValue && Gold >= GoldCost(index) && Count(Config.upgrades[index].material) >= MaterialCost(index);

        public bool TryBuy(int index)
        {
            if (!CanBuy(index)) return false;
            var upgrade = Config.upgrades[index];
            Gold -= GoldCost(index);
            if (upgrade.material) inventory[upgrade.material.id] -= MaterialCost(index);
            levels[upgrade.id] = Level(index) + 1;
            return true;
        }

        public bool TryRebirth()
        {
            if (!CanRebirth) return false;
            Seeds++; Rebirths++; Gold = Config.startingGold; CycleGold = 0;
            // Existing keys are retained so removed catalog entries survive config edits/save cycles.
            var cropKeys = new List<string>(inventory.Keys);
            foreach (string key in cropKeys) inventory[key] = 0;
            var upgradeKeys = new List<string>(levels.Keys);
            foreach (string key in upgradeKeys) levels[key] = 0;
            return true;
        }

        public GardenSaveData Capture()
        {
            var saved = new GardenSaveData { gold = Gold, cycleGold = CycleGold, seeds = Seeds, rebirths = Rebirths, autoSell = AutoSell,
                inventory = new InventorySave[inventory.Count], upgrades = new UpgradeSave[levels.Count], themes = new string[unlockedThemes.Count], chests = Chests, firstHarvestReward = firstHarvestReward };
            unlockedThemes.CopyTo(saved.themes);
            int index = 0;
            foreach (var item in inventory) saved.inventory[index++] = new InventorySave { id = item.Key, count = item.Value };
            index = 0;
            foreach (var item in levels) saved.upgrades[index++] = new UpgradeSave { id = item.Key, level = item.Value };
            return saved;
        }

        public static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        public static string Number(double value)
        {
            if (!Finite(value)) return "Limit";
            if (value >= 1e12) return value.ToString("0.##e+0", CultureInfo.InvariantCulture);
            if (value >= 1e9) return (value / 1e9).ToString("0.#", CultureInfo.InvariantCulture) + "B";
            if (value >= 1e6) return (value / 1e6).ToString("0.#", CultureInfo.InvariantCulture) + "M";
            if (value >= 1e3) return (value / 1e3).ToString("0.#", CultureInfo.InvariantCulture) + "K";
            return value.ToString("0", CultureInfo.InvariantCulture);
        }
    }
}
