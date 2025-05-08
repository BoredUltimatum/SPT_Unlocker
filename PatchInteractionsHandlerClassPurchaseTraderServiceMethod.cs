using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SPT_Unlocker
{
    internal class PatchInteractionsHandlerClassPurchaseTraderServiceMethod : ModulePatch // all patches must inherit ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            //return typeof(InteractionsHandlerClass).GetMethod("PurchaseTraderService", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //ARGS: BackendConfigSettingsClass.ServiceData serviceData, string subServiceId, AbstractQuestControllerClass questController, InventoryController inventoryController, bool simulate
            return AccessTools.Method(
                typeof(InteractionsHandlerClass),
                nameof(InteractionsHandlerClass.PurchaseTraderService),
                new Type[] {
                    typeof(BackendConfigSettingsClass.ServiceData),
                    typeof(string),
                    typeof(AbstractQuestControllerClass),
                    typeof(InventoryController),
                    typeof(bool)
                });
        }

        [PatchPrefix]
        private static void Prefix(BackendConfigSettingsClass.ServiceData serviceData, string subServiceId, AbstractQuestControllerClass questController, InventoryController inventoryController, bool simulate)
        {
            Plugin.LogSource.LogInfo("PatchInteractionsHandlerClassPurchaseTraderServiceMethod.Prefix()");
        }

        [PatchPostfix]
        private static void Postfix(ref GStruct455<GClass3319> __result)
        {
            Plugin.LogSource.LogInfo("PatchInteractionsHandlerClassPurchaseTraderServiceMethod.Postfix()");
        }
    }
}
