using Blish_HUD;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch.Controls
{
    public class ItemTooltipView : View, ITooltipView
    {
        private static readonly Logger Logger = Logger.GetLogger<ItemSearchModule>();

        private const int WIDTH = 300;
        private const int UPGRADE_X_MARGIN = 30;
        private const int LABEL_Y_GAP = 5;

        private Label m_itemNameLabel;
        private Label m_itemParentLabel;
        private Label m_itemSourceLabel;
        private List<Label> m_itemUpgradeLabels = new List<Label>();

        private InventoryItem m_item;
        public InventoryItem Item
        {
            get => m_item;
            set => UpdateLabelValueAndWidth(value);
        }

        public ItemTooltipView(InventoryItem item)
        {
            m_itemNameLabel = new Label()
            {
                ShowShadow = true,
                AutoSizeHeight = true,
                AutoSizeWidth = false,
                WrapText = true,
                Width = WIDTH,
                Font = GameService.Content.DefaultFont16,
            };

            m_itemParentLabel = new Label()
            {
                ShowShadow = true,
                AutoSizeHeight = true,
                AutoSizeWidth = false,
                WrapText = true,
                Width = WIDTH,
                Font = GameService.Content.DefaultFont14,
            };

            m_itemSourceLabel = new Label()
            {
                ShowShadow = true,
                AutoSizeHeight = true,
                AutoSizeWidth = false,
                WrapText = true,
                Width = WIDTH,
                Font = GameService.Content.DefaultFont14,
            };

            this.Item = item;
        }

        protected override void Build(Container buildPanel)
        {
            m_itemNameLabel.Parent = buildPanel;
            m_itemParentLabel.Parent = buildPanel;
            m_itemSourceLabel.Parent = buildPanel;
            foreach (var label in m_itemUpgradeLabels)
            {
                label.Parent = buildPanel;
            }
        }

        private void UpdateLabelValueAndWidth(InventoryItem item)
        {
            m_item = item;

            if (m_item != null)
            {
                StaticItemInfo itemInfo;
                if (StaticItemInfo.AllItems.TryGetValue(item.Id, out itemInfo))
                {
                    m_itemNameLabel.Text = itemInfo.Name;
                    m_itemNameLabel.TextColor = RarityToColor(itemInfo.Rarity);
                }
                else
                {
                    Logger.Warn($"Cannot find info for {item.Id}");
                    m_itemNameLabel.Text = Strings.ItemTooltip_FallbackTextPrefix + item.Id;
                }

                // Upgrades
                int nextY = m_itemNameLabel.Bottom + LABEL_Y_GAP;
                m_itemUpgradeLabels.Clear();
                if (item.Upgrades != null)
                {
                    foreach (var upgrade in item.Upgrades)
                    {
                        var label = AddNewUpgradeLabel(upgrade);
                        label.Location = new Point(UPGRADE_X_MARGIN, nextY);
                        nextY = label.Bottom + LABEL_Y_GAP;
                    }
                }
                if (item.Infusions != null)
                {
                    foreach (var upgrade in item.Infusions)
                    {
                        var label = AddNewUpgradeLabel(upgrade);
                        label.Location = new Point(UPGRADE_X_MARGIN, nextY);
                        nextY = label.Bottom + LABEL_Y_GAP;
                    }
                }

                // Parent
                if (m_item.ParentItem != null)
                {
                    nextY += LABEL_Y_GAP;
                    StaticItemInfo parentItemInfo;
                    if (StaticItemInfo.AllItems.TryGetValue(m_item.ParentItem.Id, out parentItemInfo))
                    {
                        m_itemParentLabel.Text = Strings.ItemTooltip_ContainedIn + parentItemInfo.Name;
                        m_itemParentLabel.TextColor = RarityToColor(parentItemInfo.Rarity);
                    }
                    else
                    {
                        Logger.Warn($"Cannot find info for {item.Id}");
                        m_itemParentLabel.Text = Strings.ItemTooltip_ContainedIn + Strings.ItemTooltip_FallbackTextPrefix + m_item.ParentItem.Id;
                    }
                        
                    m_itemParentLabel.Location = new Point(0, nextY);
                    nextY = m_itemParentLabel.Bottom + LABEL_Y_GAP;
                }

                // Source
                nextY += LABEL_Y_GAP;
                if (m_item.Source == InventoryItemSource.CharacterInventory)
                {
                    m_itemSourceLabel.Text = Strings.ItemTooltip_Location + Strings.ItemTooltip_CharacterInventory;
                }
                else if (m_item.Source == InventoryItemSource.CharacterEquipment)
                {
                    if (m_item.EquipmentTabId > 0)
                    {
                        m_itemSourceLabel.Text = Strings.ItemTooltip_Location + Strings.ItemTooltip_CharacterEquipment + String.Format(Strings.ItemTooltip_Tab, m_item.EquipmentTabId);
                    }
                    else
                    {
                        m_itemSourceLabel.Text = Strings.ItemTooltip_Location + Strings.ItemTooltip_CharacterEquipment;
                    }
                }
                else
                {
                    m_itemSourceLabel.Visible = false;
                }

                if (m_itemSourceLabel.Visible)
                {
                    m_itemSourceLabel.Location = new Point(0, nextY);
                }
            }
        }

        private Label AddNewUpgradeLabel(int upgrade)
        {
            Label label = null;
            StaticItemInfo itemInfo;
            if (StaticItemInfo.AllItems.TryGetValue(upgrade, out itemInfo))
            {
                label = new Label()
                {
                    ShowShadow = true,
                    AutoSizeHeight = true,
                    AutoSizeWidth = false,
                    Font = GameService.Content.DefaultFont14,
                    Text = itemInfo.Name,
                    Width = WIDTH,
                    WrapText = true,
                    TextColor = RarityToColor(itemInfo.Rarity),
                };
                m_itemUpgradeLabels.Add(label);
            }
                
            return label;
        }

        private Color RarityToColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Junk: return Color.LightGray;
                case ItemRarity.Basic: return Color.White;
                case ItemRarity.Fine: return Color.DeepSkyBlue;
                case ItemRarity.Masterwork: return Color.ForestGreen;
                case ItemRarity.Rare: return Color.Yellow;
                case ItemRarity.Exotic: return Color.Orange;
                case ItemRarity.Ascended: return Color.HotPink;
                case ItemRarity.Legendary: return Color.BlueViolet;
                default: return Color.White;
            }
        }
    }
}
