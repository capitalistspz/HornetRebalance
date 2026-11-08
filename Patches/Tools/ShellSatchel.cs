using GlobalSettings;
using HarmonyLib;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class ShellSatchel
{
    private const int ToolReplenishmentPeriod = 4;
    
    [HarmonyPatch(typeof(Gameplay), nameof(Gameplay.Awake))]
    [HarmonyPostfix]
    private static void SetBonusTools(Gameplay __instance)
    {
        __instance.shellSatchelToolIncrease = 0;
    }
    
    [HarmonyPatch(typeof(ToolItemLimiter), nameof(ToolItemLimiter.ReportToolUsed))]
    [HarmonyPostfix]
    private static void ReplenishUsage(ToolItem thrownTool)
    {
        if ((ToolItemLimiter._lastThrowNum % ToolReplenishmentPeriod) != 0)
            return;
        if (Gameplay.ShellSatchelTool.IsEquippedHud)
            thrownTool.CollectFree(1);
    }
}