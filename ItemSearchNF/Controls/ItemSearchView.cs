﻿using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ItemSearch.Controls
{
    internal class ItemSearchView : View
    {
        private const int CONTENT_X_MARGIN = 10;
        private const int CONTENT_Y_PADDING = 5;
        private const int SEARCH_DEBOUNCE_MILLIS = 500;

        private ItemSearchResultPanel m_resultPanel;
        private TextBox m_searchQueryBox;
        private ItemIndex m_searchEngine;
        private Timer m_searchDebounceTimer;

        public ItemSearchView(ItemIndex searchEngine)
        {
            m_searchEngine = searchEngine;
        }

        protected override void Build(Container buildPanel)
        {
            m_searchQueryBox = new TextBox()
            {
                Parent = buildPanel,
                Location = new Point(0, 0),
                Size = new Point(358, 43),
                PlaceholderText = Strings.SearchWindow_SearchPlaceholder,
                Font = GameService.Content.DefaultFont16,
            };
            m_searchQueryBox.EnterPressed += M_searchQueryBox_EnterPressed;
            m_searchQueryBox.InputFocusChanged += M_searchQueryBox_InputFocusChanged;
            m_searchQueryBox.TextChanged += M_searchQueryBox_TextChanged;
            m_searchDebounceTimer = new Timer(SEARCH_DEBOUNCE_MILLIS);
            m_searchDebounceTimer.AutoReset = false;
            m_searchDebounceTimer.Elapsed += M_searchDebounceTimer_Elapsed;

            m_resultPanel = new ItemSearchResultPanel()
            {
                Parent = buildPanel,
                Size = new Point(400, 400),
            };

            RepositionObjects();
            buildPanel.Resized += BuildPanel_Resized;
        }

        private void M_searchDebounceTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = PerformSearchQuery(m_searchQueryBox.Text);
        }

        private void M_searchQueryBox_TextChanged(object sender, EventArgs e)
        {
            m_searchDebounceTimer.Stop();
            m_searchDebounceTimer.Start();
        }

        private void BuildPanel_Resized(object sender, ResizedEventArgs e)
        {
            RepositionObjects();
        }

        private void RepositionObjects()
        {
            var parent = m_searchQueryBox.Parent;
            m_searchQueryBox.Size = new Point(parent.ContentRegion.Width - CONTENT_X_MARGIN, m_searchQueryBox.Size.Y);
            m_resultPanel.Location = m_searchQueryBox.Location + new Point(0, m_searchQueryBox.Height + CONTENT_Y_PADDING);
            m_resultPanel.Size = new Point(parent.ContentRegion.Width - CONTENT_X_MARGIN, parent.ContentRegion.Height - CONTENT_Y_PADDING - m_resultPanel.Top);
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
                _ = PerformSearchQuery(query);
            }
        }

        private async Task PerformSearchQuery(string query)
        {
            if (query.Length >= 3)
            {
                var result = await m_searchEngine.Search(query);
                m_resultPanel.SetSearchResult(result);
            }
        }
    }
}
