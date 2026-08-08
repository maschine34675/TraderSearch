using System.Reflection;
using EFT.InputSystem;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace TraderSearch.Patches
{
    internal class SuppressAlphanumericCommandsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(InputKey), nameof(InputKey.Update));
        }

        [PatchPrefix]
        private static bool Prefix(InputKey __instance)
        {
            if (__instance.Key < KeyCode.Alpha0 || __instance.Key > KeyCode.Tilde)
            {
                return true;
            }
            TraderSearchController controller = TraderSearchController.Current;
            if (controller == null || !controller.IsInputFocused)
            {
                return true;
            }
            __instance.Press = InputManager.UpdateInputMatrix[0, (int)__instance.Press];
            return false;
        }
    }
}
