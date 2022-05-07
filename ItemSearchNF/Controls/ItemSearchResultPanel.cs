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
        private Panel m_ungroupedPanel;
        private SearchOptions m_searchOptions;
        private SavedSearch m_savedSearch;
        private List<ItemIcon> m_itemIcons = new List<ItemIcon>();

        public ItemSearchResultPanel(SearchOptions options, SavedSearch savedSearch)
        {
            CanScroll = true;

            m_layout = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 5),
                HeightSizingMode = SizingMode.AutoSize,
            };

            m_searchOptions = options;
            m_searchOptions.FilterChanged += M_filter_FilterChanged;
            m_searchOptions.OptionsChanged += M_filter_OptionsChanged;

            m_savedSearch = savedSearch;

            m_initialized = true;
        }

        private void M_filter_OptionsChanged(object sender, EventArgs e)
        {
            DisplayItemIcons();
            Invalidate();
        }

        private void M_filter_FilterChanged(object sender, EventArgs e)
        {
            DisplayItemIcons();
            Invalidate();
        }

        public void SetSearchResult(List<InventoryItem> items)
        {
            // Build the list of ItemIcons
            m_itemIcons.ForEach(item => item.Dispose());
            m_itemIcons.Clear();

            if (items != null)
            {
                m_itemIcons.AddRange(items.Select(item => new ItemIcon(item)
                {
                    ShowSetSearchIconContextMenu = m_savedSearch != null,
                }));
            }
            
            foreach (var icon in m_itemIcons)
            {
                icon.SetAsSearchIcon += Icon_SetAsSearchIcon;
            }

            DisplayItemIcons();

            Invalidate();
        }

        private void Icon_SetAsSearchIcon(object sender, EventArgs e)
        {
            ItemIcon icon = sender as ItemIcon;
            if (icon != null && m_savedSearch != null)
            {
                m_savedSearch.TabIconUrl = icon.ItemInfo.IconUrl;
                m_savedSearch.UpdateSearch();
            }
        }

        private void DisplayItemIcons()
        {
            switch (m_searchOptions.StackGrouping)
            {
                case StackGrouping.ByLocationMerged:
                case StackGrouping.ByLocation:
                    if (m_ungroupedPanel != null)
                    {
                        m_ungroupedPanel.Parent = null;
                    }
                    DisplayItemIconsByGroup();
                    break;

                case StackGrouping.Merged:
                    ForAllSourcePanels(panel => panel.Parent = null);
                    DisplayItemIconsUngrouped();
                    break;
            }
        }


        private void DisplayItemIconsByGroup()
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
                if (m_searchOptions.FilterItem(itemIcon.ItemInfo))
                {
                    var panel = GetPanelForItem(itemIcon.Item);
                    itemIcon.Parent = panel;
                }
            }

            if (m_searchOptions.StackGrouping != StackGrouping.ByLocation)
            {
                // For merged display, convert the ItemIcon to ItemIconMerged
                ForAllSourcePanels(panel =>
                {
                    var groups = panel.Children.Select(child => child as ItemIcon).GroupBy(icon => icon.Item.Id).ToList();
                    panel.Children.Clear();
                    foreach (var group in groups)
                    {
                        var iconMerged = new ItemIconMerged(group)
                        {
                            ShowSetSearchIconContextMenu = m_savedSearch != null,
                            Parent = panel,
                        };
                        iconMerged.SetAsSearchIcon += Icon_SetAsSearchIcon;
                    }
                });
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

        private void DisplayItemIconsUngrouped()
        {
            // Clear current children and suspend layout
            if (m_ungroupedPanel == null)
            {
                m_ungroupedPanel = NewResultPanel();
                m_ungroupedPanel.CanCollapse = false;
                m_ungroupedPanel.ShowBorder = false;
            }
            m_ungroupedPanel.SuspendLayout();
            m_ungroupedPanel.ClearChildren();

            // Set results
            if (m_searchOptions.StackGrouping == StackGrouping.Merged)
            {
                var groups = m_itemIcons.Where(itemIcon => m_searchOptions.FilterItem(itemIcon.ItemInfo)).GroupBy(itemIcon => itemIcon.Item.Id);
                foreach (var group in groups)
                {
                    var iconMerged = new ItemIconMerged(group)
                    {
                        ShowSetSearchIconContextMenu = m_savedSearch != null,
                        Parent = m_ungroupedPanel,
                    };
                    iconMerged.SetAsSearchIcon += Icon_SetAsSearchIcon;
                }
            }
            else
            {
                foreach (var itemIcon in m_itemIcons)
                {
                    if (m_searchOptions.FilterItem(itemIcon.ItemInfo))
                    {
                        itemIcon.Parent = m_ungroupedPanel;
                    }
                }
            }

            m_ungroupedPanel.ResumeLayout();
            m_ungroupedPanel.Parent = m_layout;
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
                    panel.Title = InventoryItem.ItemSourceToString(item.Source);
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

                if (m_ungroupedPanel != null && m_ungroupedPanel.Parent != null)
                {
                    LayoutHelper.SetWidthSizeMode(m_ungroupedPanel, DimensionSizeMode.Inherit, 20);
                }
            }
        }
    }
}
