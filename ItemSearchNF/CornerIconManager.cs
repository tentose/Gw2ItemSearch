using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using ItemSearch.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch
{
    public enum CornerIconPosition
    {
        Normal,
        BelowInventory,
        Off,
    }

    public class CornerIconManager : IDisposable
    {
        private string m_loadingMessage;
        public string LoadingMessage
        {
            get => m_loadingMessage;
            set
            {
                lock (m_lock)
                {
                    switch (m_currentPosition)
                    {
                        case CornerIconPosition.Normal: m_normalCornerIcon.LoadingMessage = value; break;
                        case CornerIconPosition.BelowInventory: m_customCornerIcon.LoadingMessage = value; break;
                    }
                }
                m_loadingMessage = value;
            }
        }

        private AsyncTexture2D m_icon;
        public AsyncTexture2D Icon
        {
            get => m_icon;
            set
            {
                lock (m_lock)
                {
                    switch (m_currentPosition)
                    {
                        case CornerIconPosition.Normal: m_normalCornerIcon.Icon = value; break;
                        case CornerIconPosition.BelowInventory: m_customCornerIcon.Icon = value; break;
                    }
                }
                m_icon = value;
            }
        }

        private AsyncTexture2D m_hoverIcon;
        public AsyncTexture2D HoverIcon
        {
            get => m_hoverIcon;
            set
            {
                lock (m_lock)
                {
                    switch (m_currentPosition)
                    {
                        case CornerIconPosition.Normal: m_normalCornerIcon.HoverIcon = value; break;
                        case CornerIconPosition.BelowInventory: m_customCornerIcon.HoverIcon = value; break;
                    }
                }
                m_hoverIcon = value;
            }
        }

        private string m_iconName;
        public string IconName
        {
            get => m_iconName;
            set
            {
                lock (m_lock)
                {
                    switch (m_currentPosition)
                    {
                        case CornerIconPosition.Normal: m_normalCornerIcon.IconName = value; break;
                        case CornerIconPosition.BelowInventory: m_customCornerIcon.IconName = value; break;
                    }
                }
                m_iconName = value;
            }
        }

        private EventHandler<MouseEventArgs> m_clickHandlers;
        public event EventHandler<MouseEventArgs> Click
        {
            add
            {
                lock (m_lock)
                {
                    switch (m_currentPosition)
                    {
                        case CornerIconPosition.Normal: m_normalCornerIcon.Click += value; break;
                        case CornerIconPosition.BelowInventory: m_customCornerIcon.Click += value; break;
                    }
                }
                m_clickHandlers += value;
            }
            remove
            {
                lock (m_lock)
                {
                    switch (m_currentPosition)
                    {
                        case CornerIconPosition.Normal: m_normalCornerIcon.Click -= value; break;
                        case CornerIconPosition.BelowInventory: m_customCornerIcon.Click -= value; break;
                    }
                }
                m_clickHandlers -= value;
            }
        }

        private CornerIconPosition m_currentPosition;
        private CornerIcon m_normalCornerIcon;
        private CustomCornerIcon m_customCornerIcon;

        private object m_lock = new object();
        private bool m_disposedValue;

        public CornerIconManager()
        {
            var positionSetting = ItemSearchModule.Instance.GlobalSettings.CornerIconPosition;
            m_currentPosition = positionSetting.Value;
            positionSetting.SettingChanged += PositionSetting_SettingChanged;
            UpdateIconPosition();
        }

        private void PositionSetting_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<CornerIconPosition> e)
        {
            if (e.NewValue != m_currentPosition)
            {
                lock (m_lock)
                {
                    m_currentPosition = e.NewValue;
                    UpdateIconPosition();
                }
            }
        }

        private void UpdateIconPosition()
        {
            if (m_normalCornerIcon != null)
            {
                m_normalCornerIcon.Dispose();
                m_normalCornerIcon = null;
            }

            if (m_customCornerIcon != null)
            {
                m_customCornerIcon.Dispose();
                m_customCornerIcon = null;
            }

            if (m_currentPosition == CornerIconPosition.Normal)
            {
                m_normalCornerIcon = new CornerIcon()
                {
                    IconName = this.IconName,
                    Icon = this.Icon,
                    HoverIcon = this.HoverIcon,
                    Priority = 5,
                    LoadingMessage = this.LoadingMessage,
                };
                m_normalCornerIcon.Click += m_clickHandlers;
            }
            else if (m_currentPosition == CornerIconPosition.BelowInventory)
            {
                m_customCornerIcon = new CustomCornerIcon()
                {
                    IconName = this.IconName,
                    Icon = this.Icon,
                    HoverIcon = this.HoverIcon,
                    LoadingMessage = this.LoadingMessage,
                };
                m_customCornerIcon.Click += m_clickHandlers;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposedValue)
            {
                if (disposing)
                {
                    if (m_normalCornerIcon != null)
                    {
                        m_normalCornerIcon.Dispose();
                        m_normalCornerIcon = null;
                    }

                    if (m_customCornerIcon != null)
                    {
                        m_customCornerIcon.Dispose();
                        m_customCornerIcon = null;
                    }
                }

                m_disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
