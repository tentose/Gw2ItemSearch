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
        FlowPanel m_layout;
        bool m_initialized = false;

        Dictionary<InventoryItemSource, Panel> m_accountSourcePanels = new Dictionary<InventoryItemSource, Panel>();
        Dictionary<string, Panel> m_characterSourcePanels = new Dictionary<string, Panel>();

        public ItemSearchResultPanel()
        {
            CanScroll = true;

            m_layout = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 5),
                HeightSizingMode = SizingMode.AutoSize,
            };

            m_initialized = true;
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
            // Clear current children and suspend layout
            ForAllSourcePanels(panel =>
            {
                panel.SuspendLayout();
                panel.ClearChildren();
            });

            // Set results
            foreach (var item in items)
            {
                var panel = GetPanelForItem(item);
                var itemIcon = new ItemIcon(item)
                {
                    Parent = panel,
                };
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
                panel.ResumeLayout(panel.Visible);
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

            RecalculateLayout();
        }

        public Panel GetPanelForItem(InventoryItem item)
        {
            if (item.Source == InventoryItemSource.CharacterInventory || item.Source == InventoryItemSource.CharacterEquipment)
            {
                Panel panel;
                if (!m_characterSourcePanels.TryGetValue(item.LocationHint, out panel))
                {
                    panel = NewResultPanel();
                    panel.Title = Strings.ResultTitle_Character + item.LocationHint;
                    m_characterSourcePanels.Add(item.LocationHint, panel);
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
