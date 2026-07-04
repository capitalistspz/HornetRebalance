using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using GlobalSettings;
using HarmonyLib;

namespace Rebalance.Patches;

[HarmonyPatch]
public static class Hunter
{
    private const float DownAttackWindup = 0.04f;

    [HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
    [HarmonyPostfix]
    private static void ReduceDownAttackWindup(HeroController __instance)
    {
        // Feels hacky
        foreach (var configGroup in __instance.configs)
        {
            if (configGroup.Config != null && configGroup.Config.GetName() == "Default")
            {
                configGroup.Config.downspikeAnticTime = DownAttackWindup;
            }
        }
    }

    private static void SetPostHitFocus(HeroController heroController)
    {
        var gameplaySettings = Gameplay.Get();
        var hits = Math.Min(heroController.hunterUpgState.CurrentMeterHits,
            gameplaySettings.hunterCombo2Hits + gameplaySettings.hunterCombo2ExtraHits);
        var regularComboAmount = Gameplay.Get().hunterComboHits;
        heroController.hunterUpgState = new HeroController.HunterUpgCrestStateInfo
        {
            CurrentMeterHits = Math.Max(hits - regularComboAmount, 0)
        };
    }

    [HarmonyPatch(typeof(HeroController), nameof(HeroController.TakeDamage))]
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.DoSpecialDamage))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> ReduceFocusLoss(IEnumerable<CodeInstruction> instructions)
    {
        var newInstr = Transpilers.EmitDelegate<Action<HeroController>>(SetPostHitFocus);

        var match = new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(
                    OpCodes.Call,
                    AccessTools.Method(typeof(HeroController), nameof(HeroController.ResetHunterUpgCrestState))))
            .Set(newInstr.opcode, newInstr.operand);

        if (match.IsValid)
            RebalancePlugin.Logger.LogInfo("Patched focus loss reduction in hero damage function");
        else
            RebalancePlugin.Logger.LogError("Failed to patch focus loss reduction in hero damage function");

        return match.InstructionEnumeration();
    }
    
    // Not using `Transpilers.EmitDelegate` here because MonoMod is not happy with the stack size
    // (Seems to not pop arguments from the stack)

    private static void SetFocusBarValue(UiProgressBar bar, int meterHits, int maxHits)
    {
        bar.SetValueInstant(meterHits / (float)maxHits);
    }

    private static void SetBarAnim(BindOrbHudFrame frame, string fullAnimA, bool wasFullExtra)
    {
        if (frame == null || !wasFullExtra)
            return;
        frame.PlayFrameAnim(fullAnimA);
    }

    [HarmonyPatch(typeof(BindOrbHudFrame), nameof(BindOrbHudFrame.HunterCrestUpgradedRoutine), MethodType.Enumerator)]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var enumeratorType =
            ExtendedAccessTools.EnumeratorType(
                AccessTools.Method(typeof(BindOrbHudFrame), nameof(BindOrbHudFrame.HunterCrestUpgradedRoutine)));
        var matcher = new CodeMatcher(instructions)
            // Select correct animation
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(
                    OpCodes.Ldfld,
                    AccessTools.Field(enumeratorType, "extraBar")),
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Brtrue))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(new CodeInstruction(
                OpCodes.Ldfld,
                AccessTools.Field(enumeratorType, "fullAnimA")))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(new CodeInstruction(
                OpCodes.Ldfld,
                AccessTools.Field(enumeratorType, "<wasFullExtra>5__5")))
            .InsertAndAdvance(new CodeInstruction(
                OpCodes.Call,
                AccessTools.Method(typeof(Hunter), nameof(SetBarAnim))))
            // Set correct bar level
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(
                    OpCodes.Ldfld,
                    AccessTools.Field(enumeratorType, "bar")),
                new CodeMatch(OpCodes.Ldc_R4),
                new CodeMatch(
                    OpCodes.Callvirt,
                    AccessTools.Method(typeof(UiProgressBar), nameof(UiProgressBar.SetValueInstant))
                )
            )
            .Advance(-1)
            .SetAndAdvance(OpCodes.Ldloc_2, null)
            .SetAndAdvance(
                OpCodes.Ldfld,
                AccessTools.Field(typeof(HeroController.HunterUpgCrestStateInfo),
                    nameof(HeroController.HunterUpgCrestStateInfo.CurrentMeterHits)))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(new CodeInstruction(
                OpCodes.Ldfld,
                AccessTools.Field(enumeratorType, "maxHits")))
            .InsertAndAdvance(new CodeInstruction(
                OpCodes.Call,
                AccessTools.Method(typeof(Hunter), nameof(SetFocusBarValue))));

        if (matcher.IsValid)
            RebalancePlugin.Logger.LogInfo("Successfully patched the hunter focus bar");
        else
            RebalancePlugin.Logger.LogError("Hunter focus bar patch failed");

        return matcher.InstructionEnumeration();
    }
}