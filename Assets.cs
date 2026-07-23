using HarmonyLib;
using Silksong.AssetHelper.ManagedAssets;
using UnityEngine;

namespace Rebalance;

[HarmonyPatch]
public static class Assets
{
    public static ManagedAsset<GameObject> MemoryCrystal { get; private set; } = null!;

    public static void InitAll()
    {
        MemoryCrystal = ManagedAsset<GameObject>.FromNonSceneAsset(
            "Assets/Prefabs/Heroes/Tools/Revenge Crystal Hornet.prefab",
            "localpoolprefabs_assets_shared");
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.StartNewGame))]
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.ContinueGame))]
    [HarmonyPostfix]
    public static void LoadAssets()
    {
        MemoryCrystal.Load();
    }

    [HarmonyPatch(typeof(QuitToMenu), nameof(QuitToMenu.Start))]
    [HarmonyPrefix]
    public static void UnloadAssets()
    {
        MemoryCrystal.Unload();
    }
}