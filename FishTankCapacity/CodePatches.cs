using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

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
            public static void Postfix(FishTankFurniture __instance)
            {
                if (!Config.ModEnabled)
                    return;
                List<string> placed = new();
                for(int i = 0; i < __instance.heldItems.Count; i++)
                {
                    var item = __instance.heldItems[i];
                    skip = true;
                    if (!__instance.CanBeDeposited(item))
                    {
                        var slot = __instance.floorDecorations.FindIndex(kvp => kvp is null);
                        if(slot > -1)
                        {
                            var slots = __instance.decorationSlots.Where(s => !__instance.floorDecorations.Any(f => f is KeyValuePair<Rectangle, Vector2> kvp && Vector2.Distance(kvp.Value, s) < 8));
                            if(slots.Any())
                                __instance.floorDecorations[slot] = new KeyValuePair<Rectangle, Vector2>(new Rectangle(i, 0, 0, 0), slots.ElementAt(Game1.random.Next(slots.Count())));
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
                        item.drawInMenu(spriteBatch, Game1.GlobalToLocal(new Vector2((float)__instance.GetTankBounds().Left + decoration_position.X * 4f - 32, (float)(__instance.GetTankBounds().Bottom - 4) - decoration_position.Y * 4f - 32)), 1f, 1f, decoration_layer, StackDrawType.Hide, Color.White, false);
                    }
                }
            }
        }
    }
}