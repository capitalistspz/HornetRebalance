using GlobalSettings;
using HarmonyLib;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class LongClaw
{
    [HarmonyPatch(typeof(HeroController.ConfigGroup), nameof(HeroController.ConfigGroup.Setup))]
    [HarmonyPostfix]
    private static void AffectNeedleStrike(HeroController.ConfigGroup __instance)
    {
        // Looks like TeamCherry forgot something
        if (__instance.ChargeSlash)
            __instance.ChargeSlash.AddComponentIfNotPresent<NailAttackBase>();
    }
}