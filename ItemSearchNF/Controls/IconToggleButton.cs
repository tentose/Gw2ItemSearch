using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Blish_HUD.Graphics;
using Blish_HUD.Controls;
using Blish_HUD;
using System;

namespace ItemSearch.Controls
{
    public class IconToggleButon : Control
    {
        private const float DARKEN_MULTIPLIER = 0.8f;

        private Color m_multiplierWhenNotHovered = Color.White;
        private bool m_darkenUnlessHovered = false;
        public bool DarkenUnlessHovered
        {
            get => m_darkenUnlessHovered;
            set
            {
                m_darkenUnlessHovered = value;
                m_multiplierWhenNotHovered = m_darkenUnlessHovered ? Color.White * DARKEN_MULTIPLIER : Color.White;
            }
        }

        private AsyncTexture2D m_uncheckedIcon;
        public AsyncTexture2D UncheckedIcon
        {
            get => m_uncheckedIcon;
            set => SetProperty(ref m_uncheckedIcon, value);
        }

        private AsyncTexture2D m_checkedIcon;
        public AsyncTexture2D CheckedIcon
        {
            get => m_checkedIcon;
            set => SetProperty(ref m_checkedIcon, value);
        }

        private bool m_isChecked;
        public bool IsChecked
        {
            get => m_isChecked;
            set
            {
                m_isChecked = value;
                OnCheckChanged(m_isChecked);
            }
        }

        public event EventHandler<bool> CheckChanged;
        private void OnCheckChanged(bool isChecked)
        {
            CheckChanged?.Invoke(this, isChecked);
        }

        public IconToggleButon()
        {
        }

        public IconToggleButon(AsyncTexture2D uncheckedIcon, AsyncTexture2D checkedIcon)
        {
            m_uncheckedIcon = uncheckedIcon;
            m_checkedIcon = checkedIcon;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            Content.PlaySoundEffectByName(@"button-click");

            IsChecked = !IsChecked;

            base.OnClick(e);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (m_checkedIcon != null && m_uncheckedIcon != null)
            {
                if (this.MouseOver && this.Enabled)
                {
                    spriteBatch.DrawOnCtrl(this, m_isChecked ? m_checkedIcon : m_uncheckedIcon, bounds);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(this, m_isChecked ? m_checkedIcon : m_uncheckedIcon, bounds, m_multiplierWhenNotHovered);
                }
            }
        }
    }
}
