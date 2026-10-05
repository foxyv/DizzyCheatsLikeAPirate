using UnityEngine;

namespace Dizzy.Cheats
{
    // Pushes the boat you are standing on. Crest's BoatProbes has its own debug
    // engine, but it reads the vanilla W/S axis and A/D keys, so it would drive the
    // boat whenever you walk. This applies its own force to the boat's rigidbody.
    internal static class BoatMotor
    {
        private static int _throttle;
        private static float _turn;
        private static Transform _boat;
        private static Rigidbody _body;

        internal static void Update(bool keys)
        {
            // GameState.currentBoat is the world boat you stand on; null once you step off.
            Transform aboard = GameState.currentBoat;

            // ReferenceEquals: a boat destroyed under a running motor must still stop it.
            if (!ReferenceEquals(_boat, null) && aboard != _boat)
            {
                bool wasRunning = _throttle != 0;
                StopMotor();
                if (wasRunning)
                    Notify.Show("All stop: you left the boat");
            }

            _turn = 0f;
            if (!keys || aboard == null)
                return;

            if (Hotkeys.Held(CheatsConfig.TurnLeftKey))
                _turn -= 1f;
            if (Hotkeys.Held(CheatsConfig.TurnRightKey))
                _turn += 1f;

            int throttle = _throttle;
            if (Hotkeys.Down(CheatsConfig.ThrottleUpKey))
                throttle++;
            if (Hotkeys.Down(CheatsConfig.ThrottleDownKey))
                throttle--;
            if (Hotkeys.Down(CheatsConfig.MotorStopKey))
                throttle = 0;

            throttle = Mathf.Clamp(throttle, -CheatsConfig.MotorReverseSteps.Value, CheatsConfig.MotorForwardSteps.Value);

            if (_turn != 0f || throttle != 0)
                Attach(aboard);

            if (throttle != _throttle)
            {
                _throttle = throttle;
                Notify.Show(ThrottleText());
            }
        }

        internal static void FixedUpdate()
        {
            if (_throttle == 0 && _turn == 0f)
                return;
            if (!GameState.playing || GameState.recovering || _body == null || _body.isKinematic)
                return;

            if (_throttle != 0)
            {
                Vector3 forward = _body.transform.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude > 0.0001f)
                    _body.AddForce(forward.normalized * (_throttle * CheatsConfig.MotorStepAcceleration.Value), ForceMode.Acceleration);
            }

            if (_turn != 0f)
                _body.AddTorque(Vector3.up * (_turn * CheatsConfig.MotorTurnAcceleration.Value), ForceMode.Acceleration);
        }

        private static void Attach(Transform aboard)
        {
            // Look up once per boat, so a boat with no rigidbody warns once, not every frame.
            if (aboard == _boat)
                return;

            _boat = aboard;
            // The rigidbody sits on the boat root, the parent of the world boat
            // (the same object vanilla's debug engine and GameState.lastBoat use).
            Transform root = aboard.parent != null ? aboard.parent : aboard;
            _body = root.GetComponent<Rigidbody>();
            if (_body == null)
                _body = aboard.GetComponentInParent<Rigidbody>();
            if (_body == null)
                Plugin.Log.LogWarning($"Motor: no rigidbody found on {root.name}.");
        }

        private static void StopMotor()
        {
            _throttle = 0;
            _turn = 0f;
            _boat = null;
            _body = null;
        }

        internal static string Status =>
            GameState.currentBoat == null ? "Motor: board a boat to use it" : "Motor: " + ThrottleText();

        private static string ThrottleText()
        {
            if (_throttle > 0)
                return $"Ahead {_throttle} of {CheatsConfig.MotorForwardSteps.Value}";
            if (_throttle < 0)
                return $"Astern {-_throttle} of {CheatsConfig.MotorReverseSteps.Value}";
            return "All stop";
        }
    }
}
