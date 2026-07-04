using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class DruidsEye
{
    private const int DruidsEyePips = 3;
    private const int DruidsEyesPips = 4;
    

    [HarmonyPatch(typeof(HeroController), nameof(HeroController.DoMossToolHit))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> IncreaseMossPips(IEnumerable<CodeInstruction> instructions)
    {
        var match = new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Call))
            .Set(OpCodes.Ldc_I4, DruidsEyePips)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldc_I4_2),
                new CodeMatch(OpCodes.Call))
            .Set(OpCodes.Ldc_I4, DruidsEyesPips);
        if (match.IsValid) 
            RebalancePlugin.Logger.LogInfo("Successfully patched Druid's Eye pip count");
        else
            RebalancePlugin.Logger.LogError("Failed to patch Druid's Eye pip count");
        return match.InstructionEnumeration();
    }

    [HarmonyPatch(typeof(EnemyDeathEffects), nameof(EnemyDeathEffects.RecordKillForJournal))]
    [HarmonyPrefix]
    public static void GetMossPipsOnEnemyKill(EnemyDeathEffects __instance)
    {
        var hm = __instance.GetComponent<HealthManager>();
        if (hm == null || !hm.WillAwardJournalKill)
            return;
        HeroController.instance.DoMossToolHit();
    }
}