using EFT;
using EFT.Interactive;
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
    internal class PatchTraderControllerClassTryRunNetworkTransactionMethod : ModulePatch // all patches must inherit ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(TraderControllerClass).GetMethod("TryRunNetworkTransaction", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [PatchPrefix]
        private static void Prefix(TraderControllerClass __instance)
        {
            Plugin.LogSource.LogInfo("PatchTraderControllerClassTryRunNetworkTransactionMethod.Postfix()");
        }

        [PatchPostfix]
        private static void Postfix(TraderControllerClass __instance)
        {
            Plugin.LogSource.LogInfo("PatchTraderControllerClassTryRunNetworkTransactionMethod.Postfix()");
        }
    }
}
