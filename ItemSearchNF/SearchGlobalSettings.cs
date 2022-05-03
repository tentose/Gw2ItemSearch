using Blish_HUD.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch
{
    public enum RenderCacheMethod
    {
        None,
        Memory,
        File,
    }

    public class SearchGlobalSettings
    {
        public SettingEntry<int> PlayerDataRefreshIntervalMinutes { get; private set; }
        public SettingEntry<RenderCacheMethod> RenderCacheMethod { get; private set; }
        public SettingEntry<bool> HideLegendaryArmory { get; private set; }

        public SearchGlobalSettings(SettingCollection settings)
        {
            PlayerDataRefreshIntervalMinutes = settings.DefineSetting("PlayerDataRefreshIntervalMinutes", 
                                                                        10, 
                                                                        () => Strings.Settings_PlayerDataRefreshIntervalMinutes_Name, 
                                                                        () => Strings.Settings_PlayerDataRefreshIntervalMinutes_Description);
            PlayerDataRefreshIntervalMinutes.SetRange(3, 30);

            RenderCacheMethod = settings.DefineSetting("RenderCacheMethod",
                                                        ItemSearch.RenderCacheMethod.File,
                                                        () => Strings.Settings_RenderCacheMethod_Name,
                                                        () => Strings.Settings_RenderCacheMethod_Description);

            HideLegendaryArmory = settings.DefineSetting("HideLegendaryArmory",
                                                        false,
                                                        () => Strings.Settings_HideLegendaryArmory_Name,
                                                        () => Strings.Settings_HideLegendaryArmory_Description);
        }
    }
}
