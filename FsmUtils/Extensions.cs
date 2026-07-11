using System;
using HutongGames.PlayMaker;

namespace Rebalance.FsmUtils;

public static class Extensions
{
    public static TAction? GetFirstActionOfTypeMatching<TAction>(this FsmState state, Func<TAction, bool> predicate) where TAction : FsmStateAction
    {
        foreach (var fsmStateAction in state.Actions)
        {
            if (fsmStateAction is TAction action && predicate(action))
            {
                return action;
            }
        }
        return null;
    }

    public static bool MutateFirstActionOfType<TAction>(this FsmState state, Action<TAction> mutator) where TAction : FsmStateAction
    {
        foreach (var action in state.Actions)
        {
            if (action is TAction t)
            {
                mutator(t);
                return true;
            }
        }

        return false;
    }
}