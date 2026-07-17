using System.Reflection;
using EFT.InputSystem;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TraderSearch.Patches
{
    internal class TraderScreensGroupTranslateCommandPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(TraderScreensGroup), nameof(TraderScreensGroup.TranslateCommand));
        }

        [PatchPrefix]
        private static bool Prefix(ECommand command, ref InputNode.ETranslateResult __result)
        {
            TraderSearchController controller = TraderSearchController.Current;
            if (controller == null)
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
