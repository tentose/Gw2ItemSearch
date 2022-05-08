using Blish_HUD.Input;
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
        public SettingEntry<bool> HideEquippedBags { get; private set; }
        public SettingEntry<bool> AutoFocusSearchField { get; private set; }
        public SettingEntry<StackGrouping> DefaultStackGrouping { get; private set; }
        public SettingEntry<CornerIconPosition> CornerIconPosition { get; private set; }
        public SettingEntry<KeyBinding> SearchHotkey { get; private set; }

        public SearchGlobalSettings(SettingCollection settings)
        {
            SearchHotkey = settings.DefineSetting(
                                "SearchHotkey",
                                new KeyBinding(),
                                () => Strings.Settings_SearchHotkey_Name,
                                () => Strings.Settings_SearchHotkey_Description);
            SearchHotkey.Value.BlockSequenceFromGw2 = true;
            SearchHotkey.Value.Enabled = true;

            CornerIconPosition = settings.DefineSetting("CornerIconPosition",
                    ItemSearch.CornerIconPosition.Normal,
                    () => Strings.Settings_CornerIconPosition_Name,
                    () => Strings.Settings_CornerIconPosition_Description);

            DefaultStackGrouping = settings.DefineSetting("DefaultStackGrouping",
                                StackGrouping.ByLocation,
                                () => Strings.Settings_DefaultStackGrouping_Name,
                                () => Strings.Settings_DefaultStackGrouping_Description);

            AutoFocusSearchField = settings.DefineSetting("AutoFocusSearchField",
                                false,
                                () => Strings.Settings_AutoFocusSearchField_Name,
                                () => Strings.Settings_AutoFocusSearchField_Description);

            HideLegendaryArmory = settings.DefineSetting("HideLegendaryArmory",
                                            false,
                                            () => Strings.Settings_HideLegendaryArmory_Name,
                                            () => Strings.Settings_HideLegendaryArmory_Description);

            HideEquippedBags = settings.DefineSetting("HideEquippedBags",
                                            false,
                                            () => Strings.Settings_HideEquippedBags_Name,
                                            () => Strings.Settings_HideEquippedBags_Description);

            PlayerDataRefreshIntervalMinutes = settings.DefineSetting("PlayerDataRefreshIntervalMinutes", 
                                                                        10, 
                                                                        () => Strings.Settings_PlayerDataRefreshIntervalMinutes_Name, 
                                                                        () => Strings.Settings_PlayerDataRefreshIntervalMinutes_Description);
            PlayerDataRefreshIntervalMinutes.SetRange(3, 33);

            RenderCacheMethod = settings.DefineSetting("RenderCacheMethod",
                                                        ItemSearch.RenderCacheMethod.File,
                                                        () => Strings.Settings_RenderCacheMethod_Name,
                                                        () => Strings.Settings_RenderCacheMethod_Description);

        }
    }
}
