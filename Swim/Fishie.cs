using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;

namespace Swim
{
    public partial class ModEntry
    {
        public static List<string> fishTextures = new List<string>()
        {
            "BlueFish",
            "PinkFish",
            "GreenFish",
        };

        [HarmonyPatch(typeof(Serpent), nameof(Serpent.drawAboveAllLayers))]
        public static class Serpent_drawAboveAllLayers_Patch
        {
            public static bool Prefix(Serpent __instance, SpriteBatch b)
            {
                if (!IsMonster(__instance))
                    return true;
                __instance.invincibleCountdown = 1000;
                if (Utility.isOnScreen(__instance.Position, 128))
                {
                    b.Draw(Game1.shadowTexture, __instance.getLocalPosition(Game1.viewport) + new Vector2(64f, (float)__instance.GetBoundingBox().Height), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, (float)(__instance.StandingPixel.Y - 1) / 10000f);
                    b.Draw(__instance.Sprite.Texture, __instance.getLocalPosition(Game1.viewport) + new Vector2(64f, (float)(__instance.GetBoundingBox().Height / 2)), __instance.Sprite.SourceRect, Color.White, __instance.rotation, new Vector2(16f, 16f), Math.Max(0.2f, __instance.Scale) * 4f, __instance.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, __instance.drawOnTop ? 0.991f : ((float)(__instance.StandingPixel.Y + 8) / 10000f)));
                    if (__instance.isGlowing)
                    {
                        b.Draw(__instance.Sprite.Texture, __instance.getLocalPosition(Game1.viewport) + new Vector2(64f, (float)(__instance.GetBoundingBox().Height / 2)), __instance.Sprite.SourceRect, __instance.glowingColor * __instance.glowingTransparency, __instance.rotation, new Vector2(16f, 16f), Math.Max(0.2f, __instance.Scale) * 4f, __instance.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, __instance.drawOnTop ? 0.991f : ((float)(__instance.StandingPixel.Y + 8) / 10000f + 0.0001f)));
                    }
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(Serpent), "updateAnimation")]
        public static class Serpent_updateAnimation_Patch
        {
            public static void Postfix(Serpent __instance, GameTime time)
            {
                if (!IsMonster(__instance))
                    return;
                if (__instance.wasHitCounter >= 0)
                {
                    __instance.wasHitCounter -= time.ElapsedGameTime.Milliseconds;
                }
                __instance.Sprite.Animate(time, 0, 9, 40f);
                float xSlope = (float)(-(float)(__instance.Player.GetBoundingBox().Center.X - __instance.GetBoundingBox().Center.X));
                float ySlope = (float)(__instance.Player.GetBoundingBox().Center.Y - __instance.GetBoundingBox().Center.Y);
                float t = Math.Max(1f, Math.Abs(xSlope) + Math.Abs(ySlope));
                if (t < 64f)
                {
                    __instance.xVelocity = Math.Max(-7f, Math.Min(7f, __instance.xVelocity * 1.1f));
                    __instance.yVelocity = Math.Max(-7f, Math.Min(7f, __instance.yVelocity * 1.1f));
                }
                xSlope /= t;
                ySlope /= t;
                if (__instance.wasHitCounter <= 0)
                {
                    __instance.targetRotation = (float)Math.Atan2((double)(-(double)ySlope), (double)xSlope) - 1.57079637f;
                    if ((double)(Math.Abs(__instance.targetRotation) - Math.Abs(__instance.rotation)) > 2.748893571891069 && Game1.random.NextDouble() < 0.5)
                    {
                        __instance.turningRight = true;
                    }
                    else if ((double)(Math.Abs(__instance.targetRotation) - Math.Abs(__instance.rotation)) < 0.39269908169872414)
                    {
                        __instance.turningRight = false;
                    }
                    if (__instance.turningRight)
                    {
                        __instance.rotation -= (float)Math.Sign(__instance.targetRotation - __instance.rotation) * 0.0490873866f;
                    }
                    else
                    {
                        __instance.rotation += (float)Math.Sign(__instance.targetRotation - __instance.rotation) * 0.0490873866f;
                    }
                    __instance.rotation %= 6.28318548f;
                    __instance.wasHitCounter = 5 + Game1.random.Next(-1, 2);
                }
                float maxAccel = Math.Min(7f, Math.Max(2f, 7f - t / 64f / 2f));
                xSlope = (float)Math.Cos((double)__instance.rotation + 1.5707963267948966);
                ySlope = -(float)Math.Sin((double)__instance.rotation + 1.5707963267948966);
                __instance.xVelocity += -xSlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
                __instance.yVelocity += -ySlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
                if (Math.Abs(__instance.xVelocity) > Math.Abs(-xSlope * 7f))
                {
                    __instance.xVelocity -= -xSlope * maxAccel / 6f;
                }
                if (Math.Abs(__instance.yVelocity) > Math.Abs(-ySlope * 7f))
                {
                    __instance.yVelocity -= -ySlope * maxAccel / 6f;
                }
                AccessTools.Method(typeof(Monster), "resetAnimationSpeed").Invoke(__instance, Array.Empty<object>());
            }
        }
    }
}
