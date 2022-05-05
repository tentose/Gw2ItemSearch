using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch
{
    public class SavedSearch
    {
        public string Query { get; set; }
        public SearchFilter Filter { get; set; }
        public string TabIconUrl { get; set; }

        public void UpdateSearch()
        {
            OnUpdated();
        }

        public event EventHandler Updated;
        private void OnUpdated()
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }
    }
}
