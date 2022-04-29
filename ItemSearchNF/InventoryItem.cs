using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gw2Sharp.WebApi.V2.Models;

namespace ItemSearch
{
    public enum InventoryItemSource
    {
        Unknown,
        CharacterInventory,
        CharacterEquipment,
        SharedInventory,
        Bank,
        MaterialStorage,
        TradingPostDeliveryBox,
        TradingPostSellOrder,
    }

    public class InventoryItem
    {
        public InventoryItemSource Source { get; set; }
        public string CharacterName { get; set; }
        public int EquipmentTabId { get; set; } = -1;
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public int? Charges { get; set; }
        public int[] Infusions { get; set; }
        public int[] Upgrades { get; set; }
        public int? Skin { get; set; }
        public string Binding { get; set; }
        public string BoundTo { get; set; }
        public InventoryItem ParentItem { get; set; }

        public InventoryItem(AccountItem item, InventoryItemSource source, string character = null)
        {
            Source = source;
            CharacterName = character;
            Id = item.Id;
            Count = item.Count;
            Charges = item.Charges;
            Infusions = item.Infusions?.ToArray();
            Upgrades = item.Upgrades?.ToArray();
            Skin = item.Skin;
            Binding = item.Binding?.ToString();
            BoundTo = item.BoundTo;
        }

        public InventoryItem(CharacterEquipmentItem item, InventoryItemSource source, string character = null, int equipmentTab = -1)
        {
            Source = source;
            CharacterName = character;
            Id = item.Id;
            Infusions = item.Infusions?.ToArray();
            Upgrades = item.Upgrades?.ToArray();
            Skin = item.Skin;
            Binding = item.Binding?.ToString();
            BoundTo = item.BoundTo;
            EquipmentTabId = equipmentTab;
        }

        public InventoryItem(int itemId, InventoryItemSource source, string character = null)
        {
            Source = source;
            Id = itemId;
            CharacterName = character;
        }

        public InventoryItem(AccountMaterial item)
        {
            Source = InventoryItemSource.MaterialStorage;
            Id = item.Id;
            Count = item.Count;
        }

        public InventoryItem(CommerceDeliveryItem item)
        {
            Source = InventoryItemSource.TradingPostDeliveryBox;
            Id = item.Id;
            Count = item.Count;
        }

        public InventoryItem(CommerceTransactionCurrent item)
        {
            Source = InventoryItemSource.TradingPostSellOrder;
            Id = item.ItemId;
            Count = item.Quantity;
        }

        public InventoryItem()
        {
            Source = InventoryItemSource.Unknown;
        }

        public static InventoryItem FromParentItem(InventoryItem parent, int itemId)
        {
            InventoryItem item = new InventoryItem(itemId, parent.Source, parent.CharacterName);
            item.ParentItem = parent;
            item.EquipmentTabId = parent.EquipmentTabId;
            return item;
        }
    }
}
