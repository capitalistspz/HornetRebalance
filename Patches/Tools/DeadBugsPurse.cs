using GlobalSettings;
using HarmonyLib;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class DeadBugsPurse
{
    [HarmonyPatch(typeof(Gameplay), nameof(Gameplay.Awake))]
    [HarmonyPostfix]
    private static void ChangeDeathDropAmount(Gameplay __instance)
    {
        // Team Cherry does not know how percentages work
        __instance.deadPurseHoldPercent = 1;
    }
}