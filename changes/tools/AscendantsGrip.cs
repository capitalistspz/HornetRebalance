using GlobalSettings;
using HarmonyLib;
using UnityEngine;

namespace Rebalance.changes.tools;

public static class AscendantsGrip
{
    [HarmonyPatch]
    public static class IncreaseAttackRangePatch
    {
        private const float ScaleMultiplier = 1.3f;
        [HarmonyPatch(typeof(NailAttackBase), nameof(NailAttackBase.OnSlashStarting))]
        [HarmonyPostfix]
        public static void NailAttackScale(NailAttackBase __instance)
        {
            // For Shaman Crest slash, presumably
            if (__instance.overrideLongNeedleScale)
                return;
            if (__instance.hc.cState.wallClinging && Gameplay.WallClingTool.IsEquipped)
            {
                var existingScale = __instance.transform.localScale;
                __instance.transform.localScale = new Vector3(existingScale.x * ScaleMultiplier, existingScale.y * ScaleMultiplier, existingScale.z);
            }
        }
    }
}