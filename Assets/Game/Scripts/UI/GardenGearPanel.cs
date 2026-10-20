using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace IdleGarden
{
    public sealed class GardenGearPanel : MonoBehaviour
    {
        public GardenController garden;
        public GardenGearRow[] rows;
        public Text inventoryLabel, timerLabel, pityLabel, ratesLabel, essenceLabel, selectedTitle, selectedStats, equippedLabel, noticeLabel;
        public Button openChestButton, buyChestButton, equipButton, salvageButton;
        public Text equipButtonLabel;
        private int selectedIndex = -1;
        private string notice;

        public void Select(int index)
        {
            if (garden && index >= 0 && index < garden.Progression.GearInventory.Count)
            {
                selectedIndex = index;
                Refresh();
            }
        }

        public void OpenChest()
        {
            var progress = garden.Progression;
            if (!progress.TryOpenChest(out var drop))
                notice = progress.Chests <= 0 ? "No chest to open yet." : "Inventory full. Your chest is safe.";
            else
            {
                var definition = progress.Config.gear.FindEquipment(drop.equipmentId);
                notice = "Found " + (definition ? definition.displayName : drop.equipmentId) + " · " + drop.rarity + ".";
                selectedIndex = progress.GearInventory.Count - 1;
            }
            garden.SaveGame();
            Refresh();
        }

        public void BuyChest()
        {
            var progress = garden.Progression;
            notice = progress.TryBuyChestWithEssence()
                ? "Exchanged " + progress.Config.gear.essencePerChest + " essence for a chest."
                : "Need more essence or chest capacity is full.";
            garden.SaveGame();
            Refresh();
        }

        public void EquipSelected()
        {
            var progress = garden.Progression;
            var item = Selected(progress);
            if (item == null) return;
            var definition = progress.Config.gear.FindEquipment(item.equipmentId);
            if (definition && progress.IsEquipped(item))
                notice = progress.TryUnequip(definition.slot) ? "Unequipped " + definition.displayName + "." : "Could not unequip item.";
            else
                notice = progress.TryEquip(item.id) ? "Equipped " + (definition ? definition.displayName : item.equipmentId) + "." : "Item definition is unavailable.";
            garden.SaveGame();
            Refresh();
        }

        public void SalvageSelected()
        {
            var progress = garden.Progression;
            var item = Selected(progress);
            if (item == null) return;
            var definition = progress.Config.gear.FindEquipment(item.equipmentId);
            string name = definition ? definition.displayName : item.equipmentId;
            notice = progress.TrySalvage(item.id) ? "Salvaged " + name + " for essence." : "Equipped gear cannot be salvaged.";
            if (progress.GearInventory.Count == 0) selectedIndex = -1;
            else selectedIndex = Mathf.Clamp(selectedIndex, 0, progress.GearInventory.Count - 1);
            garden.SaveGame();
            Refresh();
        }

        public void Refresh()
        {
            if (!garden || garden.Progression == null || !garden.Progression.Config.gear) return;
            var progress = garden.Progression;
            var gear = progress.Config.gear;
            int count = progress.GearInventory.Count;
            if (selectedIndex < 0 || selectedIndex >= count) selectedIndex = count == 0 ? -1 : 0;
            inventoryLabel.text = "Inventory  " + count + " / " + gear.inventoryCapacity + "   ·   Chests  " + progress.Chests;
            int seconds = Mathf.CeilToInt((float)progress.ChestSecondsRemaining);
            timerLabel.text = "Next free chest  " + (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00") + "   ·   Earned in real, unpaused time.";
            pityLabel.text = progress.ChestPityMisses >= gear.pityThreshold - 1
                ? "Pity: next chest guarantees Epic+ (80% Epic / 20% Legendary)."
                : "Epic+ pity: " + progress.ChestPityMisses + " / " + (gear.pityThreshold - 1) + " consecutive misses.";
            essenceLabel.text = "Essence  " + progress.Essence + "   ·   " + gear.essencePerChest + " buys 1 chest";
            openChestButton.interactable = progress.Chests > 0 && count < gear.inventoryCapacity;
            buyChestButton.interactable = progress.Essence >= gear.essencePerChest && progress.Chests < int.MaxValue;
            ratesLabel.text = DropRates(gear);
            for (int i = 0; i < rows.Length; i++)
            {
                var item = i < count ? progress.GearInventory[i] : null;
                var definition = item != null ? gear.FindEquipment(item.equipmentId) : null;
                rows[i].Refresh(item, i == selectedIndex, definition ? definition.displayName : item != null ? item.equipmentId : null);
            }
            var selected = Selected(progress);
            if (selected == null)
            {
                selectedTitle.text = "Select an item";
                selectedStats.text = "Open a chest to collect tools and gear.";
                equipButton.interactable = false;
                equipButtonLabel.text = "Equip";
                salvageButton.interactable = false;
            }
            else
            {
                var definition = gear.FindEquipment(selected.equipmentId);
                selectedTitle.text = (definition ? definition.displayName : selected.equipmentId) + "  ·  " + selected.rarity;
                selectedStats.text = ItemStats(definition, selected);
                bool equipped = progress.IsEquipped(selected);
                equipButton.interactable = definition;
                equipButtonLabel.text = equipped ? "Unequip" : "Equip";
                salvageButton.interactable = !equipped;
            }
            equippedLabel.text = "Equipped   Can: " + EquippedName(progress, GearSlot.WateringCan) +
                                 "   Boots: " + EquippedName(progress, GearSlot.Boots) +
                                 "   Gloves: " + EquippedName(progress, GearSlot.Gloves);
            noticeLabel.text = notice ?? "Choose a tool to equip it; equipped gear stays protected from salvage.";
        }

        private GearInstance Selected(GardenProgression progress) =>
            progress.GearInventory.Count > 0 && progress.GearInventory.Count > selectedIndex && selectedIndex >= 0
                ? progress.GearInventory[selectedIndex] : null;

        private static string EquippedName(GardenProgression progress, GearSlot slot)
        {
            var item = progress.Equipped(slot);
            var definition = item != null ? progress.Config.gear.FindEquipment(item.equipmentId) : null;
            return definition ? definition.displayName : "—";
        }

        private static string DropRates(GearConfig gear)
        {
            double total = 0;
            for (int i = 0; i < gear.rarities.Length; i++) total += gear.rarities[i].weight;
            var text = new StringBuilder("Base drops: ");
            for (int i = 0; i < gear.rarities.Length; i++)
            {
                var rule = gear.rarities[i];
                if (i > 0) text.Append("  ");
                text.Append(rule.rarity).Append(' ').Append((rule.weight * 100 / total).ToString("0.#")).Append('%');
            }
            return text.ToString();
        }

        private static string ItemStats(EquipmentDefinition definition, GearInstance item)
        {
            var text = new StringBuilder();
            if (definition)
                for (int i = 0; i < definition.baseStats.Length; i++)
                {
                    var stat = definition.baseStats[i];
                    if (text.Length > 0) text.Append('\n');
                    text.Append("Base  ").Append(stat.stat).Append(" +");
                    if (stat.flatBonus > 0) text.Append(stat.flatBonus.ToString("0.#"));
                    if (stat.percentBonus > 0) text.Append((stat.percentBonus * 100).ToString("0.#")).Append('%');
                }
            for (int i = 0; i < item.affixes.Length; i++)
            {
                if (text.Length > 0) text.Append('\n');
                text.Append(item.affixes[i].stat).Append(" +").Append((item.affixes[i].percentBonus * 100).ToString("0.#")).Append('%');
            }
            return text.Length == 0 ? "No bonuses." : text.ToString();
        }
    }
}
