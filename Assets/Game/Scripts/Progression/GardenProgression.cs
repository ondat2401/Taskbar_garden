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
        private readonly List<GearInstance> gearInventory = new List<GearInstance>();
        private readonly Dictionary<GearSlot, GearInstance> equippedGear = new Dictionary<GearSlot, GearInstance>();
        private readonly System.Random random;
        private double chestTimerSeconds;
        private int chestPityMisses;
        private long essence;
        private static readonly GearSlot[] GearSlots = { GearSlot.WateringCan, GearSlot.Boots, GearSlot.Gloves };
        private static readonly UpgradeStat[] AffixStats =
        {
            UpgradeStat.GrowSpeed, UpgradeStat.CropYield, UpgradeStat.MoveSpeed,
            UpgradeStat.ActionSpeed, UpgradeStat.TankCapacity, UpgradeStat.WaterEfficiency
        };
        public int Chests { get; private set; }
        public long Essence => essence;
        public int ChestPityMisses => chestPityMisses;
        public double ChestSecondsRemaining => Config.gear ? Math.Max(0, Config.gear.chestIntervalSeconds - chestTimerSeconds) : 0;
        public System.Collections.Generic.IReadOnlyList<GearInstance> GearInventory => gearInventory;
        private bool firstHarvestReward;
        public double Gold { get; private set; }
        public double CycleGold { get; private set; }
        public long Seeds { get; private set; }
        public int Rebirths { get; private set; }
        public bool AutoSell = true;
        public double RebirthGoal => Config.rebirthBaseGoal * Math.Pow(Config.rebirthGoalGrowth, Rebirths);
        public double GoldMultiplier => 1 + Seeds * (double)Config.goldBonusPerSeed;
        public bool CanRebirth => Finite(RebirthGoal) && CycleGold >= RebirthGoal && Seeds < long.MaxValue && Rebirths < int.MaxValue;

        public GardenProgression(ProgressionConfig config, GardenSaveData saved = null, System.Random random = null)
        {
            if (!config || !config.Validate(out _)) throw new ArgumentException("Invalid progression config.");
            Config = config; Gold = config.startingGold; this.random = random ?? new System.Random();
            if (saved != null)
            {
                Gold = saved.gold; CycleGold = saved.cycleGold; Seeds = saved.seeds; Rebirths = saved.rebirths; AutoSell = saved.autoSell;
                foreach (var entry in saved.inventory) inventory[entry.id] = entry.count;
                foreach (var entry in saved.upgrades) levels[entry.id] = entry.level;
                if (saved.themes != null) foreach (string id in saved.themes) unlockedThemes.Add(id);
                Chests = saved.chests; firstHarvestReward = saved.firstHarvestReward;
                essence = Math.Max(0, saved.essence);
                chestPityMisses = Math.Max(0, saved.chestPityMisses);
                if (saved.gearInventory != null)
                    foreach (var item in saved.gearInventory) if (ValidGearInstance(item) && FindGear(item.id) == null) gearInventory.Add(item);
                if (saved.equippedGear != null)
                    foreach (var entry in saved.equippedGear)
                    {
                        var item = entry == null ? null : FindGear(entry.instanceId);
                        var definition = item != null && Config.gear ? Config.gear.FindEquipment(item.equipmentId) : null;
                        if (item != null && definition && Enum.IsDefined(typeof(GearSlot), entry.slot) && !equippedGear.ContainsKey(entry.slot) &&
                            definition.slot == entry.slot) equippedGear.Add(entry.slot, item);
                    }
                if (Config.gear && GardenProgression.Finite(saved.chestTimerSeconds) && saved.chestTimerSeconds >= 0)
                    chestTimerSeconds = saved.chestTimerSeconds % Config.gear.chestIntervalSeconds;
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
        public GearInstance Equipped(GearSlot slot) => equippedGear.TryGetValue(slot, out var item) ? item : null;
        public bool IsEquipped(GearInstance item)
        {
            if (item == null) return false;
            for (int i = 0; i < GearSlots.Length; i++)
                if (equippedGear.TryGetValue(GearSlots[i], out var equipped) && equipped.id == item.id) return true;
            return false;
        }

        public void TickChestTimer(float elapsedSeconds, bool paused)
        {
            if (paused || !Config.gear || Chests == int.MaxValue || !Finite(elapsedSeconds) || elapsedSeconds <= 0) return;
            double total = chestTimerSeconds + elapsedSeconds;
            double interval = Config.gear.chestIntervalSeconds;
            double completed = Math.Floor(total / interval);
            if (completed < 1) { chestTimerSeconds = total; return; }
            Chests += (int)Math.Min(completed, int.MaxValue - Chests);
            chestTimerSeconds = total - completed * interval;
        }

        public bool TryOpenChest(out GearInstance drop)
        {
            drop = null;
            if (!Config.gear || Chests <= 0 || gearInventory.Count >= Config.gear.inventoryCapacity) return false;
            GearRarity rarity = RollRarity();
            var definition = Config.gear.equipment[random.Next(Config.gear.equipment.Length)];
            var rule = Config.gear.FindRarity(rarity);
            var affixes = RollAffixes(rule);
            drop = new GearInstance { id = Guid.NewGuid().ToString("N"), equipmentId = definition.id, rarity = rarity, affixes = affixes };
            gearInventory.Add(drop);
            Chests--;
            chestPityMisses = rarity < GearRarity.Epic ? Math.Min(int.MaxValue, chestPityMisses + 1) : 0;
            return true;
        }

        public bool TryEquip(string gearId)
        {
            var item = FindGear(gearId);
            var definition = item != null && Config.gear ? Config.gear.FindEquipment(item.equipmentId) : null;
            if (!definition) return false;
            equippedGear[definition.slot] = item;
            return true;
        }

        public bool TryUnequip(GearSlot slot) => equippedGear.Remove(slot);

        public bool TrySalvage(string gearId)
        {
            if (!Config.gear) return false;
            int index = gearInventory.FindIndex(item => item.id == gearId);
            if (index < 0 || IsEquipped(gearInventory[index])) return false;
            var rule = Config.gear.FindRarity(gearInventory[index].rarity);
            if (rule == null || essence > long.MaxValue - rule.salvageEssence) return false;
            essence += rule.salvageEssence;
            gearInventory.RemoveAt(index);
            return true;
        }

        public bool TryBuyChestWithEssence()
        {
            if (!Config.gear || Chests == int.MaxValue || essence < Config.gear.essencePerChest) return false;
            essence -= Config.gear.essencePerChest;
            Chests++;
            return true;
        }

        public float GearFlatBonus(UpgradeStat stat)
        {
            if (!Config.gear) return 0;
            double bonus = 0;
            for (int i = 0; i < GearSlots.Length; i++)
            {
                var item = Equipped(GearSlots[i]);
                var definition = item != null ? Config.gear.FindEquipment(item.equipmentId) : null;
                if (!definition) continue;
                for (int j = 0; j < definition.baseStats.Length; j++)
                    if (definition.baseStats[j].stat == stat) bonus += definition.baseStats[j].flatBonus;
            }
            return (float)Math.Min(1000000, bonus);
        }

        private GearRarity RollRarity()
        {
            var config = Config.gear;
            if (chestPityMisses >= config.pityThreshold - 1)
            {
                double pityTotal = (double)config.pityEpicWeight + config.pityLegendaryWeight;
                return random.NextDouble() * pityTotal < config.pityEpicWeight ? GearRarity.Epic : GearRarity.Legendary;
            }
            double total = 0;
            for (int i = 0; i < config.rarities.Length; i++) total += config.rarities[i].weight;
            double roll = random.NextDouble() * total;
            for (int i = 0; i < config.rarities.Length; i++)
            {
                roll -= config.rarities[i].weight;
                if (roll < 0) return config.rarities[i].rarity;
            }
            return GearRarity.Legendary;
        }

        private GearAffix[] RollAffixes(GearRarityRule rule)
        {
            var result = new GearAffix[rule.affixCount];
            var order = new int[AffixStats.Length];
            for (int i = 0; i < order.Length; i++) order[i] = i;
            for (int i = 0; i < result.Length; i++)
            {
                int swap = random.Next(i, order.Length);
                int temp = order[i]; order[i] = order[swap]; order[swap] = temp;
                float range = rule.maximumAffixPercent - rule.minimumAffixPercent;
                result[i] = new GearAffix
                {
                    stat = AffixStats[order[i]],
                    percentBonus = rule.minimumAffixPercent + (float)random.NextDouble() * range
                };
            }
            return result;
        }

        private GearInstance FindGear(string id)
        {
            for (int i = 0; i < gearInventory.Count; i++) if (gearInventory[i].id == id) return gearInventory[i];
            return null;
        }

        private static bool ValidGearInstance(GearInstance item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.id) || string.IsNullOrWhiteSpace(item.equipmentId) ||
                !Enum.IsDefined(typeof(GearRarity), item.rarity) || item.affixes == null || item.affixes.Length > AffixStats.Length) return false;
            for (int i = 0; i < item.affixes.Length; i++)
            {
                var affix = item.affixes[i];
                if (!Enum.IsDefined(typeof(UpgradeStat), affix.stat) || !Finite(affix.percentBonus) || affix.percentBonus < 0 || affix.percentBonus > 1) return false;
                for (int j = 0; j < i; j++) if (item.affixes[j].stat == affix.stat) return false;
            }
            return true;
        }

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
            if (Config.gear)
                for (int i = 0; i < GearSlots.Length; i++)
                {
                    var item = Equipped(GearSlots[i]);
                    var definition = item != null ? Config.gear.FindEquipment(item.equipmentId) : null;
                    if (!definition) continue;
                    for (int j = 0; j < definition.baseStats.Length; j++)
                        if (definition.baseStats[j].stat == stat) multiplier += definition.baseStats[j].percentBonus;
                    for (int j = 0; j < item.affixes.Length; j++)
                        if (item.affixes[j].stat == stat) multiplier += item.affixes[j].percentBonus;
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
                inventory = new InventorySave[inventory.Count], upgrades = new UpgradeSave[levels.Count], themes = new string[unlockedThemes.Count], chests = Chests, firstHarvestReward = firstHarvestReward,
                gearInventory = gearInventory.ToArray(), equippedGear = new EquippedGearSave[equippedGear.Count], essence = essence, chestPityMisses = chestPityMisses, chestTimerSeconds = chestTimerSeconds };
            unlockedThemes.CopyTo(saved.themes);
            int index = 0;
            foreach (var item in inventory) saved.inventory[index++] = new InventorySave { id = item.Key, count = item.Value };
            index = 0;
            foreach (var item in levels) saved.upgrades[index++] = new UpgradeSave { id = item.Key, level = item.Value };
            index = 0;
            foreach (var item in equippedGear) saved.equippedGear[index++] = new EquippedGearSave { slot = item.Key, instanceId = item.Value.id };
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
