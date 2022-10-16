using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using xTile.Dimensions;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace HedgeMaze
{
    public partial class ModEntry
    {
        public static Vector2[] surrounding = new Vector2[]
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,0),
            new Vector2(1,1),
            new Vector2(0,-1),
            new Vector2(-1,0),
            new Vector2(-1,-1),
            new Vector2(1,-1),
            new Vector2(-1,1),
            new Vector2(-1,-2),
            new Vector2(0,-2),
            new Vector2(1,-2)
        };
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.Update))]
        public class Farmer_Update_Patch
        {
            public static void Postfix(Farmer __instance)
            {
                if (!Config.ModEnabled || __instance.currentLocation is not Woods)
                    return;

                Vector2 tile = __instance.getTileLocation();
                if (!IsTileInMaze(tile))
                    return;
                foreach(var t in surrounding)
                {
                    if(__instance.currentLocation.Map.GetLayer("AlwaysFront").Tiles.Array[(int)(t.X + tile.X), (int)(t.Y + tile.Y)] is StaticTile)
                        __instance.currentLocation.Map.GetLayer("AlwaysFront").Tiles.Array[(int)(t.X + tile.X), (int)(t.Y + tile.Y)] = null;
                }

            }
        }
        private static int fairyFrame;
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.draw), new Type[] { typeof(SpriteBatch) })]
        public class GameLocation_draw_Patch
        {
            public static void Postfix(GameLocation __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || !__instance.Name.Equals("Woods"))
                    return;
                fairyFrame++;
                fairyFrame %= 32;
                foreach(var f in fairyTiles)
                {
                    b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, f * 64), new Rectangle?(new Rectangle(16 + fairyFrame / 8 * 16, 592, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9999999f);
                }

            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static void Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
            {
                if (!Config.ModEnabled || !Game1.currentLocation.Name.Equals("Woods"))
                    return;

                foreach(var f in fairyTiles)
                {
                    if (new Rectangle((int)f.X - 1, (int)f.Y - 1, 2, 2).Contains(new Point(tileLocation.X, tileLocation.Y)))
                    {
                        __instance.localSound("yoba");
                        who.health = who.maxHealth;
                        who.stamina = who.MaxStamina;
                        fairyTiles.Remove(f);
                        return;
                    }
                }
            }
        }
    }
}