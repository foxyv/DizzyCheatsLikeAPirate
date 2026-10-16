using System;
using System.Reflection;
using HarmonyLib;

namespace Dizzy.Cheats.Patches
{
    // Patches one method at a time so a member moved by a game update only
    // disables the cheat that needs it, not the whole plugin.
    internal static class PatchApplier
    {
        internal static void Apply(Harmony harmony)
        {
            FlyMode.Available = TryPatch(
                harmony,
                typeof(OVRPlayerController),
                "UpdateController",
                typeof(PlayerMovementPatches),
                nameof(PlayerMovementPatches.UpdateControllerPrefix),
                nameof(PlayerMovementPatches.UpdateControllerPostfix));

            TryPatch(
                harmony,
                typeof(PlayerClimb),
                "Update",
                typeof(PlayerMovementPatches),
                nameof(PlayerMovementPatches.PlayerClimbUpdatePrefix),
                null);

            TryPatch(
                harmony,
                typeof(PlayerCrouching),
                "Update",
                typeof(PlayerMovementPatches),
                nameof(PlayerMovementPatches.PlayerCrouchingUpdatePrefix),
                null);

            TryPatch(
                harmony,
                typeof(BoatMass),
                nameof(BoatMass.UpdateMass),
                typeof(BoatMassPatches),
                null,
                nameof(BoatMassPatches.UpdateMassPostfix));

            TryPatch(
                harmony,
                typeof(StartMenu),
                "LateUpdate",
                typeof(StartMenuPatches),
                nameof(StartMenuPatches.LateUpdatePrefix),
                null);

            // The item window needs all three key reads silenced to keep typing from
            // driving the game; scroll and the 1-5 hotkeys are extras.
            Type[] inputName = { typeof(InputName) };
            bool keysBlocked = true;
            foreach (string read in new[] { nameof(GameInput.GetKey), nameof(GameInput.GetKeyDown), nameof(GameInput.GetKeyUp) })
            {
                keysBlocked &= TryPatch(
                    harmony,
                    typeof(GameInput),
                    read,
                    typeof(GameInputPatches),
                    nameof(GameInputPatches.KeyPrefix),
                    null,
                    inputName);
            }

            ItemSpawnWindow.GameKeysBlocked = keysBlocked;

            TryPatch(
                harmony,
                typeof(GameInput),
                nameof(GameInput.GetScrollAxis),
                typeof(GameInputPatches),
                nameof(GameInputPatches.ScrollPrefix),
                null);

            TryPatch(
                harmony,
                typeof(InventoryHotkeys),
                "Update",
                typeof(GameInputPatches),
                nameof(GameInputPatches.InventoryHotkeysUpdatePrefix),
                null);
        }

        private static bool TryPatch(Harmony harmony, Type target, string methodName, Type patchType, string prefixName, string postfixName, Type[] parameters = null)
        {
            MethodInfo original = AccessTools.DeclaredMethod(target, methodName, parameters ?? Type.EmptyTypes);
            MethodInfo prefix = prefixName != null ? AccessTools.DeclaredMethod(patchType, prefixName) : null;
            MethodInfo postfix = postfixName != null ? AccessTools.DeclaredMethod(patchType, postfixName) : null;
            if (original == null || (prefixName != null && prefix == null) || (postfixName != null && postfix == null))
            {
                Plugin.Log.LogWarning($"Cheats: missing {target.Name}.{methodName}, skip patch.");
                return false;
            }

            try
            {
                harmony.Patch(
                    original,
                    prefix: prefix != null ? new HarmonyMethod(prefix) : null,
                    postfix: postfix != null ? new HarmonyMethod(postfix) : null);
                return true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"Cheats: failed to patch {target.Name}.{methodName}: {e}");
                return false;
            }
        }
    }
}
