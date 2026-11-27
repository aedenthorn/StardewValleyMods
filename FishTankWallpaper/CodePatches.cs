using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace FishTankWallpaper
{
    public partial class ModEntry
    {
        private static bool skip = false;

       
        [HarmonyPatch(typeof(FishTankFurniture), nameof(FishTankFurniture.checkForAction))]
        public static class FishTankFurniture_checkForAction_Patch
        {
            public static bool Prefix(FishTankFurniture __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
            {
                if (!Config.ModEnabled || justCheckingForActivity || __instance.Location is not GameLocation location || !TankDict.TryGetValue(__instance.ItemId, out var path))
                    return true;

                if (SHelper.Input.IsDown(Config.RemoveModKey))
                {
                    location.playSound("grassyStep");

                    __instance.modData.Remove(modKey);
                    return false;
                }
                if (who.ActiveObject is not Wallpaper wp)
                    return true;

                location.playSound("dropItemInWater");
                if (__instance.modData.TryGetValue(modKey, out var oldId))
                {
                    var setId = oldId.Split(':');
                    var item = !string.IsNullOrEmpty(setId[0]) ? new Wallpaper(setId[0], int.Parse(setId[1])) : new Wallpaper(int.Parse(setId[1]));
                    var oldPaper = who.addItemToInventory(item);
                    if (oldPaper != null)
                    {
                        Game1.createItemDebris(oldPaper, new Vector2(__instance.TileLocation.X + (float)__instance.getTilesWide() / 2f + 0.5f, __instance.TileLocation.Y + 0.5f) * 64f, -1, location, -1, false);
                    }
                }
                if (who.CurrentItem.ConsumeStack(1) == null)
                {
                    who.removeItemFromInventory(who.CurrentItem);
                    who.showNotCarrying();
                }
                __instance.modData[modKey] = $"{(wp.setId.Value is null ? ":" : "")}{wp.ItemId}:0";
                __result = true;
                return false;
            }
        }
       

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public static class Furniture_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Furniture.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(ParsedItemData), nameof(ParsedItemData.GetTexture)))
                    {
                        codes.Insert(i + 1, new(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.CheckTexture))));
                        codes.Insert(i + 1, new(OpCodes.Ldarg_0));
                        i += 2;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(FishTankFurniture), nameof(FishTankFurniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public static class FishTankFurniture_draw_Patch
        {
            public static void Prefix(FishTankFurniture __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
            {
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(modKey, out var str) || !TankDict.TryGetValue(__instance.ItemId, out var data))
                    return;
                var setId = str.Split(':');
                int which;
                Texture2D texture;
                if (!string.IsNullOrEmpty(setId[0]))
                {
                    try
                    {
                        texture = SHelper.GameContent.Load<Texture2D>(DataLoader.AdditionalWallpaperFlooring(Game1.content).First(e => e.Id == setId[0]).Texture);
                        which = int.Parse(setId[1]);
                    }
                    catch
                    {
                        return;
                    }
                }
                else
                {
                    texture = SHelper.GameContent.Load<Texture2D>("Maps/walls_and_floors");
                    which = int.Parse(setId[1]);
                }
                int offset = -int.Parse(setId[2]);
                var height = data.WallpaperBounds.Height;
                Rectangle sourceRect = new(which * 16 % texture.Width, (which * 16 / texture.Width) * 48 + offset, 16, height);
                float scale = 4;
                int width = 16;
                int count = data.WallpaperBounds.Width / width;
                int extra = data.WallpaperBounds.Width % width;
                int left = (int)Math.Ceiling(extra / 2f);
                int right = extra / 2;
                var box = __instance.boundingBox.Value;
                var layer = (float)(box.Top + 16) / 10000f;
                var loc = Game1.GlobalToLocal(new Vector2(box.Left + data.WallpaperBounds.X * 4, box.Bottom - 192 + data.WallpaperBounds.Y * 4));
                if (extra > 0)
                {
                    spriteBatch.Draw(texture, loc, new(sourceRect.X + 16 - left, sourceRect.Y, left, height), Color.White * alpha, 0f, Vector2.Zero, scale, SpriteEffects.None, layer);
                    if(right > 0)
                    {
                        spriteBatch.Draw(texture, loc + new Vector2(data.WallpaperBounds.Width * 4f - right * scale, 0), new(sourceRect.X, sourceRect.Y, right, height), Color.White * alpha, 0f, Vector2.Zero, scale, SpriteEffects.None, layer);
                    }
                }
                for(int i = 0; i < count; i++)
                {
                    spriteBatch.Draw(texture, loc + new Vector2(i * width * 4 + left * scale, 0), sourceRect, Color.White * alpha, 0f, Vector2.Zero, scale, SpriteEffects.None, layer);
                }
            }
        }
    }
}