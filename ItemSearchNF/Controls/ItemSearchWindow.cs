using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using ItemSearch.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch.Controls
{
    internal class ItemSearchWindow : InternalTabbedWindow2
    {
        private static readonly Logger Logger = Logger.GetLogger<ItemSearchModule>();

        private const int MIN_WIDTH = 300;
        private const int MIN_HEIGHT = 300;
        private const int MAX_WIDTH = 1024;
        private const int MAX_HEIGHT = 1024;
        private const int CONTENT_X_MARGIN = 80;
        private const int CONTENT_Y_PADDING = 5;
        private const int TITLE_BAR_SIZE = 40;
        private const int SAVED_SEARCH_TAB_ICON_SIZE = 30;

        private ContentsManager m_contentsManager;
        private SavedSearchManager m_savedSearchManager;
        private ItemIndex m_searchEngine;
        private Texture2D m_defaultTabIcon;

        List<(SavedSearch savedSearch, Tab tab)> m_savedSearchToTab = new List<(SavedSearch savedSearch, Tab tab)>();

        public ItemSearchWindow(ContentsManager contentManager, ItemIndex searchEngine, SavedSearchManager savedSearchManager) : base(contentManager.GetTexture("Textures/155985.png"), new Rectangle(0, 0, 600, 600), new Thickness(20, 0, 20, 55))
        {
            m_contentsManager = contentManager;
            m_searchEngine = searchEngine;

            Parent = GameService.Graphics.SpriteScreen;
            Title = Strings.SearchWindow_Title;
            Emblem = m_contentsManager.GetTexture("Textures/WindowIcon.png");
            Subtitle = Strings.SearchWindow_MainSubtitle;
            SavesPosition = true;
            Id = $"{nameof(ItemSearchWindow)}_{nameof(ItemSearchModule)}_5f05a7af-8a00-45d4-87c2-511cddb418fc";
            CanResize = true;

            m_contentsManager = contentManager;
            m_savedSearchManager = savedSearchManager;

            m_defaultTabIcon = m_contentsManager.GetTexture("Textures/SearchTabIcon.png");
            Tabs.Add(new Tab(m_defaultTabIcon, () => new ItemSearchView(m_searchEngine), Strings.SearchWindow_DefaultTabName));

            foreach(var savedSearch in m_savedSearchManager.SavedSearchList)
            {
                AddTabForSavedSearch(savedSearch);
            }
            m_savedSearchManager.SavedSearchList.ItemAdded += SavedSearchList_ItemAdded;
            m_savedSearchManager.SavedSearchList.ItemRemoved += SavedSearchList_ItemRemoved;
        }

        private void SavedSearchList_ItemRemoved(object sender, MonoGame.Extended.Collections.ItemEventArgs<SavedSearch> e)
        {
            bool found = false;
            for (int i = 0; i < m_savedSearchToTab.Count; i++)
            {
                if (e.Item == m_savedSearchToTab[i].savedSearch)
                {
                    found = true;
                    var pair = m_savedSearchToTab[i];
                    m_savedSearchToTab.RemoveAt(i);
                    Tabs.Remove(pair.tab);
                    break;
                }
            }

            SelectedTab = Tabs.ElementAt(0);

            if (!found)
            {
                Logger.Warn("Saved search removed but failed to find corresponding tab to remove");
            }
        }

        private void SavedSearchList_ItemAdded(object sender, MonoGame.Extended.Collections.ItemEventArgs<SavedSearch> e)
        {
            SelectedTab = AddTabForSavedSearch(e.Item);
        }

        private Tab AddTabForSavedSearch(SavedSearch savedSearch)
        {
            var tabIcon = savedSearch.TabIconUrl != null ? RenderTextureHelper.GetAsyncTexture2DForRenderUrl(savedSearch.TabIconUrl, m_defaultTabIcon) : new AsyncTexture2D(m_defaultTabIcon);
            (SavedSearch savedSearch, Tab tab) pair = (savedSearch, new SearchTab(tabIcon, () => new ItemSearchView(m_searchEngine, savedSearch))
            {
                Name = savedSearch.Query,
                UseCustomIconSize = true,
                IconWidth = SAVED_SEARCH_TAB_ICON_SIZE,
                IconHeight = SAVED_SEARCH_TAB_ICON_SIZE,
            });
            m_savedSearchToTab.Add(pair);
            Tabs.Add(pair.tab);
            savedSearch.Updated += SavedSearch_Updated;

            return pair.tab;
        }

        private void SavedSearch_Updated(object sender, EventArgs e)
        {
            SavedSearch savedSearch = sender as SavedSearch;
            if (savedSearch != null)
            {
                foreach (var pair in m_savedSearchToTab)
                {
                    if (pair.savedSearch == savedSearch)
                    {
                        pair.tab.Icon = RenderTextureHelper.GetAsyncTexture2DForRenderUrl(savedSearch.TabIconUrl, m_defaultTabIcon);
                        break;
                    }
                }
            }
        }

        protected override Point HandleWindowResize(Point newSize)
        {
            return new Point(MathHelper.Clamp(newSize.X, MIN_WIDTH, MAX_WIDTH),
                        MathHelper.Clamp(newSize.Y, MIN_HEIGHT, MAX_HEIGHT));
        }
    }
}
