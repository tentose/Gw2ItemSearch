using Blish_HUD;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch
{
    public class ExternalLink
    {
        public string Url { get; set; }
        public string Name { get; set; }
    }

    public class ItemExternalLinks
    {
        private const string EXTERNAL_LINKS_FILE_NAME = "external_links.json";

        private static readonly Logger Logger = Logger.GetLogger<ItemSearchModule>();

        private List<ExternalLink> _externalLinks = new List<ExternalLink>();

        public ItemExternalLinks()
        {
        }

        public async Task Initialize()
        {
            Logger.Info($"Initializing external links");

            string filePath = Path.Combine(ItemSearchModule.Instance.SaveDirectory, EXTERNAL_LINKS_FILE_NAME);
            try
            {
                if (!File.Exists(filePath))
                {
                    return;
                }

                List<ExternalLink> links = null;
                await Task.Run(() =>
                {
                    links = JsonConvert.DeserializeObject<List<ExternalLink>>(File.ReadAllText(filePath));
                });

                if (links != null)
                {
                    _externalLinks = links;
                }
            }
            catch (Exception e)
            {
                Logger.Warn(e, $"Failed to initialize external links: {PathHelper.StripPII(filePath)}");
            }
        }

        public IEnumerable<ExternalLink> GetForItem(int id, StaticItemInfo item)
        {
            List<ExternalLink> linksForItem = new List<ExternalLink>();
            foreach (var link in _externalLinks)
            {
                ExternalLink linkForItem = new ExternalLink()
                {
                    Name = link.Name,
                };

                linkForItem.Url = link.Url.Replace("@@name@@", item.Name)
                                          .Replace("@@itemid@@", id.ToString());
                linksForItem.Add(linkForItem);
            }
            return linksForItem;
        }
    }
}
