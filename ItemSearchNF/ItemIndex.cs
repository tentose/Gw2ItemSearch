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

        private PlayerItems m_playerItems;

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

            m_playerItems = await PlayerItems.NewAsync(client, permissions);

            Logger.Info($"InitializeIndex: {stopwatch.ElapsedMilliseconds}");
        }

        public async Task<List<InventoryItem>> Search(string query)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var lowerQuery = query.ToLowerInvariant();
            var results = StaticItemInfo.AllItems.Where(item => item.Value.Name.ToLowerInvariant().Contains(lowerQuery)).Select(kv => kv.Key);
            Logger.Info($"ItemSearch: {stopwatch.ElapsedMilliseconds}");

            List<InventoryItem> playerMatchingItems = new List<InventoryItem>();
            var playerItems = m_playerItems.Items;
            foreach (var id in results)
            {
                List<InventoryItem> items;
                if (playerItems.TryGetValue(id, out items))
                {
                    foreach (var item in playerItems[id])
                    {
                        playerMatchingItems.Add(item);
                    }
                }
            }
            Logger.Info($"Matched against player items: {stopwatch.ElapsedMilliseconds}");

            return playerMatchingItems;
        }
    }
}
