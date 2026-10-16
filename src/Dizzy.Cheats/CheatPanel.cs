using System.Collections;
using UnityEngine;

namespace Dizzy.Cheats
{
    // IMGUI panel: cheat switches, needs sliders, gold, and the item spawn window.
    //
    // Cursor rules, learned the hard way against the Esc menu and Sailwind Console
    // (which opens on the same key, but only while the Esc menu is up):
    // - Open only over plain gameplay, never over another cursor menu, so the panel
    //   always owns the cursor it frees and closing can just hand back gameplay.
    // - Never touch MouseLook.ToggleMouseLook. Freeing the cursor already stops
    //   mouse look, and the Esc menu saves that flag when it opens and restores it
    //   when it closes; a panel that cleared it left mouse look dead after resuming.
    // - When the Esc menu, a load or the title screen takes over, step aside without
    //   touching the cursor. That menu owns it now and restores gameplay itself.
    // - Esc closes the panel and nothing else, like the game's own shop window; a
    //   second Esc opens the Esc menu.
    internal static class CheatPanel
    {
        private const float Width = 420f;

        // Buttons, not a text box: InventoryHotkeys reads 1-5 even in cursor menus,
        // so typing an amount would grab items from the inventory.
        private static readonly int[] GoldAmounts = { 100, 1000, 10000, 100000 };
        private static readonly string[] GoldAmountLabels = { "100", "1,000", "10,000", "100,000" };
        private static int _goldAmountIndex = 1;

        private static bool _open;
        private static bool _centered;
        private static Rect _rect;
        private static int _lastToggleFrame = -1;

        private static Coroutine _cleanup;

        // Frame in which Esc closed the panel. StartMenu.LateUpdate skips that frame so
        // the same press does not also open the Esc menu.
        internal static int EscapeFrame = -1;

        // The panel key works while the panel is open, when Hotkeys.Allowed is false
        // because the panel itself holds the cursor menu.
        internal static void Update()
        {
            // The item window switches the game's keys off while it is open, so it must
            // never outlive the panel.
            if (!_open && ItemSpawnWindow.IsOpen)
                ItemSpawnWindow.Close();

            if (_open && OtherMenuTookOver())
            {
                StepAside();
                return;
            }

            // Esc closes one window at a time, the way it closes the game's shop and
            // mission windows: the item window first, then the panel.
            if (_open && MenuKeyDown())
            {
                EscapeFrame = Time.frameCount;
                if (ItemSpawnWindow.IsOpen)
                    ItemSpawnWindow.Close();
                else
                    SetOpen(false);
                return;
            }

            HandleToggle();

            if (_open)
                HoldMenuCursor();
        }

        // The keys StartMenu.LateUpdate opens the Esc menu with.
        private static bool MenuKeyDown() =>
            Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.F10) || Input.GetKeyDown(KeyCode.JoystickButton6);

        internal static void LateUpdate()
        {
            if (_open && !OtherMenuTookOver())
                HoldMenuCursor();
        }

        // The Esc menu pauses (time scale 0); loads and the title screen stop play.
        private static bool OtherMenuTookOver() =>
            !GameState.playing || GameState.currentlyLoading || Time.timeScale == 0f;

        internal static void Close() => SetOpen(false);

        internal static void OnGUI()
        {
            if (!_open)
                return;

            if (!_centered)
            {
                _rect = new Rect((Screen.width - Width) * 0.5f, Screen.height * 0.2f, Width, 0f);
                _centered = true;
            }

            _rect = GUILayout.Window(Plugin.PluginGuid.GetHashCode(), _rect, Draw, Plugin.PluginName, GUILayout.Width(Width));
            ItemSpawnWindow.OnGUI();
        }

        private static void HandleToggle()
        {
            if (Time.frameCount == _lastToggleFrame || !Hotkeys.Down(CheatsConfig.PanelKey))
                return;
            // Hotkeys.Allowed is false in any cursor menu, including the Esc menu where
            // Sailwind Console takes the same key.
            if (!_open && (!Hotkeys.Allowed || Time.timeScale == 0f))
                return;

            _lastToggleFrame = Time.frameCount;
            SetOpen(!_open);
        }

        private static void Draw(int id)
        {
            GUILayout.Space(4f);

            if (GUILayout.Toggle(FlyMode.On, $" Fly ({CheatsConfig.FlyKey.Value})") != FlyMode.On)
                FlyMode.Toggle();
            if (GUILayout.Toggle(SuperSpeed.On, $" Super speed x{SuperSpeed.Multiplier:0.#} ({CheatsConfig.SpeedKey.Value})") != SuperSpeed.On)
                SuperSpeed.Toggle();
            if (GUILayout.Toggle(GodMode.On, $" God mode: needs stay where they are ({CheatsConfig.GodModeKey.Value})") != GodMode.On)
                GodMode.Toggle(fill: false);

            GUILayout.Space(6f);
            GUILayout.Label(BoatMotor.Status);
            GUILayout.Label($"{CheatsConfig.ThrottleUpKey.Value}/{CheatsConfig.ThrottleDownKey.Value} throttle, {CheatsConfig.TurnLeftKey.Value}/{CheatsConfig.TurnRightKey.Value} turn, {CheatsConfig.MotorStopKey.Value} all stop");

            GUILayout.Space(8f);
            GUILayout.Label("Needs (100 = full, 0 = empty)");
            PlayerNeeds.food = NeedSlider("Food", PlayerNeeds.food, ref PlayerNeeds.foodDebt);
            PlayerNeeds.water = NeedSlider("Water", PlayerNeeds.water);
            PlayerNeeds.sleep = NeedSlider("Sleep", PlayerNeeds.sleep, ref PlayerNeeds.sleepDebt);
            PlayerNeeds.vitamins = NeedSlider("Vitamins", PlayerNeeds.vitamins);
            PlayerNeeds.protein = NeedSlider("Protein", PlayerNeeds.protein);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Fill all"))
                GodMode.FillNeeds();
            if (GUILayout.Button("Close"))
                SetOpen(false);
            GUILayout.EndHorizontal();

            if (!GodMode.On)
                GUILayout.Label("Needs keep draining from what you set. Turn god mode on to hold them.");

            GUILayout.Space(8f);
            DrawGold();

            GUILayout.Space(8f);
            if (GUILayout.Button("Spawn an Item"))
                ItemSpawnWindow.Toggle(_rect);

            GUI.DragWindow();
        }

        private static void DrawGold()
        {
            int[] purse = PlayerGold.currency;
            if (purse == null)
                return;

            GUILayout.Label("Gold");
            _goldAmountIndex = GUILayout.Toolbar(_goldAmountIndex, GoldAmountLabels);
            int amount = GoldAmounts[_goldAmountIndex];

            for (int i = 0; i < purse.Length; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(PlayerGold.GetCurrencyName(i), GUILayout.Width(130f));
                GUILayout.Label(purse[i].ToString("N0"), GUILayout.Width(90f));
                if (GUILayout.Button("Add"))
                    GoldCheat.Add(i, amount);
                if (GUILayout.Button("Remove"))
                    GoldCheat.Add(i, -amount);
                GUILayout.EndHorizontal();
            }
        }

        private static float NeedSlider(string label, float value)
        {
            float debt = 100f;
            return NeedSlider(label, value, ref debt);
        }

        // Raising a need also clears its debt. Otherwise vanilla repays the debt out of
        // the need every frame and the slider would not stick.
        private static float NeedSlider(string label, float value, ref float debt)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(70f));
            float next = GUILayout.HorizontalSlider(value, 0f, 100f, GUILayout.ExpandWidth(true));
            GUILayout.Label(value.ToString("0"), GUILayout.Width(32f));
            GUILayout.EndHorizontal();

            if (Mathf.Abs(next - value) < 0.01f)
                return value;

            next = Mathf.Round(next);
            if (next > 0f)
                debt = 100f;
            return next;
        }

        private static void SetOpen(bool open)
        {
            if (_open == open)
                return;

            _open = open;
            if (open)
            {
                HoldMenuCursor();
                return;
            }

            // The panel only opens over plain gameplay, so closing hands gameplay back.
            _centered = false;
            ItemSpawnWindow.Close();
            if (OtherMenuTookOver())
                return;
            MouseLook.ToggleMouseLookAndCursor(true);
            ScheduleCursorCleanup();
        }

        private static void StepAside()
        {
            _open = false;
            _centered = false;
            ItemSpawnWindow.Close();
        }

        // Sets inCursorMenu and frees the cursor; reapplied every frame in case the game
        // relocks it. Leaves the mouse look flag alone (see the class comment).
        private static void HoldMenuCursor()
        {
            MouseLook.ToggleMouseLookAndCursor(false);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private static void ScheduleCursorCleanup()
        {
            Plugin host = Plugin.Instance;
            if (host == null)
                return;

            if (_cleanup != null)
                host.StopCoroutine(_cleanup);
            _cleanup = host.StartCoroutine(CursorCleanup());
        }

        // The game can leave the cursor showing for a few frames after a menu closes.
        private static IEnumerator CursorCleanup()
        {
            yield return null;
            yield return null;
            HideStrayCursor();
            yield return new WaitForSeconds(0.25f);
            HideStrayCursor();
            yield return new WaitForSeconds(0.25f);
            HideStrayCursor();
            _cleanup = null;
        }

        private static void HideStrayCursor()
        {
            if (_open || GameState.inCursorMenu || !GameState.playing || !Cursor.visible)
                return;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
