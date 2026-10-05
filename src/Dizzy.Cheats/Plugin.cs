using System;
using BepInEx;
using BepInEx.Logging;
using Dizzy.Cheats.Patches;
using HarmonyLib;

namespace Dizzy.Cheats
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("Sailwind.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.dizzy.sailwind.cheats";
        public const string PluginName = "Dizzy Cheats Like a Pirate";
        public const string PluginVersion = "0.1.0";

        internal static ManualLogSource Log;
        internal static Plugin Instance;

        private Harmony _harmony;
        private string _lastError;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            CheatsConfig.Bind(Config);

            _harmony = new Harmony(PluginGuid);
            PatchApplier.Apply(_harmony);

            Log.LogInfo($"{PluginName} v{PluginVersion} loaded (Panel={CheatsConfig.PanelKey.Value}, Fly={CheatsConfig.FlyKey.Value}, Speed={CheatsConfig.SpeedKey.Value}, GodMode={CheatsConfig.GodModeKey.Value}, Motor={CheatsConfig.ThrottleUpKey.Value}/{CheatsConfig.ThrottleDownKey.Value}/{CheatsConfig.TurnLeftKey.Value}/{CheatsConfig.TurnRightKey.Value}/{CheatsConfig.MotorStopKey.Value}, FlyAvailable={FlyMode.Available}).");
        }

        private void Update()
        {
            try
            {
                CheatPanel.Update();

                bool keys = Hotkeys.Allowed;
                FlyMode.Update(keys);
                SuperSpeed.Update(keys);
                GodMode.Update(keys);
                BoatMotor.Update(keys);
            }
            catch (Exception e)
            {
                LogOnce("Cheats update failed: " + e);
            }
        }

        private void LateUpdate()
        {
            try
            {
                CheatPanel.LateUpdate();
            }
            catch (Exception e)
            {
                LogOnce("Cheat panel failed: " + e);
            }
        }

        private void OnGUI()
        {
            try
            {
                CheatPanel.OnGUI();
            }
            catch (Exception e)
            {
                LogOnce("Cheat panel failed: " + e);
            }
        }

        private void FixedUpdate()
        {
            try
            {
                BoatMotor.FixedUpdate();
            }
            catch (Exception e)
            {
                LogOnce("Motor failed: " + e);
            }
        }

        // Update runs every frame; log a repeating fault once instead of flooding the log.
        private void LogOnce(string message)
        {
            if (message == _lastError)
                return;
            _lastError = message;
            Log.LogWarning(message);
        }

        // Unlike Dizzy Fixes, every cheat is switched from this component's Update. If
        // it is destroyed nothing could turn fly off again, so drop the patches with it.
        // modcheck-ignore: SW110 cheats are controlled from Update; patches must not outlive it
        private void OnDestroy()
        {
            CheatPanel.Close();
            SuperSpeed.Reset();
            _harmony?.UnpatchSelf();

            if (Instance == this)
                Instance = null;
        }
    }
}
