using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.Tools;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using xTile.Dimensions;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using System;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Object = StardewValley.Object;
using StardewValley.Characters;
using static StardewValley.Minigames.CraneGame;
using StardewValley.Locations;
using System.Text.RegularExpressions;

namespace CropHarvestBubbles
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Crop), nameof(Crop.draw))]
        public class Crop_draw_Patch
        {
            public static void Postfix(Crop __instance, SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation)
            {
                if (!Config.ModEnabled || (Config.RequireKeyPress && !Config.PressKeys.IsDown()) || __instance.forageCrop.Value || __instance.currentPhase.Value < __instance.phaseDays.Count - 1 || (__instance.fullyGrown.Value && __instance.dayOfCurrentPhase.Value > 0))
                    return;

                float base_sort = (float)((tileLocation.Y + 1) * 64) / 10000f + tileLocation.X / 50000f;
                float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(tileLocation.X * 64 - 8), (float)(tileLocation.Y * 64 - 96 - 16) + yOffset)), new Rectangle?(new Rectangle(141, 465, 20, 24)), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort + 1E-06f);

                b.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(tileLocation.X * 64 + 32), (float)(tileLocation.Y * 64 - 64 - 8) + yOffset)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, __instance.indexOfHarvest.Value, 16, 16)), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, base_sort + 1E-05f);
                if (__instance.programColored.Value)
                {
                    b.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(tileLocation.X * 64 + 32), (float)(tileLocation.Y * 64 - 64 - 8) + yOffset)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, __instance.indexOfHarvest.Value + 1, 16, 16)), __instance.tintColor.Value * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, base_sort + 1.1E-05f);
                }
            }
        }
    }
}