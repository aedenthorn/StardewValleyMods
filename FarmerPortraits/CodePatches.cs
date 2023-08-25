using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace FarmerPortraits
{
    public partial class ModEntry
    {

        private static void AdjustWindow(ref DialogueBox __instance)
        {
            __instance.x = Math.Max(520, (int)Utility.getTopLeftPositionForCenteringOnScreen(__instance.width, __instance.height, 0, 0).X + 260);
            __instance.width = (int)Math.Min(Game1.uiViewport.Width - __instance.x - 48, 1200);
            __instance.friendshipJewel = new Rectangle(__instance.x + __instance.width - 64, __instance.y + 256, 44, 44);
        }

        [HarmonyPatch(typeof(DialogueBox), new Type[] { typeof(Dialogue) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class DialogueBox_Patch
        {
            public static void Postfix(DialogueBox __instance)
            {
                if (!Config.EnableMod || !__instance.transitionInitialized || __instance.transitioning || (!Config.ShowWithQuestions && __instance.isQuestion) || (!Config.ShowWithNPCPortrait && __instance.isPortraitBox()) || (!Config.ShowWithEvents && Game1.eventUp) || (!Config.ShowMisc && !__instance.isQuestion && !__instance.isPortraitBox()))
                    return;
                AdjustWindow(ref __instance);
            }
        }

        
        [HarmonyPatch(typeof(DialogueBox), "setUpIcons")]
        public class DialogueBox_setUpIcons_Patch
        {
            public static void Prefix(DialogueBox __instance)
            {
                if (!Config.EnableMod || !__instance.transitionInitialized || __instance.transitioning || (!Config.ShowWithQuestions && __instance.isQuestion) || (!Config.ShowWithNPCPortrait && __instance.isPortraitBox()) || (!Config.ShowWithEvents && Game1.eventUp) || (!Config.ShowMisc && !__instance.isQuestion && !__instance.isPortraitBox()))
                    return;
                AdjustWindow(ref __instance);
            }
        }

        [HarmonyPatch(typeof(DialogueBox), nameof(DialogueBox.drawBox))]
        public class DialogueBox_drawBox_Patch
        {
            public static void Postfix(DialogueBox __instance, SpriteBatch b)
            {
                if (!Config.EnableMod || !__instance.transitionInitialized ||  __instance.transitioning || (!Config.ShowWithQuestions && __instance.isQuestion) ||  (!Config.ShowWithNPCPortrait && __instance.isPortraitBox()) || (!Config.ShowWithEvents && Game1.eventUp) || (!Config.ShowMisc && !__instance.isQuestion && !__instance.isPortraitBox()))
                    return;
                int boxHeight = 384;
                drawBox(b, __instance.x - 448 - 32, __instance.y + __instance.height - boxHeight, 448, boxHeight);
            }

            private static void drawBox(SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight)
            {
                b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 20, boxWidth, 24), new Rectangle?(new Rectangle(275, 313, 1, 6)), Color.White);
                b.Draw(Game1.mouseCursors, new Rectangle(xPos + 12, yPos + boxHeight, boxWidth - 20, 32), new Rectangle?(new Rectangle(275, 328, 1, 8)), Color.White);
                b.Draw(Game1.mouseCursors, new Rectangle(xPos - 32, yPos + 24, 32, boxHeight - 28), new Rectangle?(new Rectangle(264, 325, 8, 1)), Color.White);
                b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos - 28), new Rectangle?(new Rectangle(261, 311, 14, 13)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
                b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos + boxHeight - 4), new Rectangle?(new Rectangle(261, 327, 14, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
                b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 28, boxHeight), new Rectangle?(new Rectangle(293, 324, 7, 1)), Color.White);
                b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos - 28), new Rectangle?(new Rectangle(291, 311, 12, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
                b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos + boxHeight - 8), new Rectangle?(new Rectangle(291, 326, 12, 12)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);

                if (backgroundTexture != null && Config.UseCustomBackground)
                    b.Draw(backgroundTexture, new Rectangle(xPos - 4, yPos, boxWidth + 12, boxHeight + 4), null, Color.White);
                else
                    b.Draw(Game1.mouseCursors, new Vector2(xPos - 4, yPos), new Rectangle?(new Rectangle(583, 411, 115, 97)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f); // background

                int portraitBoxX = xPos + 76;
                int portraitBoxY = yPos + boxHeight / 2 - 148 - 36;
                int frame = Config.FacingFront ? 0 : 6;
                if (portraitTexture != null && Config.UseCustomPortrait)
                {
                    b.Draw(portraitTexture, new Rectangle(portraitBoxX + 20, portraitBoxY + 24, 256, 256), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.88f);
                }
                else
                {
                    FarmerRenderer.isDrawingForUI = true;
                    drawFarmer(b, frame, new Rectangle((frame % 6) * 16, Game1.player.bathingClothes.Value ? 576 : frame / 6 * 32, 16, 16), new Vector2(xPos + boxWidth / 2 - 128, yPos + boxHeight / 2 - 208), Color.White);
                    if (Game1.timeOfDay >= 1900)
                    {
                        drawFarmer(b, frame, new Rectangle((frame % 6) * 16, Game1.player.bathingClothes.Value ? 576 : frame / 6 * 32, 16, 16), new Vector2(xPos + boxWidth / 2 - 128, yPos + boxHeight / 2 - 192), Color.DarkBlue * 0.3f);
                    }
                    FarmerRenderer.isDrawingForUI = false;
                }
                SpriteText.drawStringHorizontallyCenteredAt(b, Game1.player.Name, xPos + boxWidth / 2, portraitBoxY + 296 + 16, 999999, -1, 999999, 1f, 0.88f, false, -1, 99999);

            }

            private static void drawFarmer(SpriteBatch b, int currentFrame, Rectangle sourceRect, Vector2 position, Color overrideColor)
            {
				var animationFrame = new FarmerSprite.AnimationFrame(Game1.player.bathingClothes.Value ? 108 : currentFrame, 0, false, false, null, false);
				var who = Game1.player;
                float layerDepth = 0.8f;
                float scale = 4f;

				AccessTools.Method(typeof(FarmerRenderer), "executeRecolorActions").Invoke(Game1.player.FarmerRenderer, new object[] { who });

				position = new Vector2((float)Math.Floor(position.X), (float)Math.Floor(position.Y));

                var positionOffset = new Vector2(animationFrame.positionOffset * 4, animationFrame.positionOffset * 4);

                var baseTexture = AccessTools.FieldRefAccess<FarmerRenderer, Texture2D>(Game1.player.FarmerRenderer, "baseTexture");

                // body

				b.Draw(baseTexture, position + positionOffset, new Rectangle?(sourceRect), overrideColor, 0, Vector2.Zero, 16, SpriteEffects.None, 0.8f);
				
                // eyes
                
                sourceRect.Offset(288, 0);
				if (who.currentEyes != 0 && (Game1.timeOfDay < 2600 || (who.isInBed.Value && who.timeWentToBed.Value != 0)) && ((!who.FarmerSprite.PauseForSingleAnimation && !who.UsingTool) || (who.UsingTool && who.CurrentTool is FishingRod)))
				{
					if (!who.UsingTool || who.CurrentTool is not FishingRod || (who.CurrentTool as FishingRod).isFishing)
					{
                        int x_adjustment = 5 - FarmerRenderer.featureXOffsetPerFrame[currentFrame];
                        if (!Config.FacingFront)
                        {
                            x_adjustment += 3;
                        }
                        x_adjustment *= 4;
                        b.Draw(baseTexture, position + positionOffset + new Vector2(x_adjustment, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && !Config.FacingFront) ? 36 : 40)) * scale, new Rectangle?(new Rectangle(5, 16, Config.FacingFront ? 6 : 2, 2)), overrideColor, 0f, Vector2.Zero, 16, SpriteEffects.None, 0.8f + 5E-08f);
                        b.Draw(baseTexture, position + positionOffset + new Vector2(x_adjustment, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (!Config.FacingFront ? 40 : 44)) * scale, new Rectangle?(new Rectangle(264 + 0, 2 + (who.currentEyes - 1) * 2, Config.FacingFront ? 6 : 2, 2)), overrideColor, 0f, Vector2.Zero, 16, SpriteEffects.None, 0.8f + 1.2E-07f);
                    }
                }

                // hair and accessories

                int hair_style = who.getHair(false);
                HairStyleMetadata hair_metadata = Farmer.GetHairStyleMetadata(who.hair.Value);
                if (who != null && who.hat.Value != null && who.hat.Value.hairDrawType.Value == 1 && hair_metadata != null && hair_metadata.coveredIndex != -1)
                {
                    hair_style = hair_metadata.coveredIndex;
                    hair_metadata = Farmer.GetHairStyleMetadata(hair_style);
                }
                AccessTools.Method(typeof(FarmerRenderer), "executeRecolorActions").Invoke(Game1.player.FarmerRenderer, new object[] { who });

                int hatCutoff = 4;
                int shirtCutoff = 4;
                var shirtSourceRect = new Rectangle(Game1.player.FarmerRenderer.ClampShirt(who.GetShirtIndex()) * 8 % 128, Game1.player.FarmerRenderer.ClampShirt(who.GetShirtIndex()) * 8 / 128 * 32, 8, 8 - shirtCutoff);
                Texture2D hair_texture = FarmerRenderer.hairStylesTexture;
                var hairstyleSourceRect = new Rectangle(hair_style * 16 % FarmerRenderer.hairStylesTexture.Width, hair_style * 16 / FarmerRenderer.hairStylesTexture.Width * 96, 16, 32);
                Rectangle hatSourceRect = who.hat.Value != null ? new Rectangle(20 * who.hat.Value.which.Value % FarmerRenderer.hatsTexture.Width, 20 * who.hat.Value.which.Value / FarmerRenderer.hatsTexture.Width * 20 * 4 + hatCutoff, 20, 20 - hatCutoff) : new Rectangle();
                var accessorySourceRect = who.accessory.Value >= 0 ? new Rectangle(who.accessory.Value * 16 % FarmerRenderer.accessoriesTexture.Width, who.accessory.Value * 16 / FarmerRenderer.accessoriesTexture.Width * 32, 16, 16) : new Rectangle();

                if (hair_metadata != null)
                {
                    hair_texture = hair_metadata.texture;
                    hairstyleSourceRect = new Rectangle(hair_metadata.tileX * 16, hair_metadata.tileY * 16, 16, 32);
                }
                Rectangle dyed_shirt_source_rect = shirtSourceRect;
                float dye_layer_offset = 1E-07f;
                float hair_draw_layer = 2.2E-05f;
                var heightOffset = 0;

                if (Config.FacingFront)
                {
                    dyed_shirt_source_rect = shirtSourceRect;
                    dyed_shirt_source_rect.Offset(128, 0);

                    // shirt

                    if (!who.bathingClothes.Value)
                    {
                        b.Draw(FarmerRenderer.shirtsTexture, position + positionOffset + new Vector2(16 + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (float)heightOffset * 4 - (who.IsMale ? 0 : 0)) * scale, new Rectangle?(shirtSourceRect), overrideColor.Equals(Color.White) ? Color.White : overrideColor, 0, Vector2.Zero, 16, SpriteEffects.None, 0.8f + 1.5E-07f);
                        b.Draw(FarmerRenderer.shirtsTexture, position + positionOffset + new Vector2(16 + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (float)heightOffset * 4 - (who.IsMale ? 0 : 0)), new Rectangle?(dyed_shirt_source_rect), overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, 0, Vector2.Zero, 16, SpriteEffects.None, 0.8f + 1.5E-07f + dye_layer_offset);
                    }

                    // accessory

                    if (who.accessory.Value >= 0)
                    {
                        b.Draw(FarmerRenderer.accessoriesTexture, position + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 8 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + heightOffset - 4), new Rectangle?(accessorySourceRect), (overrideColor.Equals(Color.White) && who.accessory.Value < 6) ? who.hairstyleColor.Value : overrideColor, 0, Vector2.Zero, 16, SpriteEffects.None, 0.8f + ((who.accessory.Value < 8) ? 1.9E-05f : 2.9E-05f));
                    }

                    // hair

                    b.Draw(hair_texture, position + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && who.hair.Value >= 16) ? -4 : ((!who.IsMale && who.hair.Value < 16) ? 4 : 0))) * scale, new Rectangle?(hairstyleSourceRect), overrideColor.Equals(Color.White) ? who.hairstyleColor.Value : overrideColor, 0, Vector2.Zero, 16, SpriteEffects.None, 0.8f + hair_draw_layer);
                }
                else
                {

                    shirtSourceRect.Offset(0, 8);
                    hairstyleSourceRect.Offset(0, 32);
                    dyed_shirt_source_rect = shirtSourceRect;
                    dyed_shirt_source_rect.Offset(128, 0);
                    if (who.accessory.Value >= 0)
                    {
                        accessorySourceRect.Offset(0, 16);
                    }
                    if (who.hat.Value != null)
                    {
                        hatSourceRect.Offset(0, 20);
                    }

                    // shirt

                    if (!who.bathingClothes.Value)
                    {
                        b.Draw(FarmerRenderer.shirtsTexture, position + positionOffset + new Vector2(16f + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 56f + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + heightOffset) * scale, new Rectangle?(shirtSourceRect), overrideColor.Equals(Color.White) ? Color.White : overrideColor, 0, Vector2.Zero, 4f * scale, SpriteEffects.None, layerDepth + 1.8E-07f);
                        b.Draw(FarmerRenderer.shirtsTexture, position + positionOffset + new Vector2(16f + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 56f + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + heightOffset) * scale, new Rectangle?(dyed_shirt_source_rect), overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, 0, Vector2.Zero, 4f * scale, SpriteEffects.None, layerDepth + 1.8E-07f + dye_layer_offset);
                    }

                    // accessory

                    if (who.accessory.Value >= 0)
                    {
                        b.Draw(FarmerRenderer.accessoriesTexture, position + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 4 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + heightOffset) * scale, new Rectangle?(accessorySourceRect), (overrideColor.Equals(Color.White) && who.accessory.Value < 6) ? who.hairstyleColor.Value : overrideColor, 0, Vector2.Zero, 4f * scale, SpriteEffects.None, layerDepth + ((who.accessory.Value < 8) ? 1.9E-05f : 2.9E-05f));
                    }

                    // hair

                    b.Draw(hair_texture, position + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && who.hair.Value >= 16) ? -4 : ((!who.IsMale && who.hair.Value < 16) ? 4 : 0))) * scale, new Rectangle?(hairstyleSourceRect), overrideColor.Equals(Color.White) ? who.hairstyleColor.Value : overrideColor, 0, Vector2.Zero, 16, SpriteEffects.None, layerDepth + hair_draw_layer);
                }

                // hat

                if (who.hat.Value != null && !who.bathingClothes.Value)
                {
                    float layer_offset = 3.9E-05f;
                    b.Draw(FarmerRenderer.hatsTexture, position + positionOffset * scale + new Vector2(-9 * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4 - 8, -16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (who.hat.Value.ignoreHairstyleOffset.Value ? 0 : FarmerRenderer.hairstyleHatOffset[who.hair.Value % 16]) + 8 + heightOffset + 4 * hatCutoff) * scale, new Rectangle?(hatSourceRect), who.hat.Value.isPrismatic.Value ? Utility.GetPrismaticColor(0, 1f) : Color.White, 0, Vector2.Zero, 16, SpriteEffects.None, 0.8f + layer_offset);
                }
                float arm_layer_offset = 4.9E-05f;
                sourceRect.Offset(-288 + (animationFrame.secondaryArm ? 192 : 96), 0);
                b.Draw(baseTexture, position + positionOffset + who.armOffset, new Rectangle?(sourceRect), overrideColor, 0, Vector2.Zero, 4f * scale, SpriteEffects.None, layerDepth + arm_layer_offset);
            }
        }
    }
}