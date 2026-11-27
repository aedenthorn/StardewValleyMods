using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;

namespace PetClothes
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Pet), nameof(Pet.drawHat))]
        public static class Pet_drawHat_Patch
        {
            public static void Prefix(Pet __instance, SpriteBatch b, Vector2 shake)
            {
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(modKeyTexture, out string path))
                    return;
                Texture2D texture;
                try
                {
                    texture = SHelper.GameContent.Load<Texture2D>(path);
                }
                catch
                {
                    return;
                }
                int standingY = __instance.StandingPixel.Y;

                b.Draw(texture, __instance.getLocalPosition(Game1.viewport) + new Vector2((float)(__instance.Sprite.SpriteWidth * 4 / 2), (float)(__instance.GetBoundingBox().Height / 2)) + shake, new Rectangle?(__instance.Sprite.SourceRect), Color.White, __instance.rotation, new Vector2((float)(__instance.Sprite.SpriteWidth / 2), (float)__instance.Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, __instance.Scale) * 4f, (__instance.flip || (__instance.Sprite.CurrentAnimation != null && __instance.Sprite.CurrentAnimation[__instance.Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, __instance.isSleepingOnFarmerBed.Value ? (((float)standingY + 112f) / 10000f) : ((float)standingY / 10000f)) + 1/10000f);
            }
        }
        [HarmonyPatch(typeof(Pet), nameof(Pet.checkAction))]
        public static class Pet_checkAction_Patch
        {
            public static bool Prefix(Pet __instance, Farmer who, GameLocation l, ref bool __result)
            {
                if (!Config.ModEnabled)
                    return true;
                if(__instance.modData.TryGetValue(modKeyItem, out string item))
                {
                    if (SHelper.Input.IsDown(Config.RemoveModKey))
                    {
                        __instance.modData.Remove(modKeyItem);
                        __instance.modData.Remove(modKeyTexture);
                    }
                    else if(who.Items.Count > who.CurrentToolIndex && who.Items[who.CurrentToolIndex] is not null && IsPetClothes(__instance, who.Items[who.CurrentToolIndex], out string texture))
                    {
                        Game1.playSound("dirtyHit", null);
                        __instance.modData[modKeyItem] = who.Items[who.CurrentToolIndex].QualifiedItemId;
                        __instance.modData[modKeyTexture] = texture;
                        who.reduceActiveItemByOne();
                    }
                    else
                    {
                        return true;
                    }
                    Game1.createItemDebris(ItemRegistry.Create(item, 1, 0, false), __instance.Position, __instance.FacingDirection, null, -1, false);
                    __result = true;
                    return false;
                }
                else if (IsPetClothes(__instance, who.Items[who.CurrentToolIndex], out string texture))
                {
                    Game1.playSound("dirtyHit", null);
                    __instance.modData[modKeyItem] = who.Items[who.CurrentToolIndex].QualifiedItemId;
                    __instance.modData[modKeyTexture] = texture;
                    who.reduceActiveItemByOne();
                    __result = true;
                    return false;
                }
                return true;
            }

        }
    }
}
