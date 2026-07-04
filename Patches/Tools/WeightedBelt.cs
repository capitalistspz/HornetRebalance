using GlobalSettings;
using HarmonyLib;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class WeightedBelt
{
    [HarmonyPatch(typeof(Gameplay), nameof(Gameplay.Awake))]
    [HarmonyPostfix]
    private static void Postfix(Gameplay __instance)
    {
        __instance.weightedAnkletRecoilSteps = 0;
    }
}