using GlobalSettings;
using HarmonyLib;

namespace Rebalance.Patches.tools;

[HarmonyPatch]
public static class MagmaBell
{
    [HarmonyPatch(typeof(Gameplay), nameof(Gameplay.Awake))]
    [HarmonyPostfix]
    private static void BecomeYellow(Gameplay __instance)
    {
        __instance.lavaBellTool.type = ToolItemType.Yellow;
    }
}