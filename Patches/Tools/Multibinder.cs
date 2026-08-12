using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using Rebalance.FsmUtils;
using Silksong.FsmUtil;

namespace Rebalance.Patches.tools;

public static class Multibinder
{
    private const float BindTime = (1.37f * 1.15f - 0.3f) / 2;
    
    [FsmMutator("Hero_Hornet(Clone)", "Bind")]
    [UsedImplicitly]
    public static void ChangeCooldown(Fsm fsm)
    {
        var multiBindState = fsm.MustGetState("Multi Bind");

        var success = multiBindState.MutateFirstActionOfType<SetFloatValue>(action =>
        {
            if (action.floatVariable.Name == "Bind Time") 
                action.floatValue = BindTime;
        });
        
        if (!success) 
            RebalancePlugin.Logger.LogError("Couldn't set multibinder bind time");
    }
}