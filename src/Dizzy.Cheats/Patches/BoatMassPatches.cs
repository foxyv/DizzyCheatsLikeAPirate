using System.Collections.Generic;
using UnityEngine;

namespace Dizzy.Cheats.Patches
{
    internal static class BoatMassPatches
    {
        // Matches the player weight vanilla BoatMass.UpdateMass adds.
        private const float PlayerMass = 160f;

        // UpdateMass adds the player's 160 kg and anything held, then shifts the boat's
        // centre of mass by the player's position on the boat, height included. Hovering
        // above the deck acts like a weight on a tall pole and rolls the boat over.
        // While airborne, take that share back out. UpdateMass rebuilds mass and centre
        // of mass from scratch each frame, so nothing builds up.
        internal static void UpdateMassPostfix(
            BoatMass __instance,
            Rigidbody ___body,
            float ___selfMass,
            List<ItemRigidbody> ___itemsOnBoat)
        {
            if (!FlyMode.Airborne || ___body == null || ___selfMass <= 0f)
                return;

            // Same guard vanilla uses before adding the player.
            Transform aboard = GameState.currentBoat;
            PlayerControllerMirror observer = Refs.observerMirror;
            if (aboard == null || aboard.parent != __instance.transform || observer == null)
                return;

            // Vanilla counts held items (not in an inventory slot) at the player's position too.
            float weight = PlayerMass;
            if (___itemsOnBoat != null)
            {
                for (int i = 0; i < ___itemsOnBoat.Count; i++)
                {
                    ItemRigidbody item = ___itemsOnBoat[i];
                    if (item == null || item.GetCurrentInventorySlot() != null)
                        continue;

                    ShipItem shipItem = item.GetShipItem();
                    Rigidbody itemBody = item.GetBody();
                    if (shipItem != null && itemBody != null && shipItem.held)
                        weight += itemBody.mass;
                }
            }

            Vector3 shift = Quaternion.Euler(0f, -90f, 0f) * observer.transform.localPosition * (weight / ___selfMass) * __instance.leverageMult;
            ___body.mass -= weight;
            ___body.centerOfMass -= shift;
        }
    }
}
