using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT;
using Comfort.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPT_Unlocker
{
    // first string below is your plugin's GUID, it MUST be unique to any other mod. Read more about it in BepInEx docs. Be sure to update it if you copy this project.
    [BepInPlugin("BoredUltimatum.SPT_Unlocker", "SPT_Unlocker", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        public static ConfigEntry<bool> UnlockDoorsEnabled;
        public static ConfigEntry<bool> UnlockContainersEnabled;
        public static ConfigEntry<bool> UnlockExfilsEnabled;
        public static ConfigEntry<bool> UnlockPMCScavExfils;
        public static ConfigEntry<bool> UnlockExfilsAlwaysChance;

        const string headerLabelDoors = "Doors";
        const string headerLabelExfils = "Exfils";

        private PatchDoorStartMethod patchDoorStartMethod;
        private PatchLootableContainerInitMethod patchLootableContainerInitMethod;
        private PatchExfiltrationPointLoadSettingsMethod patchExfiltrationPointLoadSettingsMethod;

        private bool lastValueUnlockDoorsEnabled;
        private bool lastValueUnlockContainersEnabled;
        private bool lastValueUnlockExfilsEnabled;
        private bool lastValueUnlockPMCScavExfils;
        private bool lastValueUnlockExfilsAlwaysChance;

        private bool inRaid = false;
        private bool processedRaidStart = false;
        private bool processedScavEPs = false;
        private GameWorld _gameWorld;
        private Player _player;
        private int updateCalls = 0;

        // BaseUnityPlugin inherits MonoBehaviour, so you can use base unity functions like Awake() and Update()
        private void Awake()
        {
            // save the Logger to variable so we can use it elsewhere in the project
            LogSource = Logger;
            LogSource.LogInfo("plugin loaded!");

            this.enabled = true;

            UnlockDoorsEnabled = Config.Bind(headerLabelDoors, "Unlock Doors Enabled", true);
            UnlockContainersEnabled = Config.Bind(headerLabelDoors, "Unlock Containers Enabled", true);
            UnlockExfilsEnabled = Config.Bind(headerLabelExfils, "Unlock Exfils Enabled", true);
            UnlockPMCScavExfils = Config.Bind(headerLabelExfils, "Unlock Scav Exfils for PMCs", true);
            UnlockExfilsAlwaysChance = Config.Bind(headerLabelExfils, "Exfil Guaranteed Chance", true);

            lastValueUnlockDoorsEnabled = UnlockDoorsEnabled.Value;
            lastValueUnlockContainersEnabled = UnlockContainersEnabled.Value;
            lastValueUnlockExfilsEnabled = UnlockExfilsEnabled.Value;
            lastValueUnlockPMCScavExfils = UnlockPMCScavExfils.Value;
            lastValueUnlockExfilsAlwaysChance = UnlockExfilsAlwaysChance.Value;

            // uncomment line(s) below to enable desired example patch, then press F6 to build the project:
            // new SimplePatch().Enable();

            patchDoorStartMethod = new PatchDoorStartMethod();
            patchLootableContainerInitMethod = new PatchLootableContainerInitMethod();
            patchExfiltrationPointLoadSettingsMethod = new PatchExfiltrationPointLoadSettingsMethod();

            if (UnlockExfilsEnabled.Value == true || UnlockExfilsAlwaysChance.Value == true || UnlockPMCScavExfils.Value == true)
            {
                PatchExfiltrationPointLoadSettingsMethod.AllEntryPoints = UnlockExfilsEnabled.Value;
                PatchExfiltrationPointLoadSettingsMethod.AlwaysChance = UnlockExfilsAlwaysChance.Value;
                PatchExfiltrationPointLoadSettingsMethod.PMCUseScavExfils = UnlockPMCScavExfils.Value;
                new PatchExfiltrationPointLoadSettingsMethod().Enable();
            }

            //if (UnlockPMCScavExfils.Value == true)
            //{
            //    new PatchScavExfiltrationPointHasMetRequirementsMethod().Enable();
            //    new PatchScavExfiltrationPointInfiltrationMatchMethod().Enable();
            //}

            //LogSource.LogInfo("Enabling PatchGameWorldOnGameStartedMethod");
            //new PatchGameWorldOnGameStartedMethod().Enable();
        }

        private void RaidStart()
        {
            processedRaidStart = true;
            Plugin.LogSource.LogInfo("Plugin RaidStart()");
            Plugin.LogSource.LogInfo("_gameWorld" + _gameWorld.ToString());
            Plugin.LogSource.LogInfo("_player" + _player.ToString());
        }

        private void RaidEnd()
        {
            Plugin.LogSource.LogInfo("Plugin RaidEnd()");
        }

        private void ProcessScavEPs()
        {
            //Plugin.LogSource.LogInfo("Plugin ProcessScavEPs()");
            //Plugin.LogSource.LogInfo($"_gameWorld: {_gameWorld}");
            //Plugin.LogSource.LogInfo($"_player: {_player}");
            //Plugin.LogSource.LogInfo($"ExfiltrationController: {_gameWorld.ExfiltrationController}");
            //Plugin.LogSource.LogInfo($"mainEPs: {_gameWorld.ExfiltrationController.ExfiltrationPoints}");

            if (_gameWorld.ExfiltrationController == null || _gameWorld.ExfiltrationController.ExfiltrationPoints == null || _gameWorld.ExfiltrationController.ScavExfiltrationPoints == null)
            {
                return;
            }

            EFT.Interactive.ScavExfiltrationPoint[] scavEPs = _gameWorld.ExfiltrationController.ScavExfiltrationPoints;
            //Plugin.LogSource.LogInfo($"Scav EP count: {scavEPs.Count()}");
            foreach (EFT.Interactive.ScavExfiltrationPoint ep in scavEPs)
            {
                //Plugin.LogSource.LogInfo($"Name: {ep.Settings.Name}");
                if (!ep.EligibleIds.Contains(_player.Profile.Id))
                {
                    //Plugin.LogSource.LogInfo("Adding");
                    ep.EligibleIds.Add(_player.Profile.Id);
                }
            }

            EFT.Interactive.ExfiltrationPoint[] mainEPs = _gameWorld.ExfiltrationController.ExfiltrationPoints;
            //Plugin.LogSource.LogInfo($"Main EP count: {mainEPs.Count()}");
            foreach (EFT.Interactive.ExfiltrationPoint ep in mainEPs)
            {
                if (ep is EFT.Interactive.ScavExfiltrationPoint)
                {
                    EFT.Interactive.ScavExfiltrationPoint sep = (EFT.Interactive.ScavExfiltrationPoint)ep;
                    //Plugin.LogSource.LogInfo($"Name: {sep.Settings.Name}");
                    if (!sep.EligibleIds.Contains(_player.Profile.Id))
                    {
                        //Plugin.LogSource.LogInfo("Adding");
                        sep.EligibleIds.Add(_player.Profile.Id);
                    }
                    if (ep.Status != EFT.Interactive.EExfiltrationStatus.RegularMode)
                    {
                        //Plugin.LogSource.LogInfo("Setting to RegularMode");
                        ep.Status = EFT.Interactive.EExfiltrationStatus.RegularMode;
                    }
                }
            }

            processedScavEPs = true;
        }

        void Update()
        {
            //if (updateCalls % (60*10) == 0)
            //{
            //    Plugin.LogSource.LogInfo($"Plugin Update() calls: {updateCalls}");
            //    Plugin.LogSource.LogInfo($"_gameWorld: {_gameWorld}");
            //    Plugin.LogSource.LogInfo($"_player: {_player}");
            //}
            //updateCalls++;

            if (lastValueUnlockDoorsEnabled != UnlockDoorsEnabled.Value)
            {
                lastValueUnlockDoorsEnabled = UnlockDoorsEnabled.Value;
                if (UnlockDoorsEnabled.Value == true)
                {
                    patchDoorStartMethod.Enable();
                } else
                {
                    patchDoorStartMethod.Disable();
                }
            }
            if (lastValueUnlockContainersEnabled != UnlockContainersEnabled.Value)
            {
                lastValueUnlockContainersEnabled = UnlockContainersEnabled.Value;
                if (UnlockContainersEnabled.Value == true)
                {
                    patchLootableContainerInitMethod.Enable();
                }
                else
                {
                    patchLootableContainerInitMethod.Disable();
                }
            }
            if (lastValueUnlockExfilsEnabled != UnlockExfilsEnabled.Value)
            {
                lastValueUnlockExfilsEnabled = UnlockExfilsEnabled.Value;
                PatchExfiltrationPointLoadSettingsMethod.AllEntryPoints = UnlockExfilsEnabled.Value;
                if (UnlockExfilsEnabled.Value == true)
                {
                    patchExfiltrationPointLoadSettingsMethod.Enable();
                }
                else
                {
                    patchExfiltrationPointLoadSettingsMethod.Disable();
                }
            }
            if (lastValueUnlockPMCScavExfils != UnlockPMCScavExfils.Value)
            {
                lastValueUnlockPMCScavExfils = UnlockPMCScavExfils.Value;
                PatchExfiltrationPointLoadSettingsMethod.PMCUseScavExfils = UnlockPMCScavExfils.Value;
            }
            if (lastValueUnlockExfilsAlwaysChance != UnlockExfilsAlwaysChance.Value)
            {
                lastValueUnlockExfilsAlwaysChance = UnlockExfilsAlwaysChance.Value;
                PatchExfiltrationPointLoadSettingsMethod.AlwaysChance = UnlockExfilsAlwaysChance.Value;
            }

            if (!processedRaidStart)
            {
                if ((!Singleton<GameWorld>.Instantiated) || (Singleton<GameWorld>.Instance.MainPlayer == null))
                {
                    if (inRaid == true)
                    {
                        RaidEnd();
                    }
                    inRaid = false;
                    processedRaidStart = false;
                    processedScavEPs = false;
                    return;
                }
                _gameWorld = Singleton<GameWorld>.Instance;
                _player = _gameWorld.MainPlayer;

                inRaid = true;
                RaidStart();
            } else
            {
                if (inRaid == true && (!Singleton<GameWorld>.Instantiated || Singleton<GameWorld>.Instance.MainPlayer == null))
                {
                    inRaid = false;
                    processedRaidStart = false;
                    processedScavEPs = false;
                    return;
                }
                if (!processedScavEPs)
                {
                    ProcessScavEPs();
                }
            }
        }
    }
}
