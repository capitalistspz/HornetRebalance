using GlobalSettings;
using HutongGames.PlayMaker;
using JetBrains.Annotations;
using Rebalance.FsmUtils;
using Silksong.FsmUtil;

namespace Rebalance.Patches.tools;


public static class FracturedMask
{
    [FsmMutator("Hero_Hornet(Clone)", "Bind")]
    [UsedImplicitly]
    public static void RestoreOnBind(Fsm fsm)
    {
        var endBindState = fsm.MustGetState("End Bind");
        endBindState.AddMethod(() =>
        {
            if (Gameplay.FracturedMaskTool.IsEquipped)
            {
                Gameplay.FracturedMaskTool.ReloadSingle();
                // To make the fractured mask display in the HUD
                EventRegister.SendEvent(EventRegisterEvents.HealthUpdate);
            }
        });
    }
}