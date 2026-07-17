using System;
using BepInEx;
using BepInEx.Logging;
using TraderSearch.Patches;

namespace TraderSearch
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.maschine.TraderSearch";
        public const string PluginName = "maschine-TraderSearch";
        public const string PluginVersion = "1.0.0";

        public static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;

            try
            {
                new TraderDealScreenShowPatch().Enable();
                Log.LogDebug("TraderDealScreen.Show patch registered successfully.");
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to register TraderDealScreen.Show patch - the search field will not appear: " + ex);
            }

            try
            {
                new HandbookFilterPatch().Enable();
                Log.LogDebug("HandbookFilterPanel.GetFilteredItems patch registered successfully.");
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to register HandbookFilterPanel.GetFilteredItems patch - typing a search will not filter the trader grid: " + ex);
            }

            try
            {
                new TraderDealScreenUpdatePatch().Enable();
                Log.LogDebug("TraderDealScreen.Update patch registered successfully.");
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to register TraderDealScreen.Update patch - pressing SPACE while typing could buy the selected item: " + ex);
            }

            try
            {
                new TraderScreensGroupTranslateCommandPatch().Enable();
                Log.LogDebug("TraderScreensGroup.TranslateCommand patch registered successfully.");
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to register TraderScreensGroup.TranslateCommand patch - ESC and other game commands will fire while typing: " + ex);
            }

            try
            {
                new SuppressAlphanumericCommandsPatch().Enable();
                Log.LogDebug("KeyPressState.Update patch registered successfully.");
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to register KeyPressState.Update patch - letter/number keybinds will fire while typing in the search field: " + ex);
            }

            try
            {
                new TraderDealScreenFullClosePatch().Enable();
                Log.LogDebug("TraderDealScreen.FullClose patch registered successfully.");
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to register TraderDealScreen.FullClose patch - a stale search text may persist after closing the trader screen: " + ex);
            }

            Log.LogInfo($"{PluginName} v{PluginVersion} loaded.");
        }
    }
}
