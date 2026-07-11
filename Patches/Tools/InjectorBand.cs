using GlobalSettings;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using Rebalance.FSMActions;
using Rebalance.FsmUtils;
using Silksong.FsmUtil;

namespace Rebalance.Patches.tools;

public static class InjectorBand
{
    private const float BindTimeMultiplier = 0.5f;
    
    [FsmMutator("Hero_Hornet(Clone)", "Bind")]
    [UsedImplicitly]
    public static void ChangeCooldown(Fsm fsm)
    {
        var quickBindState = fsm.GetState("Quick Bind?");
        if (quickBindState == null)
        {
            RebalancePlugin.Logger.LogError("Couldn't find Injector Band state");
            return;
        }

        var success = quickBindState.MutateFirstActionOfType<FloatMultiply>(action =>
        {
            if (action.floatVariable.Name == "Bind Time") 
                action.multiplyBy = BindTimeMultiplier;
        });
        
        if (!success) 
            RebalancePlugin.Logger.LogError("Couldn't set Injector Band bind time multiplier");

        var quickCraftStartState = fsm.GetState("Quick Craft Start");
        if (quickCraftStartState == null)
        {
            RebalancePlugin.Logger.LogError("Couldn't find craft bind state");
            return;
        }
        
        var groundTestIndex = quickCraftStartState.IndexFirstActionMatching(action =>
        {
            if (action is BoolTest boolTest)
            {
                return boolTest.boolVariable.Name == "On Ground";
            }
            return false;
        });

        var clipVar = fsm.AddStringVariable("Rebalance - QC Clip");

        var quickCraftStartGroundQuick = fsm.AddState("Rebalance - QC Start (Quick)");
        quickCraftStartGroundQuick.AddAction(new ConvertBoolToString
        {
            boolVariable = fsm.GetFsmBool("Quick Binding"),
            stringVariable = clipVar,
            trueString = "Quick Craft Ground",
            falseString = "Quick Craft Air"
        });
        quickCraftStartGroundQuick.AddAction(new Tk2dPlayAnimationWithEventsAtRate
        {
            gameObject = new FsmOwnerDefault(),
            clipName = clipVar,
            animationTriggerEvent = FsmEvent.Finished,
            playRate = 1.0f / BindTimeMultiplier
        });
        quickCraftStartGroundQuick.AddAction(new DecelerateV2
        {
            gameObject = new FsmOwnerDefault(),
            deceleration = fsm.GetFsmFloat("Deceleration"),
            brakeOnExit = false
        });
        quickCraftStartGroundQuick.AddTransition(FsmEvent.Finished.Name, "Crafting Started");

        var quickEvent = quickCraftStartState.AddTransition("Rebalance - QC Quick", quickCraftStartGroundQuick.Name);
        
        quickCraftStartState.InsertAction(groundTestIndex, new BoolTest
        {
            boolVariable = fsm.GetFsmBool("Quick Binding"),
            isTrue = quickEvent
        });
    }
}