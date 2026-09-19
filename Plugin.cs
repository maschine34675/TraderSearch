using System;
using BepInEx;
using BepInEx.Logging;
using SPT.Reflection.Patching;
using TraderSearch.Patches;

namespace TraderSearch
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.maschine.TraderSearch";
        public const string PluginName = "maschine-TraderSearch";
        public const string PluginVersion = "2.1.0";

        public static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            Register(() => new TraderSearchDealScreenShowPatch(), "TraderDealScreen.Show", "the search field will not appear in the trader window");
            Register(() => new TraderSearchHandbookFilterPatch(), "HandbookFilterPanel.GetFilteredItems", "typing a search will not filter the trader grid");
            Register(() => new TraderSearchDealScreenUpdatePatch(), "TraderDealScreen.Update", "pressing SPACE while typing could buy the selected item");
            Register(() => new TraderSearchScreensGroupTranslateCommandPatch(), "TraderScreensGroup.TranslateCommand", "ESC and other game commands will fire while typing in the trader window");
            Register(() => new TraderSearchDealScreenFullClosePatch(), "TraderDealScreen.FullClose", "a stale search text may persist after closing the trader screen");
            Register(() => new TraderSearchAddOfferShowPatch(), "AddOfferWindow.Show", "the search field will not appear in the flea market's Add Offer window");
            Register(() => new TraderSearchAddOfferDeselectPatch(), "RagfairNewOfferContext.DeselectItem", "changing the search text will clear the items selected for the offer");
            Register(() => new TraderSearchAddOfferTranslateCommandPatch(), "Window.TranslateCommand", "ESC will close the Add Offer window instead of clearing the search field");
            Register(() => new TraderSearchAddOfferSubmitPatch(), "AddOfferWindow.AddOffer", "keys bound to posting an offer by other mods may post it while typing");
            Register(() => new TraderSearchSuppressAlphanumericPatch(), "InputKey.Update", "letter/number keybinds will fire while typing in a search field");

            Log.LogInfo($"{PluginName} v{PluginVersion} loaded.");
        }

        private static void Register(Func<ModulePatch> create, string target, string consequence)
        {
            try
            {
                create().Enable();
                Log.LogDebug(target + " patch registered successfully.");
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to register " + target + " patch - " + consequence + ": " + ex);
            }
        }
    }
}
