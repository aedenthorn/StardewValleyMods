using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace TrainTracks
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {

        private static void Horse_draw_Prefix(Horse __instance, SpriteBatch b)
        {
            if (!Config.EnableMod || !__instance.modData.ContainsKey(trainKey) || frontTexture == null)
                return;
            
            b.Draw(frontTexture, Game1.GlobalToLocal(Game1.viewport, __instance.Position + new Vector2(__instance.flip ? 0 : -16, - 80)), new Rectangle?(__instance.Sprite.sourceRect), Color.White, 0f, Vector2.Zero, 4f, __instance.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.Position.Y + 64f) / 10000f);
        }
        private static void Horse_draw_Postfix(Horse __instance, SpriteBatch b)
        {
            if (!Config.EnableMod || !__instance.modData.ContainsKey(trainKey) || frontTexture == null)
                return;
            
            b.Draw(frontTexture, Game1.GlobalToLocal(Game1.viewport, __instance.Position + new Vector2(__instance.flip ? 0 : -16, - 80)), new Rectangle?(__instance.Sprite.sourceRect), Color.White, 0f, Vector2.Zero, 4f, __instance.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.Position.Y + 64f) / 10000f);
        }
        private static bool Flooring_draw_Prefix(Flooring __instance, SpriteBatch spriteBatch, Vector2 tileLocation)
        {
            if (!Config.EnableMod || !__instance.modData.TryGetValue(trackKey, out string indexString) || !int.TryParse(indexString, out int index ))
                return true;

            spriteBatch.Draw(trackTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle?(new Rectangle(index * 16, 0, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-09f + 0.0000001f);
            return __instance.whichFloor.Value != -42;
        }
        private static bool Flooring_performToolAction_Prefix(Flooring __instance, Tool t, int damage, Vector2 tileLocation, GameLocation location, ref bool __result)
        {
            if (!Config.EnableMod || __instance.whichFloor.Value != -42 || !__instance.modData.ContainsKey(trackKey))
                return true;
            if (location == null)
            {
                location = Game1.currentLocation;
            }
            if ((t != null || damage > 0) && (damage > 0 || t is Pickaxe || t is Axe))
            {
                Game1.createRadialDebris(location, 14, (int)tileLocation.X, (int)tileLocation.Y, 4, false, -1, false, -1);
                location.playSound(Config.RemoveSound);
                __result = true;
            }
            else
                __result = false;
            return false;
        }
        private static void Horse_dismount_Prefix(Horse __instance, ref bool from_demolish)
        {
            if (!Config.EnableMod || !__instance.modData.ContainsKey(trainKey))
                return;
            from_demolish = true;
            __instance.rider.canMove = true;
        }
        private static void Horse_checkAction_Prefix(Horse __instance, Farmer who)
        {
            if (!Config.EnableMod || !__instance.modData.ContainsKey(trainKey) || __instance.rider == null)
                return;

            __instance.rider.canMove = true;
        }
        /*
        private static bool Horse_update_Prefix(Horse __instance, GameLocation location, ref bool ___squeezingThroughGate)
        {
            if (!Config.EnableMod || !__instance.modData.ContainsKey(trainKey) || __instance.rider != null || __instance.dismounting.Value || __instance.mounting.Value)
                return true;
            __instance.currentLocation = location;
            __instance.mutex.Update(location);
            ___squeezingThroughGate = false;
            __instance.faceTowardFarmer = false;
            __instance.faceTowardFarmerTimer = -1;
            __instance.Sprite.loop = false;
            if (__instance.FacingDirection == 3)
            {
                __instance.drawOffset.Set(Vector2.Zero);
            }
            else
            {
                __instance.drawOffset.Set(new Vector2(-16f, 0f));
            }
            __instance.flip = (__instance.FacingDirection == 3);
            __instance.update(time, location);
        }
        */
    }
}