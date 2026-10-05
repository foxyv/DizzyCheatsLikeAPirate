namespace Dizzy.Cheats
{
    // Uses OVRPlayerController's own move-scale multiplier. Vanilla Sailwind never
    // sets it, so nothing fights it.
    internal static class SuperSpeed
    {
        internal static bool On;

        internal static float Multiplier => CheatsConfig.SpeedMultiplier.Value;

        internal static void Update(bool keys)
        {
            if (keys && Hotkeys.Down(CheatsConfig.SpeedKey))
                Toggle();

            // Reapply each frame: the controller is rebuilt when a save loads.
            OVRPlayerController player = Refs.ovrController;
            if (On && player != null)
                player.SetMoveScaleMultiplier(Multiplier);
        }

        internal static void Toggle()
        {
            On = !On;
            if (On)
            {
                Notify.Show($"Super speed on (x{Multiplier:0.#})");
            }
            else
            {
                Reset();
                Notify.Show("Super speed off");
            }
        }

        internal static void Reset()
        {
            OVRPlayerController player = Refs.ovrController;
            if (player != null)
                player.SetMoveScaleMultiplier(1f);
        }
    }
}
