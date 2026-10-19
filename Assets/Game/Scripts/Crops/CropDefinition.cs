using UnityEngine;

namespace IdleGarden
{
    [CreateAssetMenu(menuName = "Idle Garden/Crop")]
    public sealed class CropDefinition : ScriptableObject
    {
        public string id = "carrot";
        public string displayName = "Carrot";
        public GardenTheme theme;
        [Min(1)] public float growSeconds = 20;
        [Min(0)] public float waterPerSecond = 1;
        [Min(1)] public float waterCapacity = 12;
        [Range(0, 1)] public float waterThreshold = .3f;
        [Min(1)] public int yield = 1;
        [Min(1)] public int sellPrice = 8;
        public Sprite[] stages;
    }
}

