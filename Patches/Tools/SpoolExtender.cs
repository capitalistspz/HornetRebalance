using GlobalSettings;
using HarmonyLib;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class SpoolExtender
{
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.AddSilk), 
        typeof(int), typeof(bool), typeof(SilkSpool.SilkAddSource), typeof(bool))]
    [HarmonyPrefix]
    private static void BumpAddedSilk(HeroController __instance, ref int amount)
    {
        if (Gameplay.SpoolExtenderTool.IsEquipped)
        {
            var amountIntoExtender = (__instance.playerData.silk + amount) - __instance.playerData.CurrentSilkMaxBasic;
            if (amountIntoExtender > 0)
            {
                amount = Gameplay.SpoolExtenderSilk;
            }
        }
    }
}