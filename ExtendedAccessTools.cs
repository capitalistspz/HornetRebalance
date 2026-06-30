using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace Rebalance;

public static class ExtendedAccessTools
{
    private static void LogWarn(string msg)
    {
        RebalancePlugin.Logger.LogWarning(msg);
    }
    
    // Get get the type of an enumerator via its method
    // Mostly copied from the Harmony's `AccessTools.EnumeratorMoveNext`
    public static Type? EnumeratorType(MethodInfo? enumerator)
    {
        if (enumerator == null)
        {
            LogWarn("EnumeratorType.Method: enumerator is null");
            return null;
        }
        var context = new ILContext(new DynamicMethodDefinition(enumerator).Definition);
        var ilCursor = new ILCursor(context);
        if (context.Method.ReturnType.Name.StartsWith("UniTask"))
        {
            var firstVar = context.Body.Variables.FirstOrDefault<VariableDefinition>()?.VariableType;
            if (firstVar == null || firstVar.Name.Contains(enumerator.Name))
                return firstVar.ResolveReflection();
            LogWarn($"EnumeratorType.Method: Unexpected type name {firstVar.Name}, should contain {enumerator.Name}");
        }
        else
        {
            MethodReference? enumeratorCtor = null;
            ilCursor.GotoNext(i => i.MatchNewobj(out enumeratorCtor));
            if (enumeratorCtor == null)
            {
                LogWarn($"EnumeratorType.Method: {enumerator.FullDescription()} does not create enumerators");
                return null;
            }
            if (enumeratorCtor.Name == ".ctor") 
                return enumeratorCtor.DeclaringType.ResolveReflection();
            LogWarn($"EnumeratorType.Method: {enumerator.FullDescription()} does not create an enumerator (got {enumeratorCtor.GetID(simple: true)})");
        }

        return null;
    }
}