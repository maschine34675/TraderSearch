using System.Reflection;
using EFT.UI.Ragfair;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace TraderSearch.Patches
{
    internal class TraderSearchAddOfferSubmitPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(AddOfferWindow), nameof(AddOfferWindow.AddOffer));
        }

        [PatchPrefix]
        private static bool Prefix(AddOfferWindow __instance)
        {
            AddOfferSearchController controller = AddOfferSearchController.Current;
            if (controller == null || controller.gameObject != __instance.gameObject)
            {
                return true;
            }
            if (controller.IsInputFocused)
            {
                return false;
            }
            bool submitKeyDown = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space);
            return !(controller.WasFocusedRecently && submitKeyDown);
        }
    }
}
