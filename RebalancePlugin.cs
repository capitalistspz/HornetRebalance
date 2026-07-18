using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace Rebalance;

[BepInAutoPlugin(id: "io.github.capitalistspz.rebalance")]
[BepInDependency("org.silksong-modding.fsmutil")]
public partial class RebalancePlugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger = null!;
    private Harmony? _harmony;

    private void Awake()
    {
        RebalancePlugin.Logger = base.Logger;
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }

    private void Start()
    {
        _harmony = new Harmony(Id);
        _harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    private void OnEnable()
    {
        // First OnEnable occurs before Start
        _harmony?.PatchAll(Assembly.GetExecutingAssembly());
    }

    public void OnDisable()
    {
        _harmony?.UnpatchSelf();
    }
}