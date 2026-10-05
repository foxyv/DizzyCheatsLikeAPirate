using System;

namespace Dizzy.Cheats
{
    // PlayerGold.currency holds the four purses the save keeps (0 Al'Ankh Lions,
    // 1 Emerald Dragons, 2 Aestrin Crowns, 3 Gold Lions). PlayerGold.gold is a
    // leftover only the debug UI reads.
    internal static class GoldCheat
    {
        internal static void Add(int currency, int amount)
        {
            int[] purse = PlayerGold.currency;
            if (purse == null || currency < 0 || currency >= purse.Length)
                return;

            long next = Math.Max(0L, Math.Min(int.MaxValue, (long)purse[currency] + amount));
            int change = (int)(next - purse[currency]);
            purse[currency] = (int)next;

            string name = PlayerGold.GetCurrencyName(currency);
            Notify.Show(change >= 0 ? $"+{change:N0} {name}" : $"{change:N0} {name}");
        }
    }
}
