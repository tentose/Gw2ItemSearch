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
        private const string STATIC_ITEMS_FILE_NAME = "all_items.json";

        private SettingsManager m_settingsManager => this.ModuleParameters.SettingsManager;
        public ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        public DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        public Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        public SearchGlobalSettings GlobalSettings { get; private set; }

        public string CacheDirectory { get; private set; }
        public string LocaleSpecificCacheDirectory { get; private set; }

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

        private string LocaleToPathString(Gw2Sharp.WebApi.Locale locale)
        {
            switch(locale)
            {
                case Gw2Sharp.WebApi.Locale.English: return "en";
                case Gw2Sharp.WebApi.Locale.French: return "fr";
                case Gw2Sharp.WebApi.Locale.German: return "de";
                case Gw2Sharp.WebApi.Locale.Spanish: return "es";
                case Gw2Sharp.WebApi.Locale.Chinese: return "zh";
                default: return "en";
            }
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

            // TODO: handle this more gracefully by locking the locale depedent resources and reloading
            GameService.Overlay.UserLocaleChanged += (sender, args) =>
            {
                this.Unload();
            };

            // Static items
            m_searchIcon.LoadingMessage = Strings.CornerIconLoadingProgress_StaticItems;

            CacheDirectory = DirectoriesManager.GetFullDirectoryPath(CACHE_DIRECTORY);
            var localeDir = LocaleToPathString(GameService.Overlay.UserLocale.Value);
            LocaleSpecificCacheDirectory = Path.Combine(CacheDirectory, localeDir);
            var staticItemsJsonPath = Path.Combine(LocaleSpecificCacheDirectory, STATIC_ITEMS_FILE_NAME);

            // Fetch a copy of the static items json from resources if it doesn't exist
            if (!File.Exists(staticItemsJsonPath))
            {
                Logger.Info($"{staticItemsJsonPath} not found. Restoring cache from resources");
                var resourcePath = Path.Combine(localeDir, STATIC_ITEMS_FILE_NAME);
                Directory.CreateDirectory(Path.GetDirectoryName(staticItemsJsonPath));
                using (var inStream = ContentsManager.GetFileStream(resourcePath))
                {
                    using (var outStream = File.OpenWrite(staticItemsJsonPath))
                    {
                        inStream.CopyTo(outStream);
                    }
                }
            }

            await StaticItemInfo.Initialize(staticItemsJsonPath, Gw2ApiManager.Gw2ApiClient);

            // Player items
            m_searchIcon.LoadingMessage = Strings.CornerIconLoadingProgress_PlayerItems;

            m_searchEngine = await ItemIndex.NewAsync(Gw2ApiManager.Gw2ApiClient, Gw2ApiManager.Permissions);

            // Controls
            m_searchWindow = new ItemSearchWindow(ContentsManager, m_searchEngine);
            m_searchIcon.Click += delegate { m_searchWindow.ToggleWindow(); };
            m_searchIcon.LoadingMessage = null;
            Logger.Info($"LoadAsync: {stopwatch.ElapsedMilliseconds}");
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            GlobalSettings = new SearchGlobalSettings(settings);
        }

        protected override void Unload()
        {
            m_searchEngine = null;

            if (m_searchWindow != null)
            {
                m_searchWindow.Dispose();
                m_searchWindow = null;
            }
            
            if (m_searchIcon != null)
            {
                m_searchIcon.Dispose();
                m_searchIcon = null;
            }
            
            Instance = null;
        }
    }
}
