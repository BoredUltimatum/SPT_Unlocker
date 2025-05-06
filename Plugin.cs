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
        public static ConfigEntry<KeyboardShortcut> OpenItemTransferShortCutConfig;

        const string headerLabelDoors = "Doors";
        const string headerLabelExfils = "Exfils";
        const string headerLabelItemTransfer = "ItemTransfer";

        private PatchDoorStartMethod patchDoorStartMethod;
        private PatchLootableContainerInitMethod patchLootableContainerInitMethod;
        private PatchExfiltrationPointLoadSettingsMethod patchExfiltrationPointLoadSettingsMethod;
        private PatchLocalGameStopMethod patchLocalGameStopMethod;

        private bool lastValueUnlockDoorsEnabled = false;
        private bool lastValueUnlockContainersEnabled = false;
        private bool lastValueUnlockExfilsEnabled = false;
        private bool lastValueUnlockPMCScavExfils = false;
        private bool lastValueUnlockExfilsAlwaysChance = false;

        private bool lastValueOpenItemTransferShortCutDown = false;

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
            OpenItemTransferShortCutConfig = Config.Bind(headerLabelItemTransfer, "Open Item Transfer Menu", new KeyboardShortcut(UnityEngine.KeyCode.Insert));

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
                patchExfiltrationPointLoadSettingsMethod.Enable();
            }

            patchLocalGameStopMethod = new PatchLocalGameStopMethod();
            patchLocalGameStopMethod.Enable();

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

        private void OpenTransferView()
        {
            if ((object)_player != null)
            {
                //clientPlayer.GetTraderServicesDataFromServer("656f0f98d80a697f855d34b1");
                _player.InventoryController.GetTraderServicesDataFromServer("656f0f98d80a697f855d34b1");
                BackendConfigSettingsClass.BTRGlobalSettings btrGlobalSettings = new BackendConfigSettingsClass.BTRGlobalSettings();
                btrGlobalSettings.LocationsWithBTR = new string[] { _gameWorld.LocationId };
                btrGlobalSettings.BasePriceTaxi = 0;
                btrGlobalSettings.AddPriceTaxi = 0;
                btrGlobalSettings.CleanUpPrice = 0;
                btrGlobalSettings.DeliveryPrice = 0;
                btrGlobalSettings.ModDeliveryCost = 0f;
                btrGlobalSettings.BearPriceMod = 0f;
                btrGlobalSettings.UsecPriceMod = 0f;
                btrGlobalSettings.ScavPriceMod = 00f;
                btrGlobalSettings.CoefficientDiscountCharisma = 0.000f;
                btrGlobalSettings.TaxiMinPrice = 0;
                btrGlobalSettings.DeliveryMinPrice = 0;
                btrGlobalSettings.BotCoverMinPrice = 0;
                GClass1671 transferController = new GClass1671(_gameWorld, btrGlobalSettings, true);
                transferController.etraderServiceType_0 = ETraderServiceType.TransitItemsDelivery;
                new EFT.UI.TransferItemsInRaidScreen.GClass3603(_player.Profile, _player.InventoryController, _player.AbstractQuestControllerClass, new InsuranceCompanyClass(null, _player.Profile), transferController).ShowScreen(EFT.UI.Screens.EScreenState.Queued);
            }
        }

        private void OpenTransferView2()
        {
            if ((object)_player != null)
            {
                //clientPlayer.GetTraderServicesDataFromServer("656f0f98d80a697f855d34b1");
                _player.InventoryController.GetTraderServicesDataFromServer("656f0f98d80a697f855d34b1");
                BackendConfigSettingsClass.BTRGlobalSettings btrGlobalSettings = new BackendConfigSettingsClass.BTRGlobalSettings();
                btrGlobalSettings.LocationsWithBTR = new string[] { _gameWorld.LocationId };
                btrGlobalSettings.BasePriceTaxi = 0;
                btrGlobalSettings.AddPriceTaxi = 0;
                btrGlobalSettings.CleanUpPrice = 0;
                btrGlobalSettings.DeliveryPrice = 0;
                btrGlobalSettings.ModDeliveryCost = 0f;
                btrGlobalSettings.BearPriceMod = 0f;
                btrGlobalSettings.UsecPriceMod = 0f;
                btrGlobalSettings.ScavPriceMod = 00f;
                btrGlobalSettings.CoefficientDiscountCharisma = 0.000f;
                btrGlobalSettings.TaxiMinPrice = 0;
                btrGlobalSettings.DeliveryMinPrice = 0;
                btrGlobalSettings.BotCoverMinPrice = 0;
                GClass1670 transferController = _gameWorld.TransitController.TransferItemsController;
                transferController.InitPlayerStash(_player);
                new EFT.UI.TransferItemsInRaidScreen.GClass3603(_player.Profile, _player.InventoryController, _player.AbstractQuestControllerClass, new InsuranceCompanyClass(null, _player.Profile), transferController).ShowScreen(EFT.UI.Screens.EScreenState.Queued);
            }
        }

        private void OpenItemTransferView()
        {
            if ((object)_player != null)
            {
                _player.InventoryController.GetTraderServicesDataFromServer("656f0f98d80a697f855d34b1");
                GClass1670 transferController = _gameWorld.TransitController.TransferItemsController;
                transferController.InitPlayerStash(_player);
                new EFT.UI.TransferItemsInRaidScreen.GClass3603(_player.Profile, _player.InventoryController, _player.AbstractQuestControllerClass, new InsuranceCompanyClass(null, _player.Profile), transferController).ShowScreen(EFT.UI.Screens.EScreenState.Queued);
            }
        }

        private void OpenTransferViewOld()
        {
            Player myPlayer = GamePlayerOwner.MyPlayer;
            //ClientPlayer clientPlayer = myPlayer as ClientPlayer;
            Player clientPlayer = myPlayer;
            if ((object)clientPlayer != null)
            {
                //clientPlayer.GetTraderServicesDataFromServer("656f0f98d80a697f855d34b1");
                clientPlayer.InventoryController.GetTraderServicesDataFromServer("656f0f98d80a697f855d34b1");
                new EFT.UI.TransferItemsInRaidScreen.GClass3603(clientPlayer.Profile, clientPlayer.InventoryController, clientPlayer.AbstractQuestControllerClass, new InsuranceCompanyClass(null, clientPlayer.Profile), BTRControllerClass.Instance.TransferItemsController).ShowScreen(EFT.UI.Screens.EScreenState.Queued);
            }
        }

        private async void ForceTransferComplete()
        {
            int methodType = 1;
            if (methodType == 1)
            {
                bool bool_1 = await _player.InventoryController.TryPurchaseTraderService(ETraderServiceType.TransitItemsDelivery, _player.AbstractQuestControllerClass);
                Plugin.LogSource.LogInfo(bool_1);
            } else
            if (methodType == 2)
            {
                bool bool_1 = await _player.InventoryController.TryPurchaseTraderService(ETraderServiceType.BtrItemsDelivery, _player.AbstractQuestControllerClass);
                Plugin.LogSource.LogInfo(bool_1);
            }
        }

        private void EnableBTR()
        {
            _gameWorld.BtrController.method_10();
            _gameWorld.BtrController.BtrVehicle.MoveEnable();
        }

        private async void EnterBTR()
        {
            byte sideId = 0;
            byte placeId = 0;
            EFT.Vehicle.BTRSide btrSide = _gameWorld.BtrController.BtrView.GetBtrSide(sideId);
            await _gameWorld.BtrController.BtrView.GoIn(_player, btrSide, placeId, true);
        }

        private async void ExitBTR()
        {
            byte sideId = 0;
            byte placeId = 0;
            EFT.Vehicle.BTRSide btrSide = _gameWorld.BtrController.BtrView.GetBtrSide(sideId);
            await _gameWorld.BtrController.BtrView.GoOut(_player, btrSide, placeId, true);
        }

        private async void BTRMethod21()
        {
            _gameWorld.BtrController.method_21(_player);
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
                if (UnlockExfilsEnabled.Value == true || UnlockExfilsAlwaysChance.Value == true || UnlockPMCScavExfils.Value == true)
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

            if (OpenItemTransferShortCutConfig.Value.IsDown())
            {
                if (lastValueOpenItemTransferShortCutDown == false)
                {
                    // Is a new key press
                    lastValueOpenItemTransferShortCutDown = true;
                    OpenItemTransferView();
                }
            } else
            {
                lastValueOpenItemTransferShortCutDown = false;
            }


            int testId = 0;
            if (testId > 0)
            {
                if (testId == 100)
                {
                    OpenTransferView();
                }
                if (testId == 101)
                {
                    OpenTransferView2();
                }
                if (testId == 110)
                {
                    ForceTransferComplete();
                }
                if (testId == 200)
                {
                    EnableBTR();
                }
                if (testId == 210)
                {
                    EnterBTR();
                }
                if (testId == 220)
                {
                    ExitBTR();
                }
                if (testId == 230)
                {
                    BTRMethod21();
                }
            }
        }
    }
}
