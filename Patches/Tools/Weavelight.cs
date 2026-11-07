using GlobalSettings;
using HarmonyLib;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class Weavelight
{
    private const float RegenMultiplier = 0.5f;
    
    [HarmonyPatch(typeof(Gameplay), nameof(Gameplay.Awake))]
    [HarmonyPostfix]
    private static void IncreaseRegenSpeed(Gameplay gameplay)
    {
        gameplay.whiteRingSilkRegenTimeMultiplier = RegenMultiplier;
    }
}