using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace FillableVases
{
	public partial class ModEntry
    {
        public static Dictionary<string, CachedFlowerData> cachedFlowerData = new Dictionary<string, CachedFlowerData>();

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.draw), new Type[] { typeof(SpriteBatch),typeof(int),typeof(int),typeof(float) })]
        public static class Furniture_draw_Patch
        {
            public static void Postfix(Furniture __instance, SpriteBatch spriteBatch, float alpha, NetVector2 ___drawPosition)
            {
                if (!Config.ModEnabled)
                    return;
                if (!__instance.modData.TryGetValue(flowerKey, out var flowerData))
                    return;
                var dict = SHelper.GameContent.Load<Dictionary<string, VaseData>>(dictPath);

                if (!dict.TryGetValue(__instance.ItemId, out var vaseData))
                    return;
                
                var flowers = flowerData.Split('|');
                if (flowers.Length > vaseData.Flowers.Length)
                    return;
                Vector2 actualDrawPosition = Game1.GlobalToLocal(Game1.viewport, ___drawPosition.Value + ((__instance.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero));
                var b = Furniture.isDrawingLocationFurniture;
                var layerOffset = (__instance.furniture_type.Value == 12) ? (2E-09f + __instance.TileLocation.Y) : ((float)(__instance.boundingBox.Value.Bottom - ((__instance.furniture_type.Value == 6 || __instance.furniture_type.Value == 17 || __instance.furniture_type.Value == 13) ? 48 : 8)));
                layerOffset += 5200;
                for (int i = 0; i < flowers.Length; i++)
                {
                    var split = flowers[i].Split(',');
                    var fd = vaseData.Flowers[i];
                    if (!cachedFlowerData.TryGetValue(split[0], out var cache))
                    {
                        cache = new CachedFlowerData();
                        ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(ItemRegistry.QualifyItemId(split[0]));
                        cache.Texture = itemData.GetTexture();
                        if (Game1.objectData.TryGetValue(split[0], out var data))
                        {
                            cache.SameIndex = !data.ColorOverlayFromNextIndex;
                        }
                        var rect = itemData.GetSourceRect(0, itemData.SpriteIndex);
                        if(fd.Height > 0)
                            rect.Size = new Point(rect.Size.X, fd.Height);
                        cache.SourceRect = rect;
                        if (!cache.SameIndex)
                        {
                            var colorRect = itemData.GetSourceRect(1, itemData.SpriteIndex);
                            if (fd.Height > 0)
                                colorRect.Size = new Point(colorRect.Size.X, fd.Height);
                            cache.ColorSourceRect = colorRect;
                        }
                        cachedFlowerData[split[0]] = cache;
                    }
                    var pos = actualDrawPosition + new Vector2(fd.X, fd.Y + 4);
                    var color = StringToColor(split[1]);
                    if(split.Length > 3 && prismaticFlowersAPI is not null)
                        color = prismaticFlowersAPI.GetPrismaticColorForItemId(split[0], int.TryParse(split[3], out var offset) ? offset : 0);
                    if (!cache.SameIndex)
                    {
                        spriteBatch.Draw(cache.Texture, pos, cache.SourceRect, Color.White * alpha, fd.Rotation, fd.Origin, fd.Scale, SpriteEffects.None, (layerOffset + i * 10) / 100000f);
                        spriteBatch.Draw(cache.Texture, pos, cache.ColorSourceRect, color * alpha, fd.Rotation, fd.Origin, fd.Scale, SpriteEffects.None, (layerOffset + 1 + i * 10) / 100000f);
                    }
                    else
                    {
                        spriteBatch.Draw(cache.Texture, pos, cache.SourceRect, color * alpha, fd.Rotation, fd.Origin, fd.Scale, SpriteEffects.None, (layerOffset + i * 10) / 100000f);
                    }
                }
                if(vaseData.MaskTexture != null)
                {
                    spriteBatch.Draw(SHelper.GameContent.Load<Texture2D>(vaseData.MaskTexture), actualDrawPosition, null, Color.White * alpha, 0, Vector2.Zero, 4f, SpriteEffects.None, (layerOffset + 100) / 100000f);
                }
            }

            private static Color StringToColor(string hex)
            {
                return Utility.StringToColor(hex) ?? Color.White;
            }
        }

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.clicked))]
        public static class Furniture_clicked_Patch
        {
            public static bool Prefix(Furniture __instance, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || !SHelper.GameContent.Load<Dictionary<string, VaseData>>(dictPath).TryGetValue(__instance.ItemId, out var vaseData))
                    return true;
                if (Config.Debug)
                {
                    foreach (var kvp in Game1.getFarm().terrainFeatures.Pairs.Where(kvp => kvp.Value is HoeDirt))
                    {
                        var crop = new Crop(Game1.random.Choose("455", "453", "429", "427", "425", "431"), (int)kvp.Key.X, (int)kvp.Key.Y, Game1.getFarm());
                        //var crop = new Crop("429", (int)kvp.Key.X, (int)kvp.Key.Y, Game1.getFarm());
                        crop.growCompletely();
                        (kvp.Value as HoeDirt).crop = crop;
                    }
                }
                if (who.ActiveObject is Object obj && obj.Category == Object.flowersCategory)
                {
                    List<string> flowers = new List<string>();
                    if (__instance.modData.TryGetValue(flowerKey, out var flowerData))
                    {
                        flowers = flowerData.Split('|').ToList();
                        if (flowers.Count == vaseData.Flowers.Length)
                        {
                            ReturnFlowers(flowerData, who);
                            __instance.modData.Remove(flowerKey);
                            __result = true;
                            return false;
                        }
                    }
                    flowers.Add($"{obj.ItemId},{(obj is ColoredObject co ? MakeColorString(co.color.Value) : "")},{obj.Quality}{(obj.modData.TryGetValue(prismaticKey, out var p) ? "," + p : "")}");
                    __instance.modData[flowerKey] = string.Join('|', flowers);
                    who.currentLocation.playSound("woodyStep", null, null, SoundContext.Default);
                    who.reduceActiveItemByOne();
                }
                else
                {
                    if (!__instance.modData.TryGetValue(flowerKey, out var flowerData))
                        return false;
                    ReturnFlowers(flowerData, who);
                    __instance.modData.Remove(flowerKey);
                    Game1.playSound("coin", null);
                }
                __result = true;
                return false;
            }

        }

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.performObjectDropInAction))]
        public static class Furniture_performObjectDropInAction_Patch
        {
            public static bool Prefix(Furniture __instance, Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed, ref bool __result)
            {
                return true;

            }
        }
    }
}
