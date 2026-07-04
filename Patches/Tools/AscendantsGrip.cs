using GlobalSettings;
using HarmonyLib;
using UnityEngine;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class AscendantsGrip
{
    private const float AttackRangeMultiplier = 1.3f;

    [HarmonyPatch(typeof(NailAttackBase), nameof(NailAttackBase.OnSlashStarting))]
    [HarmonyPostfix]
    private static void IncreaseAttackRange(NailAttackBase __instance)
    {
        // For Shaman Crest slash, presumably
        if (__instance.overrideLongNeedleScale)
            return;
        if (__instance.hc.cState.wallClinging && Gameplay.WallClingTool.IsEquipped)
        {
            var existingScale = __instance.transform.localScale;
            __instance.transform.localScale = new Vector3(existingScale.x * AttackRangeMultiplier,
                existingScale.y * AttackRangeMultiplier, existingScale.z);
        }
    }
}