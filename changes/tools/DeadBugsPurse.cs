using GlobalSettings;
using HarmonyLib;

namespace Rebalance.changes.tools;

public static class DeadBugsPurse
{
    [HarmonyPatch(typeof(Gameplay), nameof(Gameplay.Awake))]
    public static class Patch
    {
        public static void Postfix(Gameplay __instance)
        {
            // Team Cherry does not know how percentages work
            __instance.deadPurseHoldPercent = 1;
        }
    }
}