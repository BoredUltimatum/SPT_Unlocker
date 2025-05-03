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
    internal class PatchExfiltrationPointLoadSettingsMethod : ModulePatch // all patches must inherit ModulePatch
    {
        public static bool AllEntryPoints = false;
        public static bool AlwaysChance = false;
        public static bool PMCUseScavExfils = false;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ExfiltrationPoint), nameof(ExfiltrationPoint.LoadSettings));
        }

        [PatchPostfix]
        static void Postfix(ref ExfiltrationPoint __instance)
        {
            Plugin.LogSource.LogInfo("PatchExfiltrationPointLoadSettingsMethod.Postfix()");
            Plugin.LogSource.LogInfo(__instance.Settings.Name + ": " + string.Join(",", __instance.EligibleEntryPoints));
            /*try
            {
                throw new Exception("CallStack Trace me");
            }
            catch (Exception e) {
                Plugin.LogSource.LogError(e);
            }*/
            if (!Singleton<GameWorld>.Instantiated)
            {
                Plugin.LogSource.LogError("GameWorld singleton not found.");
                return;
            }
            GameWorld _gameWorld = Singleton<GameWorld>.Instance;
            //Plugin.LogSource.LogInfo("_gameWorld" + _gameWorld.ToString());
            //Plugin.LogSource.LogInfo("_gameWorld.ExfiltrationController" + _gameWorld.ExfiltrationController.ToString());

            ExfiltrationPoint[] allExfiltrationPoints = _gameWorld.ExfiltrationController.ExfiltrationPoints;
            //Plugin.LogSource.LogInfo("ExfilPointCount: " + allExfiltrationPoints.Count().ToString());
            //Plugin.LogSource.LogInfo("ScavExfilPointCount: " + _gameWorld.ExfiltrationController.ScavExfiltrationPoints.Count().ToString());
            List<string> allEntryPointNames = new List<string>();
            foreach (ExfiltrationPoint ep in allExfiltrationPoints)
            {
                //Plugin.LogSource.LogInfo(ep.Settings.Id + "," + ep.Settings.Name + ": " + string.Join(",",ep.EligibleEntryPoints));
                foreach (string name in ep.EligibleEntryPoints)
                {
                    if (!allEntryPointNames.Contains(name.ToLower()))
                    {
                        allEntryPointNames.Add(name.ToLower());
                    }
                }
            }
            //Plugin.LogSource.LogInfo("All names: " + string.Join(",",allEntryPointNames.ToArray()));

            if (PMCUseScavExfils)
            {
                List<ExfiltrationPoint> newAllExfiltrationPoints = new List<ExfiltrationPoint>();
                List<string> newAllExfiltrationNames = new List<string>();
                foreach (ExfiltrationPoint ep in _gameWorld.ExfiltrationController.ExfiltrationPoints)
                {
                    if (!newAllExfiltrationNames.Contains(ep.Settings.Name.ToLower()) && !newAllExfiltrationPoints.Contains(ep))
                    {
                        newAllExfiltrationNames.Add(ep.Settings.Name.ToLower());
                        newAllExfiltrationPoints.Add(ep);
                    }
                }
                foreach (ExfiltrationPoint ep in _gameWorld.ExfiltrationController.ScavExfiltrationPoints)
                {
                    if (!newAllExfiltrationNames.Contains(ep.Settings.Name.ToLower()) && !newAllExfiltrationPoints.Contains(ep))
                    {
                        Plugin.LogSource.LogInfo("New scav exfil for PMC: " + ep.Settings.Name);
                        newAllExfiltrationNames.Add(ep.Settings.Name.ToLower());
                        newAllExfiltrationPoints.Add(ep);
                    }
                }
                _gameWorld.ExfiltrationController.ExfiltrationPoints = newAllExfiltrationPoints.ToArray();
            }

            if (AllEntryPoints)
            {
                foreach (ExfiltrationPoint ep in _gameWorld.ExfiltrationController.ExfiltrationPoints)
                {
                    ep.EligibleEntryPoints = allEntryPointNames.ToArray();
                    //ep.EligibleEntryPoints = new string[0];
                }
                // Ensure instance was also updated if not in controller response
                __instance.EligibleEntryPoints = allEntryPointNames.ToArray();
                //__instance.EligibleEntryPoints = new string[0];

                //__instance.Enable();
                //__instance.EnableInteraction();
            }

            if (AlwaysChance)
            {
                __instance.Settings.Chance = 100.0f;
            }
        }
    }
}
