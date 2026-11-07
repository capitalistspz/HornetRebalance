using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Rebalance.MiscUtils;
using UnityEngine;

namespace Rebalance.Patches;

[HarmonyPatch]
public static class DoDamageEnemiesHookPatch
{
    private static readonly List<Action<DamageEnemies>> Hooks = [];

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.StartNewGame))]
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.ContinueGame))]
    [HarmonyPostfix]
    private static void SetHooks()
    {
        Hooks.Clear();
        
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var method in assembly.GetTypes().SelectMany(t => t.GetMethods()))
        {
            var methodParams = method.GetParameters();
            var badMethod = methodParams.Length == 0 || methodParams[0].ParameterType != typeof(DamageEnemies) ||
                            method.ReturnType != typeof(void);
            var attributes = method.GetCustomAttributes<DoDamageEnemiesHookAttribute>();
            if (!attributes.Any())
                continue;
            if (badMethod)
            {
                var methodName = $"{method.DeclaringType!.Name}.{method.Name}";
                RebalancePlugin.Logger.LogError($"Damage enemies hook '{methodName}' has invalid signature");
                continue;
            }
            Hooks.Add(
                (Action<DamageEnemies>)Delegate.CreateDelegate(typeof(Action<DamageEnemies>), method));
        }
    }
    
    private static void RunPostStackMutators(DamageEnemies damageEnemies)
    {
        Hooks.ForEach(hook => hook(damageEnemies));
    }


    [HarmonyPatch(typeof(DamageEnemies), nameof(DamageEnemies.DoDamage), typeof(GameObject), typeof(bool))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> InsertMutatorsAfterDamageStack(
        IEnumerable<CodeInstruction> instructions)
    {
        var setupNewStack = AccessTools.Method(typeof(DamageStack), nameof(DamageStack.SetupNew));
        var postStackMutators = AccessTools.Method(typeof(DoDamageEnemiesHookPatch), nameof(RunPostStackMutators));

        // this.tempDamageStack.SetupNew(...)
        // RunPostStackMutators(this) <----- Inserted


        var match = new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Callvirt, setupNewStack))
            .Repeat(cm =>
                cm.Advance(1)
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, postStackMutators)));

        return match.InstructionEnumeration();
    }
}