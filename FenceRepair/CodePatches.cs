using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FenceRepair
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Fence), nameof(Fence.draw), new Type[] { typeof(SpriteBatch),typeof(int),typeof(int),typeof(float) })]
        public static class Fence_draw_Patch
        {
            public static void Postfix(Fence __instance, SpriteBatch b, int x, int y, float alpha)
            {
                if (!Config.ModEnabled || __instance.isGate.Value || !Context.IsPlayerFree || !IsShowing())
                    return;
                //if(Config.Debug && __instance.maxHealth.Value > 100)
                //    __instance.health.Value = __instance.maxHealth.Value * 0.5f;
                //if (Config.Debug && __instance.maxHealth.Value > 200)
                //    __instance.health.Value = __instance.maxHealth.Value * 0.2f;
                var fraction = __instance.health.Value / __instance.maxHealth.Value;
                var color = fraction > 0.66 ? Config.ColorHigh : (fraction > 0.33 ? Config.ColorMid : Config.ColorLow);
                var rect = Game1.GlobalToLocal(Game1.viewport, new Rectangle(new Point(x * 64, y * 64) + new Point(16, -40), new Point(32, 8)));
                var rect2 = new Rectangle(rect.Location, new Point((int)Math.Floor(rect.Width * fraction), 8));
                rect.Inflate(4, 4);
                var layer = (y * 64 + 32 + 2) / 10000f;
                var layer2 = (y * 64 + 32 + 3) / 10000f;
                b.Draw(Game1.staminaRect, rect, null, Color.Black * alpha, 0, Vector2.Zero, SpriteEffects.None, layer);
                b.Draw(Game1.staminaRect, rect2, null, color * alpha, 0, Vector2.Zero, SpriteEffects.None, layer2);
                if (Config.ShowNumber)
                {
                    var str = Math.Round(__instance.health.Value, __instance.health.Value >=1 ? 0 : 1).ToString();
                    var layer3 = (y * 64 + 32 + 4) / 10000f;
                    var m = Game1.tinyFont.MeasureString(str);
                    var pos = Game1.GlobalToLocal(new Vector2(x, y) * 64 + new Vector2(32 - m.X / 2, -40 - m.Y));
                    b.DrawString(Game1.tinyFont, str, pos, Color.White * alpha, 0, Vector2.Zero, 1f, SpriteEffects.None, layer3);
                }
            }
        }


        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public static class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || who is null)
                    return true;
                Vector2 tilePos = new Vector2(tileLocation.X, tileLocation.Y);
                if (!__instance.objects.TryGetValue(tilePos, out var obj) || obj is not Fence fence)
                    return true;
                if (!TryRepairFence(__instance, fence, who, false))
                    return true;
                __result = true;
                return false;
            }

        }

        [HarmonyPatch(typeof(Fence), nameof(Fence.performObjectDropInAction))]
        public static class Fence_performObjectDropInAction_Patch
        {
            public static void Postfix(Fence __instance, Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed, ref bool __result)
            {
                if (!Config.ModEnabled || __result || !probe || __instance.Location is not GameLocation location || __instance.repairQueued.Value)
                    return;
                __result = TryRepairFence(__instance.Location, __instance, who, true);
            }
        }
    }
}