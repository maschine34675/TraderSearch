using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TraderSearch.Patches
{
    internal class TraderSearchDealScreenUpdatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(TraderDealScreen), nameof(TraderDealScreen.Update));
        }

        [PatchPrefix]
        private static bool Prefix()
        {
            TraderSearchController controller = TraderSearchController.Current;
            return controller == null || !controller.IsInputFocused;
        }
    }
}
