using GlobalSettings;
using HarmonyLib;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class EggOfFlealia
{
    private const int FlealiaSilkCost = 3;
    private const int RegularSilkCost = 4;

    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.SilkSkillCost), MethodType.Getter)]
    [HarmonyPrefix]
    private static void OverrideSilkCost(PlayerData __instance, ref int __result, ref bool __runOriginal)
    {
        // This patch is a little worrisome because a small enough getter could be inlined,
        // For now, it isn't
        __runOriginal = false;
        __result = Gameplay.FleaCharmTool.IsEquipped ? FlealiaSilkCost : RegularSilkCost;
    }
    
}