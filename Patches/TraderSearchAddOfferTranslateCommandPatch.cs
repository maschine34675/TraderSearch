using System.Reflection;
using EFT.InputSystem;
using EFT.UI;
using EFT.UI.Ragfair;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TraderSearch.Patches
{
    internal class TraderSearchAddOfferTranslateCommandPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(Window<DialogWindowContext>), nameof(Window<DialogWindowContext>.TranslateCommand));
        }

        [PatchPrefix]
        private static bool Prefix(object __instance, ECommand command, ref InputNode.ETranslateResult __result)
        {
            if (!(__instance is AddOfferWindow window))
            {
                return true;
            }
            AddOfferSearchController controller = AddOfferSearchController.Current;
            if (controller == null || controller.gameObject != window.gameObject)
            {
                return true;
            }

            if (command.IsCommand(ECommand.Escape))
            {
                if (!controller.IsInputFocused && !controller.WasFocusedRecently)
                {
                    return true;
                }
                controller.ClearSearchAndDefocus();
                __result = InputNode.ETranslateResult.Block;
                return false;
            }

            if (!controller.IsInputFocused)
            {
                return true;
            }
            __result = InputNode.ETranslateResult.BlockAll;
            return false;
        }
    }
}
