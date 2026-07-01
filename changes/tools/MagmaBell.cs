using GlobalSettings;
using HarmonyLib;

namespace Rebalance.changes.tools;

public static class MagmaBell
{
    [HarmonyPatch]
    public static class Patch
    {
        [HarmonyPatch(typeof(Gameplay), nameof(Gameplay.Awake))]
        [HarmonyPostfix]
        public static void BecomeYellow(Gameplay __instance)
        {
            __instance.lavaBellTool.type = ToolItemType.Yellow;
        }
    }
}