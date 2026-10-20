using UnityEditor;
using UnityEngine;

namespace IdleGarden.Editor
{
    public static class GardenCosmeticVerification
    {
        [MenuItem("Tools/Idle Garden/Verify F06 Cosmetics")]
        public static void Run()
        {
            var garden = Object.FindFirstObjectByType<GardenController>();
            Check(garden && garden.cosmetics && garden.hud && garden.hud.cosmeticWindow && garden.hud.cosmeticPanel,
                "F06 service or floating-window UI is not wired in the prototype scene.");
            var catalog = garden.cosmetics.catalog;
            string error = null;
            Check(catalog && catalog.Validate(out error), "Invalid cosmetic catalog: " + error);
            int free = 0, premium = 0, bundles = 0;
            GardenCosmeticDefinition freeWorker = null, premiumItem = null, bundle = null;
            for (int i = 0; i < catalog.items.Length; i++)
            {
                var item = catalog.items[i];
                if (item.offer == GardenCosmeticOffer.Free)
                {
                    free++;
                    if (item.target == GardenCosmeticTarget.Worker) freeWorker = item;
                }
                else if (item.offer == GardenCosmeticOffer.Premium) { premium++; premiumItem = item; }
                else { bundles++; bundle = item; }
            }
            Check(free == 3 && premium == 2 && bundles == 1 && freeWorker != null && premiumItem != null && bundle != null,
                "The prototype catalog must contain three free items, two premium previews, and one fixed bundle preview.");

            var testObject = new GameObject("F06 Cosmetic Verification");
            try
            {
                var controller = testObject.AddComponent<GardenCosmeticController>();
                controller.catalog = catalog;
                controller.Restore(null);
                Check(!controller.Equip(premiumItem.id) && !controller.Equip(bundle.id), "A paid cosmetic was granted by Equip.");
                Check(controller.Preview(premiumItem.id) && controller.PreviewItem == premiumItem,
                    "Premium cosmetic preview did not activate.");
                Check(controller.EquippedId(GardenCosmeticTarget.Worker) == string.Empty,
                    "Preview changed the saved loadout.");
                controller.ClearPreview();
                Check(controller.Equip(freeWorker.id) && controller.IsEquipped(freeWorker.id),
                    "Free cosmetic could not be equipped.");
                var saved = new GardenSaveData();
                controller.CaptureInto(saved);
                Check(saved.cosmetics.workerId == freeWorker.id && saved.cosmetics.potId == string.Empty &&
                    saved.cosmetics.cropId == string.Empty && GardenSaveStore.Valid(saved),
                    "Free cosmetic selection did not produce valid persistent state.");
            }
            finally { Object.DestroyImmediate(testObject); }

            Debug.Log("PASS F06: catalog IDs/offer types, free equip, paid equip rejection, temporary preview and save capture.");
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new System.Exception(message);
        }
    }
}
