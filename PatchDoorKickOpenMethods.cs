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
    internal class PatchDoorKickOpenVBMethod : ModulePatch // all patches must inherit ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Door), nameof(Door.KickOpen), new Type[] { typeof(UnityEngine.Vector3), typeof(bool) });
        }

        [PatchPrefix]
        static bool Prefix()
        {
            Plugin.LogSource.LogError("PatchDoorKickOpenVBMethod.Prefix()");
            try
            {
                throw new Exception("CallStack Trace me");
            }
            catch (Exception e) {
                Plugin.LogSource.LogError(e);
            }
            // All kick attempts work
            return true;
        }
    }
    internal class PatchDoorKickOpenBMethod : ModulePatch // all patches must inherit ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Door), nameof(Door.KickOpen), new Type[] { typeof(bool) });
        }

        [PatchPrefix]
        static bool Prefix()
        {
            Plugin.LogSource.LogError("PatchDoorKickOpenBMethod.Prefix()");
            try
            {
                throw new Exception("CallStack Trace me");
            }
            catch (Exception e)
            {
                Plugin.LogSource.LogError(e);
            }
            // All kick attempts work
            return true;
        }
    }
}
