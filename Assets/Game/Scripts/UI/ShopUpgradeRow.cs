using UnityEngine;
using UnityEngine.UI;

namespace IdleGarden
{
    public sealed class ShopUpgradeRow : MonoBehaviour
    {
        public GardenHud owner;
        public int upgradeIndex;
        public Text title, description, price;
        public Image icon;
        public Button buyButton;
        public void Buy() { owner.Buy(upgradeIndex); }
        public void Refresh(GardenProgression progress)
        {
            if (upgradeIndex < 0 || upgradeIndex >= progress.Config.upgrades.Length) return;
            var upgrade = progress.Config.upgrades[upgradeIndex];
            int level = progress.Level(upgradeIndex);
            title.text = upgrade.displayName + "  ·  Lv " + level;
            description.text = upgrade.description;
            price.text = GardenProgression.Number(progress.GoldCost(upgradeIndex)) + " gold" +
                (upgrade.material ? " + " + GardenProgression.Number(progress.MaterialCost(upgradeIndex)) + " " + upgrade.material.displayName : "");
            icon.sprite = upgrade.icon; icon.enabled = upgrade.icon;
            buyButton.interactable = progress.CanBuy(upgradeIndex);
        }
    }
}
