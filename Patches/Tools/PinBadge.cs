using HarmonyLib;
using JetBrains.Annotations;
using Rebalance.MiscUtils;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class PinBadge
{
    private const float ChargeTime = 0.75f;
    private const float DamageMultiplier = 1.15f;
    
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
    [HarmonyPostfix]
    private static void SetNeedleStrikeTime(HeroController __instance)
    {
        __instance.NAIL_CHARGE_TIME_QUICK = ChargeTime;
    }

    [DoDamageEnemiesHook]
    [UsedImplicitly]
    public static void IncreaseNeedleStrikeDamage(DamageEnemies damager)
    {
        var hero = HeroController.instance;
        if (!hero.NailChargeTimeQuickTool.IsEquipped)
            return;

        var currentSlash = hero.CurrentConfigGroup.ChargeSlash;
        if (!damager.gameObject.transform.IsChildOf(currentSlash.transform))
            return;

        damager.tempDamageStack.AddMultiplier(DamageMultiplier);
    }
    
}