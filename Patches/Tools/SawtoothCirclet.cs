using GlobalSettings;
using HarmonyLib;
using HutongGames.PlayMaker;
using JetBrains.Annotations;
using Rebalance.FsmUtils;
using Silksong.FsmUtil;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class SawtoothCirclet
{
    [HarmonyPatch(typeof(BouncePod), nameof(BouncePod.Hit))]
    [HarmonyPrefix]
    private static void StopTriggeringBouncePods(BouncePod __instance, HitInstance damageInstance, ref bool __runOriginal, ref IHitResponder.HitResponse __result)
    {
        if (damageInstance.RepresentingTool == Gameplay.BrollySpikeTool)
        {
            __runOriginal = false;
            __result = IHitResponder.Response.None;
        }
    }

    [FsmMutator("Tool_brolly_spike_dj(Clone)", "brolly_spike_cooldown_check")]
    [FsmMutator("Tool_brolly_spike(Clone)", "brolly_spike_cooldown_check")]
    [UsedImplicitly]
    public static void ReduceSound(Fsm fsm)
    {
        var checkState = fsm.MustGetState("Check");

        var audioEventAction = checkState.GetFirstActionOfType<PlayAudioEvent>();
        audioEventAction?.volume = 0.5f;
    }
}