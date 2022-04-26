using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using ItemSearch.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch
{
    [Export(typeof(Module))]
    public class ItemSearchModule : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<ItemSearchModule>();

        private const string CACHE_DIRECTORY = "itemsearchcache";
        private const string STATIC_ITEMS_FILE_NAME = "static_items.json";

        private SettingsManager m_settingsManager => this.ModuleParameters.SettingsManager;
        public ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        private DirectoriesManager m_directoriesManager => this.ModuleParameters.DirectoriesManager;
        public Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        private ItemIndex m_searchEngine;
        private ItemSearchWindow m_searchWindow;
        private CornerIcon m_searchIcon;

        public static ItemSearchModule Instance;

        [ImportingConstructor]
        public ItemSearchModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            Instance = this;
        }

        public SettingCollection GetSettingCollection()
        {
            return m_settingsManager.ModuleSettings;
        }

        protected override async Task LoadAsync()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            m_searchIcon = new CornerIcon()
            {
                IconName = "Search",
                Icon = ContentsManager.GetTexture(@"Textures\CornerIcon.png"),
                HoverIcon = ContentsManager.GetTexture(@"Textures\CornerIconHover.png"),
                Priority = 5,
                LoadingMessage = Strings.CornerIconLoadingProgress_Initial,
            };
            
            await ItemIcon.LoadIconResources();

            m_searchIcon.LoadingMessage = Strings.CornerIconLoadingProgress_StaticItems;
            var cacheDir = m_directoriesManager.GetFullDirectoryPath(CACHE_DIRECTORY);
            var staticItemsJsonPath = Path.Combine(cacheDir, STATIC_ITEMS_FILE_NAME);

            // Fetch a copy of the static items json from resources if it doesn't exist
            if (!File.Exists(staticItemsJsonPath))
            {
                Logger.Info($"{STATIC_ITEMS_FILE_NAME} not found in cache. Restoring from resources");
                using (var inStream = ContentsManager.GetFileStream(STATIC_ITEMS_FILE_NAME))
                {
                    using (var outStream = File.OpenWrite(staticItemsJsonPath))
                    {
                        inStream.CopyTo(outStream);
                    }
                }
            }

            await StaticItemInfo.Initialize(staticItemsJsonPath, Gw2ApiManager.Gw2ApiClient);

            m_searchIcon.LoadingMessage = Strings.CornerIconLoadingProgress_BuildingSearchTree;
            m_searchEngine = new ItemIndex();
            await m_searchEngine.InitializeIndex(Gw2ApiManager.Gw2ApiClient, Gw2ApiManager.Permissions);

            // Controls
            m_searchWindow = new ItemSearchWindow(ContentsManager, m_searchEngine);
            m_searchIcon.Click += delegate { m_searchWindow.ToggleWindow(); };
            m_searchIcon.LoadingMessage = null;
            Logger.Info($"LoadAsync: {stopwatch.ElapsedMilliseconds}");
        }
    }
}
