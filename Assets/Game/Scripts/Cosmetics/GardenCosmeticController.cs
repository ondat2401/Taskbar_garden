using UnityEngine;

namespace IdleGarden
{
    public sealed class GardenCosmeticController : MonoBehaviour
    {
        public GardenController garden;
        public GardenCosmeticCatalog catalog;
        public GardenCosmeticDefinition PreviewItem => preview;

        private GardenCosmeticSave loadout = new GardenCosmeticSave();
        private GardenCosmeticDefinition preview;

        public void Restore(GardenCosmeticSave saved)
        {
            loadout = new GardenCosmeticSave();
            if (saved != null)
            {
                loadout.workerId = ValidEquipped(saved.workerId, GardenCosmeticTarget.Worker) ? saved.workerId : string.Empty;
                loadout.potId = ValidEquipped(saved.potId, GardenCosmeticTarget.Pot) ? saved.potId : string.Empty;
                loadout.cropId = ValidEquipped(saved.cropId, GardenCosmeticTarget.Crop) ? saved.cropId : string.Empty;
            }
            preview = null;
            ApplyAppearance();
        }

        public bool Preview(string id)
        {
            var item = catalog ? catalog.Find(id) : null;
            if (item == null) return false;
            preview = item;
            ApplyAppearance();
            return true;
        }

        public void ClearPreview()
        {
            if (preview == null) return;
            preview = null;
            ApplyAppearance();
        }

        public bool Equip(string id)
        {
            var item = catalog ? catalog.Find(id) : null;
            if (item == null || item.offer != GardenCosmeticOffer.Free || item.target == GardenCosmeticTarget.Set) return false;
            switch (item.target)
            {
                case GardenCosmeticTarget.Worker: loadout.workerId = item.id; break;
                case GardenCosmeticTarget.Pot: loadout.potId = item.id; break;
                case GardenCosmeticTarget.Crop: loadout.cropId = item.id; break;
                default: return false;
            }
            preview = null;
            ApplyAppearance();
            if (garden) garden.SaveGame();
            return true;
        }

        public bool IsEquipped(string id) =>
            !string.IsNullOrEmpty(id) && (loadout.workerId == id || loadout.potId == id || loadout.cropId == id);

        public string EquippedId(GardenCosmeticTarget target)
        {
            switch (target)
            {
                case GardenCosmeticTarget.Worker: return loadout.workerId;
                case GardenCosmeticTarget.Pot: return loadout.potId;
                case GardenCosmeticTarget.Crop: return loadout.cropId;
                default: return string.Empty;
            }
        }

        public void CaptureInto(GardenSaveData saved)
        {
            saved.cosmetics = new GardenCosmeticSave
            {
                workerId = loadout.workerId,
                potId = loadout.potId,
                cropId = loadout.cropId
            };
        }

        private bool ValidEquipped(string id, GardenCosmeticTarget target)
        {
            var item = catalog ? catalog.Find(id) : null;
            return item != null && item.offer == GardenCosmeticOffer.Free && item.target == target;
        }

        private void ApplyAppearance()
        {
            if (!garden) return;
            var worker = catalog ? catalog.Find(loadout.workerId) : null;
            var pot = catalog ? catalog.Find(loadout.potId) : null;
            var crop = catalog ? catalog.Find(loadout.cropId) : null;
            Color workerTint = worker != null ? worker.workerTint : Color.white;
            Color potTint = pot != null ? pot.potTint : Color.white;
            Color cropTint = crop != null ? crop.cropTint : Color.white;

            if (preview != null)
            {
                switch (preview.target)
                {
                    case GardenCosmeticTarget.Worker: workerTint = preview.workerTint; break;
                    case GardenCosmeticTarget.Pot: potTint = preview.potTint; break;
                    case GardenCosmeticTarget.Crop: cropTint = preview.cropTint; break;
                    case GardenCosmeticTarget.Set:
                        workerTint = preview.workerTint; potTint = preview.potTint; cropTint = preview.cropTint;
                        break;
                }
            }

            if (garden.workerRenderer) garden.workerRenderer.color = workerTint;
            if (garden.plots == null) return;
            for (int i = 0; i < garden.plots.Length; i++)
                if (garden.plots[i]) garden.plots[i].SetCosmeticTints(potTint, cropTint);
        }
    }
}
