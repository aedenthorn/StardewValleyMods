using HarmonyLib;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using System.Linq;

namespace LadderSpawnOnEntry
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(MineShaft), "populateLevel")]
        public class MineShaft_populateLevel_Patch
        {
            public static void Postfix(MineShaft __instance)
            {
                if (!Config.EnableMod || (!Config.EnableForDangerous && __instance.GetAdditionalDifficulty() > 0) || __instance.ladderHasSpawned || __instance.mustKillAllMonstersToAdvance() || !__instance.shouldCreateLadderOnThisLevel())
                    return;
                var stones = __instance.Objects.Pairs.Where(p => p.Value.IsBreakableStone()).ToList();
                if(!stones.Any())
                {
                    SMonitor.Log("No stones on this level");
                    return;
                }
                var stone = Game1.random.ChooseFrom(stones);
                var pos = stone.Key;
                SMonitor.Log($"Creating ladder at {pos}; removing object {stone.Value.Name}");
                __instance.createLadderDown((int)pos.X, (int)pos.Y, false);
                __instance.Objects.Remove(pos);
            }
        }
    }
}