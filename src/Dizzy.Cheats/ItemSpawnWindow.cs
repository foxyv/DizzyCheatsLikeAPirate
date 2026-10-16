using System.Collections.Generic;
using UnityEngine;

namespace Dizzy.Cheats
{
    // IMGUI window opened from the cheat panel: search the item list, spawn items.
    // It belongs to the panel: the panel closes it whenever the panel itself closes.
    internal static class ItemSpawnWindow
    {
        private const float Width = 460f;
        private const float ListHeight = 420f;
        private const int MaxRows = 200;
        private const string SearchControl = "DizzyCheatsItemSearch";

        // While true, GameInputPatches answers "not pressed" for the game's keys.
        internal static bool IsOpen { get; private set; }

        // False when those patches could not be applied; typing then also moves you.
        internal static bool GameKeysBlocked;

        private static List<ItemSpawner.Entry> _catalog = new List<ItemSpawner.Entry>();
        private static readonly List<ItemSpawner.Entry> Matches = new List<ItemSpawner.Entry>();
        private static string _search = "";
        private static string _appliedSearch;
        private static Vector2 _scroll;
        private static Rect _rect;
        private static bool _placed;
        private static bool _focusSearch;

        internal static void Toggle(Rect panel)
        {
            if (IsOpen)
            {
                Close();
                return;
            }

            _catalog = ItemSpawner.BuildCatalog();
            _appliedSearch = null;
            _scroll = Vector2.zero;
            _focusSearch = true;
            if (!_placed)
            {
                _rect = new Rect(Mathf.Min(panel.xMax + 12f, Screen.width - Width - 8f), panel.y, Width, 0f);
                _placed = true;
            }

            IsOpen = true;
        }

        internal static void Close() => IsOpen = false;

        internal static void OnGUI()
        {
            if (!IsOpen)
                return;

            _rect = GUILayout.Window(Plugin.PluginGuid.GetHashCode() + 1, _rect, Draw, "Spawn an Item", GUILayout.Width(Width));
        }

        private static void Draw(int id)
        {
            GUILayout.Space(4f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Search", GUILayout.Width(52f));
            GUI.SetNextControlName(SearchControl);
            _search = GUILayout.TextField(_search ?? "", 40, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Clear", GUILayout.Width(56f)))
            {
                _search = "";
                _focusSearch = true;
            }
            GUILayout.EndHorizontal();

            if (_focusSearch && Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl(SearchControl);
                _focusSearch = false;
            }

            // IMGUI lays the window out once, then replays that layout for each input
            // event. The row count may only change on the layout pass, or the replay
            // asks for controls that were never laid out and throws.
            if (Event.current.type == EventType.Layout)
                Refilter();

            GUILayout.Label(StatusText());

            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(ListHeight));
            int shown = Mathf.Min(Matches.Count, MaxRows);
            for (int i = 0; i < shown; i++)
            {
                ItemSpawner.Entry entry = Matches[i];
                GUILayout.BeginHorizontal();
                GUILayout.Label(entry.Name, GUILayout.Width(200f));
                GUILayout.Label(entry.Detail, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Spawn", GUILayout.Width(64f)))
                    ItemSpawner.Spawn(entry);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            if (!GameKeysBlocked)
                GUILayout.Label("Game keys could not be switched off: typing also moves you (see LogOutput.log).");

            if (GUILayout.Button("Close"))
                Close();

            GUI.DragWindow();
        }

        private static void Refilter()
        {
            string search = (_search ?? "").Trim().ToLowerInvariant();
            if (search == _appliedSearch)
                return;

            _appliedSearch = search;
            _scroll = Vector2.zero;
            Matches.Clear();
            for (int i = 0; i < _catalog.Count; i++)
            {
                if (search.Length == 0 || _catalog[i].Search.Contains(search))
                    Matches.Add(_catalog[i]);
            }
        }

        private static string StatusText()
        {
            if (_catalog.Count == 0)
                return "No items found. Load a save first.";
            if (Matches.Count == 0)
                return "No item matches that search.";
            if (Matches.Count > MaxRows)
                return $"Showing the first {MaxRows} of {Matches.Count} items. Type to narrow it down.";
            return Matches.Count == 1 ? "1 item. It appears in front of you." : $"{Matches.Count} items. They appear in front of you.";
        }
    }
}
