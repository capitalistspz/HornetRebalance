using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using HutongGames.PlayMaker;
using Rebalance.FsmUtils;

namespace Rebalance.Patches;

[HarmonyPatch]
public static class FsmMutatorPatch
{
    private static Dictionary<(string, string), Action<Fsm>>? _mutators;

    [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Start))]
    [HarmonyPrefix]
    public static void OnFsmStart(PlayMakerFSM __instance)
    {
        // Warning: Hornet FSMs come already started
        if (__instance.fsm.Started)
            return;
        if (_mutators == null)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _mutators = new Dictionary<(string, string), Action<Fsm>>();
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods())
                {
                    foreach (var attr in method.GetCustomAttributes<FsmMutatorAttribute>())
                    {
                        _mutators.Add((attr.ObjectName, attr.FsmName), (Action<Fsm>)Delegate.CreateDelegate(typeof(Action<Fsm>), method));
                    }
                }
            }
            stopwatch.Stop();
            
            RebalancePlugin.Logger.LogInfo($"Collected {_mutators.Count} fsm mutators in {stopwatch.ElapsedMilliseconds} ms");
        }

        if (_mutators.TryGetValue((__instance.gameObject.name, __instance.FsmName), out var mutator))
        {
            mutator(__instance.fsm);
        }
    }
}