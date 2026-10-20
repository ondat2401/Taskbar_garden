using UnityEngine;
using UnityEngine.UI;

namespace IdleGarden
{
    public sealed class GardenCosmeticRow : MonoBehaviour
    {
        public GardenCosmeticPanel owner;
        public Button button;
        public Image swatch;
        public Text label;
        public int index;

        public void Select() => owner.Select(index);

        public void Refresh(GardenCosmeticDefinition item, bool selected, bool equipped)
        {
            button.interactable = item != null;
            GardenUiPalette.StyleButton(button, selected && item != null);
            if (item == null)
            {
                label.text = string.Empty;
                swatch.color = Color.clear;
                return;
            }
            label.text = item.displayName + "  ·  " + OfferLabel(item.offer) + (equipped ? "  ·  EQUIPPED" : string.Empty);
            swatch.color = item.target == GardenCosmeticTarget.Pot ? item.potTint :
                item.target == GardenCosmeticTarget.Crop ? item.cropTint : item.workerTint;
        }

        private static string OfferLabel(GardenCosmeticOffer offer) => offer switch
        {
            GardenCosmeticOffer.Free => "FREE",
            GardenCosmeticOffer.Premium => "PREMIUM",
            GardenCosmeticOffer.FixedBundle => "BUNDLE",
            _ => ""
        };
    }
}
