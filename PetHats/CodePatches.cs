using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;
using System;

namespace PetHats
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Pet), nameof(Pet.checkAction))]
        public class Pet_checkAction_Patch
        {
            public static void Postfix(Pet __instance, Farmer who, GameLocation l, ref bool __result)
            {
                if (!Config.EnableMod || __result)
                    return;
                __instance.modData.TryGetValue(hatKey, out string str);
                if(who.CurrentItem is Hat)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        TryReturnHat(__instance, who, str);
                    }
                    SMonitor.Log($"Placing {who.CurrentItem.Name} on {__instance.Name}");
                    hatDict[GetHatString((Hat)who.CurrentItem)] = (Hat)who.CurrentItem;
                    __instance.modData[hatKey] = GetHatString((Hat)who.CurrentItem);
                    Game1.playSound("dirtyHit");
                    who.reduceActiveItemByOne();
                }
                else if(!string.IsNullOrEmpty(str) && (Config.RetrieveButton == SButton.None || SHelper.Input.IsDown(Config.RetrieveButton)))
                {
                    TryReturnHat(__instance, who, str);
                    __instance.modData.Remove(hatKey);
                }
            }
        }
        [HarmonyPatch(typeof(Pet), nameof(Pet.draw))]
        public class Pet_draw_Patch
        {
            public static void Postfix(Pet __instance, SpriteBatch b, int ___shakeTimer)
            {
                if (!Config.EnableMod || !__instance.modData.TryGetValue(hatKey, out string str))
                    return;
                var hat = hatDict.TryGetValue(str, out var ahat) ? ahat : GetHat(str);
                if (hat is null)
                    return;

                if (!GetFrameOffsetsBool(__instance, out int x, out int y, out int direction))
                    return;
                if (!GetHatOffsetBool(__instance, hat, out Vector2 hatOffset))
                    return;
                if (hatOffset.X <= -100f)
                {
                    return;
                }
                float draw_layer = (float)__instance.GetBoundingBox().Center.Y / 10000f + 1E-07f;
                if (___shakeTimer > 0)
                {
                    hatOffset += new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
                }
                if(__instance is Cat && Config.CatOffsets.Count > __instance.whichBreed.Value)
                {
                    hatOffset += Config.CatOffsets[__instance.whichBreed.Value].ToVector2();
                }
                else if(__instance is Dog && Config.DogOffsets.Count > __instance.whichBreed.Value)
                {
                    hatOffset += Config.DogOffsets[__instance.whichBreed.Value].ToVector2() * 2;
                }
                hat.draw(b, Utility.snapDrawPosition(__instance.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(x, y)), 1.33333337f, 1f, draw_layer, direction);

            }
            private static int frame = 0;
            private static bool flip = true;
            public static void Prefix(Pet __instance, SpriteBatch b)
            {
                return;
                if (!Config.Debug)
                    return;
                if (SHelper.Input.IsDown(SButton.Z))
                {
                    SHelper.Input.Suppress(SButton.Z);
                    frame++;
                    frame %= 36;
                }
                __instance.Sprite.CurrentFrame = frame;
                if (SHelper.Input.IsDown(SButton.X))
                {
                    SHelper.Input.Suppress(SButton.X);
                    __instance.FacingDirection++;
                    __instance.FacingDirection %= 4;
                }
                if(SHelper.Input.IsDown(SButton.V))
                {
                    SHelper.Input.Suppress(SButton.V);
                    flip = !flip;

                }
                __instance.flip = flip;
                b.DrawString(Game1.dialogueFont, __instance.Sprite.CurrentFrame + " " + __instance.flip + " " + __instance.FacingDirection, Game1.GlobalToLocal(__instance.position) + new Vector2(0, -128), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 2);
            }
        }
    }
}