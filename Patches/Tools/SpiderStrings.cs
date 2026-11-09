#if false
using System.Collections.Generic;
using System.Reflection.Emit;
using GlobalSettings;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Silksong.FsmUtil;
using UnityEngine;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class SpiderStrings
{
    private const float SingDuration = 5;
    private const float RangeMultiplier = 1.5f;

    [HarmonyPatch(typeof(SpiderStrings), nameof(Gameplay))]
    [HarmonyPostfix]
    private static void SetRangeMultiplier(Gameplay __instance)
    {
        __instance.musicianCharmNeedolinRangeMult = RangeMultiplier;
    }

    [HarmonyPatch(typeof(EnemySingControl), nameof(EnemySingControl.OnEnter))]
    [HarmonyPostfix]
    private static void SetSingDuration(EnemySingControl __instance)
    {
        if (Gameplay.MusicianCharmTool.IsEquipped)
            __instance.singDuration = SingDuration;
    }
    
    private static HeroPerformanceRegion.AffectedState GetAffectedStateIgnorePerforming(Transform transform, bool ignoreRange, CheckHeroPerformanceRegionBase cpr)
    {
        if (!Gameplay.MusicianCharmTool.IsEquipped)
            return HeroPerformanceRegion.GetAffectedState(transform, ignoreRange);

        return cpr switch
        {
            // This is a heuristic instead of solid logic, 
            // CheckHeroPerformanceRegion in state A with ActiveInner/ActiveOuter moves to state B
            // which repeatedly triggers CheckHeroPerformanceRegion with None event
            CheckHeroPerformanceRegion cprv1 when FsmEvent.IsNullOrEmpty(cprv1.ActiveInner) &&
                                                  FsmEvent.IsNullOrEmpty(cprv1.ActiveOuter) 
                => HeroPerformanceRegion._instance.InternalGetAffectedRange(transform, ignoreRange),
            CheckHeroPerformanceRegionV2 cprv2 when FsmEvent.IsNullOrEmpty(cprv2.ActiveInner) &&
                                                    FsmEvent.IsNullOrEmpty(cprv2.ActiveOuter) 
                => HeroPerformanceRegion ._instance.InternalGetAffectedRange(transform, ignoreRange),
            _ => HeroPerformanceRegion.GetAffectedState(transform, ignoreRange)
        };
    }
    
    private static HeroPerformanceRegion.AffectedState GetAffectedStateWithRadiusIgnorePerforming(Transform transform, float radius, CheckHeroPerformanceRegionBase cpr)
    {
        if (!Gameplay.MusicianCharmTool.IsEquipped)
            return HeroPerformanceRegion.GetAffectedStateWithRadius(transform, radius);
        return cpr switch
        {
            CheckHeroPerformanceRegion cprv1 when FsmEvent.IsNullOrEmpty(cprv1.ActiveInner) &&
                                                  FsmEvent.IsNullOrEmpty(cprv1.ActiveOuter) 
                => HeroPerformanceRegion._instance.InternalGetAffectedRangeWithRadius(transform, radius),
            CheckHeroPerformanceRegionV2 cprv2 when FsmEvent.IsNullOrEmpty(cprv2.ActiveInner) &&
                                                    FsmEvent.IsNullOrEmpty(cprv2.ActiveOuter) 
                => HeroPerformanceRegion ._instance.InternalGetAffectedRangeWithRadius(transform, radius),
            _ => HeroPerformanceRegion.GetAffectedStateWithRadius(transform, radius)
        };
    }
    
    [HarmonyPatch(typeof(CheckHeroPerformanceRegionBase), nameof(CheckHeroPerformanceRegionBase.DoAction))]
    [HarmonyTranspiler]
    // Allow enemy to keep singing after Hornet stops playing the Needolin
    private static IEnumerable<CodeInstruction> PreventCancellation(IEnumerable<CodeInstruction> instructions)
    {
        // <--- HeroPerformanceRegion.GetAffectedState(this.TargetTransform, !this.UseNeedolinRange);
        // ---> GetAffectedStateIgnorePerforming(this.TargetTransform, !this.UseNeedolinRange, this);
        //
        // <--- HeroPerformanceRegion.GetAffectedStateWithRadius(this.TargetTransform, this.TargetRadius);
        // ---> GetAffectedStateWithRadiusIgnorePerforming(this.TargetTransform, this.TargetRadius, this);
        
        var withoutRadiusFn =
            AccessTools.Method(typeof(HeroPerformanceRegion), nameof(HeroPerformanceRegion.GetAffectedState));
        
        var withoutRadiusFnReplacement = 
            AccessTools.Method(typeof(SpiderStrings), nameof(GetAffectedStateIgnorePerforming));
        
        var withRadiusFn = AccessTools.Method(typeof(HeroPerformanceRegion),
            nameof(HeroPerformanceRegion.GetAffectedStateWithRadius));

        var withRadiusFnReplacement = AccessTools.Method(typeof(SpiderStrings), nameof(GetAffectedStateWithRadiusIgnorePerforming));
        
        var match = new CodeMatcher(instructions)
            .MatchStartForward(new CodeMatch(OpCodes.Call, withoutRadiusFn))
            .SetAndAdvance(OpCodes.Ldarg_0, null)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Call, withoutRadiusFnReplacement))
            .MatchStartForward(new CodeMatch(OpCodes.Call, withRadiusFn))
            .SetAndAdvance(OpCodes.Ldarg_0, null)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Call, withRadiusFnReplacement));
        
        return match.InstructionEnumeration();
    }
    
    
    private static void DecreaseSingDuration(GameObject gameObject, string eventName, bool isRecursive)
    {
        if (!Gameplay.MusicianCharmTool.IsEquipped)
        {
            FSMUtility.SendEventToGameObject(gameObject, eventName, isRecursive);
            return;
        }

        var controlFsm = gameObject.GetFsm("Control")!;
        var currentState = controlFsm.GetState(controlFsm.ActiveStateName)!;
        foreach (var action in currentState.ActiveActions)
            if (action is EnemySingControl singControl)
            {
                singControl.singDuration -= 1;
                if (singControl.singDuration < 0) 
                    FSMUtility.SendEventToGameObject(gameObject, eventName, isRecursive);
                break;
            }
        
    }

    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.TakeDamage))]
    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.ApplyTagDamage))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> OnlyDecreaseSingDurationOnDamage(IEnumerable<CodeInstruction> instructions)
    {
        var sendEventFn = AccessTools.Method(typeof(FSMUtility), nameof(FSMUtility.SendEventToGameObject), [
            typeof(GameObject), typeof(string), typeof(bool)
        ]);
        var replacementFn = AccessTools.Method(typeof(SpiderStrings), nameof(DecreaseSingDuration));

        // <--- FSMUtility.SendEventToGameObject(gameObject, "SING DURATION END");
        // ---> DecreaseSingDuration(gameObject, "SING DURATION END");
        
        var match = new CodeMatcher(instructions)
            .MatchStartForward(
                new CodeMatch(OpCodes.Ldstr, "SING DURATION END"),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Call, sendEventFn))
            .Advance(2)
            .Set(OpCodes.Call, replacementFn);

        return match.InstructionEnumeration();
    }
}
#endif