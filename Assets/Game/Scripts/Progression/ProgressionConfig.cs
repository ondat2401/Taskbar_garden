using UnityEngine;

namespace IdleGarden
{
    [CreateAssetMenu(menuName = "Idle Garden/Progression")]
    public sealed class ProgressionConfig : ScriptableObject
    {
        public CropDefinition[] crops;
        public UpgradeDefinition[] upgrades;
        [Min(0)] public double startingGold = 30;
        [Min(0)] public int autoSellReserve = 4;
        [Min(1)] public double rebirthBaseGoal = 1000;
        [Min(1)] public float rebirthGoalGrowth = 1.6f;
        [Min(.01f)] public float goldBonusPerSeed = .25f;

        public bool Validate(out string error)
        {
            if (crops == null || crops.Length == 0 || upgrades == null || upgrades.Length == 0)
            { error = "Progression needs crops and upgrades."; return false; }
            if (!GardenProgression.Finite(startingGold) || startingGold < 0 || !GardenProgression.Finite(rebirthBaseGoal) || rebirthBaseGoal <= 0 || rebirthGoalGrowth < 1 || goldBonusPerSeed <= 0)
            { error = "Invalid gold / rebirth settings."; return false; }
            for (int i = 0; i < crops.Length; i++)
            {
                if (!crops[i] || string.IsNullOrWhiteSpace(crops[i].id) || crops[i].sellPrice <= 0)
                { error = $"Catalog crop {i + 1} is invalid."; return false; }
                for (int j = 0; j < i; j++) if (crops[i].id == crops[j].id)
                { error = "Duplicate crop ID: " + crops[i].id; return false; }
            }
            for (int i = 0; i < upgrades.Length; i++)
            {
                var u = upgrades[i];
                if (!u || string.IsNullOrWhiteSpace(u.id) || !GardenProgression.Finite(u.baseGoldCost) || u.baseGoldCost <= 0 || u.costGrowth <= 1 || u.bonusPerLevel <= 0 || u.baseMaterialCost < 0 || u.materialStepLevels < 1 ||
                    (u.material && !Contains(u.material)) || (u.baseMaterialCost > 0 && !u.material) || (u.targetCrop && !Contains(u.targetCrop)))
                { error = $"Upgrade {i + 1}: check ID, costs, bonus and catalog references."; return false; }
                for (int j = 0; j < i; j++) if (upgrades[j].id == u.id)
                { error = "Duplicate upgrade ID: " + u.id; return false; }
            }
            error = null; return true;
        }

        public bool Contains(CropDefinition crop)
        {
            for (int i = 0; i < crops.Length; i++) if (crops[i] == crop) return true;
            return false;
        }
    }
}
