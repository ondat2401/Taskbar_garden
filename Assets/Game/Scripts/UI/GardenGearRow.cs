using UnityEngine;
using UnityEngine.UI;

namespace IdleGarden
{
    public sealed class GardenGearRow : MonoBehaviour
    {
        public GardenGearPanel owner;
        public Button button;
        public Text label;
        public int index;

        public void Select()
        {
            if (owner) owner.Select(index);
        }

        public void Refresh(GearInstance item, bool selected, string displayName)
        {
            button.interactable = item != null;
            label.text = item == null ? "—" : (selected ? "▶ " : "") + item.rarity + "  ·  " + displayName;
        }
    }
}
