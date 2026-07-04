using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using Rebalance.FSMActions;
using Silksong.FsmUtil;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class ScuttleBrace
{
    private const float InvincibilityDuration = 0.4f;
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
    [HarmonyPostfix]
    private static void MakeInvulnerable(HeroController __instance)
    {
        var scuttleStartState = __instance.toolsFSM.GetState("Scuttle Start");
        if (scuttleStartState == null)
        {
            RebalancePlugin.Logger.LogError("Failed to find Scuttle Start state");
            return;
        }
        
        scuttleStartState.RemoveFirstActionMatching((action => action is SetHeroCState state && state.VariableName.Value == "evading"));

        scuttleStartState.AddAction(new MakeHornetInvulnerable
        {
            duration = InvincibilityDuration
        });
        
    }
}