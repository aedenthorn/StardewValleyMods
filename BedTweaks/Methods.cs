using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Netcode;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedTweaks
{
    partial class ModEntry
    {
        public static string[] getFurnitureData(string itemId)
        {
            if(!DataLoader.Furniture(Game1.content).TryGetValue(itemId, out string data))
            {
                return null;
            }

            return data.Split("/");
        }

        public static Texture2D getTexture(string itemId)
        {
            string[] data = getFurnitureData(itemId);
            // If there is a custom texture, it will be in data[9]. If there is not, data may be less than 10 long, or it may be greater but data[9] is just "".
            if(data == null || data.Length < 10 || data[9] == "")
            {
                return Game1.content.Load<Texture2D>(Furniture.furnitureTextureName);
            }

            return Game1.content.Load<Texture2D>(data[9]);
        }

        public static bool drawBedAtLocation(BedFurniture bed, SpriteBatch spriteBatch, Vector2 drawPosition, float alpha)
        {
            int bedWidth = Math.Max(Config.BedWidth, 3);
            bed.boundingBox.Width = bedWidth * 64;

            Rectangle drawn_rect = bed.sourceRect.Value;
            int third = drawn_rect.Width / 3;
            Rectangle drawn_first = new Rectangle(drawn_rect.X, drawn_rect.Y, third, drawn_rect.Height);
            Rectangle drawn_second = new Rectangle(drawn_rect.X + third, drawn_rect.Y, third, drawn_rect.Height);
            Rectangle drawn_third = new Rectangle(drawn_rect.X + third + third, drawn_rect.Y, third, drawn_rect.Height);

            BedData bedData = BedManager.getBedData(bed.ItemId);

            Texture2D furnitureTexture = getTexture(bed.ItemId);

            spriteBatch.Draw(furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((bed.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), new Rectangle?(drawn_first), Color.White * alpha, 0f, Vector2.Zero, 4f, bed.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (bed.boundingBox.Value.Top + 1) / 10000f);
            for (int i = 1; i < bedWidth - 1; i++)
            {
                spriteBatch.Draw(furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((bed.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)) + new Vector2(third * i * 4, 0), new Rectangle?(drawn_second), Color.White * alpha, 0f, Vector2.Zero, 4f, bed.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (bed.boundingBox.Value.Top + 1) / 10000f);
            }
            spriteBatch.Draw(furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((bed.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)) + new Vector2(third * (bedWidth - 1) * 4, 0), new Rectangle?(drawn_third), Color.White * alpha, 0f, Vector2.Zero, 4f, bed.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (bed.boundingBox.Value.Top + 1) / 10000f);

            if(Config.RedrawMiddlePillows && bedData.shouldRedrawPillow)
            {
                drawPillowsAtLocation(drawPosition, spriteBatch, furnitureTexture, bedData, bed, bedWidth, alpha);
            }

            drawn_rect.X += drawn_rect.Width;
            drawn_first = new Rectangle(drawn_rect.X, drawn_rect.Y, third, drawn_rect.Height);
            drawn_second = new Rectangle(drawn_rect.X + third, drawn_rect.Y, third, drawn_rect.Height);
            drawn_third = new Rectangle(drawn_rect.X + third + third, drawn_rect.Y, third, drawn_rect.Height);

            int solidHeight = bedData.lowerBedframeTop;

            Rectangle drawn_first_t = new Rectangle(drawn_rect.X, drawn_rect.Y + solidHeight, third, drawn_rect.Height - solidHeight);
            Rectangle drawn_second_t = new Rectangle(drawn_rect.X + third, drawn_rect.Y + solidHeight, third, drawn_rect.Height - solidHeight);
            Rectangle drawn_third_t = new Rectangle(drawn_rect.X + third * 2, drawn_rect.Y + solidHeight, third, drawn_rect.Height - solidHeight);

            float sheetAlpha = alpha;
            sheetAlpha *= Config.SheetTransparency;

            spriteBatch.Draw(furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((bed.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), new Rectangle?(drawn_first), Color.White * sheetAlpha, 0f, Vector2.Zero, 4f, bed.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (bed.boundingBox.Value.Bottom - 1) / 10000f);
            spriteBatch.Draw(furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((bed.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)) + new Vector2(0, solidHeight * 4), new Rectangle?(drawn_first_t), Color.White, 0f, Vector2.Zero, 4f, bed.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (bed.boundingBox.Value.Bottom - 2) / 10000f);

            for (int i = 1; i < bedWidth - 1; i++)
            {
                spriteBatch.Draw(furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((bed.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)) + new Vector2(third * i * 4, 0), new Rectangle?(drawn_second), Color.White * sheetAlpha, 0f, Vector2.Zero, 4f, bed.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (bed.boundingBox.Value.Bottom - 1) / 10000f);
                spriteBatch.Draw(furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((bed.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)) + new Vector2(0, solidHeight * 4) + new Vector2(third * i * 4, 0), new Rectangle?(drawn_second_t), Color.White, 0f, Vector2.Zero, 4f, bed.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (bed.boundingBox.Value.Bottom - 1) / 10000f);
            }
            spriteBatch.Draw(furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((bed.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)) + new Vector2(third * (bedWidth - 1) * 4, 0), new Rectangle?(drawn_third), Color.White * sheetAlpha, 0f, Vector2.Zero, 4f, bed.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (bed.boundingBox.Value.Bottom - 1) / 10000f);
            spriteBatch.Draw(furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((bed.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)) + new Vector2(0, solidHeight * 4) + new Vector2(third * (bedWidth - 1) * 4, 0), new Rectangle?(drawn_third_t), Color.White, 0f, Vector2.Zero, 4f, bed.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (bed.boundingBox.Value.Bottom - 1) / 10000f);

            return false;
        }

        private static void drawPillowsAtLocation(Vector2 drawPosition, SpriteBatch spriteBatch, Texture2D furnitureTexture, BedData bedData, BedFurniture bed, int bedWidth, float alpha)
        {
            Rectangle pillowRect = new Rectangle(bed.sourceRect.X + bedData.pillowStartX, bed.sourceRect.Y + bedData.pillowStartY, bedData.pillowWidth, bedData.pillowHeight);

            Rectangle littlePillowRect;
            if(bedData.hasLittlePillows)
            {
                littlePillowRect = new Rectangle(bed.sourceRect.X + bedData.pillowStartX + bedData.pillowWidth, bed.sourceRect.Y + bedData.pillowStartY, (int)getDistanceBetweenPillows(bedData, 3), bedData.pillowHeight);
            }
            else
            {
                littlePillowRect = new Rectangle();
            }

            float distanceBetweenPillows = getDistanceBetweenPillows(bedData, bedWidth);

            for(int i = 0; i < bedWidth - 1; i++)
            {
                float pillowStartX = getPillowStartX(i, bedData, distanceBetweenPillows);
                spriteBatch.Draw(furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((bed.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)) + new Vector2(pillowStartX * 4, bedData.pillowStartY * 4), new Rectangle?(pillowRect), Color.White * alpha, 0f, Vector2.Zero, 4f, bed.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (bed.boundingBox.Value.Top + 3+i) / 10000f);
                if(bedData.hasLittlePillows && i < bedWidth - 2)
                {
                    float littlePillowStartX = pillowStartX + bedData.pillowWidth + distanceBetweenPillows/2 - getDistanceBetweenPillows(bedData, 3)/2;
                    spriteBatch.Draw(furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((bed.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)) + new Vector2(littlePillowStartX * 4, bedData.pillowStartY*4), new Rectangle?(littlePillowRect), Color.White * alpha, 0f, Vector2.Zero, 4f, bed.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (bed.boundingBox.Value.Top + 2) / 10000f);
                }
            }
        }

        /// <summary>
        /// Gets the distance, in pixels, to draw a given pillow from the left of the start of the bed sprite
        /// </summary>
        /// <param name="pillowNumber">The index of the pillow from the left of the bed (zero-indexed)</param>
        /// <param name="bedData">The data for the bed with these pillows</param>
        /// <param name="bedWidthTiles">The width, in tiles, of the bed</param>
        /// <returns></returns>
        private static float getPillowStartX(int pillowNumber, BedData bedData, float distanceBetweenPillows)
        {
            return bedData.pillowStartX + pillowNumber * (bedData.pillowWidth + distanceBetweenPillows);
        }

        private static float getDistanceBetweenPillows(BedData bedData, int bedWidthTiles)
        {
            return (bedWidthTiles * 16 - bedData.pillowStartX - (48 - bedData.pillowEndX) - (bedWidthTiles - 1) * bedData.pillowWidth) / (float)(bedWidthTiles - 2);
        }
    }
}
