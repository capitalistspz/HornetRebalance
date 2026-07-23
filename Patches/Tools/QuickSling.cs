using System.Collections.Generic;
using System.Reflection.Emit;
using GlobalSettings;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using Rebalance.FsmUtils;
using Silksong.FsmUtil;
using Silksong.FsmUtil.Actions;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class QuickSling
{
    private const float DamageMultiplier = 0.7f;
    private const string QuickSlingEquippedVarName = "Rebalance - Quick Sling Equipped";

    private static readonly HashSet<string> DefaultUnaffectedToolNames =
    [
        "WebShot Forge", "WebShot Architect", "WebShot Weaver",
        "Flea Brew", "Lifeblood Syringe", "Silk Snare"
    ];

    private static void DidUseAttackToolAlt(HeroController instance, ToolItemsData.Data data, bool isAutoThrow)
    {
        if (isAutoThrow && Gameplay.QuickSlingTool.IsEquipped)
            return;
        instance.DidUseAttackTool(data);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.ThrowTool))]
    private static IEnumerable<CodeInstruction> DontConsumeExtra(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        var targetFn = AccessTools.Method(
            typeof(HeroController),
            nameof(HeroController.DidUseAttackTool));

        var replacementFn = AccessTools.Method(
            typeof(QuickSling),
            nameof(DidUseAttackToolAlt));

        matcher.MatchForward(false,
                new CodeMatch(OpCodes.Call, targetFn))
            .Repeat(m =>
            {
                m.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldarg_1));
                m.InsertAndAdvance(new CodeInstruction(OpCodes.Call, replacementFn));
            });

        return matcher.InstructionEnumeration();
    }

    [HarmonyPatch(typeof(DamageEnemies), nameof(DamageEnemies.Start))]
    [HarmonyPostfix]
    private static void ReduceDamage(DamageEnemies __instance)
    {
        var tool = __instance.RepresentingTool;
        if (tool == null)
            return;
        if (tool.Type != ToolItemType.Red)
            return;
        if (!Gameplay.QuickSlingTool.IsEquipped)
            return;
        if (DefaultUnaffectedToolNames.Contains(tool.name))
            return;
        __instance.damageMultiplier *= DamageMultiplier;
    }

    [FsmMutator("Hero_Hornet(Clone)", "Tool Attacks")]
    [UsedImplicitly]
    public static void ShootFaster(Fsm fsm)
    {
        var takeControlState = fsm.MustGetState("Take Control");
        var shootState = fsm.MustGetState("Shoot Loop");
        var webshotAFireState = fsm.MustGetState("WebShot A Fire");
        var cooldownState = fsm.MustGetState("Cooldown");

        var quickSlingBool = fsm.AddBoolVariable(QuickSlingEquippedVarName);
        takeControlState.InsertAction(0, new CheckIfToolEquipped
        {
            Tool = Gameplay.QuickSlingTool,
            RequiredAmountLeft = 0,
            storeValue = quickSlingBool
        });

        // Rosary Cannon
        shootState.ReplaceFirstActionOfType<Wait>(new WaitSelect
        {
            condition = quickSlingBool,
            trueTime = 0.05f,
            falseTime = 0.1f,
            finishEvent = FsmEvent.Finished,
            realTime = false
        });

        // Silkshot (Architect)
        webshotAFireState.ReplaceFirstActionOfType<Wait>(new WaitSelect
        {
            condition = quickSlingBool,
            trueTime = 0.05f,
            falseTime = 0.1f,
            finishEvent = FsmEvent.Finished,
            realTime = false
        });

        cooldownState.ReplaceFirstActionOfType<HeroControllerMethods>(
            new DelegateAction<FsmStateAction>
            {
                Method = _ =>
                {
                    var hero = HeroController.instance;
                    if (hero == null)
                        return;
                    var tool = hero.willThrowTool;

                    if (Gameplay.QuickSlingTool.IsEquipped && tool != null && DefaultUnaffectedToolNames.Contains(tool.name))
                        hero.SetToolCooldown(0.05f);
                    else // The default
                        hero.SetToolCooldown(0.1f);
                },
                Arg = null
            });
    }
}