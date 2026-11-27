using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Tools;
using System;
using Object = StardewValley.Object;

namespace NozzleAndEnricher
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public static class Object_draw_Patch
        {
            public static void Postfix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
            {
                if (!Config.ModEnabled || !__instance.IsSprinkler())
                {
                    return;
                }
                Rectangle bounds = __instance.GetBoundingBoxAt(x, y);
                if(__instance.modData.TryGetValue(nozzleKey, out var id))
                {
                    ParsedItemData heldItemData = ItemRegistry.GetDataOrErrorItem(id);
                    spriteBatch.Draw(heldItemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), new Rectangle?(heldItemData.GetSourceRect(1, null)), Color.White * alpha, 0f, new Vector2(8f, 8f), (__instance.scale.Y > 1f) ? __instance.getScale().Y : 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.isPassable() ? bounds.Top : bounds.Bottom) / 10000f + 1E-06f);
                }
                if (__instance.modData.TryGetValue(scarecrowKey, out id))
                {
                    Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y - 1.5f) * 64);
                    Rectangle destination = new Rectangle((int)position.X + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)position.Y + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), 64, 128);
                    ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(id);
                    spriteBatch.Draw(itemData.GetTexture(), destination, new Rectangle?(itemData.GetSourceRect()), Color.White * alpha, 0f, Vector2.Zero, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.isPassable() ? bounds.Top : bounds.Bottom) / 10000f + 1E-06f);
                }
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.IsScarecrow))]
        public static class Object_IsScarecrow_Patch
        {
            public static bool Prefix(Object __instance, ref bool __result)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(scarecrowKey))
                {
                    return true;
                }
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.GetModifiedRadiusForSprinkler))]
        public static class Object_GetModifiedRadiusForSprinkler_Patch
        {
            public static void Postfix(Object __instance, ref int __result)
            {
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(nozzleKey, out var id))
                {
                    return;
                }
                if(!Nozzles.TryGetValue(id, out var bonus))
                {
                    return;
                }
                __result += bonus;
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.performToolAction))]
        public static class Object_performToolAction_Patch
        {
            public static bool Prefix(Object __instance, Tool t, ref bool __result)
            {
                if (!Config.ModEnabled || (t is not Pickaxe && t is not Axe) || (__instance.heldObject.Value is not null && !SHelper.Input.IsDown(Config.ModKey)) || !__instance.IsSprinkler())
                {
                    return true;
                }
                if(__instance.modData.TryGetValue(nozzleKey, out var id))
                {
                    __instance.playNearbySoundAll("hammer");
                    var obj = ItemRegistry.Create<Object>(id);
                    __instance.Location.debris.Add(new Debris(obj, __instance.TileLocation * 64f + new Vector2(32f, 32f)));
                    __instance.modData.Remove(nozzleKey);
                    __result = false;
                    return false;
                }
                if(__instance.modData.TryGetValue(scarecrowKey, out id))
                {
                    __instance.playNearbySoundAll("hammer");
                    var obj = ItemRegistry.Create<Object>(id);
                    __instance.Location.debris.Add(new Debris(obj, __instance.TileLocation * 64f + new Vector2(32f, 32f)));
                    __instance.modData.Remove(scarecrowKey);
                    __result = false;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.performRemoveAction))]
        public static class Object_performRemoveAction_Patch
        {
            public static void Postfix(Object __instance)
            {
                if (!Config.ModEnabled || !__instance.IsSprinkler())
                {
                    return;
                }
                if(__instance.modData.TryGetValue(nozzleKey, out var id))
                {
                    var obj = ItemRegistry.Create<Object>(id);
                    __instance.Location.debris.Add(new Debris(obj, __instance.TileLocation * 64f + new Vector2(32f, 32f)));
                    __instance.modData.Remove(nozzleKey);
                }
                else if (__instance.modData.TryGetValue(scarecrowKey, out id))
                {
                    var obj = ItemRegistry.Create<Object>(id);
                    __instance.Location.debris.Add(new Debris(obj, __instance.TileLocation * 64f + new Vector2(32f, 32f)));
                    __instance.modData.Remove(scarecrowKey);
                }
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.performObjectDropInAction))]
        public static class Object_performObjectDropInAction_Patch
        {
            public static bool Prefix(Object __instance, ref Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed, ref bool __result)
            {
                if (!Config.ModEnabled || !__instance.IsSprinkler())
                {
                    return true;
                }

                var heldId = __instance.heldObject.Value?.QualifiedItemId;
                var dropInId = dropInItem?.QualifiedItemId;
                if (heldId is not null && Nozzles.ContainsKey(heldId))
                {
                    __instance.modData[nozzleKey] = heldId;
                    __instance.heldObject.Value = null;

                }
                if (Nozzles.ContainsKey(dropInId))
                {
                    if (__instance.modData.ContainsKey(nozzleKey))
                    {

                        return false;
                    }
                    else
                    {
                        if (!probe)
                        {
                            __instance.modData[nozzleKey] = dropInId;
                            __instance.Location.playSound("axe");
                        }
                        __result = true;
                        return false;
                    }
                }
                if (dropInItem is Object obj && obj.IsScarecrow())
                {
                    if (__instance.modData.ContainsKey(scarecrowKey))
                    {
                        return false;
                    }
                    else
                    {
                        if (!probe)
                        {
                            __instance.modData[scarecrowKey] = dropInId;
                            __instance.Location.playSound("axe");
                        }
                        __result = true;
                        return false;
                    }
                }
                return true;

            }
        }
    }
}