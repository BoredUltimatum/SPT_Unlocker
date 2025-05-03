using EFT;
using EFT.Interactive;
using HarmonyLib;
using Comfort.Common;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SPT_Unlocker
{
    internal class PatchScavExfiltrationPointInfiltrationMatchMethod : ModulePatch // all patches must inherit ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ScavExfiltrationPoint), nameof(ScavExfiltrationPoint.InfiltrationMatch), new Type[] { typeof(string) });
        }

        [PatchPostfix]
        static void Postfix(ref bool __result)
        {
            Plugin.LogSource.LogInfo("PatchScavExfiltrationPointInfiltrationMatchMethod.Postfix()");
            __result = true;
        }
    }
}
