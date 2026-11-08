using HarmonyLib;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class MagnetiteDice
{
    private static int _hitCounter;
    
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.NailHitEnemy))]
    [HarmonyPostfix]
    private static void IncrementCounter()
    {
        _hitCounter += 1;
    }

    [HarmonyPatch(typeof(ToolItemManager), nameof(ToolItemManager.SendEquippedChangedEvent))]
    [HarmonyPostfix]
    private static void WatchToolEquips()
    {
        _hitCounter = 0;
    }

    [HarmonyPatch(typeof(HeroController), nameof(HeroController.GetLuckyDiceShieldThreshold))]
    [HarmonyPostfix]
    private static void OverrideShieldThreshold(ref int __result)
    {
        if (_hitCounter >= 20)
        {
            __result = 100;
            _hitCounter = 0;
        }
        else
            __result = 0;
    }
}