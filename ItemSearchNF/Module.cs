using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using ItemSearch.Controls;
using Newtonsoft.Json;
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
        private const string SAVE_DIRECTORY = "itemsearchsave";
        private const string STATIC_ITEMS_FILE_NAME = "all_items.json";
        private const string CACHE_VERSION_FILE_NAME = "cache_version.json";
        private const string RENDER_CACHE_FILE_NAME = "render_cache";
        private const string EXTERNAL_LINKS_FILE_NAME = "external_links.json";

        private SettingsManager m_settingsManager => this.ModuleParameters.SettingsManager;
        public ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        public DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        public Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        public SearchGlobalSettings GlobalSettings { get; private set; }
        public Gw2Sharp.WebApi.Render.IGw2WebApiRenderClient RenderClient { get; private set; }

        public string CacheDirectory { get; private set; }
        public string LocaleSpecificCacheDirectory { get; private set; }

        public string SaveDirectory { get; private set; }
        public ItemExternalLinks ExternalLinks { get; private set; }

        private ItemIndex m_searchEngine;
        private ItemSearchWindow m_searchWindow;
        private CornerIconManager m_searchIcon;
        private Gw2Sharp.Gw2Client m_gw2sharpClientForRender;
        private SavedSearchManager m_savedSearchManager;

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

        class ModuleVersion
        {
            public int Major { get; set; }
            public int Minor { get; set; }
            public int Patch { get; set; }
            public ModuleVersion(SemVer.Version v)
            {
                Major = v.Major;
                Minor = v.Minor;
                Patch = v.Patch;
            }
            public ModuleVersion()
            {
            }
        }

        protected override async Task LoadAsync()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            m_searchIcon = new CornerIconManager()
            {
                IconName = Strings.CornerIcon_HoverTooltip,
                Icon = ContentsManager.GetTexture(@"Textures\CornerIcon.png"),
                HoverIcon = ContentsManager.GetTexture(@"Textures\CornerIconHover.png"),
                LoadingMessage = Strings.CornerIconLoadingProgress_Initial,
            };

            await ItemIcon.LoadIconResources();

            // TODO: handle this more gracefully by locking the locale depedent resources and reloading
            GameService.Overlay.UserLocaleChanged += (sender, args) =>
            {
                Logger.Error("Locale changed. Unloading.");
                this.Unload();
            };

            CacheDirectory = DirectoriesManager.GetFullDirectoryPath(CACHE_DIRECTORY);
            EnsureCacheVersion();

            var localeDir = LocaleToPathString(GameService.Overlay.UserLocale.Value);
            LocaleSpecificCacheDirectory = Path.Combine(CacheDirectory, localeDir);

            SaveDirectory = DirectoriesManager.GetFullDirectoryPath(SAVE_DIRECTORY);
            m_savedSearchManager = new SavedSearchManager();
            await m_savedSearchManager.Initialize();

            EnsureFileCopiedFromArchive(Path.Combine(localeDir, EXTERNAL_LINKS_FILE_NAME), Path.Combine(SaveDirectory, EXTERNAL_LINKS_FILE_NAME));
            ExternalLinks = new ItemExternalLinks();
            await ExternalLinks.Initialize();

            // Static items
            m_searchIcon.LoadingMessage = Strings.CornerIconLoadingProgress_StaticItems;

            var staticItemsJsonPath = Path.Combine(LocaleSpecificCacheDirectory, STATIC_ITEMS_FILE_NAME);
            EnsureFileCopiedFromArchive(Path.Combine(localeDir, STATIC_ITEMS_FILE_NAME), staticItemsJsonPath);

            await StaticItemInfo.Initialize(staticItemsJsonPath, Gw2ApiManager.Gw2ApiClient);

            // Player items
            m_searchIcon.LoadingMessage = Strings.CornerIconLoadingProgress_PlayerItems;

            m_searchEngine = await ItemIndex.NewAsync(Gw2ApiManager.Gw2ApiClient, Gw2ApiManager.Permissions);

            // Render cache
            var renderCachePath = Path.Combine(CacheDirectory, RENDER_CACHE_FILE_NAME);
            Directory.CreateDirectory(renderCachePath);
            var renderConnection = new Gw2Sharp.Connection();
            switch (GlobalSettings.RenderCacheMethod.Value)
            {
                case RenderCacheMethod.Memory:
                    renderConnection.RenderCacheMethod = new Gw2Sharp.WebApi.Caching.MemoryCacheMethod();
                    break;
                case RenderCacheMethod.File:
                    renderConnection.RenderCacheMethod = new FileCacheMethod(renderCachePath);
                    break;
                case RenderCacheMethod.None:
                    renderConnection.RenderCacheMethod = new Gw2Sharp.WebApi.Caching.NullCacheMethod();
                    break;
            }
            m_gw2sharpClientForRender = new Gw2Sharp.Gw2Client(renderConnection);
            RenderClient = m_gw2sharpClientForRender.WebApi.Render;

            // Controls
            m_searchWindow = new ItemSearchWindow(ContentsManager, m_searchEngine, m_savedSearchManager);
            m_searchWindow.Hide();
            m_searchIcon.Click += delegate { m_searchWindow.ToggleWindow(); };
            m_searchIcon.LoadingMessage = null;

            GlobalSettings.SearchHotkey.Value.Activated += delegate { m_searchWindow.ToggleWindow(); };

            Logger.Info($"LoadAsync: {stopwatch.ElapsedMilliseconds}");
        }

        private void EnsureFileCopiedFromArchive(string archivePath, string extractedPath)
        {
            if (!File.Exists(extractedPath))
            {
                Logger.Info($"{extractedPath} not found. Restoring cache from resources");
                Directory.CreateDirectory(Path.GetDirectoryName(extractedPath));
                using (var inStream = ContentsManager.GetFileStream(archivePath))
                {
                    using (var outStream = File.OpenWrite(extractedPath))
                    {
                        inStream.CopyTo(outStream);
                    }
                }
            }
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

            if (m_gw2sharpClientForRender != null)
            {
                m_gw2sharpClientForRender.Dispose();
                m_gw2sharpClientForRender = null;
            }
            
            Instance = null;
        }

        public void AddSavedSearch(SavedSearch savedSearch)
        {
            m_savedSearchManager.AddSavedSearch(savedSearch);
        }

        public void RemoveSavedSearch(SavedSearch savedSearch)
        {
            m_savedSearchManager.RemoveSavedSearch(savedSearch);
        }

        private void EnsureCacheVersion()
        {
            bool isCacheVersionMismatch = true;
            var cacheVerFilePath = Path.Combine(CacheDirectory, CACHE_VERSION_FILE_NAME);
            try
            {
                if (File.Exists(cacheVerFilePath))
                {
                    var moduleVer = this.Version;
                    var cacheVer = JsonConvert.DeserializeObject<ModuleVersion>(File.ReadAllText(cacheVerFilePath));
                    isCacheVersionMismatch = cacheVer.Major != moduleVer.Major || cacheVer.Minor < moduleVer.Minor;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to compute cache version");
            }

            if (isCacheVersionMismatch)
            {
                Logger.Info("Cache version mismatch. Clearing cache.");
                try
                {
                    Directory.Delete(CacheDirectory, true);
                    Directory.CreateDirectory(CacheDirectory);

                    var currentVer = new ModuleVersion(this.Version);
                    File.WriteAllText(cacheVerFilePath, JsonConvert.SerializeObject(currentVer));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to clear cache.");
                }
            }
        }
    }
}
