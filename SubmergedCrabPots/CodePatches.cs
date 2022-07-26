using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System;

namespace SubmergedCrabPots
{
    public partial class ModEntry
    {
        public static bool drawingConnectedPot;
        public static int potIndex;
        public static int topIndex;
        public static int leftIndex;
        public static int rightIndex;

        [HarmonyPatch(typeof(CrabPot), nameof(CrabPot.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class CrabPot_draw_Patch
        {
            public static bool Prefix(CrabPot __instance, SpriteBatch spriteBatch, int x, int y, float alpha, ref float ___yBob, Vector2 ___shake)
            {
                if (!Config.EnableMod || (__instance.heldObject.Value == null && !__instance.readyForHarvest.Value && __instance.bait.Value == null) || (__instance.readyForHarvest.Value && __instance.heldObject.Value is not null && !Config.SubmergeHarvestable))
                    return true;
                ___yBob = (float)(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 500.0 + x * 64) * 8.0 + 8.0);
                int tileIndexToShow = (int)((___yBob - 1) / 5);
                if (__instance.heldObject.Value is not null || __instance.readyForHarvest.Value)
                {
                    tileIndexToShow = 2;
                    ___yBob /= 2;
                }
                else if (Config.ShowRipples && ___yBob <= 0.001f)
                {
                    Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 150f, 8, 0, __instance.directionOffset.Value + new Vector2(x * 64 + 9, y * 64 + 24), false, Game1.random.NextDouble() < 0.5, 0.001f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f, false));
                }
                spriteBatch.Draw(bobberTexture, Game1.GlobalToLocal(Game1.viewport, __instance.directionOffset.Value + new Vector2(x * 64, y * 64 + (int)___yBob)) + new Vector2(32, 32) * (1 - Config.BobberScale / 4f ) + ___shake, new Rectangle?(Game1.getSourceRectForStandardTileSheet(bobberTexture, tileIndexToShow, 16, 16)), Config.BobberTint * (Config.BobberOpacity / 100f), 0f, Vector2.Zero, Config.BobberScale, SpriteEffects.None, (y * 64 + __instance.directionOffset.Y + x % 4) / 10000f);

                return false;
            }
        }
        [HarmonyPatch(typeof(CrabPot), nameof(CrabPot.performObjectDropInAction))]
        public class CrabPot_performObjectDropInAction_Patch
        {
            public static void Postfix(CrabPot __instance, bool __result, Farmer who, bool probe)
            {
                if (!Config.EnableMod || probe || !__result)
                    return;
                who.currentLocation.playSound("dropItemInWater");
            }
        }
    }
}