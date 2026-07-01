using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Silksong.FsmUtil;

namespace Rebalance.changes.tools;

public static class SilkspeedAnklets
{
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
    public class StopDrainPatch
    {
        public static void Postfix(HeroController __instance)
        {
            var silkUsageFsm = __instance.gameObject.LocateMyFSM("Sprint Silk Usage");

            if (silkUsageFsm == null)
            {
                RebalancePlugin.Logger.LogError("Failed to find silk usage FSM");
                return;
            }

            
            
            var idleState = silkUsageFsm.GetState("Idle");
            if (idleState == null)
            {
                RebalancePlugin.Logger.LogError("Failed to find silk usage FSM Idle state");
                return;
            }
            
            var startUsageTransition = idleState.GetTransition("START SILK USAGE");
            if (startUsageTransition == null)
            {
                RebalancePlugin.Logger.LogInfo("Failed to find Sprint Silk Usage FSM 'START SILK USAGE' transition");
                return;
            }
            
            var stopFreeUsageState = silkUsageFsm.AddState("Rebalance - STOP FREE SILK USAGE");
            stopFreeUsageState.AddAction(new SetFsmBool
            {
                gameObject = new FsmOwnerDefault { gameObject = __instance.gameObject },
                fsmName = "Sprint",
                variableName = "Sprintmaster Active",
                setValue = false,
                everyFrame = false
            });
            stopFreeUsageState.AddTransition("FINISHED", idleState.name);
            
            
            var freeUsageState = silkUsageFsm.AddState("Rebalance - FREE SILK USAGE");
            freeUsageState.AddTransition("STOP SILK USAGE", stopFreeUsageState.name);
            
            var startFreeUsageState = silkUsageFsm.AddState("Rebalance - START FREE SILK USAGE");
            startFreeUsageState.AddAction(new SetFsmBool
            {
                gameObject = new FsmOwnerDefault { gameObject = __instance.gameObject },
                fsmName = "Sprint",
                variableName = "Sprintmaster Active",
                setValue = true,
                everyFrame = false
            });
            startFreeUsageState.AddTransition("FINISHED", freeUsageState.name);
            
            startUsageTransition.ToFsmState = startFreeUsageState;

        }
    }
}