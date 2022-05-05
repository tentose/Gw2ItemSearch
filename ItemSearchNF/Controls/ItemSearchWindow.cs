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

        private ContentsManager m_contentsManager;
        private ItemSearchResultPanel m_resultPanel;
        private SavedSearchManager m_savedSearchManager;
        private TextBox m_searchQueryBox;
        private IconButon m_searchFilterToggleButton;
        private ItemIndex m_searchEngine;
        private Texture2D m_defaultTabIcon;
        private bool m_initialized = false;

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
                Tabs.Add(new Tab(savedSearch.TabIconUrl != null ? RenderTextureHelper.GetAsyncTexture2DForRenderUrl(savedSearch.TabIconUrl, m_defaultTabIcon) : new AsyncTexture2D(m_defaultTabIcon),
                                () => new ItemSearchView(m_searchEngine, savedSearch), savedSearch.Query));
            }

            m_savedSearchManager.SavedSearchList.CollectionChanged +=

            m_initialized = true;
        }

        protected override Point HandleWindowResize(Point newSize)
        {
            return new Point(MathHelper.Clamp(newSize.X, MIN_WIDTH, MAX_WIDTH),
                        MathHelper.Clamp(newSize.Y, MIN_HEIGHT, MAX_HEIGHT));
        }
    }
}
