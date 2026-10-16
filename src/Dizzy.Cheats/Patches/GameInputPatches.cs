namespace Dizzy.Cheats.Patches
{
    // The item window's search box takes the keyboard, but the game still polls its
    // keys every frame: typing "wood" would walk, strafe and more. GameInput is what
    // movement, pick up, throw, inventory and camera mode all read, so while the
    // window is open it answers "not pressed". Mods that read UnityEngine.Input
    // directly are not covered.
    internal static class GameInputPatches
    {
        // GameInput.GetKey / GetKeyDown / GetKeyUp (InputName).
        internal static bool KeyPrefix(ref bool __result)
        {
            if (!ItemSpawnWindow.IsOpen)
                return true;

            __result = false;
            return false;
        }

        // GameInput.GetScrollAxis: the wheel scrolls the item list, not a winch.
        internal static bool ScrollPrefix(ref float __result)
        {
            if (!ItemSpawnWindow.IsOpen)
                return true;

            __result = 0f;
            return false;
        }

        // InventoryHotkeys.Update reads the 1-5 keys straight from Input, even in
        // cursor menus, so typing a prefab number would pull items from the inventory.
        internal static bool InventoryHotkeysUpdatePrefix() => !ItemSpawnWindow.IsOpen;
    }
}
