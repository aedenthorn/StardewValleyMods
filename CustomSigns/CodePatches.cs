using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Tools;
using System;
using Object = StardewValley.Object;

namespace CustomSigns
{
    public partial class ModEntry
    {
        private static Object placedSign;
        
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool) })]
        public class isCollidingPosition_Patch
        {
            public static bool Prefix(GameLocation __instance, Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile, bool ignoreCharacterRequirement, ref bool __result)
            {
                if (!Config.EnableMod)
                    return true;
                foreach(var obj in __instance.objects.Values)
                {
                    if (!customSignTypeDict.ContainsKey(obj.Name) || !obj.modData.ContainsKey(templateKey))
                        continue;
                    if(obj.getBoundingBox(obj.TileLocation).Intersects(position))
                    {
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
        public class placementAction_Patch
        {
            public static void Postfix(Object __instance, GameLocation location, int x, int y, bool __result)
            {
                if (!Config.EnableMod || !__result || !SHelper.Input.IsDown(Config.ModKey) || !customSignTypeDict.ContainsKey(__instance.Name))
                    return;
                Vector2 placementTile = new Vector2(x / 64, y / 64);
                if (!location.objects.TryGetValue(placementTile, out Object obj))
                    return;
                placedSign = obj;
                ReloadSignData();
                OpenPlacementDialogue();
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.getBoundingBox))]
        public class getBoundingBox_Patch
        {
            public static void Postfix(Object __instance, Vector2 tileLocation, ref Rectangle __result, NetRectangle ___boundingBox)
            {
                if (!Config.EnableMod || !customSignTypeDict.ContainsKey(__instance.Name) || !__instance.modData.TryGetValue(templateKey, out string template) || !customSignDataDict.TryGetValue(template, out CustomSignData data))
                    return;
                var x = Environment.StackTrace;
                __result = new Rectangle((int)tileLocation.X * 64 + 32 / 2 - data.tileWidth * 64 / 2, (int)tileLocation.Y * 64 + 64 - data.tileHeight * 64, data.tileWidth * 64, data.tileHeight * 64);
                ___boundingBox.Set(__result);
            }
        }
        [HarmonyPatch(typeof(Pickaxe), nameof(Pickaxe.DoFunction))]
        public class Pickaxe_DoFunction_Patch
        {
            public static void Postfix(Pickaxe __instance, GameLocation location, int x, int y, int power, Farmer who)
            {
                if (!Config.EnableMod)
                    return;
                foreach(var kvp in location.objects.Pairs)
                {
                    if(kvp.Value.boundingBox.Value.Contains(x, y) && customSignTypeDict.ContainsKey(kvp.Value.Name) && kvp.Value.modData.TryGetValue(templateKey, out string template) && customSignDataDict.ContainsKey(template))
                    {
                        if (kvp.Value.performToolAction(__instance, location))
                        {
                            kvp.Value.performRemoveAction(kvp.Key, location);
                            Game1.currentLocation.debris.Add(new Debris(kvp.Value.bigCraftable.Value ? (-kvp.Value.ParentSheetIndex) : kvp.Value.ParentSheetIndex, who.GetToolLocation(false), new Vector2(who.GetBoundingBox().Center.X, who.GetBoundingBox().Center.Y)));
                            Game1.currentLocation.Objects.Remove(kvp.Key);
                            return;
                        }

                    }
                }
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class draw_Patch
        {

            public static bool Prefix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
            {
                if (!Config.EnableMod || __instance.isTemporarilyInvisible || !__instance.bigCraftable.Value || !customSignTypeDict.ContainsKey(__instance.Name) || !__instance.modData.TryGetValue(templateKey, out string template) || !customSignDataDict.TryGetValue(template, out CustomSignData data) || data.texture == null)
                    return true;
				Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 64)) - new Vector2(data.texture.Width / 2, data.texture.Height) * data.scale;
				float draw_layer = Math.Max(0f, ((y + 1) * 64 - 24) / 10000f) + x * 1E-05f;
				spriteBatch.Draw(data.texture, position, null, Color.White * alpha, 0f, Vector2.Zero, data.scale, SpriteEffects.None, draw_layer);
                if(data.text != null)
                {
                    for(int i = 0; i < data.text.Length; i++)
                    {
                        var text = data.text[i];
                        if (!fontDict.ContainsKey(text.fontPath))
                            continue;
                        Vector2 pos;
                        if (text.center)
                        {
                            pos = new Vector2(position.X + text.X - fontDict[text.fontPath].MeasureString(text.text).X / 2 * text.scale, position.Y + text.Y);
                        }
                        else
                        {
                            pos = new Vector2(position.X + text.X, position.Y + text.Y);
                        }
                        spriteBatch.DrawString(fontDict[text.fontPath], text.text, pos, text.color, 0, Vector2.Zero, text.scale, SpriteEffects.None, draw_layer + 1 / 10000f * (i+1));
                    }
                }
                return false;
			}
		}
    }
}