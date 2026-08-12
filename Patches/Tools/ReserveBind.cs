using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using Rebalance.FSMActions;
using Rebalance.FsmUtils;
using Silksong.FsmUtil;

namespace Rebalance.Patches.tools;

public static class ReserveBind
{
    private const float ChargeRate = 5.0f;
    private const string StartHealthVarName = "Rebalance - Starting Health";

    [FsmMutator("Hero_Hornet(Clone)", "Bind")]
    [UsedImplicitly]
    public static void ReduceChargeTimeAndMakeRestorable(Fsm fsm)
    {
        var canBindState = fsm.MustGetState("Can Bind?");

        var endBindState = fsm.MustGetState("End Bind");

        var reserveBindChargeState = fsm.MustGetState("Reserve Bind Charge");

        var reserveBindBurst = fsm.MustGetState("Reserve Bind Burst");

        #region "Reduce Reserve Bind Delay"    
        reserveBindChargeState.ReplaceFirstActionOfType<Tk2dPlayAnimationWithEvents>(
            new Tk2dPlayAnimationWithEventsAtRate
            {
                gameObject = new FsmOwnerDefault(),
                clipName = fsm.GetFsmString("Play Anim"),
                animationCompleteEvent = FsmEvent.Finished,
                playRate = ChargeRate
            });
        
        reserveBindBurst.RemoveFirstActionOfType<FreezeMoment>(); 
        
        reserveBindBurst.ReplaceFirstActionOfType<Tk2dPlayAnimationWithEvents>(
            new Tk2dPlayAnimationWithEventsAtRate
            {
                gameObject = new FsmOwnerDefault(),
                clipName = fsm.GetFsmString("Play Anim"),
                animationCompleteEvent = FsmEvent.Finished,
                playRate = ChargeRate
            });
        #endregion
        #region "Restore on Bind"
        var startingHealthVar = fsm.AddIntVariable(StartHealthVarName);
        canBindState.InsertAction(0, new GetPlayerDataInt
        {
            intName = "health",
            storeValue = startingHealthVar
        });
        
        endBindState.AddMethod(action =>
        {
            if (startingHealthVar.IsNone)
                return;
            if (startingHealthVar.Value < PlayerData.instance.CurrentMaxHealth)
                return;
            
            // Don't want reserve bind to produce another reserve bind
            var usedReserveBind = action.fsm.GetFsmBool("Used Reserve Bind");
            if (!NamedVariable.IsNullOrNone(usedReserveBind) && usedReserveBind.Value)
                return;
            var reserveBindTool = ToolItemManager.GetToolByName("Reserve Bind");
            if (reserveBindTool != null)
            {
                reserveBindTool.ReloadSingle();
                // To make the reserve bind display in the HUD
                EventRegister.SendEvent(EventRegisterEvents.EquipsChangedEvent);
            }
        });
        #endregion
    }
}