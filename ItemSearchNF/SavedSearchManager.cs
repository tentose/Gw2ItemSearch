using Blish_HUD;
using MonoGame.Extended.Collections;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch
{
    public class SavedSearchManager
    {
        private const string SAVED_SEARCH_FILE_NAME = "saved_searches.json";

        private static readonly Logger Logger = Logger.GetLogger<ItemSearchModule>();

        public ObservableCollection<SavedSearch> SavedSearchList { get; private set; }
        public SavedSearchManager()
        {
            SavedSearchList = new ObservableCollection<SavedSearch>();
        }

        public async Task Initialize()
        {
            Logger.Info($"Initializing saved searches from storage");

            string savesPath = Path.Combine(ItemSearchModule.Instance.SaveDirectory, SAVED_SEARCH_FILE_NAME);
            try
            {
                if (!File.Exists(savesPath))
                {
                    return;
                }

                List<SavedSearch> savedSearches = null;
                await Task.Run(() =>
                {
                    savedSearches = JsonConvert.DeserializeObject<List<SavedSearch>>(File.ReadAllText(savesPath));
                });

                foreach (var save in savedSearches)
                {
                    save.Updated += Save_Updated;
                    SavedSearchList.Add(save);
                }
            }
            catch (Exception e)
            {
                Logger.Warn(e, $"Failed to initialize saved searches: {PathHelper.StripPII(savesPath)}");
            }
        }

        private void Save_Updated(object sender, EventArgs e)
        {
            _ = PersistSaves();
        }

        public async Task PersistSaves()
        {
            Logger.Info($"Writing saved searches to storage");

            string savesPath = Path.Combine(ItemSearchModule.Instance.SaveDirectory, SAVED_SEARCH_FILE_NAME);
            try
            {
                List<SavedSearch> savedSearches = SavedSearchList.ToList();
                await Task.Run(() =>
                {
                    File.WriteAllText(savesPath, JsonConvert.SerializeObject(savedSearches));
                });
            }
            catch (Exception e)
            {
                Logger.Warn(e, $"Failed to save searches: {PathHelper.StripPII(savesPath)}");
            }
        }

        public void AddSavedSearch(SavedSearch savedSearch)
        {
            SavedSearchList.Add(savedSearch);
            savedSearch.Updated += Save_Updated;
            _ = PersistSaves();
        }

        public void RemoveSavedSearch(SavedSearch savedSearch)
        {
            SavedSearchList.Remove(savedSearch);
            _ = PersistSaves();
        }
    }

}
