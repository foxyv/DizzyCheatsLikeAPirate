using UnityEngine;

namespace Dizzy.Cheats
{
    // Drives the game's own PlayerNeeds.godMode (the flag behind the P+N+T debug
    // mode), which skips the whole needs update.
    internal static class GodMode
    {
        private static PlayerNeeds _appliedTo;

        internal static void Update(bool keys)
        {
            if (keys && Hotkeys.Down(CheatsConfig.GodModeKey))
                Toggle(fill: true);

            PlayerNeeds needs = PlayerNeeds.instance;
            if (needs == null)
                return;

            // Apply once per PlayerNeeds (a new one comes with each loaded save), so the
            // vanilla debug toggle can still turn god mode on without us undoing it.
            if (needs != _appliedTo)
            {
                _appliedTo = needs;
                needs.godMode = CheatsConfig.GodModeOn.Value;
            }

            // God mode also freezes the eat cooldown, which would block a second bite
            // or drink until it is turned off. Tick it down the way vanilla does.
            if (needs.godMode && needs.eatCooldown > 0f)
                needs.eatCooldown = Mathf.Max(0f, needs.eatCooldown - Time.deltaTime);
        }

        internal static bool On => CheatsConfig.GodModeOn.Value;

        // The hotkey fills needs when turning on; the cheat panel does not, so values
        // set there can be frozen as they are.
        internal static void Toggle(bool fill)
        {
            bool on = !CheatsConfig.GodModeOn.Value;
            CheatsConfig.GodModeOn.Value = on;
            _appliedTo = null;
            if (on && fill)
                FillNeeds();
            Notify.Show(on ? "God mode on: hunger, thirst and sleepiness are frozen" : "God mode off");
        }

        internal static void FillNeeds()
        {
            PlayerNeeds.food = 100f;
            PlayerNeeds.foodDebt = 100f;
            PlayerNeeds.water = 100f;
            PlayerNeeds.sleep = 100f;
            PlayerNeeds.sleepDebt = 100f;
            PlayerNeeds.vitamins = 100f;
            PlayerNeeds.protein = 100f;
        }
    }
}
