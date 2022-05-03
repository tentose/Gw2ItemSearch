using Blish_HUD;
using Gw2Sharp.WebApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ApiModels = Gw2Sharp.WebApi.V2.Models;

namespace ItemSearch
{
    internal static class DictionaryExtensions
    {
        public static void AddOrUpdate(this Dictionary<int, List<InventoryItem>> dict, int id, InventoryItem item)
        {
            List<InventoryItem> existing;
            if (!dict.TryGetValue(id, out existing))
            {
                existing = new List<InventoryItem>();
                dict[id] = existing;
            }
            existing.Add(item);
        }
    }

    internal class PlayerItemCollection
    {
        private const string ITEMS_CACHE_FILE_NAME = "player_items.json";
        private const int MINUTES_TO_MILLIS = 60 * 1000;
        private const int API_REFRESH_TIMEOUT_MILLIS = 5 * MINUTES_TO_MILLIS;

        private static readonly Logger Logger = Logger.GetLogger<ItemSearchModule>();

        private IGw2WebApiClient m_apiClient;
        private List<ApiModels.TokenPermission> m_permissions;
        private Timer m_refreshTimer;
        private string m_accountName;
        private int m_apiRefreshIntervalMillis;

        public Dictionary<int, List<InventoryItem>> Items { get; private set; }

        public static async Task<PlayerItemCollection> NewAsync(IGw2WebApiClient client, List<ApiModels.TokenPermission> permissions)
        {
            PlayerItemCollection items = new PlayerItemCollection();
            await items.Initialize(client, permissions);
            return items;
        }

        private PlayerItemCollection()
        {
        }

        private async Task Initialize(IGw2WebApiClient client, List<ApiModels.TokenPermission> permissions)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            m_apiClient = client;
            m_permissions = permissions;

            ApiModels.Account account = null;
            while (account == null)
            {
                account = await ApiHelper.Fetch(() => m_apiClient.V2.Account.GetAsync());
            }
            m_accountName = account.Name;

            Items = await InitializeFromCache(ITEMS_CACHE_FILE_NAME, Items);
            if (Items == null)
            {
                Items = await GetPlayerItems();
                await WriteToCache(ITEMS_CACHE_FILE_NAME, Items);
            }

            var apiRefreshIntervalSetting = ItemSearchModule.Instance.GlobalSettings.PlayerDataRefreshIntervalMinutes;
            m_apiRefreshIntervalMillis = apiRefreshIntervalSetting.Value * MINUTES_TO_MILLIS;
            apiRefreshIntervalSetting.SettingChanged += ApiRefreshIntervalSetting_SettingChanged;

            m_refreshTimer = new Timer();
            m_refreshTimer.AutoReset = true;
            m_refreshTimer.Interval = 1;
            m_refreshTimer.Elapsed += M_refreshTimer_Elapsed;
            m_refreshTimer.Start();

            Logger.Info($"InitializePlayerItems: {stopwatch.ElapsedMilliseconds}");
        }

        private void ApiRefreshIntervalSetting_SettingChanged(object sender, ValueChangedEventArgs<int> e)
        {
            m_apiRefreshIntervalMillis = e.NewValue * MINUTES_TO_MILLIS;
        }

        private void M_refreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            m_refreshTimer.Stop();

            try
            {
                if (!RefreshApiData().Wait(API_REFRESH_TIMEOUT_MILLIS))
                {
                    Logger.Error("Timed out while refreshing player data");
                }
            }
            catch(Exception ex)
            {
                Logger.Error(ex, "Error while refreshing player data");
            }
            finally
            {
                m_refreshTimer.Interval = m_apiRefreshIntervalMillis;
                m_refreshTimer.Start();
            }
        }

        private async Task RefreshApiData()
        {
            Logger.Info($"Refreshing player data from API");

            var items = await GetPlayerItems();
            Items = items;
            await WriteToCache(ITEMS_CACHE_FILE_NAME, items);
        }

        private async Task<T> InitializeFromCache<T>(string filename, T obj) where T : class
        {
            Logger.Info($"Initializing player data {filename} from cache");
            string cachePath = "";
            try
            {
                cachePath = GetCachePath(filename);

                if (!File.Exists(cachePath))
                {
                    return null;
                }

                return await Task.Run(() =>
                {
                    return JsonConvert.DeserializeObject<T>(File.ReadAllText(cachePath));
                });
            }
            catch (Exception e)
            {
                Logger.Warn(e, $"Failed to initialize player item data from cache: {PathHelper.StripPII(cachePath)}");
                return null;
            }
        }

        private async Task WriteToCache<T>(string filename, T items)
        {
            Logger.Info($"Persisting player data {filename} to cache");
            string cachePath = "";
            try
            {
                cachePath = GetCachePath(filename);

                await Task.Run(() =>
                {
                    File.WriteAllText(cachePath, JsonConvert.SerializeObject(items));
                });
            }
            catch (Exception e)
            {
                Logger.Warn(e, $"Failed to persist player item data to cache: {PathHelper.StripPII(cachePath)}");
            }
        }

        private string GetCachePath(string filename)
        {
            var accountDir = Path.Combine(ItemSearchModule.Instance.CacheDirectory, m_accountName);
            Directory.CreateDirectory(accountDir);

            return Path.Combine(accountDir, filename);
        }

        public async Task<Dictionary<int, List<InventoryItem>>> GetPlayerItems()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Dictionary<int, List<InventoryItem>> allPlayerItems = new Dictionary<int, List<InventoryItem>>();
            var webApiClient = m_apiClient.V2;

            Action<InventoryItem> addItemToAllItems = (InventoryItem item) =>
            {
                allPlayerItems.AddOrUpdate(item.Id, item);
                if (item.Skin.HasValue && item.Skin.Value > 0)
                {
                    allPlayerItems.AddOrUpdate(item.Skin.Value, item);
                }
                if (item.Infusions != null)
                {
                    foreach (var infusionId in item.Infusions)
                    {
                        allPlayerItems.AddOrUpdate(infusionId, InventoryItem.FromParentItem(item, infusionId));
                    }
                }
                if (item.Upgrades != null)
                {
                    foreach (var upgradeId in item.Upgrades)
                    {
                        allPlayerItems.AddOrUpdate(upgradeId, InventoryItem.FromParentItem(item, upgradeId));
                    }
                }
            };

            if (m_permissions.Contains(ApiModels.TokenPermission.Inventories))
            {
                var bankItems = await ApiHelper.Fetch(() => webApiClient.Account.Bank.GetAsync());
                if (bankItems != null)
                {
                    foreach (var item in bankItems)
                    {
                        if (item != null)
                        {
                            addItemToAllItems(new InventoryItem(item, InventoryItemSource.Bank));
                        }
                    }
                }
                else
                {
                    Logger.Warn("Failed to retrieve bank items.");
                }

                var sharedInventoryItems = await ApiHelper.Fetch(() => webApiClient.Account.Inventory.GetAsync());
                if (sharedInventoryItems != null)
                {
                    foreach (var item in sharedInventoryItems)
                    {
                        if (item != null)
                        {
                            addItemToAllItems(new InventoryItem(item, InventoryItemSource.SharedInventory));
                        }
                    }
                }
                else
                {
                    Logger.Warn("Failed to retrieve shared inventory items.");
                }

                var materials = await ApiHelper.Fetch(() => webApiClient.Account.Materials.GetAsync());
                if (materials != null)
                {
                    foreach (var item in materials)
                    {
                        if (item.Count > 0)
                        {
                            addItemToAllItems(new InventoryItem(item));
                        }
                    }
                }
                else
                {
                    Logger.Warn("Failed to retrieve materials.");
                }

                var characters = await ApiHelper.Fetch(() => webApiClient.Characters.AllAsync());
                if (characters != null)
                {
                    foreach (var character in characters)
                    {
                        if (character.Bags != null)
                        {
                            foreach (var bag in character.Bags)
                            {
                                if (bag != null)
                                {
                                    addItemToAllItems(new InventoryItem(bag.Id, InventoryItemSource.CharacterInventory, character.Name));
                                    foreach (var item in bag.Inventory)
                                    {
                                        if (item != null)
                                        {
                                            addItemToAllItems(new InventoryItem(item, InventoryItemSource.CharacterInventory, character.Name));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Logger.Warn("Failed to retrieve character bags");
                        }

                        if (character.EquipmentTabs != null)
                        {
                            foreach (var tab in character.EquipmentTabs)
                            {
                                if (tab != null)
                                {
                                    foreach (var equipItem in tab.Equipment)
                                    {
                                        if (equipItem != null)
                                        {
                                            addItemToAllItems(new InventoryItem(equipItem, InventoryItemSource.CharacterEquipment, character.Name, tab.Tab));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Logger.Warn("Failed to retrieve character equipment");
                        }

                        if (character.Equipment != null)
                        {
                            foreach (var equip in character.Equipment)
                            {
                                if (equip != null)
                                {
                                    if (equip.Slot == ApiModels.ItemEquipmentSlotType.Axe ||
                                        equip.Slot == ApiModels.ItemEquipmentSlotType.Sickle ||
                                        equip.Slot == ApiModels.ItemEquipmentSlotType.Pick)
                                    {
                                        addItemToAllItems(new InventoryItem(equip, InventoryItemSource.CharacterEquipment, character.Name));
                                    }
                                }
                            }
                        }
                        else
                        {
                            Logger.Warn("Failed to retrieve character equipment");
                        }
                    }
                }
                else
                {
                    Logger.Warn("Failed to retrieve characters.");
                }
            }

            if (m_permissions.Contains(ApiModels.TokenPermission.Tradingpost))
            {
                var tpBox = (await ApiHelper.Fetch(() => webApiClient.Commerce.Delivery.GetAsync()));
                if (tpBox != null)
                {
                    var tpBoxItems = tpBox.Items;
                    foreach (var item in tpBoxItems)
                    {
                        addItemToAllItems(new InventoryItem(item));
                    }
                }
                else
                {
                    Logger.Warn("Failed to retrieve trading post delivery box items.");
                }

                var tpSellItems = await ApiHelper.Fetch(() => webApiClient.Commerce.Transactions.Current.Sells.GetAsync());
                if (tpSellItems != null)
                {
                    foreach (var item in tpSellItems)
                    {
                        addItemToAllItems(new InventoryItem(item));
                    }
                }
                else
                {
                    Logger.Warn("Failed to retrieve trading post sell orders.");
                }
            }

            Logger.Debug($"GetPlayerItems: {stopwatch.ElapsedMilliseconds}");

            return allPlayerItems;
        }

        // Not used at the moment. Legendary runes when stat selected get a different item ID than the un-specialized legendary rune.
        // The API will only return the unspecialized version. So this list alone cannot be used to filter out legendary armory equipment.
        private async Task<SortedSet<int>> GetPlayerLegendaryArmory()
        {
            SortedSet<int> armoryItems = new SortedSet<int>();

            Stopwatch stopwatch = Stopwatch.StartNew();

            var webApiClient = m_apiClient.V2;

            var armory = await ApiHelper.Fetch(() => webApiClient.Account.LegendaryArmory.GetAsync());

            if (armory != null)
            {
                foreach (var item in armory)
                {
                    armoryItems.Add(item.Id);
                }
            }

            Logger.Debug($"GetPlayerLegendaryArmory: {stopwatch.ElapsedMilliseconds}");

            return armoryItems;
        }
    }
}
