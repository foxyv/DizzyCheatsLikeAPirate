using UnityEngine;

namespace Dizzy.Cheats.Patches
{
    internal static class PlayerMovementPatches
    {
        // OVRPlayerController.UpdateController: walking, gravity and the grounded snap.
        // While flying, skip it and move with FlyMode instead. Nothing in Sailwind
        // subscribes to the CameraUpdated / PreCharacterMove events it raises.
        internal static bool UpdateControllerPrefix(
            OVRPlayerController __instance,
            CharacterController ___Controller,
            ref Vector3 ___MoveThrottle,
            ref float ___FallSpeed,
            ref float ___buttonRotation)
        {
            if (!FlyMode.Active || ___Controller == null)
                return true;

            // Clear vanilla momentum so turning fly off does not fling or drop you.
            ___MoveThrottle = Vector3.zero;
            ___FallSpeed = 0f;
            ___buttonRotation = 0f;
            // PlayerNeeds drains food and water from this; flying is free.
            __instance.outLastMovement = Vector3.zero;

            FlyMode.Move(__instance, ___Controller);
            return false;
        }

        // Super speed: report the step at normal pace so running fast does not
        // multiply the food and water cost (it scales with the step squared).
        internal static void UpdateControllerPostfix(OVRPlayerController __instance, bool __runOriginal)
        {
            if (!__runOriginal || !SuperSpeed.On)
                return;

            float multiplier = SuperSpeed.Multiplier;
            if (multiplier > 1f)
                __instance.outLastMovement /= multiplier;
        }

        // PlayerClimb.Update runs the jump and its own gravity. Skip it while flying,
        // but keep the camera rotation mirror it maintains in sync.
        internal static bool PlayerClimbUpdatePrefix(
            PlayerClimb __instance,
            ref float ___fallMomentum,
            ref bool ___jumping)
        {
            if (!FlyMode.Active)
                return true;

            ___fallMomentum = 0f;
            ___jumping = false;
            GameState.onRatlines = false;

            Transform mirror = __instance.camRotMirror;
            Transform rig = Refs.ovrCameraRig;
            if (mirror != null && rig != null)
            {
                mirror.localPosition = rig.localPosition;
                mirror.localRotation = rig.localRotation;
            }

            return false;
        }

        // PlayerCrouching.Update toggles crouch on the crouch key, which fly uses to
        // sink. Leave the head height where it is while flying.
        internal static bool PlayerCrouchingUpdatePrefix() => !FlyMode.Active;
    }
}
