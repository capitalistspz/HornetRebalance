using System;
using GlobalSettings;
using HarmonyLib;
using Unity.Mathematics.Geometry;

namespace Rebalance.changes.tools;

public class WreathOfPurity
{
    [HarmonyPatch]
    public static class Patch
    {
        [HarmonyPatch(typeof(Gameplay), nameof(Gameplay.Awake))]
        [HarmonyPostfix]
        public static void BecomeYellowAndLastForever(Gameplay __instance)
        {
            __instance.maggotCharm.type = ToolItemType.Yellow;
            __instance.maggotCharmEnterWaterAddTime = 0;
            __instance.maggotCharmHealthLossTime = Single.MaxValue;
        }
    }
}