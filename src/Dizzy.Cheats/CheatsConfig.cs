using BepInEx.Configuration;
using UnityEngine;

namespace Dizzy.Cheats
{
    internal enum FlyStyle
    {
        Hover,
        Flappy
    }

    internal static class CheatsConfig
    {
        internal static ConfigEntry<KeyCode> PanelKey;
        internal static ConfigEntry<KeyCode> FlyKey;
        internal static ConfigEntry<KeyCode> SpeedKey;
        internal static ConfigEntry<KeyCode> GodModeKey;
        internal static ConfigEntry<KeyCode> ThrottleUpKey;
        internal static ConfigEntry<KeyCode> ThrottleDownKey;
        internal static ConfigEntry<KeyCode> TurnLeftKey;
        internal static ConfigEntry<KeyCode> TurnRightKey;
        internal static ConfigEntry<KeyCode> MotorStopKey;

        internal static ConfigEntry<FlyStyle> Style;
        internal static ConfigEntry<float> FlySpeed;
        internal static ConfigEntry<float> FlyBoost;
        internal static ConfigEntry<bool> NoClip;
        internal static ConfigEntry<float> FlappyRiseSpeed;
        internal static ConfigEntry<float> FlappyGravity;

        internal static ConfigEntry<float> SpeedMultiplier;

        internal static ConfigEntry<bool> GodModeOn;

        internal static ConfigEntry<float> MotorStepAcceleration;
        internal static ConfigEntry<int> MotorForwardSteps;
        internal static ConfigEntry<int> MotorReverseSteps;
        internal static ConfigEntry<float> MotorTurnAcceleration;

        internal static void Bind(ConfigFile config)
        {
            PanelKey = config.Bind(
                "Hotkeys",
                "PanelKey",
                KeyCode.BackQuote,
                "Open or close the cheat panel: cheat switches, sliders to set food, water, sleep, vitamins and protein, buttons to add or remove each currency, and a searchable window to spawn items. The default is the ` key (left of 1). It only opens during play, not in the Esc menu, so it can share ` with Sailwind Console, which only opens in the Esc menu.");

            FlyKey = config.Bind(
                "Hotkeys",
                "FlyKey",
                KeyCode.V,
                "Toggle fly mode.");

            SpeedKey = config.Bind(
                "Hotkeys",
                "SpeedKey",
                KeyCode.B,
                "Toggle super speed for walking, running and swimming.");

            GodModeKey = config.Bind(
                "Hotkeys",
                "GodModeKey",
                KeyCode.H,
                "Toggle the game's own god mode: hunger, thirst and sleepiness stop changing.");

            ThrottleUpKey = config.Bind(
                "Hotkeys",
                "ThrottleUpKey",
                KeyCode.I,
                "Invisible motor: one step more ahead (or one step less astern).");

            ThrottleDownKey = config.Bind(
                "Hotkeys",
                "ThrottleDownKey",
                KeyCode.K,
                "Invisible motor: one step less ahead (or one step more astern).");

            TurnLeftKey = config.Bind(
                "Hotkeys",
                "TurnLeftKey",
                KeyCode.J,
                "Invisible motor: hold to swing the bow to port (left).");

            TurnRightKey = config.Bind(
                "Hotkeys",
                "TurnRightKey",
                KeyCode.L,
                "Invisible motor: hold to swing the bow to starboard (right).");

            MotorStopKey = config.Bind(
                "Hotkeys",
                "MotorStopKey",
                KeyCode.O,
                "Invisible motor: all stop.");

            Style = config.Bind(
                "Fly",
                "Style",
                FlyStyle.Hover,
                "Hover: move where you look, your jump key rises, your crouch key sinks, and you stay put when you let go. Flappy: hold your jump key to rise, let go to fall back down.");

            FlySpeed = config.Bind(
                "Fly",
                "Speed",
                8f,
                new ConfigDescription(
                    "Fly speed in meters per second (1 game unit = 1 m).",
                    new AcceptableValueRange<float>(1f, 100f)));

            FlyBoost = config.Bind(
                "Fly",
                "BoostMultiplier",
                4f,
                new ConfigDescription(
                    "Fly speed is multiplied by this while your run key is held.",
                    new AcceptableValueRange<float>(1f, 20f)));

            NoClip = config.Bind(
                "Fly",
                "NoClip",
                false,
                "Hover style only: fly through walls, hulls and the ground. Flappy style always collides, so you land instead of falling through the deck.");

            FlappyRiseSpeed = config.Bind(
                "Fly",
                "FlappyRiseSpeed",
                6f,
                new ConfigDescription(
                    "Flappy style: how fast you rise while your jump key is held, in meters per second.",
                    new AcceptableValueRange<float>(1f, 50f)));

            FlappyGravity = config.Bind(
                "Fly",
                "FlappyGravity",
                9.81f,
                new ConfigDescription(
                    "Flappy style: how fast you speed up while falling, in meters per second squared.",
                    new AcceptableValueRange<float>(1f, 40f)));

            SpeedMultiplier = config.Bind(
                "Speed",
                "Multiplier",
                4f,
                new ConfigDescription(
                    "Walk, run, swim and fly speed is multiplied by this while super speed is on. It stacks with the fly BoostMultiplier. Moving faster does not cost extra food or water.",
                    new AcceptableValueRange<float>(1f, 20f)));

            GodModeOn = config.Bind(
                "Needs",
                "GodMode",
                false,
                "On: hunger, thirst and sleepiness (and vitamins and protein) stop changing. The GodModeKey flips this and it is remembered between sessions. Turning it on with the key also fills every need to full.");

            MotorStepAcceleration = config.Bind(
                "Motor",
                "StepAcceleration",
                1f,
                new ConfigDescription(
                    "Push per throttle step, in meters per second squared. It ignores boat size, so every boat speeds up the same; water drag still sets the top speed.",
                    new AcceptableValueRange<float>(0.1f, 10f)));

            MotorForwardSteps = config.Bind(
                "Motor",
                "ForwardSteps",
                20,
                new ConfigDescription(
                    "Number of throttle steps ahead.",
                    new AcceptableValueRange<int>(1, 50)));

            MotorReverseSteps = config.Bind(
                "Motor",
                "ReverseSteps",
                3,
                new ConfigDescription(
                    "Number of throttle steps astern.",
                    new AcceptableValueRange<int>(0, 50)));

            MotorTurnAcceleration = config.Bind(
                "Motor",
                "TurnAcceleration",
                0.5f,
                new ConfigDescription(
                    "How hard the turn keys swing the bow, in radians per second squared.",
                    new AcceptableValueRange<float>(0.05f, 5f)));
        }
    }
}
