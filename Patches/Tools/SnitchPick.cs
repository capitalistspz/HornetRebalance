using HarmonyLib;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class SnitchPick
{
    private record InitialRosaries
    {
        public int Small;
        public int Medium;
        public int Large;
        public int LargeSmooth;
    }
    
    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.StealCurrency))]
    [HarmonyPrefix]
    private static void StoreInitialRosaries(HealthManager __instance, out InitialRosaries __state)
    {
        __state = new InitialRosaries
        {
            Small = __instance.smallGeoDrops,
            Medium = __instance.mediumGeoDrops,
            Large = __instance.largeGeoDrops,
            LargeSmooth = __instance.largeSmoothGeoDrops
        };
    }

    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.StealCurrency))]
    [HarmonyPostfix]
    private static void RestoreInitialRosaries(HealthManager __instance, InitialRosaries __state)
    {
        __instance.smallGeoDrops = __state.Small;
        __instance.mediumGeoDrops = __state.Medium;
        __instance.largeGeoDrops = __state.Large;
        __instance.largeSmoothGeoDrops = __state.LargeSmooth;
    }

    [HarmonyPatch(typeof(HealthManager.StealLagHit), MethodType.Constructor, typeof(HealthManager), typeof(bool))]
    [HarmonyPostfix]
    private static void DoSameDamageAsNeedle(HealthManager.StealLagHit __instance)
    {
        __instance.NailDamageMultiplier = 1.0f;
    }
}