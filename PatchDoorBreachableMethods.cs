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
    internal class PatchDoorIsBreachAngleMethod : ModulePatch // all patches must inherit ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Door), nameof(Door.IsBreachAngle), new Type[] { typeof(UnityEngine.Vector3) });
        }

        [PatchPrefix]
        static bool Prefix()
        {
            Plugin.LogSource.LogError("PatchDoorIsBreachAngleMethod.Prefix()");
            // All kick attempts work
            return true;
        }
    }
    internal class PatchDoorBreachSuccessRollMethod : ModulePatch // all patches must inherit ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Door), nameof(Door.BreachSuccessRoll), new Type[] { typeof(UnityEngine.Vector3) });
        }

        [PatchPrefix]
        static bool Prefix()
        {
            Plugin.LogSource.LogError("PatchDoorBreachSuccessRollMethod.Prefix()");
            // All kick attempts work
            return true;
        }
    }
}
