using HarmonyLib;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class MagnetiteBrooch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CurrencyObjectBase), nameof(CurrencyObjectBase.Awake))]
    private static void AttractShards(CurrencyObjectBase __instance)
    {
        if (__instance.CurrencyType == CurrencyType.Shard)
        {
            __instance.magnetTool = ToolItemManager.GetToolByName("Rosary Magnet");
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.LeaveScene))]
    private static void CollectOnLeave(HeroController __instance)
    {
        foreach (var currencyObject in CurrencyObjectBase._currencyObjects.List)
        {
            if (currencyObject.isActiveAndEnabled && currencyObject.isMoving)
                currencyObject.DoCollect();
        }
    }
    //TODO: figure out how to prevent currency break in spikes when brooch is equipped
    // `CurrencyObjectBase.Break` and `CurrencyObjectBase.Disable(int)` are not called when landing in spikes
    // `CurrencyObjectBase.isBroken` is not set when landing in spikes
}