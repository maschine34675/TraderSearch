using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using EFT.Quests;
using EFT.Trading;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TraderSearch.Patches
{
    internal class TraderSearchDealScreenShowPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(TraderDealScreen), nameof(TraderDealScreen.Show), new[]
            {
                typeof(Trader),
                typeof(Profile),
                typeof(InventoryController),
                typeof(ETradeMode),
                typeof(ItemUiContext),
                typeof(QuestController),
                typeof(IEnumerable<Trader>)
            });
        }

        [PatchPostfix]
        private static void Postfix(TraderDealScreen __instance, TradingGridView ____traderGridView, DefaultUIButton ____updateAssort, FilterTab ____allItemsTab)
        {
            try
            {
                TraderSearchController controller = __instance.GetComponent<TraderSearchController>();
                if (controller == null)
                {
                    controller = __instance.gameObject.AddComponent<TraderSearchController>();
                }
                controller.OnTraderScreenShown(____traderGridView, ____updateAssort, ____allItemsTab);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("TraderSearch: failed to set up the search field in TraderDealScreen.Show: " + ex);
            }
        }
    }
}
