using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using Rebalance.FSMActions;
using Rebalance.FsmUtils;
using Silksong.FsmUtil;
using UnityEngine;

namespace Rebalance.Patches.tools;

public static class ClawMirror
{
    private const float ClawMirrorDamageScale = 50.0f / 37.0f;
    private const float ClawMirrorsDamageScale = 59.0f / 51.0f;
    private const string QuickCraftingVarName = "Rebalance - Quick Crafting";
    private const string DazzleBindObjectVarName = "Rebalance - Dazzle Bind Object";
    private const string BindEndNowEventName = "Rebalance - BIND END NOW";

    [RequireComponent(typeof(DamageEnemies))]
    private class DamageAdjuster : MonoBehaviour
    {
        private DamageEnemies _damageEnemies;
        private static ToolItem _dazzleBindTool;
        private static ToolItem _dazzleBindUpgradedTool;
        public void Awake()
        {
            _damageEnemies = GetComponent<DamageEnemies>();
            if (_dazzleBindTool == null)
                _dazzleBindTool = ToolItemManager.GetToolByName("Dazzle Bind");
            if (_dazzleBindUpgradedTool == null)
                _dazzleBindUpgradedTool = ToolItemManager.GetToolByName("Dazzle Bind Upgraded");
            
            if (_damageEnemies.RepresentingTool == _dazzleBindTool)
            {
                _damageEnemies.damageDealt = (int)Math.Round(_damageEnemies.damageDealt * ClawMirrorDamageScale);
                var collider = gameObject.GetComponentInParent<CircleCollider2D>();
                collider.radius *= 1.23f;
            }
            else if (_damageEnemies.RepresentingTool == _dazzleBindUpgradedTool)
            {
                _damageEnemies.damageDealt = (int)Math.Round(_damageEnemies.damageDealt * ClawMirrorsDamageScale);
            }
            RebalancePlugin.Logger.LogInfo($"Adjusted claw mirror damager {_damageEnemies.damageDealt}");
        }
    }
    
    [FsmMutator("Hero_Hornet(Clone)", "Bind")]
    [UsedImplicitly]
    public static void Upgrade(Fsm fsm)
    {
        var dazzleState = fsm.MustGetState("Dazzle?");
        var quickCraftCheckState = fsm.MustGetState("Quick Craft?");
        var quickCraftStartState = fsm.MustGetState("Quick Craft Start");
        var quickCraftBurstState = fsm.MustGetState("Quick Craft Burst");
        var endBindState = fsm.MustGetState("End Bind");
        
        
        #region "Increase Damage and Radius"
        var dazzleBindObjectVar = fsm.AddGameObjectVariable(DazzleBindObjectVarName);
        dazzleState.GetFirstActionOfType<SpawnObjectFromGlobalPool>()!.storeObject = dazzleBindObjectVar;
        dazzleState.AddMethod(action =>
        {
            var objectVar = action.fsm.GetGameObjectVariable(DazzleBindObjectVarName);
            var damager = objectVar.Value.GetComponentInChildren<DamageEnemies>();
            damager.gameObject.AddComponentIfNotPresent<DamageAdjuster>();
        });;
        #endregion
        
        #region "Support Craft Bind"
        var quickCraftingVar = fsm.AddBoolVariable(QuickCraftingVarName);
        quickCraftCheckState.InsertAction(0, new SetBoolValue
        {
            boolValue = false,
            boolVariable = quickCraftingVar
        });
        
        quickCraftStartState.InsertAction(0, new SetBoolValue
        {
            boolValue = true,
            boolVariable = quickCraftingVar
        });

        quickCraftBurstState.RemoveTransition(FsmEvent.Finished.Name);
        quickCraftBurstState.AddTransition(FsmEvent.Finished.Name, dazzleState.Name);
        

        var bindEndNowEvent = dazzleState.AddTransition(BindEndNowEventName, endBindState.name);
        dazzleState.AddAction(new BoolTest
        {
            boolVariable = quickCraftingVar,
            isTrue = bindEndNowEvent
        });
        #endregion
        
    }
}