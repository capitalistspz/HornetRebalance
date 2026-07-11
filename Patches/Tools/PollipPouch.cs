using System;
using GlobalSettings;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using Rebalance.FsmUtils;
using Silksong.FsmUtil;
using Object = UnityEngine.Object;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class PollipPouch
{
    private const string LifebloodVenomNailName = "Lifeblood Venom Nail";
    private static NailImbuementConfig? LifebloodVenomImbuement { get; set; }
    private static NailImbuementConfig? FlintslateVenomImbuement { get; set; }

    private static void SetLifebloodVenomImbuement()
    {
        var imbuement = HeroController.instance.NailImbuement;
        // Don't want to replace venom Flint Slate effect
        if (LifebloodVenomImbuement == null || imbuement.currentImbuement == FlintslateVenomImbuement ||
            imbuement.currentImbuement == LifebloodVenomImbuement)
        {
            return;
        }

        // Basically HeroImbuement.SetElement
        if (imbuement.CurrentElement == NailElements.Fire)
        {
            EventRegister.SendEvent(EventRegisterEvents.FlintSlateExpire);
        }

        imbuement.CurrentElement = NailElements.Poison;
        if (imbuement.spawnedParticles)
        {
            imbuement.spawnedParticles.StopParticleSystems();
            imbuement.spawnedParticles = null;
        }
        
        var oldConfig = imbuement.currentImbuement;
        imbuement.currentImbuement = LifebloodVenomImbuement;
        imbuement.imbuementTimeLeft = imbuement.currentImbuement.Duration;
        imbuement.spriteFlash.flashFocusHeal();
        
        if (imbuement.currentImbuement != oldConfig ||
            !imbuement.spriteFlash.IsFlashing(repeating: true, imbuement.flashingHandle))
        {
            var heroFlashing = imbuement.currentImbuement.HeroFlashing;
            imbuement.flashingHandle = imbuement.spriteFlash.Flash(heroFlashing.Colour, heroFlashing.Amount,
                heroFlashing.TimeUp, heroFlashing.StayTime, heroFlashing.TimeDown, 0f, repeating: true, 0, 1);
            if (imbuement.imbuedHeroLightRenderer)
            {
                imbuement.imbuedHeroLightRenderer.color = imbuement.currentImbuement.ExtraHeroLightColor;
            }

            if (imbuement.imbuedHeroLightGroup)
            {
                imbuement.imbuedHeroLightGroup.FadeTo(1f, imbuement.imbuedHeroLightFadeInDuration);
            }
        }

        if (imbuement.currentImbuement.HeroParticles)
        {
            imbuement.spawnedParticles = imbuement.currentImbuement.HeroParticles.Spawn(imbuement.transform.position);
            imbuement.spawnedParticles.PlayParticleSystems();
        }
    }

    [HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
    [HarmonyPostfix]
    private static void VenomImbuementDamage(HeroController __instance)
    {
        foreach (var config in __instance.NailImbuement.nailConfigs)
        {
            if (config == null || config.name != "Venom Nail")
                continue;

            LifebloodVenomImbuement = Object.Instantiate(config);
            LifebloodVenomImbuement.NailDamageMultiplier = 1;
            LifebloodVenomImbuement.Duration = Single.MaxValue;
            LifebloodVenomImbuement.name = LifebloodVenomNailName;

            config.NailDamageMultiplier = Effects.FireNail.NailDamageMultiplier;
            FlintslateVenomImbuement = config;
            RebalancePlugin.Logger.LogInfo("Applied venom damage change");
            return;
        }
    }

    [FsmMutator("Blue Health(Clone)", "blue_health_display")]
    [UsedImplicitly]
    public static void PlasmiumSynergy(Fsm fsm)
    {
        #region "State getting"
        var startingIdleState = fsm.GetState("Starting Idle?");

        if (startingIdleState == null)
        {
            RebalancePlugin.Logger.LogError("Failed to find starting idle state in blue_health_display");
            return;
        }

        var idleState = fsm.GetState("Idle");
        if (idleState == null)
        {
            RebalancePlugin.Logger.LogError("Failed to find idle state in blue_health_display");
            return;
        }
        
        var frostedIdleState = fsm.GetState("Frosted Antic Idle");
        if (frostedIdleState == null)
        {
            RebalancePlugin.Logger.LogError("Failed to find frosted idle state in blue_health_display");
            return;
        }

        var purpleBreakState = fsm.GetState("Purple Break?");
        if (purpleBreakState == null)
        {
            RebalancePlugin.Logger.LogError("Failed to find purple break state in blue_health_display");
            return;
        }
        
        var boolTestIndex = startingIdleState.IndexLastActionOfType<BoolTest>();
        if (boolTestIndex == -1)
        {
            RebalancePlugin.Logger.LogWarning("Failed to find bool test in starting idle state in blue_health_display");
            boolTestIndex = purpleBreakState.Actions.Length;
        }
        
        var startTimerAction = startingIdleState.GetFirstActionOfType<StartGameplayTimer>();
        if (startTimerAction == null)
        {
            RebalancePlugin.Logger.LogError("Failed to find StartGameplayTimer");
            return;
        }
        #endregion
        // Last forever
        startTimerAction.Duration = Single.MaxValue;
        
        Action<FsmStateAction> imbueIfVenomous = action =>
        {
            var isPoison = action.fsm.GetFsmBool("Is Poison");
            if (isPoison is { Value: true })
                SetLifebloodVenomImbuement();
        };

        // Apply the imbuement
        startingIdleState.InsertMethod(boolTestIndex, imbueIfVenomous);

        {
            var imbueNailState = fsm.AddState("Rebalance - Imbue Nail");
            imbueNailState.AddMethod(imbueIfVenomous);
            imbueNailState.AddTransition(FsmEvent.Finished.Name, idleState.name);
            // Reapply when Flintslate is depleted (This event is named in EventRegisterEvents)
            idleState.AddTransition("FLINT SLATE EXPIRE", imbueNailState.name);
        }
        {
            var imbueNailStateFrosted = fsm.AddState("Rebalance - Imbue Nail (Frosted)");
            imbueNailStateFrosted.AddMethod(imbueIfVenomous);
            imbueNailStateFrosted.AddTransition(FsmEvent.Finished.Name, frostedIdleState.name);
            frostedIdleState.AddTransition("FLINT SLATE EXPIRE", imbueNailStateFrosted.name);
        }
        // Prevent other purple hearts from being destroyed and breaking the UI
        var adjustState = fsm.GetState("Send Purple Adjust?");
        adjustState.RemoveFirstActionOfType<SendEventToRegisterV2>();
        
        // Reset imbuement if purple health goes to 0
        purpleBreakState.AddAction(new IntCompare
        {
            integer1 = fsm.GetFsmInt("Current HP"),
            integer2 = 0,
            greaterThan = FsmEvent.Finished,
            everyFrame = false
        });
        purpleBreakState.AddAction(new SetHeroNailImbuement
        {
            Target = new FsmOwnerDefault
            {
                OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                gameObject = HeroController.instance.gameObject
            },
            Element = NailElements.None,
        });
    }
}