using GlobalSettings;
using HarmonyLib;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class LongClaw
{
    private const float HorizontalScale = 1.25f;

    [HarmonyPatch(typeof(Gameplay), nameof(Gameplay.Awake))]
    [HarmonyPostfix]
    private static void ChangeHScale(Gameplay __instance)
    {
        __instance.longNeedleMultiplier.x = HorizontalScale;
        // NOTE: Witch is unaffected by this
    }
    
    [HarmonyPatch(typeof(HeroController.ConfigGroup), nameof(HeroController.ConfigGroup.Setup))]
    [HarmonyPostfix]
    private static void AffectNeedleStrike(HeroController.ConfigGroup __instance)
    {
        // Looks like TeamCherry forgot something
        if (__instance.ChargeSlash)
            __instance.ChargeSlash.AddComponentIfNotPresent<NailAttackBase>();
    }
}