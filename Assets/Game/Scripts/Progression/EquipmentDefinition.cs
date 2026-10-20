using System;
using UnityEngine;

namespace IdleGarden
{
    public enum GearSlot { WateringCan, Boots, Gloves }
    public enum GearRarity { Common, Uncommon, Rare, Epic, Legendary }

    [Serializable]
    public struct GearBaseStat
    {
        public UpgradeStat stat;
        [Min(0)] public float flatBonus;
        [Range(0, 1)] public float percentBonus;
    }

    [Serializable]
    public struct GearAffix
    {
        public UpgradeStat stat;
        [Range(0, 1)] public float percentBonus;
    }

    [Serializable]
    public sealed class GearInstance
    {
        public string id;
        public string equipmentId;
        public GearRarity rarity;
        public GearAffix[] affixes = Array.Empty<GearAffix>();
    }

    [Serializable]
    public sealed class EquippedGearSave
    {
        public GearSlot slot;
        public string instanceId;
    }

    [CreateAssetMenu(menuName = "Idle Garden/Equipment")]
    public sealed class EquipmentDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public GearSlot slot;
        public GearBaseStat[] baseStats = Array.Empty<GearBaseStat>();
    }
}
