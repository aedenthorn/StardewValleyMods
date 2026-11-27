using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Objects;
using System;
using Object = StardewValley.Object;

namespace SlimeHutchFertilizer
{
    public partial class ModEntry
    {


        [HarmonyPatch(typeof(Fence), nameof(Fence.performObjectDropInAction))]
        public static class Fence_performObjectDropInAction_Patch
        {
            public static void Postfix(Fence __instance, Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed, ref bool __result)
            {
                if (!Config.ModEnabled || __result || dropInItem is not Object obj || __instance.Location is not GameLocation location || __instance.heldObject.Value != null || __instance.isGate.Value || Config.DisallowedDecorations.Contains(dropInItem.QualifiedItemId))
                    return;
                if (!probe)
                {
                    __instance.heldObject.Value = (Object)obj.getOne();
                    location.playSound("axe", null, null, SoundContext.Default);
                    __instance.heldObject.Value.Location = __instance.Location;

                    __instance.heldObject.Value.boundingBox.X = __instance.boundingBox.X;
                    __instance.heldObject.Value.boundingBox.Y = __instance.boundingBox.Y;
                    __instance.heldObject.Value.performDropDownAction(who);
                    __instance.heldObject.Value.Fragility = 2;

                }
                __result = true;
            }
        }


        [HarmonyPatch(typeof(Fence), nameof(Fence.minutesElapsed))]
        public static class Fence_minutesElapsed_Patch
        {
            public static void Postfix(Fence __instance, ref bool __result) 
            {
                if (!Config.ModEnabled || __instance.heldObject.Value == null)
                    return;
                if(__result)
                {
                    if (Config.ReturnOnDestroy)
                    {
                        Game1.createItemDebris(__instance.heldObject.Value, __instance.TileLocation * 64 + new Vector2(32f, 32f), -1, __instance.Location, -1, false);

                    }
                    return;
                }
                if(__instance.heldObject.Value is Furniture f && f.Location is not null)
                {
                    if (f.timeToTurnOnLights())
                        f.addLights();
                    else
                        f.removeLights();
                }
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch),typeof(int),typeof(int),typeof(float),typeof(float) })]
        public static class Object_draw_Patch
        {
            public static bool Prefix(Object __instance, SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha) 
            {
                if (!Config.ModEnabled)
                    return true;

                if(__instance is Furniture f)
                {
                    Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile, yNonTile));
                    f.drawAtNonTileSpot(spriteBatch, position, layerDepth, alpha);
                    return false;
                }
                else if (__instance is ColoredObject co)
                {
                    DrawColoredObject(co, spriteBatch, xNonTile, yNonTile, alpha);
                }
                return true;
            }

        }
    }
}