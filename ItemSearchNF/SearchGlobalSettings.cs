using Blish_HUD.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch
{
    public class SearchGlobalSettings
    {
        public SettingEntry<int> PlayerDataRefreshIntervalMinutes { get; private set; }

        public SearchGlobalSettings(SettingCollection settings)
        {
            PlayerDataRefreshIntervalMinutes = settings.DefineSetting("PlayerDataRefreshIntervalMinutes", 
                                                                        10, 
                                                                        () => Strings.Settings_PlayerDataRefreshIntervalMinutes_Name, 
                                                                        () => Strings.Settings_PlayerDataRefreshIntervalMinutes_Description);
            PlayerDataRefreshIntervalMinutes.SetRange(3, 30);
        }
    }
}
