using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Reflection.Metadata.Ecma335;
using Object = StardewValley.Object;

namespace ChestSigns
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Chest), nameof(Chest.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public static class Chest_draw_Patch
        {
            public static void Postfix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
            {
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(signKey, out var id))
                {
                    return;
                }
                int yoff = 128;
                bool hasContent = __instance.modData.TryGetValue(contentsKey, out var contents) && !string.IsNullOrEmpty(contents);
                Rectangle bounds = __instance.GetBoundingBoxAt(x, y);
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - yoff));
                Rectangle destination = new Rectangle((int)position.X + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)position.Y + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), 64, 128);
                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(id);
                spriteBatch.Draw(itemData.GetTexture(), destination, new Rectangle?(itemData.GetSourceRect(!hasContent && id == "(BC)TextSign" ? 1 : 0)), Color.White * alpha, 0f, Vector2.Zero, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, bounds.Top / 10000f - 1E-06f);

                if (hasContent)
                {
                    if(id == "(BC)TextSign")
                    {
                        if (__instance.hovering)
                        {
                            Vector2 pos = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32), (float)(y * 64 - yoff)));
                            SpriteText.drawSmallTextBubble(spriteBatch, contents, pos, 256, 0.98f + __instance.TileLocation.X * 0.0001f + __instance.TileLocation.Y * 1E-06f, false);
                            __instance.hovering = false;
                        }
                    }
                    else
                    {
                        position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y - 1.5f) * 64);
                        destination = new Rectangle((int)position.X + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)position.Y + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), 64, 128);
                        itemData = ItemRegistry.GetDataOrErrorItem(contents);
                        int spriteIndex = itemData.SpriteIndex;
                        Texture2D texture = itemData.GetTexture();
                        Rectangle drawnSourceRect = itemData.GetSourceRect();
                        Vector2 offset = Vector2.Zero;
                        Vector2 shadowOffset = Vector2.Zero;
                        bool shadow = true;
                        var scale = 0.75f;
                        switch (itemData.GetItemTypeId())
                        {
                            case "(H)":
                                offset = new Vector2(-5, -5);
                                scale *= 0.75f;
                                break;
                            case "(F)":
                                break;
                            default:
                                if (itemData.Category == Object.ringCategory)
                                {
                                    offset = new Vector2(-3, -1);
                                }
                                else if (itemData.HasTypeBigCraftable())
                                {
                                    offset = new Vector2(-3, -14);
                                    scale /= 2f;
                                    shadow = false;
                                }
                                else
                                {
                                    offset = new Vector2(1, 1);
                                    shadowOffset = new Vector2(0, -1);
                                }
                                break;
                        }
                        if (itemData.IsErrorItem)
                        {
                            drawnSourceRect = itemData.GetSourceRect(0, null);
                        }
                        var location = Game1.GlobalToLocal(new Vector2((float)(x * 64 + 6), (float)(y * 64 - yoff + 21 + 8))) + offset;
                        var layerDepth = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 1E-05f;
                        if (shadow)
                        {
                            spriteBatch.Draw(texture, location + shadowOffset + new Vector2(32f, 32f + 4), new Rectangle?(drawnSourceRect), Color.Black * 0.45f, 0f, new Vector2(10f, 10f), 4f * scale, SpriteEffects.None, layerDepth);
                        }
                        spriteBatch.Draw(texture, location + new Vector2(32f, 32f), new Rectangle?(drawnSourceRect), Color.White, 0f, new Vector2(10f, 10f), 4f * scale, SpriteEffects.None, layerDepth + 1E-05f);
                    }
                }
            }
        }


        [HarmonyPatch(typeof(Chest), nameof(Chest.checkForAction))]
        public static class Chest_checkForAction_Patch
        {
            public static bool Prefix(Chest __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
            {
                if (!Config.ModEnabled || Game1.activeClickableMenu is not null || !__instance.modData.TryGetValue(signKey, out var id))
                {
                    return true;
                }
                if(id == "(BC)TextSign")
                {
                    if(!SHelper.Input.IsDown(Config.ModKey))
                    if (!justCheckingForActivity)
                    {
                        __instance.modData.TryGetValue(contentsKey, out string text);
                        TitleTextInputMenu signMenu = new TitleTextInputMenu(Game1.content.LoadString("Strings\\UI:TextSignEntry"), null, text, null, true);
                        signMenu.pasteButton.visible = false;
                        signMenu.doneNaming = delegate (string text)
                        {
                            __instance.modData[contentsKey] = text.Trim();
                            signMenu.exitThisMenu(true);
                        };
                        signMenu.textBox.textLimit = 60;
                        Game1.activeClickableMenu = signMenu;
                    }
                    __result = true;
                    return false;
                }
                else if (__instance.performObjectDropInAction(who.CurrentItem, justCheckingForActivity, who))
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(Object), nameof(Object.farmerAdjacentAction))]
        public static class Object_farmerAdjacentAction_Patch
        {
            public static bool Prefix(Object __instance)
            {
                if (!Config.ModEnabled || __instance is not Chest || !__instance.modData.TryGetValue(signKey, out var id) || id != "(BC)TextSign")
                {
                    return true;
                }
                __instance.hovering = true;
                return false;
            }
        }


        [HarmonyPatch(typeof(Chest), nameof(Chest.performToolAction))]
        public static class Chest_performToolAction_Patch
        {
            public static bool Prefix(Chest __instance, Tool t, ref bool __result)
            {
                if (!Config.ModEnabled || t?.getLastFarmerToUse() != Game1.player || t is MeleeWeapon || !t.isHeavyHitter() || !__instance.modData.TryGetValue(signKey, out var id))
                {
                    return true;
                }
                __instance.playNearbySoundAll("hammer");
                var obj = ItemRegistry.Create<Object>(id);
                __instance.Location.debris.Add(new Debris(obj, __instance.TileLocation * 64f + new Vector2(32f, 32f)));
                __instance.modData.Remove(signKey);
                __instance.modData.Remove(contentsKey);
                __result = false;
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.performRemoveAction))]
        public static class Object_performRemoveAction_Patch
        {
            public static void Postfix(Object __instance)
            {
                if (!Config.ModEnabled || __instance is not Chest || !__instance.modData.TryGetValue(signKey, out var id))
                {
                    return;
                }
                var obj = ItemRegistry.Create<Object>(id);
                __instance.Location.debris.Add(new Debris(obj, __instance.TileLocation * 64f + new Vector2(32f, 32f)));
                __instance.modData.Remove(signKey);
                __instance.modData.Remove(contentsKey);
            }
        }

        [HarmonyPatch(typeof(Chest), nameof(Chest.performObjectDropInAction))]
        public static class Chest_performObjectDropInAction_Patch
        {
            public static bool Prefix(Chest __instance, ref Item dropInItem, bool probe, bool returnFalseIfItemConsumed, ref bool __result)
            {
                if (!Config.ModEnabled || dropInItem is null || dropInItem is Chest)
                {
                    return true;
                }
                var dropInId = dropInItem?.QualifiedItemId;
                if (dropInItem.HasContextTag("sign_item") || dropInItem.ItemId == "TextSign")
                {
                    if (!probe)
                    {
                        if (__instance.modData.TryGetValue(signKey, out var id))
                        {
                            var o = ItemRegistry.Create<Object>(id);
                            __instance.Location.debris.Add(new Debris(o, __instance.TileLocation * 64f + new Vector2(32f, 32f)));
                        }
                        __instance.modData[signKey] = dropInId;
                        __instance.Location.playSound("axe");
                    }
                    __result = true;
                    return false;
                }
                else if (__instance.modData.ContainsKey(signKey))
                {
                    if (!probe)
                    {
                        Game1.playSound("coin", null);
                        __instance.modData[contentsKey] = dropInItem.ItemId != "TextSign" ? dropInId : dropInItem.DisplayName;
                    }
                    __result = true;
                    return false;
                }
                return true;
            }
        }
    }
}