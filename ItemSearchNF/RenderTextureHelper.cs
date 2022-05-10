using Blish_HUD;
using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                        var loadedTexture = TextureUtil.FromStreamPremultiplied(textureStream);
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
    }
}
