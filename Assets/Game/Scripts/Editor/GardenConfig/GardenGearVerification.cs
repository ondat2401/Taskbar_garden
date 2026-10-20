using System;
using UnityEditor;
using UnityEngine;

namespace IdleGarden.Editor
{
    public static class GardenGearVerification
    {
        [MenuItem("Tools/Idle Garden/Verify F04 Gear")]
        public static void Run()
        {
            var profile = AssetDatabase.LoadAssetAtPath<GardenProfile>(GardenPrototypeBuilder.ProfilePath);
            var config = profile ? profile.progression : null;
            string error = null;
            Check(config && config.gear && config.Validate(out error), "Invalid gear config: " + error);
            var gear = config.gear;

            var timed = Progression(config, new System.Random(17), Save(timer: gear.chestIntervalSeconds - .25));
            timed.TickChestTimer(5, true);
            Check(timed.Chests == 0 && Math.Abs(timed.ChestSecondsRemaining - .25) < .001, "Paused time advanced chest timer.");
            timed.TickChestTimer(.5f, false);
            Check(timed.Chests == 1 && Math.Abs(timed.ChestSecondsRemaining - (gear.chestIntervalSeconds - .25)) < .001, "Timed chest boundary/remainder is wrong.");
            var resumed = Progression(config, new System.Random(18), timed.Capture());
            resumed.TickChestTimer(gear.chestIntervalSeconds - .25f, false);
            Check(resumed.Chests == 2, "Saved timer remainder did not grant exactly one chest.");

            var pitySave = Save(chests: 1, pityMisses: gear.pityThreshold - 1);
            var pity = Progression(config, new System.Random(4), pitySave);
            Check(pity.TryOpenChest(out var guaranteed) && guaranteed.rarity >= GearRarity.Epic && pity.Chests == 0 && pity.ChestPityMisses == 0,
                "Pity chest was not guaranteed and consumed exactly once.");
            CheckAffixes(gear, guaranteed);

            var first = Progression(config, new System.Random(12345));
            var second = Progression(config, new System.Random(12345));
            for (int i = 0; i < 30; i++)
            {
                first.GrantChest(); second.GrantChest();
                bool leftOpened = first.TryOpenChest(out var left);
                bool rightOpened = second.TryOpenChest(out var right);
                Check(leftOpened && rightOpened, "Seeded chest could not open.");
                Check(left.rarity == right.rarity && left.equipmentId == right.equipmentId && SameAffixes(left, right), "Seeded drop was not repeatable.");
                CheckAffixes(gear, left);
            }

            var full = Save(chests: 1);
            full.gearInventory = new GearInstance[gear.inventoryCapacity];
            for (int i = 0; i < full.gearInventory.Length; i++)
                full.gearInventory[i] = Item("full-" + i, "wooden-can", GearRarity.Common);
            var packed = Progression(config, new System.Random(5), full);
            Check(!packed.TryOpenChest(out _) && packed.Chests == 1 && packed.GearInventory.Count == gear.inventoryCapacity,
                "Full inventory consumed a chest or dropped gear.");

            var boot = Item("boot", "garden-boots", GearRarity.Common);
            var can = Item("can", "wooden-can", GearRarity.Common);
            var equippedSave = Save();
            equippedSave.gearInventory = new[] { boot, can };
            equippedSave.equippedGear = new[]
            {
                new EquippedGearSave { slot = GearSlot.Boots, instanceId = boot.id },
                new EquippedGearSave { slot = GearSlot.WateringCan, instanceId = can.id }
            };
            var equipped = Progression(config, new System.Random(6), equippedSave);
            Check(Math.Abs(equipped.Multiplier(UpgradeStat.MoveSpeed) - 1.05f) < .001f, "Boot base stat was not applied.");
            var simulation = new GardenSimulation(profile, equipped);
            Check(Math.Abs(simulation.TankCapacity - (profile.worker.tankCapacity + 4)) < .001f, "Can capacity did not reach simulation.");
            Check(!equipped.TrySalvage(boot.id), "Equipped gear was salvaged.");
            Check(equipped.TryUnequip(GearSlot.Boots) && equipped.TrySalvage(boot.id) && equipped.Essence == 1,
                "Unequipped gear did not salvage into essence.");

            var exchangeSave = Save(essence: gear.essencePerChest);
            var exchange = Progression(config, new System.Random(7), exchangeSave);
            Check(exchange.TryBuyChestWithEssence() && exchange.Essence == 0 && exchange.Chests == 1, "Essence exchange did not charge/grant exactly once.");
            var captured = equipped.Capture();
            Check(GardenSaveStore.Valid(captured), "Gear capture produced invalid save data.");
            var restored = Progression(config, new System.Random(8), captured);
            Check(restored.GearInventory.Count == 1 && restored.Equipped(GearSlot.WateringCan)?.id == can.id && restored.Essence == 1,
                "Gear/equip/essence did not survive capture/restore.");

            Debug.Log("PASS F04: paused timer/remainder, pity, seeded drops, affix bounds/uniqueness, full bag, equip effects, protected salvage, essence exchange and capture/restore.");
        }

        private static GardenProgression Progression(ProgressionConfig config, System.Random random, GardenSaveData save = null) =>
            new GardenProgression(config, save ?? Save(), random);

        private static GardenSaveData Save(int chests = 0, int pityMisses = 0, double timer = 0, long essence = 0) => new GardenSaveData
        {
            gold = 0, cycleGold = 0, chests = chests, chestPityMisses = pityMisses, chestTimerSeconds = timer, essence = essence,
            inventory = Array.Empty<InventorySave>(), upgrades = Array.Empty<UpgradeSave>(), plots = Array.Empty<PlotSave>(),
            themes = Array.Empty<string>(), gearInventory = Array.Empty<GearInstance>(), equippedGear = Array.Empty<EquippedGearSave>()
        };

        private static GearInstance Item(string id, string equipmentId, GearRarity rarity) =>
            new GearInstance { id = id, equipmentId = equipmentId, rarity = rarity, affixes = Array.Empty<GearAffix>() };

        private static bool SameAffixes(GearInstance left, GearInstance right)
        {
            if (left.affixes.Length != right.affixes.Length) return false;
            for (int i = 0; i < left.affixes.Length; i++)
                if (left.affixes[i].stat != right.affixes[i].stat || left.affixes[i].percentBonus != right.affixes[i].percentBonus) return false;
            return true;
        }

        private static void CheckAffixes(GearConfig config, GearInstance item)
        {
            var rule = config.FindRarity(item.rarity);
            Check(rule != null && item.affixes.Length == rule.affixCount, "Affix count did not match rarity.");
            for (int i = 0; i < item.affixes.Length; i++)
            {
                var affix = item.affixes[i];
                Check(affix.percentBonus >= rule.minimumAffixPercent && affix.percentBonus <= rule.maximumAffixPercent,
                    "Affix value fell outside rarity bounds.");
                for (int j = 0; j < i; j++) Check(item.affixes[j].stat != affix.stat, "Affix stat was duplicated.");
            }
        }

        private static void Check(bool condition, string error)
        {
            if (!condition) throw new Exception(error);
        }
    }
}
