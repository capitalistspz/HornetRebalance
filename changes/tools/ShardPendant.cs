using System.Linq;
using GlobalSettings;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace Rebalance.changes.tools;

public static class ShardPendant
{
    [HarmonyPatch]
    public static class ShardDropsPatch
    {
        // Names of objects that contain `HealthManager`s
        private static readonly string[] BossObjectNames = [
            "Mossbone Mother", // Moss Mother
            "Bell Beast", 
            "Skull King", // Skull Tyrant
            "Dock Guard Thrower", // Gron
            "Dock Guard Slasher", // Signis
            "Bone Flyer Giant", // Savage Beastfly
            "SG_head", // Fourth Chorus
            "Bone Hunter Trapper", // Gurr the Outcast
            "Vampire Gnat", // Moorwing
            "Wisp Pyre Effigy", // Father of the Flame!
            "Crawfather",
            "Roachkeeper Chef", // Disgraced Chef Lugoli!
            "Swamp Shaman", // Groal the Great!
            "Splinter Queen", // Sister Splinter!
            "Seth",
            "Last Judge",
            "Driller A", "Driller B", // Great Conchflies
            "Coral Warrior Grey", // Watcher at the Edge!
            "Song Knight", // Second Sentinel
            "Conductor Boss", // Unravelled!
            "Dancer A", "Dancer B", // Cogwork Dancers
            "Trobbio",
            "Tormented Trobbio",
            "Slab Fly Broodmother", // Broodmother
            "Giant Centipede Butt", "Giant Centipede Head", // Bell Eater!
            "Pinstress Boss",
            "Spinner Boss", // Widow
            "Zap Core Enemy", // Voltwyrm
            "Blue Assistant" // Plasmified Zango
            
            // The following should not drop shards
            
            
            // Final bosses
            // "Lost Lace",
            // "Silk Boss", // Grand Mother Silk
            
            // Repeatable fights
            // "Shakra",
            // "Garmond",
            
            // Pure silk, may change this
            // "Lace Boss1", // Lace at the Docks
            // "Lace Boss 2 New", // Lace in the Cradle
            // "Phantom",
            
            // Memory Bosses (Memory enemies can't drop anything anyway)
        ];

        [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.Die), typeof(float?), typeof(AttackTypes), typeof(NailElements), typeof(GameObject), typeof(bool), typeof(float), typeof(bool), typeof(bool))]
        [HarmonyPrefix]
        public static void BossesDropShards(HealthManager __instance, bool overrideSpecialDeath)
        {
            if (__instance.isDead || (__instance.hasSpecialDeath && !overrideSpecialDeath))
                return;
            var pendantTool = Gameplay.BoneNecklaceTool;
            if (pendantTool == null || !pendantTool.IsEquipped || !BossObjectNames.Contains(__instance.name))
                return;
            // Shard count set at Die because `BattleScene`s set shard count of all enemies to 0
            __instance.SetShellShards(100);
        }
    }
}