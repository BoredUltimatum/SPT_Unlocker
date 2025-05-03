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
    internal class PatchLootableContainerInitMethod : ModulePatch // all patches must inherit ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(LootableContainer), nameof(LootableContainer.Init));
        }

        [PatchPostfix]
        static void Postfix(ref LootableContainer __instance)
        {
            //Plugin.LogSource.LogInfo("PatchLootableContainerInitMethod.Postfix()");
            /*try
            {
                throw new Exception("CallStack Trace me");
            }
            catch (Exception e) {
                Plugin.LogSource.LogError(e);
            }*/
            if (__instance.DoorState == EDoorState.Locked)
            {
                Plugin.LogSource.LogInfo("Container locked, unlocking");
                //__instance.DoorState = EDoorState.Shut;
                __instance.Unlock();
            }
        }
    }
}
