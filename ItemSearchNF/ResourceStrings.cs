using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch
{
    public class ResourceStrings
    {
        private static System.Resources.ResourceManager s_resourceMan;
        internal static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (ReferenceEquals(s_resourceMan, null))
                {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager("ItemSearch.Strings", typeof(Strings).Assembly);
                    s_resourceMan = temp;
                }
                return s_resourceMan;
            }
        }

        public static string Get(string resourceName)
        {
            return ResourceManager.GetString(resourceName);
        }
    }
}
