using Blish_HUD;
using Gw2Sharp.WebApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiModels = Gw2Sharp.WebApi.V2.Models;

namespace ItemSearch
{


    internal class ItemIndex
    {
        private static readonly Logger Logger = Logger.GetLogger<ItemSearchModule>();

        private PlayerItemCollection m_playerItems;

        public static async Task<ItemIndex> NewAsync(IGw2WebApiClient client, List<ApiModels.TokenPermission> permissions)
        {
            ItemIndex index = new ItemIndex();
            await index.Initialize(client, permissions);
            return index;
        }

        private ItemIndex()
        {
        }

        private async Task Initialize(IGw2WebApiClient client, List<ApiModels.TokenPermission> permissions)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Logger.Info("Token permissions: " + String.Join(", ", permissions));

            m_playerItems = await PlayerItemCollection.NewAsync(client, permissions);

            Logger.Info($"InitializeIndex: {stopwatch.ElapsedMilliseconds}");
        }

        public async Task<List<InventoryItem>> Search(string query)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var lowerQuery = query.ToLowerInvariant();
            var allItems = StaticItemInfo.AllItems;
            var results = allItems.Where(item => item.Value.Name.ToLowerInvariant().Contains(lowerQuery)).Select(kv => kv.Key);
            Logger.Debug($"ItemSearch: {stopwatch.ElapsedMilliseconds}");

            List<InventoryItem> playerMatchingItems = new List<InventoryItem>();
            var playerItems = m_playerItems.Items;
            bool excludeArmory = ItemSearchModule.Instance.GlobalSettings.HideLegendaryArmory.Value;
            foreach (var id in results)
            {
                List<InventoryItem> items;
                if (playerItems.TryGetValue(id, out items))
                {
                    foreach (var item in playerItems[id])
                    {
                        if (excludeArmory && item.Binding.HasValue && item.Binding.Value == ItemBinding.Account && item.ItemInfo != null)
                        {
                            // Exclude armory, this is an account bound item with info we can check against.
                            var info = item.ItemInfo;
                            if (info.Rarity == ItemRarity.Legendary &&
                                (info.Type == ItemType.Armor || info.Type == ItemType.Weapon || info.Type == ItemType.Trinket || info.Type == ItemType.UpgradeComponent))
                            {
                                // Is legendary and is equipment, let's skip this
                                continue;
                            }
                        }
                        playerMatchingItems.Add(item);
                    }
                }
            }
            Logger.Debug($"Matched against player items: {stopwatch.ElapsedMilliseconds}");

            return playerMatchingItems;
        }

        public async Task<List<InventoryItem>> Browse(SearchFilter filter)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var allItems = StaticItemInfo.AllItems;
            var results = allItems.Where(item => filter.FilterItem(item.Value)).Select(kv => kv.Key);
            Logger.Debug($"ItemBrowse: {stopwatch.ElapsedMilliseconds}");

            List<InventoryItem> playerMatchingItems = new List<InventoryItem>();
            var playerItems = m_playerItems.Items;
            bool excludeArmory = ItemSearchModule.Instance.GlobalSettings.HideLegendaryArmory.Value;
            foreach (var id in results)
            {
                List<InventoryItem> items;
                if (playerItems.TryGetValue(id, out items))
                {
                    foreach (var item in playerItems[id])
                    {
                        if (excludeArmory && item.Binding.HasValue && item.Binding.Value == ItemBinding.Account && item.ItemInfo != null)
                        {
                            // Exclude armory, this is an account bound item with info we can check against.
                            var info = item.ItemInfo;
                            if (info.Rarity == ItemRarity.Legendary &&
                                (info.Type == ItemType.Armor || info.Type == ItemType.Weapon || info.Type == ItemType.Trinket || info.Type == ItemType.UpgradeComponent))
                            {
                                // Is legendary and is equipment, let's skip this
                                continue;
                            }
                        }
                        playerMatchingItems.Add(item);
                    }
                }
            }
            Logger.Debug($"Matched against player items: {stopwatch.ElapsedMilliseconds}");

            return playerMatchingItems;
        }
    }
}
