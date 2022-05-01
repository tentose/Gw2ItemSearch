using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch
{
    public class SearchFilter
    {
        private ItemType? m_type;
        public ItemType? Type
        {
            get => m_type;
            set
            {
                if (m_type != value)
                {
                    m_type = value == ItemType.Unknown ? null : value;
                    OnFilterChanged();
                }
            }
        }

        private ItemSubType? m_subType;
        public ItemSubType? SubType
        {
            get => m_subType;
            set
            {
                if (m_subType != value)
                {
                    m_subType = value == ItemSubType.Unknown ? null : value;
                    OnFilterChanged();
                }
            }
        }

        private ItemRarity? m_rarity;
        public ItemRarity? Rarity
        {
            get => m_rarity;
            set
            {
                if (m_rarity != value)
                {
                    m_rarity = value == ItemRarity.Unknown ? null : value;
                    OnFilterChanged();
                }
            }
        }

        public void Clear()
        {
            m_type = null;
            m_subType = null;
            m_rarity = null;
        }

        public event EventHandler FilterChanged;
        private void OnFilterChanged()
        {
            FilterChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///  Filter a single InventoryItem according to current filter state. True if item passes filter. False if item fails filter.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>True if item passes filter. False if item fails filter.</returns>
        public bool FilterItem(InventoryItem item)
        {
            // Test for common case with no filters
            if (m_type == null && m_subType == null && m_rarity == null)
            {
                return true;
            }

            if (StaticItemInfo.AllItems.TryGetValue(item.Id, out var itemInfo))
            {
                return FilterItem(itemInfo);
            }
            return true;
        }

        /// <summary>
        ///  Filter a single StaticItemInfo according to current filter state. True if item passes filter. False if item fails filter.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>True if item passes filter. False if item fails filter.</returns>
        public bool FilterItem(StaticItemInfo itemInfo)
        {
            if (m_type != null && itemInfo.Type != m_type)
            {
                return false;
            }
            else if (m_subType != null && itemInfo.SubType != m_subType)
            {
                return false;
            }
            else if (m_rarity != null && itemInfo.Rarity != m_rarity)
            {
                return false;
            }
            return true;
        }
    }
}
