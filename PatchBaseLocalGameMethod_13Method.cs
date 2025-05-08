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
    internal class PatchBaseLocalGameMethod_13Method : ModulePatch // all patches must inherit ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BaseLocalGame<EFT.EftGamePlayerOwner>).GetMethod("method_13", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [PatchPrefix]
        private static void Prefix(BaseLocalGame<EFT.EftGamePlayerOwner> __instance)
        {
            Plugin.LogSource.LogInfo("PatchBaseLocalGameMethod_13Method.Prefix()");
        }

        [PatchPostfix]
        private static void Postfix(Dictionary<string, GClass1319[]> __result)
        {
            Plugin.LogSource.LogInfo("PatchBaseLocalGameMethod_13Method.Postfix()");
            foreach(var entry in __result)
            {
                string id = entry.Key.Split('_')[0];
                foreach(GClass1319 item in entry.Value)
                {
                    if (item.parentId == id)
                    {
                        item.slotId = "main";
                    }
                }
            }
        }
    }
}
