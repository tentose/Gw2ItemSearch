using Blish_HUD;
using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch.Controls
{
    public class RenderTextureHelper
    {
        private static readonly Logger Logger = Logger.GetLogger<ItemSearchModule>();

        public static AsyncTexture2D GetAsyncTexture2DForRenderUrl(string url, Texture2D defaultTexture)
        {
            AsyncTexture2D asyncTexture = defaultTexture;

            Task.Run(async () =>
            {
                try
                {
                    var imageBytes = await ItemSearchModule.Instance.RenderClient.DownloadToByteArrayAsync(url);
                    using (var textureStream = new MemoryStream(imageBytes))
                    {
                        var loadedTexture = FromStreamPremultipliedShim(textureStream);
                        asyncTexture.SwapTexture(loadedTexture);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"Failed loading item image {url}");
                }
            });

            return asyncTexture;
        }

        private static bool m_useWorkaround = false;
        public static Texture2D FromStreamPremultipliedShim(Stream stream)
        {
            // WORKAROUND: BH <0.11.8 and older builds of 0.11.8
            Texture2D value = null;
            if (!m_useWorkaround)
            {
                try
                {
                    value = FromStreamPremultiplied(stream);
                }
                catch (MissingMethodException ex)
                {
                    // Happens in older versions of BH (<0.11.8 and older builds of 0.11.8).
                    m_useWorkaround = true;
                }
            }

            if (m_useWorkaround)
            {
                // GameService.Graphics.GraphicsDevice is obsolete with isError = true. Access it
                // through reflection instead.
                var graphicsService = GameService.Graphics;
                GraphicsDevice device = (GraphicsDevice)graphicsService.GetType().GetProperty("GraphicsDevice").GetValue(graphicsService);
                value = TextureUtil.FromStreamPremultiplied(device, stream);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Texture2D FromStreamPremultiplied(Stream stream)
        {
            return TextureUtil.FromStreamPremultiplied(stream);
        }
    }
}
