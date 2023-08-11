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
using Microsoft.Xna.Framework.Graphics.PackedVector;
using StardewValley.TerrainFeatures;

namespace CropWateringBubbles
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.draw))]
        public class HoeDirt_draw_Patch
        {
            public static void Postfix(HoeDirt __instance, SpriteBatch spriteBatch, Vector2 tileLocation)
            {
                if (!Config.ModEnabled || !isEmoting || __instance.crop is null || __instance.crop.dead.Value || __instance.state.Value != 0 || (__instance.crop.currentPhase.Value >= __instance.crop.phaseDays.Count - 1 && (!__instance.crop.fullyGrown.Value || __instance.crop.dayOfCurrentPhase.Value <= 0) && !CanBecomeGiant(__instance)) || __instance.crop.isPaddyCrop() || (Config.OnlyWhenWatering && Game1.player.CurrentTool is not WateringCan))
                    return;
                Vector2 emotePosition = Game1.GlobalToLocal(tileLocation * 64);
                float movePercent = (100 - Config.SizePercent) / 100f;
                emotePosition.Y -= 48 - movePercent * 32;
                emotePosition += new Vector2(movePercent * 32, movePercent * 32);
                spriteBatch.Draw(Game1.emoteSpriteSheet, emotePosition, new Rectangle?(new Rectangle(currentEmoteFrame * 16 % Game1.emoteSpriteSheet.Width, currentEmoteFrame * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16)), Color.White * (Config.OpacityPercent / 100f), 0f, Vector2.Zero, 4f * Config.SizePercent / 100f, SpriteEffects.None, ((float)tileLocation.Y * 64 + 33) / 10000f);
            }

        }
    }
}