using UnityEngine;
using UnityEngine.UI;

namespace IdleGarden
{
    public sealed class GardenCosmeticPanel : MonoBehaviour
    {
        public GardenController garden;
        public GardenCosmeticRow[] rows;
        public Text equippedLabel, selectedTitle, description, noticeLabel, previewButtonLabel, equipButtonLabel;
        public Button previewButton, equipButton;

        private int selectedIndex;
        private string notice;

        public void Select(int index)
        {
            var catalog = garden && garden.cosmetics ? garden.cosmetics.catalog : null;
            if (!catalog || catalog.items == null || index < 0 || index >= catalog.items.Length) return;
            selectedIndex = index;
            notice = null;
            Refresh();
        }

        public void PreviewSelected()
        {
            var item = Selected();
            if (item == null || !garden.cosmetics) return;
            if (garden.cosmetics.PreviewItem == item)
            {
                garden.cosmetics.ClearPreview();
                notice = "Preview ended; equipped colors are restored.";
            }
            else if (garden.cosmetics.Preview(item.id))
                notice = "Preview only. This does not grant ownership or save a paid item.";
            Refresh();
        }

        public void EquipSelected()
        {
            var item = Selected();
            if (item == null || !garden.cosmetics) return;
            notice = garden.cosmetics.Equip(item.id)
                ? "Free cosmetic equipped and saved."
                : "Only free single-slot cosmetics can be equipped in this prototype.";
            Refresh();
        }

        public void Refresh()
        {
            var controller = garden ? garden.cosmetics : null;
            var catalog = controller ? controller.catalog : null;
            if (!catalog || catalog.items == null || catalog.items.Length == 0)
            {
                equippedLabel.text = "Cosmetic catalog unavailable.";
                selectedTitle.text = "No cosmetics";
                description.text = string.Empty;
                previewButton.interactable = equipButton.interactable = false;
                noticeLabel.text = notice ?? "Open the Editor setup for F06.";
                return;
            }

            selectedIndex = Mathf.Clamp(selectedIndex, 0, catalog.items.Length - 1);
            for (int i = 0; i < rows.Length; i++)
            {
                var item = i < catalog.items.Length ? catalog.items[i] : null;
                rows[i].Refresh(item, i == selectedIndex, item != null && controller.IsEquipped(item.id));
            }

            var selected = catalog.items[selectedIndex];
            string worker = NameFor(controller, GardenCosmeticTarget.Worker);
            string pot = NameFor(controller, GardenCosmeticTarget.Pot);
            string crop = NameFor(controller, GardenCosmeticTarget.Crop);
            equippedLabel.text = "Equipped  ·  Worker: " + worker + "  /  Pot: " + pot + "  /  Crop: " + crop;
            selectedTitle.text = selected.displayName + "  ·  " + TargetLabel(selected.target) + "  ·  " + OfferLabel(selected.offer);
            description.text = selected.description;
            bool previewing = controller.PreviewItem == selected;
            previewButton.interactable = true;
            previewButtonLabel.text = previewing ? "End preview" : "Preview on garden";
            bool free = selected.offer == GardenCosmeticOffer.Free && selected.target != GardenCosmeticTarget.Set;
            bool equipped = controller.IsEquipped(selected.id);
            equipButton.interactable = free && !equipped;
            equipButtonLabel.text = equipped ? "Equipped" : "Equip free";
            noticeLabel.text = notice ?? "Premium and fixed bundles can only be previewed; checkout is not part of this prototype.";
        }

        private GardenCosmeticDefinition Selected()
        {
            var catalog = garden && garden.cosmetics ? garden.cosmetics.catalog : null;
            return catalog && catalog.items != null && selectedIndex >= 0 && selectedIndex < catalog.items.Length
                ? catalog.items[selectedIndex] : null;
        }

        private static string NameFor(GardenCosmeticController controller, GardenCosmeticTarget target)
        {
            string id = controller.EquippedId(target);
            var item = controller.catalog ? controller.catalog.Find(id) : null;
            return item != null ? item.displayName : "Base";
        }

        private static string TargetLabel(GardenCosmeticTarget target) => target switch
        {
            GardenCosmeticTarget.Worker => "Worker",
            GardenCosmeticTarget.Pot => "Pot",
            GardenCosmeticTarget.Crop => "Crops",
            GardenCosmeticTarget.Set => "Full set",
            _ => "Cosmetic"
        };

        private static string OfferLabel(GardenCosmeticOffer offer) => offer switch
        {
            GardenCosmeticOffer.Free => "Free",
            GardenCosmeticOffer.Premium => "Premium preview",
            GardenCosmeticOffer.FixedBundle => "Fixed bundle preview",
            _ => ""
        };
    }
}
