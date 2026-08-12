using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Silksong.FsmUtil;

namespace Rebalance.Patches;

[HarmonyPatch]
public static class ClawlineSilk
{
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
    [HarmonyPostfix]
    private static void RemoveSilkCostAndCheck(HeroController __instance)
    {
        var fsm = __instance.harpoonDashFSM;
        if (fsm == null)
        {
            RebalancePlugin.Logger.LogError("Clawline FSM is null");
            return;
        }

        // Remove silk check
        var canDoState = fsm.MustGetState("Can Do?");

        var cancelEvent = canDoState.GetTransition("CANCEL")?.FsmEvent;
        if (cancelEvent == null)
        {
            RebalancePlugin.Logger.LogError("Failed to get move cancel event (CANCEL)");
            return;
        }

        var sendCancelEvent = new SendEvent
        {
            eventTarget = new FsmEventTarget
            {
                gameObject = new FsmOwnerDefault()
            },
            sendEvent = cancelEvent,
            delay = 0,
            everyFrame = false
        };

        canDoState.ReplaceFirstActionOfType<IntCompare>(sendCancelEvent);

        // Remove silk removal
        var takeControlState = fsm.MustGetState("Take Control");
        takeControlState.RemoveActionsOfType<TakeSilk>();

        var harpoonDashDamagerGo = __instance.gameObject.FindChildByPath("Attacks/Harpoon Dash Damager");
        if (harpoonDashDamagerGo == null)
        {
            RebalancePlugin.Logger.LogError("Failed to get Harpoon Dash Damager object");
            return;
        }

        var damager = harpoonDashDamagerGo.GetComponent<DamageEnemies>();
        damager.SetSilkGenerationNone();
        RebalancePlugin.Logger.LogInfo("Successfully applied Clawline silk changes");
    }
}