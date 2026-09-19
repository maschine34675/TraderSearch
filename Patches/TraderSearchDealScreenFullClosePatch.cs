using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TraderSearch.Patches
{
    internal class TraderSearchDealScreenFullClosePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(TraderDealScreen), nameof(TraderDealScreen.FullClose));
        }

        [PatchPostfix]
        private static void Postfix(TraderDealScreen __instance)
        {
            TraderSearchController controller = __instance.GetComponent<TraderSearchController>();
            if (controller != null)
            {
                controller.OnScreenFullClose();
            }
        }
    }
}
