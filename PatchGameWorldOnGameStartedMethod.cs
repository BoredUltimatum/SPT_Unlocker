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
    internal class PatchGameWorldOnGameStartedMethod : ModulePatch // all patches must inherit ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod("OnGameStarted", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [PatchPostfix]
        private static void Postfix(GameWorld __instance)
        {
            Plugin.LogSource.LogInfo("PatchGameWorldOnGameStartedMethod.Postfix()");
            __instance.gameObject.AddComponent<InRaidUnlockerManager>();
        }
    }
}
