using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using HutongGames.PlayMaker;
using Rebalance.FsmUtils;
using Silksong.FsmUtil;

namespace Rebalance.Patches;

[HarmonyPatch]
public static class FsmMutatorPatch
{
    private static Dictionary<(string, string), List<Action<Fsm>>>? _mutators;

    private const string MutationTagVarName = "Rebalance - Mutated"; 

    private static Dictionary<(string, string), List<Action<Fsm>>> CollectMutators()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var mutators = new Dictionary<(string, string), List<Action<Fsm>>>();
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var type in assembly.GetTypes())
        {
            foreach (var method in type.GetMethods())
            {
                foreach (var attr in method.GetCustomAttributes<FsmMutatorAttribute>())
                {
                    var mutator = (Action<Fsm>)Delegate.CreateDelegate(typeof(Action<Fsm>), method);
                    if (mutators.TryGetValue((attr.ObjectName, attr.FsmName), out var list))
                    {
                        list.Add(mutator);   
                    }
                    else
                    {
                        mutators.Add((attr.ObjectName, attr.FsmName), [mutator]);
                    }
                }
            }
        }
        stopwatch.Stop();
        RebalancePlugin.Logger.LogInfo($"Collected {mutators.Count} fsm mutators in {stopwatch.ElapsedMilliseconds} ms");
        return mutators;
    }

    [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.OnEnable))]
    [HarmonyPostfix]
    public static void OnFsmStart(PlayMakerFSM __instance)
    {
        _mutators ??= CollectMutators();
        
        // A tag variable
        if (__instance.fsm.FindBoolVariable(MutationTagVarName) != null)
            return;

        if (_mutators.TryGetValue((__instance.gameObject.name, __instance.FsmName), out var mutators))
        {
            foreach (var mutator in mutators)
            {
                
                RebalancePlugin.Logger.LogDebug($"Applied mutator '{mutator.Method.DeclaringType!.Name}{mutator.Method.Name}' to {__instance.gameObject.name}: {__instance.FsmName}");
                mutator(__instance.fsm);
            }
            __instance.fsm.AddBoolVariable(MutationTagVarName);
        }
    }
}