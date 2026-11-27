using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Projectiles;
using System;
using System.Linq;
using Object = StardewValley.Object;

namespace AreaOfEffect
{
    public partial class ModEntry
    {
       
        [HarmonyPatch(typeof(Game1), nameof(Game1.pressUseToolButton))]
        private static class Game1_pressUseToolButton_Patch
        {
            private static bool Prefix(ref bool __result)
            {
                if (!Config.ModEnabled || Game1.player.CurrentTool is not Tool t || !TryGetTool(t, out var tdata))
                    return true;

                if (!TryGetEffect(t, out _))
                {
                    if (Config.ForceRecast)
                    {
                        Game1.activeClickableMenu = new CastSpellMenu(t, tdata.Spells);
                    }
                    else
                    {
                        Game1.showRedMessage(string.Format(SHelper.Translation.Get("x-no-spell"), t.DisplayName));
                    }
                    __result = true;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Tool), nameof(Tool.draw))]
        public class Tool_draw_Patch
        {
            public static bool Prefix(Tool __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || __instance.lastUser is not Farmer f || !TryGetTool(__instance, out var tdata))
                {
                    return true;
                }
                if(f.toolPower.Value > 0)
                {
                    int size = 60 * f.toolPower.Value;
                    Point center = f.GetBoundingBox().Center;
                    b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Rectangle(center - new Point(size, size), new Point(size * 2, size * 2))), new Rectangle(88, 1779, 30, 30), tdata.AuraColor * 0.25f, 0, Vector2.Zero, SpriteEffects.None, 1f);
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(Tool), nameof(Tool.DoFunction))]
        public class Tool_DoFunction_Patch
        {
            public static void Postfix(Tool __instance)
            {
                if (!Config.ModEnabled || __instance.lastUser is not Farmer f || !TryGetTool(__instance, out var tdata))
                {
                    return;
                }
                if (!TryGetEffect(__instance, out var data))
                {
                    if (Config.AutoOpenUI)
                    {
                        Game1.activeClickableMenu = new CastSpellMenu(__instance, tdata.Spells, GetTargetTile(f, data, tdata.MaxDistance));
                    }
                    else
                    {
                        Game1.showRedMessage(string.Format(SHelper.Translation.Get("x-no-spell"), __instance.DisplayName));
                    }
                    return;
                }
                var tile = GetTargetTile(f, data, tdata.MaxDistance);
                CastSpell(__instance, f, tdata, data, f.toolPower.Value, tile);
            }
        }

        [HarmonyPatch(typeof(Tool), nameof(Tool.doesShowTileLocationMarker))]
        public class Tool_doesShowTileLocationMarker_Patch
        {
            public static bool Prefix(Tool __instance)
            {
                if (!Config.ModEnabled || !TryGetEffect(__instance, out _))
                {
                    return true;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Tool), nameof(Tool.canBeDropped))]
        public class Tool_canBeDropped_Patch
        {
            public static bool Prefix(Tool __instance, ref bool __result)
            {
                if (!Config.ModEnabled || !ToolDict.ContainsKey(__instance.ItemId))
                {
                    return true;
                }
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(Item), nameof(Item.canBeTrashed))]
        public class Item_canBeTrashed_Patch
        {
            public static bool Prefix(Item __instance, ref bool __result)
            {
                if (!Config.ModEnabled || __instance is not Tool || !ToolDict.ContainsKey(__instance.ItemId))
                {
                    return true;
                }
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.GetTexture))]
        public class Projectile_GetTexture_Patch
        {
            public static bool Prefix(Projectile __instance, ref Texture2D __result)
            {
                if (!Config.ModEnabled || __instance is not SpellProjectile sp || sp.Texture is null)
                {
                    return true;
                }
                __result = SHelper.GameContent.Load<Texture2D>(sp.Texture);
                return false;
            }
        }
        [HarmonyPatch(typeof(Projectile), nameof(Projectile.GetSourceRect))]
        public class Projectile_GetSourceRect_Patch
        {
            public static bool Prefix(Projectile __instance, ref Rectangle __result)
            {
                if (!Config.ModEnabled || __instance is not SpellProjectile sp || sp.SourceRect is null)
                {
                    return true;
                }
                __result = sp.SourceRect.Value;
                return false;
            }
        }

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw), new Type[] { typeof(SpriteBatch) })]
        public class Farmer_draw_Patch
        {
            public static void Postfix(Farmer __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || Game1.activeClickableMenu != null || Game1.eventUp || !__instance.IsLocalPlayer || __instance.CurrentTool is not Tool tool || !TryGetTool(tool, out var tdata) || !TryGetEffect(tool, out var data))
                {
                    return;
                }
                Vector2 mouse_position = Utility.PointToVector2(Game1.getMousePosition()) + new Vector2((float)Game1.viewport.X, (float)Game1.viewport.Y);
                Vector2 draw_location = Game1.GlobalToLocal(Game1.viewport, GetTargetTile(__instance, data, tdata.MaxDistance) * 64);
                b.Draw(Game1.mouseCursors, draw_location, new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 29, -1, -1)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, draw_location.Y / 10000f);
            }
        }

        [HarmonyPatch(typeof(Tool), nameof(Tool.drawInMenu), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) })]
        public static class Tool_drawInMenu_Patch
        {
            public static void Postfix(Tool __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
            {
                if (!Config.ModEnabled || !TryGetTool(__instance, out var data) || data.MaxCharges <= 0)
                {
                    return;
                }
                var charges = GetCurrentCharges(__instance, data.MaxCharges);
                var fraction = charges / (float)data.MaxCharges;
                int height = 52;
                int offset = (int)Math.Round(height - fraction * height);
                int fheight = height - offset;
                int width = 4;
                int x = (int)location.X + 54;
                int y = (int)location.Y + 6;
                //spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(4f, 44f), new Rectangle?(new Rectangle(297, 420, 14, 5)), Color.White * 0.5f * transparency, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth + 0.0001f);
                spriteBatch.Draw(Game1.staminaRect, new Rectangle(x - 2, y - 2, width + 4, height + 4), Color.Black * transparency);
                if (charges > 0)
                {
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle(x, y + offset, width, fheight), data.ChargesColor * transparency);
                }
            }
        }

        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) })]
        public static class InventoryMenu_draw_Patch
        {
            public static void Prefix(InventoryMenu __instance)
            {
                if (!Config.ModEnabled || Game1.player.CursorSlotItem is not Object held || !held.HasTypeObject() || !Config.RechargeButton.JustPressed())
                {
                    return;
                }
                var mousePos = Game1.getMousePosition(true);
                for (int i = 0; i < __instance.inventory.Count; i++)
                {
                    var cc = __instance.inventory[i];
                    if (cc.containsPoint(mousePos.X, mousePos.Y))
                    {
                        if (__instance.actualInventory.Count > i && __instance.actualInventory[i] is Tool tool && TryGetTool(tool, out var data) && data.RechargeItem == held.ItemId)
                        {
                            var current = GetCurrentCharges(tool, data.MaxCharges);
                            if (current >= data.MaxCharges)
                                return;
                            int increase = Math.Min(data.MaxCharges - current, held.Stack * data.RechargeAmount);
                            held.Stack -= (int)Math.Ceiling(increase / (float)data.RechargeAmount);
                            SetCurrentCharges(tool, current + increase);
                            if(!string.IsNullOrEmpty(data.RechargeSound))
                                Game1.playSound(data.RechargeSound);
                        }
                        return;
                    }
                }

            }
        }
        //[HarmonyPatch(typeof(Building), nameof(Building.draw), new Type[] { typeof(SpriteBatch) })]
        //public class Building_draw_Patch
        //{
        //    public static void Postfix(Building __instance)
        //    {
        //        if (__instance.GetParentLocation() is not GameLocation l)
        //            return;
        //        if (!__instance.modData.TryGetValue(cropKey, out var cropType))
        //            return;
        //        if (!Config.ModEnabled)
        //        {
        //            l.buildings.Remove(__instance);
        //            return;
        //        }
        //        if (__instance.GetParentLocation().resourceClumps.FirstOrDefault(rc => rc is GiantCrop g && g.Id == cropType && g.Tile.X == __instance.tileX.Value && g.Tile.Y == __instance.tileY.Value) == null)
        //        {
        //            ToRemove.Add(__instance);
        //            SHelper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(GiantCrop), nameof(GiantCrop.performToolAction))]
        //public class GiantCrop_performToolAction_Patch
        //{
        //    public static bool Prefix(GiantCrop __instance)
        //    {
        //        if (!TutorialDict.ContainsKey(__instance.Id))
        //            return true;
        //        var x = __instance.Location.buildings.FirstOrDefault(i => (int)__instance.Tile.X == i.tileX.Value && (int)__instance.Tile.Y == i.tileY.Value);
        //        if (__instance.Location.buildings.FirstOrDefault(i => (int)__instance.Tile.X == i.tileX.Value && (int)__instance.Tile.Y == i.tileY.Value) is Building exists && exists.buildingType.Value == modPrefix + __instance.Id && exists.GetIndoors() is GameLocation l && (l.farmers.Any() || l.characters.Any() || l.animals.Any() || ((l.furniture.Any() || l.objects.Values.Where(o => o.Fragility != 2).Any()) && Config.ProtectObjects)))
        //        {
        //            Game1.showRedMessage(SHelper.Translation.Get("not-empty"));
        //            return false;
        //        }
        //        return true;
        //    }
        //}

        //[HarmonyPatch(typeof(DecoratableLocation), "IsFloorableTile")]
        //public class DecoratableLocation_IsFloorableTile_Patch
        //{
        //    public static void Postfix(DecoratableLocation __instance, int x, int y, ref bool __result)
        //    {
        //        if (!__result)
        //            return;

        //        string floor_id = __instance.GetFloorID(x, y);
        //        __result = floor_id != null;
        //    }
        //}
    }
}