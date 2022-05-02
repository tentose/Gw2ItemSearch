using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch.Controls
{
    internal class ItemSearchResultPanel : Panel
    {
        private FlowPanel m_layout;
        private bool m_initialized = false;

        private Dictionary<InventoryItemSource, Panel> m_accountSourcePanels = new Dictionary<InventoryItemSource, Panel>();
        private Dictionary<string, Panel> m_characterSourcePanels = new Dictionary<string, Panel>();
        private SearchFilter m_filter;
        private List<ItemIcon> m_itemIcons = new List<ItemIcon>();

        public ItemSearchResultPanel(SearchFilter filter)
        {
            CanScroll = true;

            m_layout = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 5),
                HeightSizingMode = SizingMode.AutoSize,
            };

            m_filter = filter;
            m_filter.FilterChanged += M_filter_FilterChanged;

            m_initialized = true;
        }

        private void M_filter_FilterChanged(object sender, EventArgs e)
        {
            DisplayItemIcons();
            Invalidate();
        }

        private string ItemSourceToString(InventoryItemSource source)
        {
            switch (source)
            {
                case InventoryItemSource.Bank: return Strings.ResultTitle_Bank;
                case InventoryItemSource.SharedInventory: return Strings.ResultTitle_SharedInventory;
                case InventoryItemSource.MaterialStorage: return Strings.ResultTitle_Materials;
                case InventoryItemSource.TradingPostDeliveryBox: return Strings.ResultTitle_TPDeliveryBox;
                case InventoryItemSource.TradingPostSellOrder: return Strings.ResultTitle_TPSellOrder;
                default: return Strings.ResultTitle_Other;
            }
        }

        public void SetSearchResult(List<InventoryItem> items)
        {
            // Build the list of ItemIcons
            m_itemIcons.ForEach(item => item.Dispose());
            m_itemIcons.Clear();
            m_itemIcons.AddRange(items.Select(item => new ItemIcon(item)));

            DisplayItemIcons();

            Invalidate();
        }

        private void DisplayItemIcons()
        {
            // Clear current children and suspend layout
            ForAllSourcePanels(panel =>
            {
                panel.SuspendLayout();
                panel.ClearChildren();
            });

            // Set results
            foreach (var itemIcon in m_itemIcons)
            {
                if (m_filter.FilterItem(itemIcon.ItemInfo))
                {
                    var panel = GetPanelForItem(itemIcon.Item);
                    itemIcon.Parent = panel;
                }
            }

            // Turn off panels without results and resume layout
            ForAllSourcePanels(panel =>
            {
                if (panel.Children.Count > 0 && panel.Parent == null)
                {
                    panel.Parent = m_layout;
                }
                else if (panel.Children.Count == 0 && panel.Parent != null)
                {
                    panel.Parent = null;
                }
                panel.ResumeLayout();
            });

            m_layout.SortChildren((Panel a, Panel b) =>
            {
                bool isACharacter = m_characterSourcePanels.Values.Contains(a);
                bool isBCharacter = m_characterSourcePanels.Values.Contains(b);
                if (isACharacter && !isBCharacter)
                {
                    return 1;
                }
                else if (!isACharacter && isBCharacter)
                {
                    return -1;
                }
                else
                {
                    return a.Title.CompareTo(b.Title);
                }
            });
        }

        public Panel GetPanelForItem(InventoryItem item)
        {
            if (item.Source == InventoryItemSource.CharacterInventory || item.Source == InventoryItemSource.CharacterEquipment)
            {
                Panel panel;
                if (!m_characterSourcePanels.TryGetValue(item.CharacterName, out panel))
                {
                    panel = NewResultPanel();
                    panel.Title = item.CharacterName;
                    m_characterSourcePanels.Add(item.CharacterName, panel);
                }
                return panel;
            }
            else
            {
                Panel panel;
                if (!m_accountSourcePanels.TryGetValue(item.Source, out panel))
                {
                    panel = NewResultPanel();
                    panel.Title = ItemSourceToString(item.Source);
                    m_accountSourcePanels.Add(item.Source, panel);
                }
                return panel;
            }
        }

        public void ForAllSourcePanels(Action<Panel> action)
        {
            foreach (var panel in m_accountSourcePanels.Values)
            {
                action(panel);
            }
            foreach (var panel in m_characterSourcePanels.Values)
            {
                action(panel);
            }
        }

        private Panel NewResultPanel()
        {
            return new FlowPanel()
            {
                ShowBorder = true,
                ControlPadding = new Vector2(5, 5),
                Size = new Point(400, 100),
                Parent = m_layout,
                CanCollapse = true,
                FlowDirection = ControlFlowDirection.LeftToRight,
                HeightSizingMode = SizingMode.AutoSize,
            };
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (m_initialized)
            {
                if (m_layout.Parent != null)
                {
                    LayoutHelper.SetWidthSizeMode(m_layout, DimensionSizeMode.Inherit);
                }

                ForAllSourcePanels(panel =>
                {
                    if (panel.Parent != null)
                    {
                        LayoutHelper.SetWidthSizeMode(panel, DimensionSizeMode.Inherit, 20);
                    }
                });
            }
        }
    }
}
