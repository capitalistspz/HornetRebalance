using GlobalSettings;
using HarmonyLib;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class VoltFilament
{
    private const float DamageMultiplier = 1.2f;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Gameplay), nameof(Gameplay.Awake))]
    private static void ReduceDamageMult(Gameplay __instance)
    {
        __instance.zapDamageMult = DamageMultiplier;
    }
}