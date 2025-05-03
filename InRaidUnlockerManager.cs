using EFT;
using System;
using UnityEngine;
using Comfort.Common;

namespace SPT_Unlocker
{
    public class InRaidUnlockerManager : MonoBehaviour
    {
        private bool inRaid = false;
        private bool processedRaidStart = false;
        private GameWorld _gameWorld;
        private Player _player;
        private int updateCalls = 0;


        private void Awake()
        {
            Plugin.LogSource.LogInfo("InRaidUnlockerManager instantiated");
        }

        private void RaidStart()
        {
            processedRaidStart = true;
            Plugin.LogSource.LogInfo("InRaidUnlockerManager RaidStart()");
            Plugin.LogSource.LogInfo("_gameWorld" + _gameWorld.ToString());
            Plugin.LogSource.LogInfo("_player" + _player.ToString());
        }

        private void Update()
        {
            //if (updateCalls % (60 * 10) == 0)
            //{
            //    Plugin.LogSource.LogInfo($"InRaidUnlockerManager Update() calls: {updateCalls}");
            //    Plugin.LogSource.LogInfo($"_gameWorld: {_gameWorld}");
            //    Plugin.LogSource.LogInfo($"_player: {_player}");
            //}
            //updateCalls++;

            if (!processedRaidStart)
            {
                if (!Singleton<GameWorld>.Instantiated)
                {
                    inRaid = false;
                    return;
                }
                _gameWorld = Singleton<GameWorld>.Instance;
                if (_gameWorld.MainPlayer == null)
                {
                    inRaid = false;
                    return;
                }
                _player = _gameWorld.MainPlayer;

                inRaid = true;
                RaidStart();
            }
        }
    }
}