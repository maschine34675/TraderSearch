using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TraderSearch.Patches
{
    internal class TraderSearchAddOfferDeselectPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(RagfairNewOfferContext), nameof(RagfairNewOfferContext.DeselectItem));
        }

        [PatchPrefix]
        private static bool Prefix()
        {
            return !AddOfferSearchController.SuppressDeselect;
        }
    }
}
