using System;
using GlobalSettings;
using HarmonyLib;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class WreathOfPurity
{
    [HarmonyPatch(typeof(Gameplay), nameof(Gameplay.Awake))]
    [HarmonyPostfix]
    private static void BecomeYellowAndLastForever(Gameplay __instance)
    {
        __instance.maggotCharm.type = ToolItemType.Yellow;
        __instance.maggotCharmEnterWaterAddTime = 0;
        __instance.maggotCharmHealthLossTime = Single.MaxValue;
    }
}