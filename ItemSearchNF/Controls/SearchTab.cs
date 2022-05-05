using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch.Controls
{
    public class SearchTab : Tab
    {
        public static readonly Color DullColor = Color.FromNonPremultiplied(150, 150, 150, 255);

        public bool UseCustomIconSize { get; set; } = false;
        public int IconWidth { get; set; }
        public int IconHeight { get; set; }

        public SearchTab(AsyncTexture2D icon, Func<IView> view, string name = null, int? priority = null) : base(icon, view, name, priority)
        {
        }

        public void DrawSearchTab(Control tabbedControl, SpriteBatch spriteBatch, Rectangle bounds, bool selected, bool hovered)
        {
            if (!this.Icon.HasTexture) return;

            // TODO: If not enabled, draw darker to indicate it is disabled

            var width = UseCustomIconSize ? IconWidth : this.Icon.Texture.Width;
            var height = UseCustomIconSize ? IconHeight : this.Icon.Texture.Height;

            spriteBatch.DrawOnCtrl(tabbedControl,
                                   Icon,
                                   new Rectangle(bounds.Right - bounds.Width / 2 - width / 2,
                                                 bounds.Bottom - bounds.Height / 2 - height / 2,
                                                 width,
                                                 height),
                                   selected || hovered
                                        ? Color.White
                                        : DullColor);

            //spriteBatch.DrawOnCtrl()
        }
    }
}
