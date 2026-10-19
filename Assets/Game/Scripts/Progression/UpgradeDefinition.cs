using UnityEngine;

namespace IdleGarden
{
    public enum UpgradeStat { GrowSpeed, CropYield, MoveSpeed, ActionSpeed, TankCapacity, WaterEfficiency }

    [CreateAssetMenu(menuName = "Idle Garden/Upgrade")]
    public sealed class UpgradeDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public UpgradeStat stat;
        public CropDefinition targetCrop;
        [Min(.01f)] public float bonusPerLevel = .15f;
        [Min(1)] public double baseGoldCost = 30;
        [Min(1.01f)] public float costGrowth = 1.45f;
        public CropDefinition material;
        [Min(0)] public int baseMaterialCost = 1;
        [Min(1)] public int materialStepLevels = 5;
    }
}
