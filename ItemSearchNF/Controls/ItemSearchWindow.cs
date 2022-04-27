using Blish_HUD;
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
        private const int CONTENT_X_MARGIN = 80;
        private const int CONTENT_Y_PADDING = 5;
        private const int TITLE_BAR_SIZE = 40;

        private ContentsManager m_contentsManager;
        private ItemSearchResultPanel m_resultPanel;
        private TextBox m_searchQueryBox;
        private ItemIndex m_searchEngine;
        private bool m_initialized = false;

        public ItemSearchWindow(ContentsManager contentManager, ItemIndex searchEngine) : base(contentManager.GetTexture("Textures/155985.png"), new Rectangle(0, 0, 600, 600), new Thickness(20, 0, 20, 55))
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

            Tabs.Add(new Tab(m_contentsManager.GetTexture("Textures/placeholder.png"), () => new ItemSearchView(m_searchEngine), "Search all items"));

            m_initialized = true;
        }

        private void M_searchQueryBox_InputFocusChanged(object sender, ValueEventArgs<bool> e)
        {
            // Select all text if the user is focusing on the text box to make it easier to type a new query
            if (m_searchQueryBox.Focused && m_searchQueryBox.Text.Length > 0)
            {
                // Textbox handles cursor placement AFTER input focus changes. If we make the selection right
                // away, the text box will just overwrite it with cursor placement logic. Do the selection
                // after a small delay.
                Task.Run(async () =>
                {
                    await Task.Delay(100);
                    m_searchQueryBox.SelectionStart = 0;
                    m_searchQueryBox.SelectionEnd = m_searchQueryBox.Text.Length;
                    m_searchQueryBox.RecalculateLayout();
                });
            }
        }

        private void M_searchQueryBox_EnterPressed(object sender, EventArgs e)
        {
            string query = m_searchQueryBox.Text;
            if (query.Length >= 3)
            {
                PerformSearchQuery(query);
            }
        }

        private async Task PerformSearchQuery(string query)
        {
            var result = await m_searchEngine.Search(query);
            m_resultPanel.SetSearchResult(result);
        }

        protected override Point HandleWindowResize(Point newSize)
        {
            return new Point(MathHelper.Clamp(newSize.X, MIN_WIDTH, 850),
                        MathHelper.Clamp(newSize.Y, MIN_HEIGHT, 1024));
        }
    }
}
