using Blish_HUD;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.V2.Clients;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiModels = Gw2Sharp.WebApi.V2.Models;

namespace ItemSearch
{

    internal class AccountNameHelper
    {
        private static readonly Logger Logger = Logger.GetLogger<ItemSearchModule>();

        private const string CHARACTER_ACCOUNT_CACHE_NAME = "character_account.json";

        /// <summary>
        /// Gets the account name of the currently signed in account. 
        /// By default, we query the GW2 web API to get the account name given the current API key.
        /// However if the web API is down for whatever reason, we try to 
        /// </summary>
        public static async Task<string> GetAccountName(IGw2WebApiClient client, int maxApiRetries = 5)
        {
            int apiRetries = 0;
            ApiModels.Account account = null;
            while (account == null && apiRetries++ < maxApiRetries)
            {
                account = await ApiHelper.Fetch(() => client.V2.Account.GetAsync());
                await Task.Delay(1000);
            }

            if (account == null)
            {
                Logger.Warn("Failed to fetch account name from API. Checking cache.");

                var characterToAccount = new Dictionary<string, string>();
                characterToAccount = await ReadFromFile(CHARACTER_ACCOUNT_CACHE_NAME, characterToAccount);

                if (characterToAccount.TryGetValue(Gw2MumbleService.Gw2Mumble.PlayerCharacter.Name, out string accountName))
                {
                    Logger.Info($"Fetched account name {accountName} from cache.");
                    return accountName;
                }

                Logger.Warn("Character not found in cache.");
                throw new NoAccountNameException();
            }
            else
            {
                string accountName = account.Name;
                Logger.Info($"Fetched account name {accountName} from API. Updating cache.");

                UpdateCharacterToAccountCache(client, accountName);

                return accountName;
            }
        }

        private static async void UpdateCharacterToAccountCache(IGw2WebApiClient client, string accountName)
        {
            var characterToAccount = new Dictionary<string, string>();
            characterToAccount = await ReadFromFile(CHARACTER_ACCOUNT_CACHE_NAME, characterToAccount);
            var characterNames = await client.V2.Characters.IdsAsync();
            foreach (var characterName in characterNames)
            {
                characterToAccount[characterName] = accountName;
            }
            await WriteToFile(CHARACTER_ACCOUNT_CACHE_NAME, characterToAccount);
        }

        private static async Task<T> ReadFromFile<T>(string filename, T obj) where T : class, new()
        {
            string cachePath = "";
            try
            {
                cachePath = GetCachePath(filename);

                if (!File.Exists(cachePath))
                {
                    return new T();
                }

                return await Task.Run(() =>
                {
                    return JsonConvert.DeserializeObject<T>(File.ReadAllText(cachePath));
                });
            }
            catch (Exception e)
            {
                Logger.Warn($"Failed to read character to account cache. {e}");
                return new T();
            }
        }

        private static async Task WriteToFile<T>(string filename, T items)
        {
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
                Logger.Warn($"Failed to update character to account cache. {e}");
            }
        }

        private static string GetCachePath(string filename)
        {
            return Path.Combine(ItemSearchModule.Instance.CacheDirectory, filename);
        }
    }
}
