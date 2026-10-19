using UnityEngine;

namespace IdleGarden
{
    [CreateAssetMenu(menuName = "Idle Garden/Theme")]
    public sealed class GardenTheme : ScriptableObject
    {
        public string id;
        public string displayName;
        [Min(0)] public double unlockGold;
        public Color cropTint = Color.white;
    }
}
