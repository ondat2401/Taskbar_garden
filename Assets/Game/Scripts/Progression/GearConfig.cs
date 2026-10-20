using System;
using UnityEngine;

namespace IdleGarden
{
    [Serializable]
    public sealed class GearRarityRule
    {
        public GearRarity rarity;
        [Min(0)] public float weight;
        [Range(0, 6)] public int affixCount;
        [Range(0, 1)] public float minimumAffixPercent;
        [Range(0, 1)] public float maximumAffixPercent;
        [Min(0)] public int salvageEssence;
    }

    [CreateAssetMenu(menuName = "Idle Garden/Gear Config")]
    public sealed class GearConfig : ScriptableObject
    {
        public EquipmentDefinition[] equipment = Array.Empty<EquipmentDefinition>();
        public GearRarityRule[] rarities =
        {
            new GearRarityRule { rarity = GearRarity.Common, weight = 55, affixCount = 0, salvageEssence = 1 },
            new GearRarityRule { rarity = GearRarity.Uncommon, weight = 25, affixCount = 1, minimumAffixPercent = .03f, maximumAffixPercent = .07f, salvageEssence = 2 },
            new GearRarityRule { rarity = GearRarity.Rare, weight = 15, affixCount = 2, minimumAffixPercent = .05f, maximumAffixPercent = .12f, salvageEssence = 3 },
            new GearRarityRule { rarity = GearRarity.Epic, weight = 4, affixCount = 3, minimumAffixPercent = .08f, maximumAffixPercent = .18f, salvageEssence = 5 },
            new GearRarityRule { rarity = GearRarity.Legendary, weight = 1, affixCount = 3, minimumAffixPercent = .15f, maximumAffixPercent = .25f, salvageEssence = 8 }
        };
        [Min(1)] public int inventoryCapacity = 40;
        [Min(1)] public float chestIntervalSeconds = 600;
        [Min(1)] public int pityThreshold = 20;
        [Min(.01f)] public float pityEpicWeight = 80;
        [Min(.01f)] public float pityLegendaryWeight = 20;
        [Min(1)] public int essencePerChest = 10;

        public bool Validate(out string error)
        {
            if (equipment == null || equipment.Length == 0 || rarities == null || rarities.Length != 5 ||
                inventoryCapacity < 1 || !GardenProgression.Finite(chestIntervalSeconds) || chestIntervalSeconds < 1 ||
                pityThreshold < 1 || !GardenProgression.Finite(pityEpicWeight) || pityEpicWeight <= 0 ||
                !GardenProgression.Finite(pityLegendaryWeight) || pityLegendaryWeight <= 0 || essencePerChest < 1)
            { error = "Gear needs equipment, five rarity rules, positive capacity, timer and pity settings."; return false; }

            for (int i = 0; i < equipment.Length; i++)
            {
                var item = equipment[i];
                if (!item || string.IsNullOrWhiteSpace(item.id) || string.IsNullOrWhiteSpace(item.displayName) ||
                    !Enum.IsDefined(typeof(GearSlot), item.slot) || item.baseStats == null)
                { error = $"Equipment {i + 1} is invalid."; return false; }
                for (int j = 0; j < i; j++)
                    if (equipment[j].id == item.id) { error = "Duplicate equipment ID: " + item.id; return false; }
                for (int j = 0; j < item.baseStats.Length; j++)
                {
                    var stat = item.baseStats[j];
                    if (!Enum.IsDefined(typeof(UpgradeStat), stat.stat) || !GardenProgression.Finite(stat.flatBonus) ||
                        stat.flatBonus < 0 || !GardenProgression.Finite(stat.percentBonus) || stat.percentBonus < 0 || stat.percentBonus > 1)
                    { error = "Equipment base stats must be finite, non-negative bonuses."; return false; }
                    for (int k = 0; k < j; k++)
                        if (item.baseStats[k].stat == stat.stat) { error = "Equipment has a duplicate base stat."; return false; }
                }
            }

            double totalWeight = 0;
            var seen = new bool[5];
            for (int i = 0; i < rarities.Length; i++)
            {
                var rule = rarities[i];
                if (rule == null || !Enum.IsDefined(typeof(GearRarity), rule.rarity) || seen[(int)rule.rarity] ||
                    !GardenProgression.Finite(rule.weight) || rule.weight < 0 || rule.affixCount < 0 || rule.affixCount > 6 ||
                    !GardenProgression.Finite(rule.minimumAffixPercent) || !GardenProgression.Finite(rule.maximumAffixPercent) ||
                    rule.minimumAffixPercent < 0 || rule.maximumAffixPercent < rule.minimumAffixPercent || rule.maximumAffixPercent > 1 || rule.salvageEssence < 0)
                { error = "Rarity rules are invalid or duplicated."; return false; }
                seen[(int)rule.rarity] = true;
                totalWeight += rule.weight;
            }
            if (!GardenProgression.Finite(totalWeight) || totalWeight <= 0)
            { error = "At least one rarity needs a positive drop weight."; return false; }

            error = null;
            return true;
        }

        public EquipmentDefinition FindEquipment(string id)
        {
            for (int i = 0; i < equipment.Length; i++) if (equipment[i] && equipment[i].id == id) return equipment[i];
            return null;
        }

        public GearRarityRule FindRarity(GearRarity rarity)
        {
            for (int i = 0; i < rarities.Length; i++) if (rarities[i] != null && rarities[i].rarity == rarity) return rarities[i];
            return null;
        }
    }
}
