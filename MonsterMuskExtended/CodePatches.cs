using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Monsters;
using xTile.Layers;

namespace MonsterMuskExtended
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(MineShaft), "adjustLevelChances")]
        public class MineShaft_adjustLevelChances_Patch
        {
            public static void Postfix(MineShaft __instance, ref double monsterChance)
            {
                if (Config.ModEnabled && (__instance.isQuarryArea || __instance.getMineArea() == 77377) && AnyOnlineFarmerHasBuff("24"))
                {
                    monsterChance *= 2;
                }
            }
        }

        [HarmonyPatch(typeof(BugLand), nameof(BugLand.InitializeBugLand))]
        public class BugLand_InitializeBugLand_Patch
        {
            public static void Prefix(BugLand __instance, ref bool __state)
            {
                if (Config.ModEnabled && !__instance.hasSpawnedBugsToday && AnyOnlineFarmerHasBuff("24"))
                {
                    __state = true;
                }
            }
            public static void Postfix(BugLand __instance, bool __state)
            {
                if (!__state)
                    return;
                Layer pathsLayer = __instance.map.RequireLayer("Paths");
                for (int x = 0; x < __instance.map.Layers[0].LayerWidth; x++)
                {
                    for (int y = 0; y < __instance.map.Layers[0].LayerHeight; y++)
                    {
                        if (Game1.random.NextDouble() < 0.33)
                        {
                            int tileIndex = pathsLayer.GetTileIndexAt(x, y, null);
                            if (tileIndex == 28)
                            {
                                Vector2 tile = new Vector2((float)x, (float)y);
                                if (__instance.CanSpawnCharacterHere(tile) && __instance.characters.Count < 100)
                                {
                                    __instance.characters.Add(new Grub(new Vector2(tile.X * 64f, tile.Y * 64f), true));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}