using System;
using System.Reflection;
using EFT.HandBook;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using EFT.UI.Ragfair;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TraderSearch.Patches
{
    internal class TraderSearchAddOfferShowPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(AddOfferWindow), nameof(AddOfferWindow.Show), new[]
            {
                typeof(InventoryController),
                typeof(CompoundItem[]),
                typeof(RagFair),
                typeof(Handbook),
                typeof(ItemUiContext),
                typeof(WeaponPreviewPool),
                typeof(Item)
            });
        }

        [PatchPostfix]
        private static void Postfix(AddOfferWindow __instance, NewOfferItemContext ____itemContext, InventoryController ____inventoryController, ItemUiContext ____itemUiContext)
        {
            try
            {
                AddOfferSearchController controller = __instance.GetComponent<AddOfferSearchController>();
                if (controller == null)
                {
                    controller = __instance.gameObject.AddComponent<AddOfferSearchController>();
                }
                controller.OnAddOfferWindowShown(__instance, ____itemContext, ____inventoryController, ____itemUiContext);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("TraderSearch: failed to set up the search field in AddOfferWindow.Show: " + ex);
            }
        }
    }
}
