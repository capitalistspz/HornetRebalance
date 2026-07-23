using System;
using HarmonyLib;
using HutongGames.PlayMaker;
using Rebalance.FsmUtils;
using Silksong.AssetHelper.ManagedAssets;
using Silksong.FsmUtil;
using UnityEngine;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class MemoryCrystal
{
    private const float DamageMultiplier = 50.0f / 34.0f;
    
    [RequireComponent(typeof(DamageEnemies))]
    private class CrystalMutator : MonoBehaviour
    {
        private void Awake()
        {
            var damager = gameObject.GetComponent<DamageEnemies>();
            damager.damageDealt = (int)Math.Round(damager.damageDealt * DamageMultiplier);
        }
    }
    
    [FsmMutator("Revenge Crystal Hornet(Clone)", "FSM")]
    public static void ModifyCrystal(Fsm fsm)
    {
        var damagerObj = fsm.GetGameObjectVariable("Enemy Damager");
        damagerObj.Value.AddComponentIfNotPresent<CrystalMutator>();
    }
    
    [FsmMutator("Hero_Hornet(Clone)", "Nail Arts")]
    public static void SpawnCrystalOnChargeAttack(Fsm fsm)
    {
        var anticTypeCheckState = fsm.MustGetState("Antic Type");
        anticTypeCheckState.InsertMethod(0, (action) =>
        {
            if (!ToolItemManager.IsToolEquipped("Revenge Crystal"))
                return;
            Assets.MemoryCrystal.EnsureLoaded();
            var crystal = Assets.MemoryCrystal.InstantiateAsset();
            crystal.transform.position = action.fsm.GameObject.transform.position;
        });
    }
    
    
}