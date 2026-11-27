using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace FishTankCapacity
{
    public partial class ModEntry
    {
        private static bool skip = false;

        [HarmonyPatch(typeof(FishTankFurniture), nameof(FishTankFurniture.GetCapacityForCategory))]
        public static class FishTankFurniture_GetCapacityForCategory_Patch
        {
            public static void Postfix(FishTankFurniture __instance, FishTankFurniture.FishTankCategories category, ref int __result)
            {
                if(!Config.ModEnabled)
                    return;
                if (Config.UnlimitedCapacity)
                {
                    __result = int.MaxValue;
                }
                else if(Config.CustomCapacities.TryGetValue(__instance.ItemId, out var cap))
                {
                    __result = cap;
                }
                else
                {
                    __result = (int)Math.Round(__result * Config.CapacityMultiplier);
                }
            }
        }
        [HarmonyPatch(typeof(FishTankFurniture), nameof(FishTankFurniture.CanBeDeposited))]
        public static class FishTankFurniture_CanBeDeposited_Patch
        {
            public static bool Prefix(FishTankFurniture __instance, ref bool __result)
            {
                if (skip || !Config.ModEnabled || !SHelper.Input.IsDown(Config.AddAnyItemModKey))
                    return true;
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(FishTankFurniture), nameof(FishTankFurniture.GetCategoryFromItem))]
        public static class FishTankFurniture_GetCategoryFromItem_Patch
        {
            public static void Postfix(FishTankFurniture __instance, ref FishTankFurniture.FishTankCategories __result, Item item)
            {
                if (!Config.ModEnabled || __result != FishTankFurniture.FishTankCategories.Swim || __instance.GetAquariumData().ContainsKey(item.ItemId))
                    return;
                __result = FishTankFurniture.FishTankCategories.Decoration;
            }
        }
        [HarmonyPatch(typeof(FishTankFurniture), nameof(FishTankFurniture.UpdateDecorAndFish))]
        public static class FishTankFurniture_UpdateDecorAndFish_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling FishTankFurniture.UpdateDecorAndFish");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(FishTankFurniture), nameof(FishTankFurniture.GetItemCount)))
                    {
                        codes.Insert(i, new(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetItemCountCheck))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
            public static void Postfix(FishTankFurniture __instance)
            {
                if (!Config.ModEnabled)
                    return;
                List<string> placed = new();
                for(int i = 0; i < __instance.heldItems.Count; i++)
                {
                    var item = __instance.heldItems[i];
                    skip = true;
                    if (!__instance.CanBeDeposited(item) || DecorationDict.ContainsKey(item.ItemId))
                    {
                        var slot = __instance.floorDecorations.FindIndex(kvp => kvp is null);
                        if(slot > -1)
                        {
                            var slots = __instance.decorationSlots.Where(s => !__instance.floorDecorations.Any(f => f is KeyValuePair<Rectangle, Vector2> kvp && Vector2.Distance(kvp.Value, s) < Config.MinPixelDistance));
                            if (slots.Any())
                            {
                                var pos = slots.ElementAt(Game1.random.Next(slots.Count())) + new Vector2(Game1.random.Next(-Config.RandomShiftPixels, Config.RandomShiftPixels + 1), Game1.random.Next(-Config.RandomShiftPixels, Config.RandomShiftPixels + 1));
                                __instance.floorDecorations[slot] = new KeyValuePair<Rectangle, Vector2>(new Rectangle(i, 0, 0, 0), pos);
                            }
                        }
                    }
                    //else if(!placed.Contains(item.ItemId))
                    //{
                    //    var count = __instance.GetItemCount(item.ItemId);
                    //    if(count > 1)
                    //    {
                    //        var slot = __instance.floorDecorations.FindIndex(kvp => kvp is null);
                    //        if (slot > -1)
                    //        {
                    //            var slots = __instance.decorationSlots.Where(s => !__instance.floorDecorations.Any(f => f is KeyValuePair<Rectangle, Vector2> kvp && kvp.Value == s));
                    //            if (slots.Any())
                    //                __instance.floorDecorations[slot] = new KeyValuePair<Rectangle, Vector2>(new Rectangle(i, 0, 0, 0), slots.ElementAt(Game1.random.Next(slots.Count())));
                    //        }
                    //        placed.Add(item.ItemId);
                    //    }
                    //}
                    skip = false;
                }
            }

        }

       

        [HarmonyPatch(typeof(FishTankFurniture), nameof(FishTankFurniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public static class FishTankFurniture_draw_Patch
        {
            public static void Postfix(FishTankFurniture __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
            {
                if (!Config.ModEnabled || !Furniture.isDrawingLocationFurniture)
                    return;
                for (int j = 0; j < __instance.floorDecorations.Count; j++)
                {
                    if (__instance.floorDecorations[j] is not null && __instance.floorDecorations[j].Value is KeyValuePair<Rectangle, Vector2> pair && pair.Key.Size.X == 0 && pair.Key.Size.X == 0 && __instance.heldItems.Count > pair.Key.X && __instance.heldItems[pair.Key.X] is Item item)
                    {
                        Vector2 decoration_position = pair.Value;
                        float decoration_layer = Utility.Lerp(__instance.GetFishSortRegion().Y, __instance.GetFishSortRegion().X, decoration_position.Y / 20f) - 1E-06f;
                        if(DecorationDict.TryGetValue(item.ItemId, out var list))
                        {
                            var data = list[Game1.random.Next(list.Count)];
                            var texture = SHelper.GameContent.Load<Texture2D>(data.Texture);
                            var sourceRect = data.SourceRect is null ? new Rectangle(0, 0, texture.Width, texture.Height) : data.SourceRect.Value;
                            if(data.Frames > 1)
                            {
                                var offset = Game1.ticks / 60 % data.Frames * sourceRect.Width;
                                sourceRect.X += offset;
                            }
                            AdjustPos(__instance, ref decoration_position, sourceRect.Width);
                            decoration_position += data.DrawOffset;
                            spriteBatch.Draw(texture, Game1.GlobalToLocal(new Vector2((float)__instance.GetTankBounds().Left + decoration_position.X * 4f, (float)(__instance.GetTankBounds().Bottom - 4) - decoration_position.Y * 4f)), sourceRect, Color.White * alpha, 0f, new Vector2((float)(sourceRect.Width / 2), (float)(sourceRect.Height - 4)), data.Scale, SpriteEffects.None, decoration_layer);
                        }
                        else
                        {
                            AdjustPos(__instance, ref decoration_position, 16);
                            item.drawInMenu(spriteBatch, Game1.GlobalToLocal(new Vector2((float)__instance.GetTankBounds().Left + decoration_position.X * 4f - 32, (float)(__instance.GetTankBounds().Bottom - 4) - decoration_position.Y * 4f - 32)), 1f, 1f, decoration_layer, StackDrawType.Hide, Color.White, false);
                        }
                    }
                }
            }
        }
    }
}