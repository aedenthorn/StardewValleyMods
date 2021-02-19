using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;

namespace MultipleSpouses
{
    internal class FurniturePatches
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, IModHelper helper, ModConfig config)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }
        public static bool BedFurniture_draw_Prefix(BedFurniture __instance, SpriteBatch spriteBatch, int x, int y, float alpha, NetVector2 ___drawPosition)
        {
            if (!Config.EnableMod || __instance.isTemporarilyInvisible || __instance.bedType != BedFurniture.BedType.Double)
                return true;

            if (!Config.CustomBed)
            {
                __instance.boundingBox.Width = 3 * 64;
                return true;
            }


            Vector2 drawPosition = Furniture.isDrawingLocationFurniture ? ___drawPosition : new Vector2(x * 64, y * 64 - (__instance.sourceRect.Height * 4 - __instance.boundingBox.Height));

            int bedWidth = Misc.GetBedWidth(Utility.getHomeOfFarmer(Game1.getFarmer(__instance.owner)));
            __instance.boundingBox.Width = bedWidth * 64;

            Rectangle drawn_rect = __instance.sourceRect.Value; 
            int third = drawn_rect.Width / 3;
            Rectangle drawn_first = new Rectangle(drawn_rect.X, drawn_rect.Y, third, drawn_rect.Height);
            Rectangle drawn_second = new Rectangle(drawn_rect.X + third, drawn_rect.Y, third, drawn_rect.Height);
            Rectangle drawn_third = new Rectangle(drawn_rect.X + third + third, drawn_rect.Y, third, drawn_rect.Height);

            spriteBatch.Draw(Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((__instance.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), new Rectangle?(drawn_first), Color.White * alpha, 0f, Vector2.Zero, 4f, __instance.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.boundingBox.Value.Top + 1) / 10000f);
            for(int i = 1; i < bedWidth - 1; i++)
            {
                spriteBatch.Draw(Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((__instance.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)) + new Vector2(third * i * 4, 0), new Rectangle?(drawn_second), Color.White * alpha, 0f, Vector2.Zero, 4f, __instance.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.boundingBox.Value.Top + 1) / 10000f);
            }
            spriteBatch.Draw(Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((__instance.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)) + new Vector2(third * (bedWidth - 1) * 4, 0), new Rectangle?(drawn_third), Color.White * alpha, 0f, Vector2.Zero, 4f, __instance.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.boundingBox.Value.Top + 1) / 10000f);

            drawn_rect.X += drawn_rect.Width;
            drawn_first = new Rectangle(drawn_rect.X, drawn_rect.Y, third, drawn_rect.Height);
            drawn_second = new Rectangle(drawn_rect.X + third, drawn_rect.Y, third, drawn_rect.Height);
            drawn_third = new Rectangle(drawn_rect.X + third + third, drawn_rect.Y, third, drawn_rect.Height);

            int solidHeight = 41;

            Rectangle drawn_first_t = new Rectangle(drawn_rect.X, drawn_rect.Y + solidHeight, third, drawn_rect.Height - solidHeight);
            Rectangle drawn_second_t = new Rectangle(drawn_rect.X + third, drawn_rect.Y + solidHeight, third, drawn_rect.Height - solidHeight);
            Rectangle drawn_third_t = new Rectangle(drawn_rect.X + third * 2, drawn_rect.Y + solidHeight, third, drawn_rect.Height - solidHeight);

            alpha = Config.SheetTransparency;

            spriteBatch.Draw(Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((__instance.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), new Rectangle?(drawn_first), Color.White * alpha, 0f, Vector2.Zero, 4f, __instance.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.boundingBox.Value.Bottom - 1) / 10000f);
            spriteBatch.Draw(Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((__instance.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)) + new Vector2(0, solidHeight * 4), new Rectangle?(drawn_first_t), Color.White, 0f, Vector2.Zero, 4f, __instance.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.boundingBox.Value.Bottom - 2) / 10000f);

            for (int i = 1; i < bedWidth - 1; i++)
            {
                spriteBatch.Draw(Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((__instance.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)) + new Vector2(third * i * 4, 0), new Rectangle?(drawn_second), Color.White * alpha, 0f, Vector2.Zero, 4f, __instance.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.boundingBox.Value.Bottom - 1) / 10000f);
                spriteBatch.Draw(Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((__instance.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)) + new Vector2(0, solidHeight * 4) + new Vector2(third * i * 4, 0), new Rectangle?(drawn_second_t), Color.White, 0f, Vector2.Zero, 4f, __instance.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.boundingBox.Value.Bottom - 1) / 10000f);
            }
            spriteBatch.Draw(Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((__instance.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)) + new Vector2(third * (bedWidth - 1) * 4, 0), new Rectangle?(drawn_third), Color.White * alpha, 0f, Vector2.Zero, 4f, __instance.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.boundingBox.Value.Bottom - 1) / 10000f);
            spriteBatch.Draw(Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + ((__instance.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)) + new Vector2(0, solidHeight * 4) + new Vector2(third * (bedWidth - 1) * 4, 0), new Rectangle?(drawn_third_t), Color.White, 0f, Vector2.Zero, 4f, __instance.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.boundingBox.Value.Bottom - 1) / 10000f);

            return false;

        }
    }
}