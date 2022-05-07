using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch.Controls
{
    public class ItemIconMerged : ItemIcon
    {
        private IEnumerable<ItemIcon> m_icons;
        private IEnumerable<InventoryItem> m_items;
        private string m_fullNumber = "";

        public ItemIconMerged(InventoryItem item) : base(item)
        {
        }

        public ItemIconMerged(IEnumerable<ItemIcon> itemIcons) : base(itemIcons.First().Item)
        {
            m_icons = itemIcons;

            int totalCount = 0;
            foreach (var itemIcon in itemIcons)
            {
                totalCount += itemIcon.Item.Count;
            }

            m_items = itemIcons.Select(itemIcon => itemIcon.Item);

            if (totalCount > 1)
            {
                m_number = ShortenNumber(totalCount);
                m_fullNumber = totalCount.ToString("N0");
            }
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            if (Tooltip == null)
            {
                Tooltip = new Tooltip(new ItemTooltipView(m_items, m_fullNumber));
            }
        }

        private string ShortenNumber(int value)
        {
            if (value >= 1_000_000)
            {
                return string.Format("{0:N2}M", Math.Round(value / 1_000_000.0, 2));
            }
            else if (value >= 100_000)
            {
                return string.Format("{0:N1}K", Math.Round(value / 1_000.0, 1));
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
