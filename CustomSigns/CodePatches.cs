using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CustomSigns
{
    public partial class ModEntry
    {
        private static Object placedSign;
        
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
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
                    if(obj.GetBoundingBox().Intersects(position))
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
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, bool __result)
            {
                if (!Config.EnableMod || !SHelper.Input.IsDown(Config.ModKey))
                    return true;
                Rectangle tileRect = new Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);

                foreach (var kvp in __instance.objects.Pairs)
                {
                    if(!kvp.Value.GetBoundingBox().Intersects(tileRect) || !kvp.Value.modData.TryGetValue(templateKey, out string template) || !customSignDataDict.TryGetValue(template, out var data))
                        continue;

                    List<Response> responses = new List<Response>();
                    for (int i = 0; i < data.text.Length; i++)
                    {
                        if (data.text[i].editableName != null)
                        {
                            responses.Add(new Response(data.text[i].editableName, data.text[i].editableName));
                        }
                    }
                    if (!responses.Any())
                        return true;
                    placedSign = kvp.Value;
                    responses.Add(new Response("cancel", SHelper.Translation.Get("cancel")));
                    Game1.player.currentLocation.createQuestionDialogue(SHelper.Translation.Get("which-text"), responses.ToArray(), "CS_Edit_Text");

                    __result = true;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.GetBoundingBox))]
        public class GetBoundingBox_Patch
        {
            public static void Postfix(Object __instance, ref Rectangle __result, NetRectangle ___boundingBox)
            {
                if (!Config.EnableMod || !customSignTypeDict.ContainsKey(__instance.Name) || !__instance.modData.TryGetValue(templateKey, out string template) || !customSignDataDict.TryGetValue(template, out CustomSignData data))
                    return;
                var x = Environment.StackTrace;
                __result = new Rectangle((int)__instance.TileLocation.X * 64, (int)__instance.TileLocation.Y * 64 + 64 - data.tileHeight * 64, data.tileWidth * 64, data.tileHeight * 64);
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
                    string template = "";
                    if(kvp.Value.boundingBox.Value.Contains(x, y) && customSignTypeDict.ContainsKey(kvp.Value.Name) && kvp.Value.modData.TryGetValue(templateKey, out template) && customSignDataDict.ContainsKey(template))
                    {
                        if (kvp.Value.performToolAction(__instance))
                        {
                            kvp.Value.performRemoveAction();
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
				Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 + 64)) - new Vector2(0, data.texture.Height) * data.scale;
				float draw_layer = Math.Max(0f, ((y + 1) * 64 - 24) / 10000f) + x * 1E-05f;
				spriteBatch.Draw(data.texture, position, null, Color.White * alpha, 0f, Vector2.Zero, data.scale, SpriteEffects.None, draw_layer);
                if(data.text != null)
                {
                    for(int i = 0; i < data.text.Length; i++)
                    {
                        var text = data.text[i];
                        if (!fontDict.ContainsKey(text.fontPath))
                            continue;
                        string textStr = text.text;
                        if(text.editableName != null && __instance.modData.TryGetValue(textKey + text.editableName, out var str))
                        {
                            textStr = str;
                        }
                        Vector2 pos;
                        if (text.center)
                        {
                            pos = new Vector2(position.X + text.X - fontDict[text.fontPath].MeasureString(textStr).X / 2 * text.scale, position.Y + text.Y);
                        }
                        else
                        {
                            pos = new Vector2(position.X + text.X, position.Y + text.Y);
                        }
                        spriteBatch.DrawString(fontDict[text.fontPath], textStr, pos, text.color, 0, Vector2.Zero, text.scale, SpriteEffects.None, draw_layer + 1 / 10000f * (i+1));
                    }
                }
                return false;
			}
		}
    }
}