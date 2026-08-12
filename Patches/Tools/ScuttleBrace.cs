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
        var scuttleStartState = __instance.toolsFSM.MustGetState("Scuttle Start");
        
        scuttleStartState.RemoveFirstActionMatching((action => action is SetHeroCState state && state.VariableName.Value == "evading"));

        scuttleStartState.AddAction(new MakeHornetInvulnerable
        {
            duration = InvincibilityDuration
        });
        
    }
}