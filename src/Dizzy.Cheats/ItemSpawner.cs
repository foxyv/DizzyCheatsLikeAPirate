using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dizzy.Cheats
{
    // Lists and spawns the items in PrefabsDirectory, the table the game and other
    // mods register every item prefab in (index = prefab index).
    internal static class ItemSpawner
    {
        internal sealed class Entry
        {
            internal int Index;
            internal string Name;
            internal string Detail;
            internal string Search;
        }

        private const float SpawnDistance = 1.5f;
        private const float StaggerStep = 0.4f;

        private static int _spawned;

        // Read fresh each time the window opens, so items other mods add are included.
        internal static List<Entry> BuildCatalog()
        {
            var entries = new List<Entry>();
            PrefabsDirectory prefabs = PrefabsDirectory.instance;
            if (prefabs == null || prefabs.directory == null)
                return entries;

            for (int i = 0; i < prefabs.directory.Length; i++)
            {
                GameObject prefab = prefabs.directory[i];
                if (prefab == null)
                    continue;

                ShipItem item = prefab.GetComponent<ShipItem>();
                if (item == null)
                    continue;

                // ShipItem.name is the in-game name; the GameObject name is the prefab's.
                string prefabName = OneLine(prefab.name);
                string name = OneLine(item.name);
                if (name.Length == 0)
                    name = prefabName;

                // Many prefab names already lead with their index ("110 lantern A").
                string detail = prefabName.StartsWith(i + " ", StringComparison.Ordinal)
                    ? prefabName
                    : "#" + i + (prefabName == name ? "" : " " + prefabName);

                entries.Add(new Entry
                {
                    Index = i,
                    Name = name,
                    Detail = detail,
                    Search = (name + " " + prefabName + " " + i).ToLowerInvariant()
                });
            }

            entries.Sort((a, b) =>
            {
                int byName = string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
                return byName != 0 ? byName : a.Index.CompareTo(b.Index);
            });
            return entries;
        }

        // Same recipe as IslandMarket.SpawnGood and a loaded save: the item is marked
        // sold before its own load step runs (one frame after Awake), so it sets itself
        // up as an owned item. ShipItem.Sell is not used: its OnBuy expects that load
        // step to have run already (a fishing rod's bobber is null until then).
        internal static void Spawn(Entry entry)
        {
            GameObject spawned = null;
            try
            {
                PrefabsDirectory prefabs = PrefabsDirectory.instance;
                GameObject prefab = prefabs != null && entry.Index < prefabs.directory.Length ? prefabs.directory[entry.Index] : null;
                if (prefab == null)
                {
                    Notify.Show($"{entry.Name} is no longer in the item list");
                    return;
                }

                Camera camera = Camera.main;
                Transform view = camera != null ? camera.transform : Refs.observerMirror.transform;
                Vector3 forward = view.forward;
                forward.y = 0f;
                forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : view.forward;

                // Stagger repeat spawns in a 3 x 3 grid so they do not start inside each other.
                int slot = _spawned++ % 9;
                Vector3 position = view.position + forward * SpawnDistance
                    + view.right * ((slot % 3 - 1) * StaggerStep)
                    + Vector3.up * (slot / 3 * StaggerStep - 0.2f);

                spawned = UnityEngine.Object.Instantiate(prefab, position, Quaternion.Euler(0f, view.eulerAngles.y, 0f));
                // A mod may keep its template switched off; the copy has to run.
                if (!spawned.activeSelf)
                    spawned.SetActive(true);

                ShipItem item = spawned.GetComponent<ShipItem>();
                item.sold = true;
                // Loose items live under the shifting world so origin shifts carry them.
                spawned.transform.parent = FloatingOriginManager.instance.transform;

                Good good = spawned.GetComponent<Good>();
                if (good != null)
                    good.RegisterAsMissionless();

                SaveablePrefab saveable = spawned.GetComponent<SaveablePrefab>();
                if (saveable != null)
                    saveable.RegisterToSave();

                Notify.Show($"Spawned {entry.Name}");
            }
            catch (Exception e)
            {
                if (spawned != null)
                    UnityEngine.Object.Destroy(spawned);
                Plugin.Log.LogWarning($"Could not spawn {entry.Name} (prefab {entry.Index}): {e}");
                Notify.Show($"Could not spawn {entry.Name} (see LogOutput.log)");
            }
        }

        private static string OneLine(string text) =>
            string.IsNullOrEmpty(text) ? "" : text.Replace("\r", "").Replace('\n', ' ').Trim();
    }
}
