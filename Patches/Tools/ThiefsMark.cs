using GlobalSettings;
using HarmonyLib;
using TeamCherry.SharedUtils;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class ThiefsMark
{
    private const float DropMultiplier = 35.0f / 30.0f;

    [HarmonyPatch(typeof(Gameplay), nameof(Gameplay.Awake))]
    [HarmonyPostfix]
    private static void IncreaseDropsAndAdjustLossChance(Gameplay __instance)
    {
        __instance.thiefCharmGeoSmallIncrease *= DropMultiplier;
        __instance.thiefCharmGeoMedIncrease *= DropMultiplier;
        __instance.thiefCharmGeoLargeIncrease *= DropMultiplier;
        __instance.thiefCharmGeoLoss = new MinMaxFloat(0.02f, 0.02f);
        __instance.thiefCharmGeoLossCap = new MinMaxInt(10, 10);
    }
}