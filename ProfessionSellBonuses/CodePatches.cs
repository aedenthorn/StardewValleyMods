using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfessionSellBonuses
{
    public partial class ModEntry
    {

        //[HarmonyPatch(typeof(NPC), nameof(NPC.withinPlayerThreshold), new Type[] { typeof(int) })]
        //public static class NPC_withinPlayerThreshold_Patch
        //{
        //    public static void Prefix(NPC __instance, ref Dictionary<Farmer, Vector2> __state)
        //    {
        //        if (!Config.ModEnabled || __instance is not GreenSlime slime || __instance.currentLocation != null && !__instance.currentLocation.farmers.Any())
        //        {
        //            return;
        //        }
        //        int which = 0;
        //        if(slime.color.Value == new Color(255, 255, 50))
        //        {
        //            which = 6;
        //        }
        //        else if(slime.Name == "Frost Jelly")
        //        {
        //            which = 1;
        //        }
        //        else if (slime.Name == "Tiger Slime")
        //        {
        //            which = 7;
        //        }
        //        else if (slime.Name == "Prismatic Slime")
        //        {
        //            which = 8;
        //        }
        //        else if(slime.Name == "Sludge")
        //        {
        //            if(Math.Abs(slime.color.R - Color.BlueViolet.R) < 21 && Math.Abs(slime.color.G - Color.BlueViolet.G) < 21 && Math.Abs(slime.color.B - Color.BlueViolet.B) < 21)
        //            {
        //                which = 3;
        //            }
        //            else if(slime.color.R > 199 || slime.color.Value == new Color(50, 10, 50) * 0.7f)
        //            {
        //                which = 2;
        //            }
        //        }
        //        else if(slime.color.Value.R > 119 && slime.color.Value.R == slime.color.Value.G && slime.color.Value.B == slime.color.Value.G)
        //        {
        //            which = 5;
        //        }
        //        else if(slime.color.Value.R > 119 && slime.color.Value.G == slime.color.Value.R / 2 && slime.color.Value.B == slime.color.Value.R / 4)
        //        {
        //            which = 4;
        //        }

        //        __state = new();

        //        foreach (var f in __instance.currentLocation.farmers)
        //        {
        //            if(!Attracts(f.leftRing.Value, which))
        //            {
        //                __state[f] = f.Position;
        //                f.Position = Vector2.One * float.MaxValue;
        //                continue;
        //            }
        //            if (!Attracts(f.rightRing.Value, which))
        //            {
        //                __state[f] = f.Position;
        //                f.Position = Vector2.One * float.MaxValue;
        //            }
        //        }
        //    }
        //    public static void Postfix(Dictionary<Farmer, Vector2> __state)
        //    {
        //        if (__state?.Any() != true)
        //            return;
        //        foreach(var f in __state.Keys.ToArray())
        //        {
        //            f.Position = __state[f];
        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(Ring), nameof(Ring.drawInMenu), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) })]
        //public static class Ring_drawInMenu_Patch
        //{
        //    public static void Postfix(Ring __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        //    {
        //        if (!Config.ModEnabled || __instance.ItemId != "520")
        //            return;
        //        int i = GetWhich(__instance);
        //        if (i == 0)
        //            return;
        //        spriteBatch.Draw(SHelper.GameContent.Load<Texture2D>(ringPath), location + new Vector2(32f, 32f) * scaleSize, new Rectangle(i * 16, 0, 16, 16), (i == 8 ? Utility.GetPrismaticColor(0, 2) : color) * transparency, 0f, new Vector2(8f, 8f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
        //    }
        //}
        //[HarmonyPatch(typeof(Ring), "loadDisplayFields")]
        //public static class Ring_loadDisplayFields_Patch
        //{
        //    public static void Postfix(Ring __instance)
        //    {
        //        var e = GetEffect(__instance);
        //        if(e != "")
        //        {
        //            __instance.description += $" ({SHelper.Translation.Get(e)})";
        //        }
        //    }
        //}

        //public static int oldScrollWheelValue;

        //[HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.draw), new Type[] { typeof(SpriteBatch) })]
        //public static class InventoryMenu_draw_Patch
        //{
        //    public static void Prefix(InventoryPage __instance)
        //    {
        //        if (!Config.ModEnabled)
        //        {
        //            return;
        //        }
        //        bool pressed = Config.EffectKey.JustPressed();
        //        int scrolled = Game1.input.GetMouseState().ScrollWheelValue - oldScrollWheelValue;
        //        oldScrollWheelValue = Game1.input.GetMouseState().ScrollWheelValue;
        //        if (!pressed && scrolled == 0)
        //            return;

        //        var mpos = Game1.getMousePosition(true);
        //        foreach (ClickableComponent c in __instance.equipmentIcons)
        //        {
        //            if (c.containsPoint(mpos.X, mpos.Y))
        //            {
        //                Ring ring;
        //                switch(c.name)
        //                {
        //                    case "Left Ring":
        //                        ring = Game1.player.leftRing.Value;
        //                        break;
        //                    case "Right Ring":
        //                        ring = Game1.player.rightRing.Value;
        //                        break;
        //                    default:
        //                        return;
        //                }
        //                if (ring?.ItemId != "520")
        //                    return;
        //                if (pressed)
        //                {
        //                    string e = GetEffect(ring);
        //                    e = e switch
        //                    {
        //                        "Attract" => "Repulse",
        //                        "Repulse" => "",
        //                        _ => "Attract"
        //                    };
        //                    ring.modData[effectKey] = e;
        //                    ring.description = null;
        //                    Game1.playSound("bigSelect");
        //                }
        //                else
        //                {
        //                    int sign = Math.Sign(scrolled);
        //                    int i = GetWhich(ring);
        //                    i += sign;
        //                    if (i < 0)
        //                        i = 8;
        //                    else if (i > 8)
        //                        i = 0;
        //                    ring.modData[whichKey] = i.ToString();
        //                    Game1.playSound("grassyStep");
        //                }
        //            }
        //        }
        //    }
        //}
    }
}