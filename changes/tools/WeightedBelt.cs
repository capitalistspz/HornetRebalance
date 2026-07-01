using GlobalSettings;
using HarmonyLib;

namespace Rebalance.changes.tools;

public static class WeightedBelt
{
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
    public static class Patch
    {
        public static void Postfix()
        {
            Gameplay.Get().weightedAnkletRecoilSteps = 0;
        }
    }
}