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
        public GardenFloatingWindow gearWindow;
        public GardenGearPanel gearPanel;
        public GardenFloatingWindow cosmeticWindow;
        public GardenCosmeticPanel cosmeticPanel;
        public Text moneyLabel, shopSummary, inventoryLabel, autoSellLabel, rebirthLabel, pinLabel, lockLabel, pauseLabel, shopButtonLabel, gearButtonLabel, noticeLabel;
        public Text gardenTabLabel, upgradesTabLabel;
        public Button rebirthButton;
        public ShopUpgradeRow[] rows;
        public GameObject gardenPage, upgradesPage;
        public Text potLabel, cropLabel, themeLabel;
        public Button plantButton, unlockThemeButton;
        public int SelectedPot { get; private set; }
        private int selectedCrop;
        public bool ShopOpen => shopPanel && shopPanel.activeSelf;
        public bool GearOpen => gearWindow && gearWindow.IsOpen;
        public bool CosmeticsOpen => cosmeticWindow && cosmeticWindow.IsOpen;
        private bool rebirthArmed;
        private string notice;
        private const float GoldPulseDuration = .24f;
        private float goldPulseRemaining;
        private double previousGold;
        private bool hasPreviousGold;
        private Vector3 moneyLabelRestScale = Vector3.one;

        public void CacheEffectRestState()
        {
            if (moneyLabel) moneyLabelRestScale = moneyLabel.rectTransform.localScale;
        }

        public void TickEffects(float deltaTime)
        {
            if (!moneyLabel || goldPulseRemaining <= 0) return;
            goldPulseRemaining = Mathf.Max(0, goldPulseRemaining - deltaTime);
            float progress = 1 - goldPulseRemaining / GoldPulseDuration;
            float scale = 1 + .12f * Mathf.Sin(progress * Mathf.PI);
            moneyLabel.rectTransform.localScale = moneyLabelRestScale * scale;
            if (goldPulseRemaining == 0) moneyLabel.rectTransform.localScale = moneyLabelRestScale;
        }

        public void ToggleShop()
        {
            bool open = !ShopOpen;
            if (gearWindow) gearWindow.Close();
            CloseCosmeticsPreview();
            shopPanel.SetActive(open); menuPanel.SetActive(false);
            desktop.SetPanelExpanded(open); rebirthArmed = false; Refresh();
        }
        public void ToggleGear()
        {
            if (!gearWindow) return;
            if (GearOpen) { CloseGear(); return; }
            CloseCosmeticsPreview();
            shopPanel.SetActive(false); menuPanel.SetActive(false);
            desktop.SetPanelExpanded(true); rebirthArmed = false; gearWindow.Open(); Refresh();
        }
        public void CloseGear()
        {
            if (gearWindow) gearWindow.Close();
            desktop.SetPanelExpanded(false); Refresh();
        }
        public void ToggleCosmetics()
        {
            if (!cosmeticWindow) return;
            if (CosmeticsOpen) { CloseCosmetics(); return; }
            if (gearWindow) gearWindow.Close();
            CloseCosmeticsPreview();
            shopPanel.SetActive(false); menuPanel.SetActive(false);
            desktop.SetPanelExpanded(true); rebirthArmed = false;
            cosmeticWindow.Open();
            if (cosmeticPanel) cosmeticPanel.Refresh();
        }
        public void CloseCosmetics()
        {
            CloseCosmeticsPreview();
            desktop.SetPanelExpanded(ShopOpen || GearOpen);
            Refresh();
        }
        private void CloseCosmeticsPreview()
        {
            if (cosmeticWindow) cosmeticWindow.Close();
            if (garden && garden.cosmetics) garden.cosmetics.ClearPreview();
        }
        public void ShowGarden() { gardenPage.SetActive(true); upgradesPage.SetActive(false); Refresh(); }
        public void ShowUpgrades() { gardenPage.SetActive(false); upgradesPage.SetActive(true); Refresh(); }
        public void OpenPot(int index)
        {
            if(garden.Simulation==null) return;
            SelectedPot=Mathf.Clamp(index,0,garden.Simulation.Plots.Length-1);
            var crops=garden.Progression.Config.crops;
            for(int i=0;i<crops.Length;i++) if(crops[i]==garden.Simulation.Plots[SelectedPot].SelectedCrop) selectedCrop=i;
            if(!ShopOpen) ToggleShop(); ShowGarden();
        }
        public void PreviousPot() { OpenPot((SelectedPot+garden.Simulation.Plots.Length-1)%garden.Simulation.Plots.Length); }
        public void NextPot() { OpenPot((SelectedPot+1)%garden.Simulation.Plots.Length); }
        public void NextCrop() { selectedCrop=(selectedCrop+1)%garden.Progression.Config.crops.Length; Refresh(); }
        public void PlantSelected()
        {
            bool changed=garden.Simulation.SelectCrop(SelectedPot,garden.Progression.Config.crops[selectedCrop]);
            notice=changed?"Selection saved. A growing crop finishes before switching.":"Unlock this theme first.";
            garden.SaveGame(); Refresh();
        }
        public void UnlockSelectedTheme()
        {
            notice=garden.Progression.UnlockTheme(garden.Progression.Config.crops[selectedCrop].theme)?"Theme unlocked. One chest earned.":"Already unlocked or not enough gold.";
            garden.SaveGame(); Refresh();
        }
        public void ToggleMenu()
        {
            bool open = !menuPanel.activeSelf;
            if (open)
            {
                shopPanel.SetActive(false);
                if (gearWindow) gearWindow.Close();
                CloseCosmeticsPreview();
                desktop.SetPanelExpanded(false);
            }
            menuPanel.SetActive(open);
        }
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
            SetTabLabel(gardenTabLabel, gardenPage && gardenPage.activeSelf);
            SetTabLabel(upgradesTabLabel, upgradesPage && upgradesPage.activeSelf);
            var progress = garden.Progression;
            if (progress == null || garden.Simulation == null) return;
            if (!hasPreviousGold) hasPreviousGold = true;
            else if (progress.Gold > previousGold) goldPulseRemaining = GoldPulseDuration;
            previousGold = progress.Gold;
            moneyLabel.text = GardenProgression.Number(progress.Gold) + " gold   ·   " + progress.Seeds + " seeds";
            shopButtonLabel.text = ShopOpen ? "Garden" : "Shop";
            if (gearButtonLabel) gearButtonLabel.text = GearOpen ? "Close" : "Gear";
            if (GearOpen && gearPanel) gearPanel.Refresh();
            pinLabel.text = desktop.Topmost ? "Always on top: ON" : "Always on top: OFF";
            lockLabel.text = desktop.locked ? "Position lock: ON" : "Position lock: OFF";
            pauseLabel.text = garden.Simulation.Paused ? "Resume garden" : "Pause garden";
            if (!ShopOpen) return;
            if(gardenPage)
            {
                SelectedPot=Mathf.Clamp(SelectedPot,0,garden.Simulation.Plots.Length-1);
                selectedCrop=Mathf.Clamp(selectedCrop,0,progress.Config.crops.Length-1);
                var plot=garden.Simulation.Plots[SelectedPot]; var crop=progress.Config.crops[selectedCrop];
                potLabel.text=$"POT {SelectedPot+1}    Current: {plot.Crop.displayName}    Selected: {plot.SelectedCrop.displayName}";
                cropLabel.text=$"Choose: {crop.displayName}  |  {crop.growSeconds:0}s  |  {crop.sellPrice} gold  |  {(crop.theme?crop.theme.displayName:"Meadow")}";
                plantButton.interactable=progress.CropUnlocked(crop);
                bool locked=crop.theme && !progress.ThemeUnlocked(crop.theme);
                unlockThemeButton.interactable=locked && progress.Gold>=crop.theme.unlockGold;
                themeLabel.text=locked?"Unlock "+crop.theme.displayName+" · "+GardenProgression.Number(crop.theme.unlockGold)+" gold (+1 chest)":"Theme unlocked · cycles repeat automatically";
            }
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

        private static void SetTabLabel(Text label, bool selected)
        {
            if (!label) return;
            var color = selected ? GardenUiPalette.Accent : GardenUiPalette.TextMuted;
            var style = selected ? FontStyle.Bold : FontStyle.Normal;
            if (label.color != color) label.color = color;
            if (label.fontStyle != style) label.fontStyle = style;
        }
    }
}
