using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace IdleGarden
{
    public sealed class GardenHud : MonoBehaviour
    {
        public GardenController garden;
        public DesktopWindow desktop;
        public GameObject shopPanel, menuPanel;
        public Text moneyLabel, shopSummary, inventoryLabel, autoSellLabel, rebirthLabel, pinLabel, lockLabel, pauseLabel, shopButtonLabel, noticeLabel;
        public Button rebirthButton;
        public ShopUpgradeRow[] rows;
        public bool ShopOpen => shopPanel && shopPanel.activeSelf;
        private bool rebirthArmed;
        private string notice;

        public void ToggleShop()
        {
            bool open = !ShopOpen;
            shopPanel.SetActive(open); menuPanel.SetActive(false);
            desktop.SetPanelExpanded(open); rebirthArmed = false; Refresh();
        }
        public void ToggleMenu() { menuPanel.SetActive(!menuPanel.activeSelf); }
        public void ToggleAutoSell() { garden.Progression.AutoSell = !garden.Progression.AutoSell; garden.SaveGame(); Refresh(); }
        public void SellSurplus() { notice = "Sold surplus: " + GardenProgression.Number(garden.Progression.SellSurplus()) + " gold"; garden.SaveGame(); Refresh(); }
        public void Buy(int index)
        {
            notice = garden.Progression.TryBuy(index) ? "Upgrade purchased." : "Need more gold or crops.";
            rebirthArmed = false; garden.SaveGame(); Refresh();
        }
        public void Rebirth()
        {
            if (!garden.Progression.CanRebirth) return;
            if (!rebirthArmed)
            {
                rebirthArmed = true;
                notice = "Rebirth resets gold, stock, upgrades and plants. Keep permanent seeds. Click again to confirm.";
            }
            else if (garden.Rebirth()) { rebirthArmed = false; notice = "A new garden begins. Permanent gold bonus increased."; }
            Refresh();
        }
        public void TogglePin() { desktop.ToggleTopmost(); Refresh(); }
        public void ToggleLock() { desktop.ToggleLock(); Refresh(); }
        public void TogglePause() { garden.TogglePause(); Refresh(); }

        // Called by the controller's throttled UI timer, never from the frame loop.
        public void Refresh()
        {
            var progress = garden.Progression;
            if (progress == null || garden.Simulation == null) return;
            moneyLabel.text = GardenProgression.Number(progress.Gold) + " gold   ·   " + progress.Seeds + " seeds";
            shopButtonLabel.text = ShopOpen ? "Garden" : "Shop";
            pinLabel.text = desktop.Topmost ? "Always on top: ON" : "Always on top: OFF";
            lockLabel.text = desktop.locked ? "Position lock: ON" : "Position lock: OFF";
            pauseLabel.text = garden.Simulation.Paused ? "Resume garden" : "Pause garden";
            if (!ShopOpen) return;
            shopSummary.text = "GARDEN WORKSHOP    /    " + GardenProgression.Number(progress.Gold) + " gold    /    Gold ×" + progress.GoldMultiplier.ToString("0.##");
            var text = new StringBuilder("Stock  ");
            foreach (var crop in progress.Config.crops) text.Append(crop.displayName).Append(": ").Append(GardenProgression.Number(progress.Count(crop))).Append("   ");
            inventoryLabel.text = text.ToString();
            autoSellLabel.text = progress.AutoSell ? "Auto-sell: ON" : "Auto-sell: OFF";
            for (int i = 0; i < rows.Length; i++) rows[i].Refresh(progress);
            rebirthButton.interactable = progress.CanRebirth;
            rebirthLabel.text = rebirthArmed ? "Confirm rebirth (+1 seed)" : "Rebirth +1 seed  ·  " + GardenProgression.Number(progress.CycleGold) + " / " + GardenProgression.Number(progress.RebirthGoal);
            noticeLabel.text = notice ?? ("Auto-sell keeps " + progress.Config.autoSellReserve + " of each crop for upgrades. Turn it off to stockpile more.");
        }
    }
}
