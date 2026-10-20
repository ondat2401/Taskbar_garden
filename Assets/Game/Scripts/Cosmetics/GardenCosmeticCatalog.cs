using System;
using UnityEngine;

namespace IdleGarden
{
    public enum GardenCosmeticTarget { Worker, Pot, Crop, Set }
    public enum GardenCosmeticOffer { Free, Premium, FixedBundle }

    [Serializable]
    public sealed class GardenCosmeticDefinition
    {
        public string id;
        public string displayName;
        [TextArea(2, 4)] public string description;
        public GardenCosmeticTarget target;
        public GardenCosmeticOffer offer;
        public Color workerTint = Color.white;
        public Color potTint = Color.white;
        public Color cropTint = Color.white;
    }

    [CreateAssetMenu(menuName = "Idle Garden/Cosmetic Catalog")]
    public sealed class GardenCosmeticCatalog : ScriptableObject
    {
        public GardenCosmeticDefinition[] items = Array.Empty<GardenCosmeticDefinition>();

        public GardenCosmeticDefinition Find(string id)
        {
            if (string.IsNullOrEmpty(id) || items == null) return null;
            for (int i = 0; i < items.Length; i++)
                if (items[i] != null && items[i].id == id) return items[i];
            return null;
        }

        public bool Validate(out string error)
        {
            if (items == null || items.Length == 0) { error = "Cosmetic catalog is empty."; return false; }
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item == null || string.IsNullOrWhiteSpace(item.id) || string.IsNullOrWhiteSpace(item.displayName) ||
                    !Enum.IsDefined(typeof(GardenCosmeticTarget), item.target) || !Enum.IsDefined(typeof(GardenCosmeticOffer), item.offer))
                { error = $"Cosmetic {i + 1} has incomplete data."; return false; }
                if ((item.offer == GardenCosmeticOffer.FixedBundle) != (item.target == GardenCosmeticTarget.Set) ||
                    (item.offer == GardenCosmeticOffer.Free && item.target == GardenCosmeticTarget.Set))
                { error = $"Cosmetic {item.id} has an incompatible offer and target."; return false; }
                for (int j = 0; j < i; j++)
                    if (items[j] != null && items[j].id == item.id)
                    { error = $"Duplicate cosmetic ID: {item.id}."; return false; }
            }
            error = null;
            return true;
        }
    }
}
