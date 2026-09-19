using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TraderSearch.Patches
{
    internal class TraderSearchHandbookFilterPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(HandbookFilterPanel), nameof(HandbookFilterPanel.GetFilteredItems));
        }

        [PatchPostfix]
        private static void Postfix(HandbookFilterPanel __instance, ref IEnumerable<Item> __result)
        {
            if (!TraderSearchController.IsSearchFilterActive(__instance))
            {
                return;
            }
            __result = __result.Where(TraderSearchController.Matches).ToList();
        }
    }
}
