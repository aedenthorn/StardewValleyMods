using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using System;
using static HarmonyLib.Code;
using static StardewValley.FarmerSprite;
using Object = StardewValley.Object;

namespace CraftableBalloons
{
	public partial class ModEntry
    {

        [HarmonyPatch(typeof(Game1), nameof(Game1.pressUseToolButton))]
        public static class Game1_pressUseToolButton_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                if (!Config.ModEnabled || !Context.IsPlayerFree || !SHelper.Input.IsDown(Config.ModKey))
                    return true;
                Character npc = Game1.currentLocation.doesPositionCollideWithCharacter(Utility.getRectangleCenteredAt(Game1.player.GetToolLocation(false), 64), true);
                if(npc == null)
                {
                    Vector2 mousePosition = new Vector2((float)(Game1.getOldMouseX() + Game1.viewport.X), (float)(Game1.getOldMouseY() + Game1.viewport.Y));

                    foreach (FarmAnimal animal in Game1.currentLocation.animals.Values)
                    {
                        if (animal.wasPet.Value && animal.GetCursorPetBoundingBox().Contains((int)mousePosition.X, (int)mousePosition.Y))
                        {
                            npc = animal;
                            break;
                        }
                    }
                }
                if (npc == null)
                    return true;
                var oldHeldObj = Game1.player.ActiveObject;
                if (oldHeldObj is ColoredObject c && TryGetBalloonTexture(oldHeldObj.ItemId, out _))
                {
                    Game1.player.reduceActiveItemByOne();
                    if (npc.modData.TryGetValue(modKey, out var ct))
                    {
                        var split = ct.Split(' ');

                        var oldObj = new ColoredObject(split.Length > 1 ? split[1] : balloonKey, 1, GetColor(split[0]));
                        if (Game1.player.ActiveObject != null)
                        {
                            if (!Game1.player.addItemToInventoryBool(oldObj))
                            {
                                Game1.createItemDebris(oldObj, Game1.player.getStandingPosition(), Game1.player.FacingDirection, Game1.player.currentLocation);
                            }
                        }
                        else
                        {
                            Game1.player.ActiveObject = oldObj;
                        }
                    }
                    npc.modData[modKey] = MakeColorString(c.color.Value) + " " + c.ItemId;
                    __result = true;
                    return false;
                }
                else if (npc.modData.TryGetValue(modKey, out var ct))
                {
                    var split = ct.Split(' ');
                    var oldObj = new ColoredObject(split.Length > 1 ? split[1] : balloonKey, 1, GetColor(split[0]));

                    if (Game1.player.ActiveItem == null)
                    {
                        Game1.player.ActiveItem = oldObj;
                    }
                    else
                    {
                        if (!Game1.player.addItemToInventoryBool(oldObj))
                        {
                            Game1.createItemDebris(oldObj, Game1.player.getStandingPosition(), Game1.player.FacingDirection, Game1.player.currentLocation);
                        }
                    }
                    npc.modData.Remove(modKey);
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ObjectDataDefinition), nameof(ObjectDataDefinition.CreateItem))]
        public static class ObjectDataDefinition_CreateItem_Patch
        {
            public static bool Prefix(ParsedItemData data, ref Item __result)
            {
                if (!Config.ModEnabled || !TryGetBalloonTexture(data.ItemId, out _))
                    return true;
                var color = Game1.random.NextDouble() < Config.PrismaticChance / 100.0 ? new Color(6,6,6) : Utility.getRandomRainbowColor();
                __result =  new ColoredObject(data.ItemId, 1, color);
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.IsHeldOverHead))]
        public static class Object_IsHeldOverHead_Patch
        {
            public static bool Prefix(Object __instance, ref bool __result)
            {
                if (!Config.ModEnabled || !TryGetBalloonTexture(__instance.ItemId, out _))
                    return true;
                __result = false;
                return false;
            }
        }
        [HarmonyPatch(typeof(ColoredObject), nameof(ColoredObject.drawInMenu), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool)})]
        public static class Object_drawInMenu_Patch
        {
            public static void Prefix(ColoredObject __instance, ref Color? __state)
            {
                if (!Config.ModEnabled || !TryGetBalloonTexture(__instance.ItemId, out _) || __instance.color.Value != new Color(6,6,6))
                    return;
                __state = __instance.color.Value;
                __instance.color.Value = Utility.GetPrismaticColor(0, Config.PrismaticSpeed);
            }
            public static void Postfix(ColoredObject __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, Color? __state)
            {
                if (!Config.ModEnabled || !TryGetBalloonTexture(__instance.ItemId, out var texturePath))
                    return;
                spriteBatch.Draw(SHelper.GameContent.Load<Texture2D>(texturePath), location, new Rectangle(16, 0, 16, 16), Color.White * transparency, 0f, Vector2.Zero, 4f * scaleSize, SpriteEffects.None, layerDepth + 1);
                if(__state != null)
                    __instance.color.Value = __state.Value;

            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw), new Type[] { typeof(SpriteBatch) })]
        public static class Farmer_draw_Patch
        {
            public static void Postfix(Farmer __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || !TryGetBalloonFromCharacter(__instance, out string id, out Color color))
                    return;
                DrawBalloon(b, id, color, __instance.Position + new Vector2(0, +__instance.yJumpOffset * 2), __instance.FacingDirection, characterMovement.TryGetValue(__instance, out var data) ? data.vel : Point.Zero, __instance.StandingPixel.Y);
            }
        }
        [HarmonyPatch(typeof(NPC), nameof(NPC.draw), new Type[] { typeof(SpriteBatch), typeof(float) })]
        public static class NPC_draw_Patch
        {
            public static void Postfix(NPC __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || !TryGetBalloonFromCharacter(__instance, out string id, out Color color))
                    return;
                DrawBalloon(b, id, color, __instance.Position + new Vector2(0, +__instance.yJumpOffset * 2), __instance.FacingDirection, characterMovement.TryGetValue(__instance, out var data) ? data.vel : Point.Zero, __instance.StandingPixel.Y);
            }
        }
        [HarmonyPatch(typeof(Pet), nameof(Pet.draw), new Type[] { typeof(SpriteBatch) })]
        public static class Pet_draw_Patch
        {
            public static void Postfix(NPC __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || !TryGetBalloonFromCharacter(__instance, out string id, out Color color))
                    return;
                DrawBalloon(b, id, color, __instance.Position + new Vector2(32, +__instance.yJumpOffset * 2 + 32), __instance.FacingDirection, characterMovement.TryGetValue(__instance, out var data) ? data.vel : Point.Zero, __instance.StandingPixel.Y);
            }
        }
        [HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.draw), new Type[] { typeof(SpriteBatch) })]
        public static class FarmAnimal_draw_Patch
        {
            public static void Postfix(NPC __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || !TryGetBalloonFromCharacter(__instance, out string id, out Color color))
                    return;
                DrawBalloon(b, id, color, __instance.Position + new Vector2(32, +__instance.yJumpOffset * 2 + 32), __instance.FacingDirection, characterMovement.TryGetValue(__instance, out var data) ? data.vel : Point.Zero, __instance.StandingPixel.Y);
            }
        }
        [HarmonyPatch(typeof(Monster), nameof(Monster.draw), new Type[] { typeof(SpriteBatch) })]
        public static class Monster_draw_Patch
        {
            public static void Postfix(NPC __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || !TryGetBalloonFromCharacter(__instance, out string id, out Color color))
                    return;
                DrawBalloon(b, id, color, __instance.Position + new Vector2(32, +__instance.yJumpOffset * 2 + 32), __instance.FacingDirection, characterMovement.TryGetValue(__instance, out var data) ? data.vel : Point.Zero, __instance.StandingPixel.Y);
            }
        }
        [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.receiveLeftClick))]
        public static class InventoryPage_receiveLeftClick_Patch
        {
            public static bool Prefix(InventoryPage __instance, int x, int y)
            {
                string texturePath = null;
                if (!Config.ModEnabled || !__instance.portrait.containsPoint(x, y) || (Game1.player.CursorSlotItem is not null && !TryGetBalloonTexture(Game1.player.CursorSlotItem.ItemId, out texturePath)))
                    return true;
                var oldCursorItem = Game1.player.CursorSlotItem;
                if (oldCursorItem is ColoredObject c)
                {
                    Game1.player.CursorSlotItem.Stack--;
                    if(Game1.player.CursorSlotItem.Stack <= 0)
                    {
                        Game1.player.CursorSlotItem = null;
                    }
                    if (Game1.player.modData.TryGetValue(modKey, out var ct))
                    {
                        var split = ct.Split(' ');
                        var oldObj = new ColoredObject(split.Length > 1 ? split[1] : balloonKey, 1, GetColor(split[0]));
                        if (Game1.player.CursorSlotItem != null)
                        {
                            if (!Game1.player.addItemToInventoryBool(oldObj))
                            {
                                Game1.createItemDebris(oldObj, Game1.player.getStandingPosition(), Game1.player.FacingDirection, Game1.player.currentLocation);
                            }
                        }
                        else
                        {
                            Game1.player.CursorSlotItem = oldObj;
                        }
                    }
                    Game1.player.modData[modKey] = MakeColorString(c.color.Value) + " " + c.ItemId;

                }
                else if (Game1.player.modData.TryGetValue(modKey, out var ct))
                {
                    var split = ct.Split(' ');

                    Game1.player.CursorSlotItem = new ColoredObject(split.Length > 1 ? split[1] : balloonKey, 1, GetColor(split[0]));
                    Game1.player.modData.Remove(modKey);
                }
                else
                {
                    return true;
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(FarmerRenderer), nameof(FarmerRenderer.draw), new Type[] { typeof(SpriteBatch), typeof(AnimationFrame), typeof(int), typeof(Rectangle), typeof(Vector2), typeof(Vector2), typeof(float), typeof(int), typeof(Color), typeof(float), typeof(float), typeof(Farmer) })]
        public static class FarmerRenderer_draw_Patch
        {
            public static void Prefix(FarmerRenderer __instance, SpriteBatch b, Vector2 position, Vector2 origin, float layerDepth, int facingDirection, Color overrideColor, float rotation, float scale, Farmer who)
            {
                //b.DrawString(Game1.dialogueFont, Game1.getMousePosition().ToString(), Vector2.Zero, Color.White);
                if (!Config.ModEnabled || !FarmerRenderer.isDrawingForUI || !who.modData.TryGetValue(modKey, out var ct))
                    return;
                float scaledPixelZoom = 4f * scale;
                var split = ct.Split(' ');
                var id = split.Length > 1 ? split[1] : balloonKey;
                if (!TryGetBalloonTexture(id, out var texturePath))
                    return;
                var tex = SHelper.GameContent.Load<Texture2D>(texturePath);
                var yoff = (int)Math.Round(Math.Sin(Game1.ticks / 50f));

                b.Draw(tex, position + origin + __instance.positionOffset + new Vector2(24 + yoff, -16), new Rectangle(0, 16, 16, 16), GetDisplayColor(split[0]), rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Base, false));
                b.Draw(tex, position + origin + __instance.positionOffset + new Vector2(24, -16), new Rectangle(16, 0, 16, 16), Color.White, rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Base, false));
                b.Draw(tex, position + origin + __instance.positionOffset + new Vector2(24, 48), new Rectangle(16, 16, 16, 12), Color.White, rotation, origin, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Base, false));
            }
        }
    }
}
