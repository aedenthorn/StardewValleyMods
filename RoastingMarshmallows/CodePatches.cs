using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using System;

namespace RoastingMarshmallows
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw), [typeof(SpriteBatch)])]
        public static class Farmer_draw_Patch
        {
            public static void Postfix(Farmer __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || Game1.eventUp)
                    return;
                int roastProgress = GetRoastProgress(__instance, false, false);
                if (roastProgress < 0)
                    return;
                if(roastProgress >= Config.RoastFrames)
                {
                    __instance.modData.Remove(modKey);
                    return;
                }
                var flip = (__instance.flip || (__instance.Sprite.CurrentAnimation != null && __instance.Sprite.CurrentAnimation[__instance.Sprite.currentAnimationIndex].flip));
                var texture = SHelper.GameContent.Load<Texture2D>(texturePath);
                var frameHeight = texture.Height / 2;
                Vector2 offset;
                Vector2 smokeOffset = Vector2.Zero;
                float layerDepth = 1;
                switch (__instance.FacingDirection)
                {
                    case 0:
                        offset = new Vector2(20 + frameHeight / 2, -48);
                        smokeOffset = new Vector2(0, -32);
                        layerDepth = -1;
                        break;
                    case 1:
                        offset = new Vector2(64 + frameHeight, -8 - frameHeight);
                        smokeOffset = new Vector2(frameHeight  * 3 / 4, -frameHeight * 1.5f);
                        break;
                    case 2:
                        offset = new Vector2(38, 32);
                        smokeOffset = new Vector2(-frameHeight / 2, 0);
                        break;
                    default:
                        layerDepth = 64;
                        offset = new Vector2(-frameHeight, -8 - frameHeight);
                        smokeOffset = new Vector2(-frameHeight * 1.5f, -frameHeight * 1.5f);
                        break;
                }
                var position = __instance.getLocalPosition(Game1.viewport) + offset;
                var yFrame = __instance.FacingDirection % 2 == 0 ? 16 : 0;
                float rotation = __instance.FacingDirection == 2 ? (float)Math.PI : 0;
                b.Draw(texture, position, new(0, yFrame,frameHeight, frameHeight), Color.White, rotation, new Vector2(8,8), 4, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, (__instance.StandingPixel.Y + layerDepth) / 10000f));
                int intervalLength = Config.RoastFrames / (texture.Width / frameHeight);
                int frame = roastProgress / intervalLength;

                if (frame > 0)
                {
                    Rectangle sourceRect = new(frame * frameHeight, yFrame, frameHeight, frameHeight);
                    b.Draw(texture, position, sourceRect, Color.White, rotation, new Vector2(8, 8), 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, (__instance.StandingPixel.Y + 65) / 10000f));
                    int frames = (texture.Width / frameHeight) - 2;
                    if (frame > 1 && roastProgress % (int)(50f * frames / frame) == 0)
                    {
                        TemporaryAnimatedSprite sprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), __instance.Position + offset + smokeOffset, false, 0.002f, Color.Gray);
                        sprite.alpha = 0.5f * frame / frames;
                        sprite.motion = new Vector2(0f, -0.5f);
                        sprite.acceleration = new Vector2(0.002f, 0f);
                        sprite.interval = 99999f;
                        sprite.layerDepth = 1f;
                        sprite.scale = 1;
                        sprite.scaleChange = 0.02f;
                        sprite.rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f;
                        __instance.currentLocation.temporarySprites.Add(sprite);
                    }
                }
            }
        }
    }
}
