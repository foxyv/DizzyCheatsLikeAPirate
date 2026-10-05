using UnityEngine;

namespace Dizzy.Cheats.Patches
{
    internal static class StartMenuPatches
    {
        // StartMenu.LateUpdate only handles Esc: close the top game window (shop,
        // currency exchange, port missions), or else open the Esc menu. When that press
        // already closed the cheat panel, skip it, so Esc closes one window at a time.
        // Without this patch the panel still closes; the Esc menu just opens as well.
        internal static bool LateUpdatePrefix() => CheatPanel.EscapeFrame != Time.frameCount;
    }
}
