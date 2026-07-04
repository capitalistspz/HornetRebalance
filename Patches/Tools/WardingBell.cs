using HarmonyLib;
using Rebalance.FSMActions;
using Silksong.FsmUtil;
using UnityEngine;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class WardingBell
{
    private const int SilkRefundAmount = 6;
    
    [RequireComponent(typeof(PlayMakerFSM))]
    private class FsmComponent : MonoBehaviour
    {
        private bool _added = false;
        public void OnEnable()
        {
            if (_added)
                return;
            var wardingBellFsm = GetComponent<PlayMakerFSM>();
            var burstState = wardingBellFsm.GetState("Burst");
            if (burstState == null)
            {
                RebalancePlugin.Logger.LogError("Failed to get Warding Bell Burst state");
                return;
            }
            burstState.AddAction(new AddHeroSilkSimple
            {
                amount = SilkRefundAmount
            });
            _added = true;
        }
    }
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
    [HarmonyPostfix]
    private static void RefundSilkOnBurst(HeroController __instance)
    {
        // Would use HeroController.bellBindFSM, but it's only valid during a bind

        var wardingBellEffectObject = __instance.gameObject.FindChildByPath("Tool Effects/Bell Bind");

        if (wardingBellEffectObject == null)
        {
            RebalancePlugin.Logger.LogError("Failed to find warding bell tool effect object");
            return;
        }

        wardingBellEffectObject.AddComponent<FsmComponent>();
    }
}