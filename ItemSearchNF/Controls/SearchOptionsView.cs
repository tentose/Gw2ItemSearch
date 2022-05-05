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
    public class SearchOptionsView : View
    {
        private const int DROPDOWN_WIDTH = 200;

        private FlowPanel m_panel;
        private Dropdown m_typeDropdown;
        private Dropdown m_subtypeDropdown;
        private Dropdown m_rarityDropdown;
        private StandardButton m_clearFiltersButton;
        private StandardButton m_saveSearchButton;
        private StandardButton m_removedSavedSearchButton;

        // For looking up string selected in Dropdown and convert it to an ItemType
        private Dictionary<string, ItemType> m_itemTypeStringToItemType;

        // List of pairs that link an ItemSubType with its display string
        private List<(string resourceString, ItemSubType itemSubType)> m_itemSubTypeStringToItemSubType;
        // Above List isn't complete because it doesn't contain the display strings for "all" subitems like
        // "All weapons". This is the mapping for the "all" subitems entries.
        private List<(string resourceString, ItemType itemType)> m_allItemSubTypeStringToItemType;

        // For looking up string selected in Dropdown and convert it to an ItemSubType
        private Dictionary<string, ItemSubType> m_filteredItemSubTypeStringToItemSubType;

        // For looking up string selected in Dropdown and convert it to an ItemRarity
        private Dictionary<string, ItemRarity> m_rarityStringToRarity;

        private SearchFilter m_searchFilter;

        public SearchOptionsView(SearchFilter filter)
        {
            m_searchFilter = filter;
        }

        protected override void Build(Container buildPanel)
        {
            m_panel = new FlowPanel()
            {
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Padding = new Thickness(20f),
                OuterControlPadding = new Vector2(10, 10),
                AutoSizePadding = new Point(10, 10),
                ControlPadding = new Vector2(10, 10),
            };

            // Item Type
            m_itemTypeStringToItemType = GetResourceStringToTypeDictionary<ItemType>("ItemType");

            m_typeDropdown = new Dropdown()
            {
                Parent = m_panel,
                Width = DROPDOWN_WIDTH,
            };
            foreach (var key in m_itemTypeStringToItemType.Keys)
            {
                m_typeDropdown.Items.Add(key);
            }
            if (m_searchFilter.Type != null)
            {
                // Pre-select the current filter value
                foreach (var kv in m_itemTypeStringToItemType)
                {
                    if (kv.Value == m_searchFilter.Type)
                    {
                        m_typeDropdown.SelectedItem = kv.Key;
                        break;
                    }
                }
            }
            m_typeDropdown.ValueChanged += M_typeDropdown_ValueChanged;

            // SubType
            m_itemSubTypeStringToItemSubType = GetResourceStringToItemSubType();
            m_allItemSubTypeStringToItemType = GetResourceStringForAllItemSubTypeToItemType();

            m_subtypeDropdown = new Dropdown()
            {
                Parent = m_panel,
                Width = DROPDOWN_WIDTH,
            };
            UpdateSubTypeDropdownOptions(m_searchFilter.Type ?? ItemType.Unknown);
            if (m_searchFilter.SubType != null)
            {
                // Pre-select the current filter value
                foreach (var pair in m_itemSubTypeStringToItemSubType)
                {
                    if (pair.itemSubType == m_searchFilter.SubType)
                    {
                        m_subtypeDropdown.SelectedItem = pair.resourceString;
                        break;
                    }
                }
            }
            m_subtypeDropdown.ValueChanged += M_subtypeDropdown_ValueChanged;

            // Rarity
            m_rarityStringToRarity = GetResourceStringToTypeDictionary<ItemRarity>("Rarity");
            m_rarityDropdown = new Dropdown()
            {
                Parent = m_panel,
                Width = DROPDOWN_WIDTH,
            };
            foreach (var key in m_rarityStringToRarity.Keys)
            {
                m_rarityDropdown.Items.Add(key);
            }
            if (m_searchFilter.Rarity != null)
            {
                // Pre-select the current filter value
                foreach (var kv in m_rarityStringToRarity)
                {
                    if (kv.Value == m_searchFilter.Rarity)
                    {
                        m_rarityDropdown.SelectedItem = kv.Key;
                        break;
                    }
                }
            }
            m_rarityDropdown.ValueChanged += M_rarityDropdown_ValueChanged;

            // Clear filters
            m_clearFiltersButton = new StandardButton()
            {
                Parent = m_panel,
                Text = Strings.SearchOptionsPanel_Clear,
            };
            m_clearFiltersButton.Click += M_clearFiltersButton_Click;

            m_panel.Parent = buildPanel;
        }

        private void M_clearFiltersButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            m_searchFilter.Clear();
            m_typeDropdown.SelectedItem = m_typeDropdown.Items[0];
            m_rarityDropdown.SelectedItem = m_rarityDropdown.Items[0];
        }

        private void M_typeDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            ItemType itemType = GetSelectedItemType(e.CurrentValue);

            if (itemType == ItemType.Unknown)
            {
                m_subtypeDropdown.SelectedItem = null;
                m_searchFilter.Type = null;
                m_searchFilter.SubType = null;
            }
            else
            {
                UpdateSubTypeDropdownOptions(itemType);

                m_searchFilter.Type = itemType;
            }
        }

        private void M_subtypeDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.CurrentValue == null)
            {
                m_searchFilter.SubType = null;
                m_subtypeDropdown.Enabled = false;
            }
            else
            {
                m_subtypeDropdown.Enabled = true;
                if (!m_filteredItemSubTypeStringToItemSubType.TryGetValue(e.CurrentValue, out ItemSubType type))
                {
                    m_searchFilter.SubType = null;
                }
                else
                {
                    m_searchFilter.SubType = type;
                }
            }
        }

        private void M_rarityDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (!m_rarityStringToRarity.TryGetValue(e.CurrentValue, out ItemRarity type))
            {
                m_searchFilter.Rarity = null;
            }
            else
            {
                m_searchFilter.Rarity = type;
            }
        }

        private void UpdateSubTypeDropdownOptions(ItemType itemType)
        {
            m_filteredItemSubTypeStringToItemSubType = GetFileterdSubTypeDictionary(itemType);

            m_subtypeDropdown.Items.Clear();
            foreach (var key in m_filteredItemSubTypeStringToItemSubType.Keys)
            {
                m_subtypeDropdown.Items.Add(key);
            }

            if (m_subtypeDropdown.Items.Count > 0)
            {
                m_subtypeDropdown.SelectedItem = m_subtypeDropdown.Items[0];
            }
            else
            {
                m_subtypeDropdown.SelectedItem = null;
            }
        }

        private Dictionary<string, T> GetResourceStringToTypeDictionary<T>(string typeString) where T : Enum
        {
            Dictionary<string, T> result = new Dictionary<string, T>();
            result.Add(ResourceStrings.Get($"Filter_{typeString}_All"), (T)(object)0);
            foreach (var itemTypeEnumValue in Enum.GetValues(typeof(T)))
            {
                if ((int)itemTypeEnumValue == 0)
                {
                    continue;
                }
                T itemType = (T)itemTypeEnumValue;
                result.Add(ResourceStrings.Get($"Filter_{typeString}_{itemType}"), itemType);
            }
            return result;
        }

        private List<(string, ItemSubType)> GetResourceStringToItemSubType()
        {
            List<(string, ItemSubType)> result = new List<(string, ItemSubType)>();

            foreach (var itemSubTypeEnumValue in Enum.GetValues(typeof(ItemSubType)))
            {
                ItemSubType itemSubType = (ItemSubType)itemSubTypeEnumValue;
                if (itemSubType == ItemSubType.Unknown)
                {
                    continue;
                }
                string itemSubTypeStr = itemSubType.ToString();
                result.Add((ResourceStrings.Get($"Filter_{itemSubTypeStr}"), itemSubType));
            }
            return result;
        }

        private List<(string, ItemType)> GetResourceStringForAllItemSubTypeToItemType()
        {
            List<(string, ItemType)> result = new List<(string, ItemType)>();

            // Add the All values
            foreach (var itemTypeObj in Enum.GetValues(typeof(ItemType)))
            {
                ItemType itemType = (ItemType)itemTypeObj;
                if (itemType == ItemType.Unknown)
                {
                    continue;
                }

                string itemTypeStr = itemType.ToString();
                string resourceString = ResourceStrings.Get($"Filter_{itemTypeStr}_All");
                if (resourceString != null)
                {
                    result.Add((resourceString, itemType));
                }
            }

            return result;
        }

        private Dictionary<string, ItemSubType> GetFileterdSubTypeDictionary(ItemType currentType)
        {
            Dictionary<string, ItemSubType> result = new Dictionary<string, ItemSubType>();

            // First add the "all" subtype
            foreach (var kv in m_allItemSubTypeStringToItemType)
            {
                if (kv.itemType == currentType)
                {
                    result.Add(kv.resourceString, ItemSubType.Unknown);
                    break;
                }
            }

            // Now add the other subtypes
            string itemTypeStr = currentType.ToString();
            string targetSubTypePrefix = itemTypeStr + "_";
            foreach (var kv in m_itemSubTypeStringToItemSubType)
            {
                string itemSubTypeStr = kv.itemSubType.ToString();
                if (itemSubTypeStr.StartsWith(targetSubTypePrefix))
                {
                    result.Add(kv.resourceString, kv.itemSubType);
                }
            }

            return result;
        }

        private ItemType GetSelectedItemType(string selectedItemValue)
        {
            if (!m_itemTypeStringToItemType.TryGetValue(selectedItemValue, out ItemType type))
            {
                type = ItemType.Unknown;
            }
            return type;
        }

        protected override void Unload()
        {
            if (m_typeDropdown != null)
            {
                m_typeDropdown.Dispose();
                m_typeDropdown = null;
            }

            if (m_subtypeDropdown != null)
            {
                m_subtypeDropdown.Dispose();
                m_subtypeDropdown = null;
            }

            if (m_rarityDropdown != null)
            {
                m_rarityDropdown.Dispose();
                m_rarityDropdown = null;
            }

            if (m_panel != null)
            {
                m_panel.Dispose();
                m_panel = null;
            }

            m_itemTypeStringToItemType = null;
            m_itemSubTypeStringToItemSubType = null;
            m_allItemSubTypeStringToItemType = null;
            m_rarityStringToRarity = null;

            base.Unload();
        }
    }

}
