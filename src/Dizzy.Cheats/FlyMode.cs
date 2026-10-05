using UnityEngine;

namespace Dizzy.Cheats
{
    // Sailwind walks the player on a still copy of the boat (the walk col) and
    // mirrors the controller's local position onto the observer that carries the
    // camera on the real boat. Fly moves in the observer's space and maps the step
    // into the controller's space, so it works on deck, ashore, and between them.
    internal static class FlyMode
    {
        private const float MaxFallSpeed = 25f;
        private const float GroundedFallSpeed = -1f;

        // False when the movement patch could not be applied; the toggle then says so.
        internal static bool Available;

        internal static bool On;

        private static float _verticalSpeed;
        private static bool _grounded;

        internal static bool Active => On && GameState.playing && !GameState.currentlyLoading;

        // In the air, not standing on anything (a flappy landing counts as standing).
        internal static bool Airborne => Active && !_grounded;

        internal static void Update(bool keys)
        {
            if (keys && Hotkeys.Down(CheatsConfig.FlyKey))
                Toggle();
        }

        internal static void Toggle()
        {
            if (!Available)
            {
                Notify.Show("Fly is unavailable: the movement patch failed (see LogOutput.log)");
                return;
            }

            On = !On;
            _verticalSpeed = 0f;
            _grounded = false;

            string jump = KeyName(InputName.Jump);
            if (!On)
                Notify.Show("Fly off");
            else if (CheatsConfig.Style.Value == FlyStyle.Flappy)
                Notify.Show($"Fly on: hold {jump} to rise, let go to fall");
            else
                Notify.Show($"Fly on{(CheatsConfig.NoClip.Value ? " (no clip)" : "")}: {jump} up, {KeyName(InputName.Crouch)} down");
        }

        // Follows the player's own key bindings.
        private static string KeyName(InputName input) => GameInput.GetKeyCode(input, false, false).ToString();

        // Runs in place of OVRPlayerController.UpdateController while fly is on.
        internal static void Move(OVRPlayerController player, CharacterController controller)
        {
            float dt = Time.deltaTime;
            if (dt <= 0f)
                return;

            PlayerControllerMirror observer = Refs.observerMirror;
            Camera camera = Camera.main;
            Transform view = camera != null ? camera.transform : observer != null ? observer.transform : player.transform;

            bool hover = CheatsConfig.Style.Value == FlyStyle.Hover;
            bool input = !GameState.inCursorMenu;

            // Hover flies where you look; flappy keeps W/S level and leaves height to the jump key.
            Vector3 forward = hover ? view.forward : Flatten(view.forward);
            Vector3 right = Flatten(view.right);

            Vector3 direction = Vector3.zero;
            if (input)
            {
                if (GameInput.GetKey(InputName.MoveUp))
                    direction += forward;
                if (GameInput.GetKey(InputName.MoveDown))
                    direction -= forward;
                if (GameInput.GetKey(InputName.MoveRight))
                    direction += right;
                if (GameInput.GetKey(InputName.MoveLeft))
                    direction -= right;
                if (hover && GameInput.GetKey(InputName.Jump))
                    direction += Vector3.up;
                if (hover && GameInput.GetKey(InputName.Crouch))
                    direction -= Vector3.up;
            }

            if (direction.sqrMagnitude > 1f)
                direction.Normalize();

            float speed = CheatsConfig.FlySpeed.Value;
            if (SuperSpeed.On)
                speed *= SuperSpeed.Multiplier;
            if (input && GameInput.GetKey(InputName.Run))
                speed *= CheatsConfig.FlyBoost.Value;

            Vector3 step = direction * (speed * dt);

            if (!hover)
            {
                if (input && GameInput.GetKey(InputName.Jump))
                    _verticalSpeed = CheatsConfig.FlappyRiseSpeed.Value;
                else
                    _verticalSpeed = Mathf.Max(_verticalSpeed - CheatsConfig.FlappyGravity.Value * dt, -MaxFallSpeed);

                step.y += _verticalSpeed * dt;
            }

            Transform observerParent = observer != null ? observer.transform.parent : null;
            Vector3 localStep = observerParent != null ? observerParent.InverseTransformVector(step) : step;

            if ((hover && CheatsConfig.NoClip.Value) || !controller.enabled)
            {
                player.transform.localPosition += localStep;
                _grounded = false;
                return;
            }

            Transform controllerParent = player.transform.parent;
            controller.Move(controllerParent != null ? controllerParent.TransformVector(localStep) : localStep);
            _grounded = controller.isGrounded;

            // Landed: stop building fall speed so the next flap starts clean.
            if (!hover && controller.isGrounded && _verticalSpeed < 0f)
                _verticalSpeed = GroundedFallSpeed;
        }

        private static Vector3 Flatten(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude > 0.0001f ? v.normalized : Vector3.zero;
        }
    }
}
