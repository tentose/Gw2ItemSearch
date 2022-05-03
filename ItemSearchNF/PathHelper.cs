using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch
{
    public class PathHelper
    {
        private static string s_rootPath = "";

        public static string StripPII(string path)
        {
            if (s_rootPath == "")
            {
                s_rootPath = ItemSearchModule.Instance?.CacheDirectory ?? "";
            }

            if (path.StartsWith(s_rootPath))
            {
                return path.Substring(s_rootPath.Length);
            }
            return Path.GetFileName(path);
        }
    }
}
