using BepInEx.Configuration;
using UnityEngine;

namespace Dizzy.Cheats
{
    internal static class Hotkeys
    {
        // Ignore cheat keys in menus, shops and while a save loads.
        internal static bool Allowed =>
            GameState.playing && !GameState.inCursorMenu && !GameState.currentlyLoading;

        internal static bool Down(ConfigEntry<KeyCode> key) =>
            key.Value != KeyCode.None && Input.GetKeyDown(key.Value);

        internal static bool Held(ConfigEntry<KeyCode> key) =>
            key.Value != KeyCode.None && Input.GetKey(key.Value);
    }

    internal static class Notify
    {
        internal static void Show(string text)
        {
            Plugin.Log.LogInfo(text);
            NotificationUi ui = NotificationUi.instance;
            if (ui != null)
                ui.ShowNotification(text, 2f);
        }
    }
}
