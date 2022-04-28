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
        public string LocationHint { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public int? Charges { get; set; }
        public int[] Infusions { get; set; }
        public int[] Upgrades { get; set; }
        public int? Skin { get; set; }
        public string Binding { get; set; }
        public string BoundTo { get; set; }

        public InventoryItem(AccountItem item, InventoryItemSource source, string locationHint = null)
        {
            Source = source;
            LocationHint = locationHint;
            Id = item.Id;
            Count = item.Count;
            Charges = item.Charges;
            Infusions = item.Infusions?.ToArray();
            Upgrades = item.Upgrades?.ToArray();
            Skin = item.Skin;
            Binding = item.Binding?.ToString();
            BoundTo = item.BoundTo;
        }

        public InventoryItem(CharacterEquipmentItem item, InventoryItemSource source, string locationHint = null)
        {
            Source = source;
            LocationHint = locationHint;
            Id = item.Id;
            Infusions = item.Infusions?.ToArray();
            Upgrades = item.Upgrades?.ToArray();
            Skin = item.Skin;
            Binding = item.Binding?.ToString();
            BoundTo = item.BoundTo;
        }

        public InventoryItem(int itemId, InventoryItemSource source, string locationHint = null)
        {
            Source = source;
            Id = itemId;
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
    }
}
