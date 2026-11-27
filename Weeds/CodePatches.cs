using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Audio;
using StardewValley.TerrainFeatures;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Weeds
{
	public partial class ModEntry
    {
        internal static bool HoeDirt_performUseAction_Prefix(TerrainFeature __instance, ref bool __result, Vector2 tileLocation)
        {
            if (!Config.ModEnabled || !__instance.modData.TryGetValue(modKey, out var data) || int.Parse(data) < 25)
                return true;
            Game1.player.mostRecentlyGrabbedItem = null;
            Game1.player.animateOnce(279 + Game1.player.FacingDirection);
            Game1.player.currentLocation.playSound("moss_cut");
            Game1.player.gainExperience(0, Config.WeedExp);
            Game1.player.Stamina -= Math.Max(0, Config.WeedStaminaUse - (float)Game1.player.FarmingLevel * 0.1f);
            __instance.modData.Remove(modKey);
            __instance.modData.Remove(modFlippedKey);
            __result = true;
            return false;
        }
        internal static bool Crop_newDay_Prefix(Crop __instance)
        {
            if (!Config.ModEnabled || __instance.fullyGrown.Value || __instance.dead.Value || __instance.isWildSeedCrop() || (Game1.currentLocation.IsOutdoors && !__instance.IsInSeason(Game1.currentLocation)) || !Game1.currentLocation.terrainFeatures.TryGetValue(__instance.tilePosition, out var tf) || tf is not HoeDirt)
                return true;
            int weed = 0;
            if(tf.modData.TryGetValue(modKey, out var weedStr))
            {
                weed = int.Parse(weedStr);
            }
            if(weed < 25 && Config.WeededDoubleGrowth)
            {
                __instance.dayOfCurrentPhase.Value = Math.Min(__instance.dayOfCurrentPhase.Value + 1, (__instance.phaseDays.Count > 0) ? __instance.phaseDays[Math.Min(__instance.phaseDays.Count - 1, __instance.currentPhase.Value)] : 0);
                if (__instance.dayOfCurrentPhase.Value >= ((__instance.phaseDays.Count > 0) ? __instance.phaseDays[Math.Min(__instance.phaseDays.Count - 1, __instance.currentPhase.Value)] : 0) && __instance.currentPhase.Value < __instance.phaseDays.Count - 1)
                {
                    __instance.currentPhase.Value++;
                    __instance.dayOfCurrentPhase.Value = 0;
                }
                while (__instance.currentPhase.Value < __instance.phaseDays.Count - 1 && __instance.phaseDays.Count > 0 && __instance.phaseDays[__instance.currentPhase.Value] <= 0)
                {
                    __instance.currentPhase.Value++;
                }
            }
            else if (weed >= 100 && Config.WeedsStopGrowth)
            {
                return false;
            }
            return true;
        }
        internal static void HoeDirt_dayUpdate_Postfix(HoeDirt __instance)
        {
            if (!Config.ModEnabled)
                return;
            if(Game1.currentLocation.IsOutdoors && Game1.season == Season.Winter)
            {
                __instance.modData[modKey] = "0";
                if (Game1.random.NextDouble() < 0.5)
                    __instance.modData[modFlippedKey] = "true";
                return;
            }
            int weed = 1;
            if (__instance.modData.TryGetValue(modKey, out var weedStr))
            {
                weed = int.Parse(weedStr);
            }
            else
            {
                if (Game1.random.NextDouble() < 0.5)
                    __instance.modData[modFlippedKey] = "true";
            }
            weed += Game1.random.Next(Config.WeedGrowthPerDayMin, Config.WeedGrowthPerDayMax);
            __instance.modData[modKey] = weed.ToString();
        }
        internal static void HoeDirt_draw_Postfix(HoeDirt __instance, SpriteBatch spriteBatch)
        {
            if (!Config.ModEnabled)
                return;
            int weed = 0;
            if (__instance.modData.TryGetValue(modKey, out var weedStr))
            {
                weed = int.Parse(weedStr);
            }
            if (weed < 25)
            {
                return;
            }
            Vector2 drawPos = Game1.GlobalToLocal(Game1.viewport, __instance.Tile * 64f);
            int x = 0;
            if (weed < 50)
            {
                x = 48;
            }
            else if (weed < 75)
            {
                x = 32;
            }
            else if (weed < 100)
            {
                x = 16;
            }
            spriteBatch.Draw(weedTex, drawPos, new Rectangle?(new Rectangle(x, 0, 16, 16)), new Color(Config.WeedTintR, Config.WeedTintG, Config.WeedTintB, Config.WeedTintA), 0f, Vector2.Zero, 4f, __instance.modData.ContainsKey(modFlippedKey) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1.3E-08f);
        }
        internal static void Utility_canGrabSomethingFromHere_Postfix(ref bool __result, int x, int y, Farmer who)
        {
            if (!Config.ModEnabled || __result || Game1.currentLocation == null || !who.IsLocalPlayer)
                return;
            Vector2 tileLocation = new Vector2((float)(x / 64), (float)(y / 64));

            if (Game1.currentLocation.terrainFeatures.TryGetValue(tileLocation, out var obj))
            {
                if (Game1.currentLocation.terrainFeatures.TryGetValue(tileLocation, out var tf))
                {
                    if (tf is HoeDirt && tf.modData.TryGetValue(modKey, out var data) && int.Parse(data) >= 25 && !(tf as HoeDirt).readyForHarvest())
                    {
                        Game1.mouseCursor = Game1.cursor_harvest;
                        if (Utility.withinRadiusOfPlayer(x, y, 1, who))
                        {
                            __result = true;
                        }
                    }
                }
            }
        }
    }
}
