using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Reflection;

namespace MobileTelevision
{
    public class MobileTV : TV
    {
        public MobileTV(int which, Vector2 tile) : base(which, tile)
        {

            Vector2 ps = ModEntry.api.GetScreenSize(false);
            Vector2 ls = ModEntry.api.GetScreenSize(true);
            backgroundTexture = new Texture2D(Game1.graphics.GraphicsDevice, (int)ps.X, (int)ps.Y);
            backgroundPortraitTexture = new Texture2D(Game1.graphics.GraphicsDevice, (int)ls.X, (int)ls.Y);
            Color[] data = new Color[backgroundTexture.Width * backgroundTexture.Height];
            Color[] data2 = new Color[backgroundPortraitTexture.Width * backgroundPortraitTexture.Height];
            int i = 0;
            while(i < data.Length || i < data2.Length)
            {
                if (i < data.Length)
                    data[i] = Color.Black;
                if (i < data2.Length)
                    data2[i] = Color.Black;
                i++;
            }
            backgroundTexture.SetData(data);
            backgroundPortraitTexture.SetData(data2);
        }

        private Vector2 screenSize;
        private float screenScale;
        private Texture2D backgroundTexture;
        private Texture2D backgroundPortraitTexture;

        public override Vector2 getScreenPosition()
        {
            Vector2 basePos =  base.getScreenPosition();
            ModEntry.context.Monitor.Log($"basePos {basePos}");
            SetScreenScale();
            Vector2 phoneScreenSize = ModEntry.api.GetScreenSize();
            Vector2 result;
            if (phoneScreenSize.X > phoneScreenSize.Y)
            {
                result = ModEntry.api.GetScreenPosition() + new Vector2(phoneScreenSize.X / 2f - screenSize.X / 2f, 0);
            }
            else
            {
                ModEntry.context.Monitor.Log($"portrait");

                result = ModEntry.api.GetScreenPosition() + new Vector2(0, phoneScreenSize.Y / 2f - screenSize.Y / 2f);
            }
            boundingBox.Value = new Rectangle((int)result.X, (int)result.Y, (int)screenSize.X, (int)screenSize.Y);
            ModEntry.context.Monitor.Log($"screenSize {screenSize}, screenScale {screenScale}, phoneScreenPos {ModEntry.api.GetScreenPosition()} phoneScreenSize {phoneScreenSize} pos {result}");
            return result;
        }

        public override float getScreenSizeModifier()
        {
            SetScreenScale();
            return screenScale;
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            SetScreenScale();
            TemporaryAnimatedSprite sprite = (TemporaryAnimatedSprite)typeof(TV).GetField("screen", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
            if(sprite != null)
            {
                spriteBatch.Draw(backgroundTexture, ModEntry.api.GetScreenRectangle(), Color.White);
                sprite.scale = GetScale(sprite.sourceRect);
                typeof(TV).GetField("screen", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, sprite);
                sprite.update(Game1.currentGameTime);
                sprite.draw(spriteBatch, true, 0, 0, 1f);
                TemporaryAnimatedSprite  sprite2 = (TemporaryAnimatedSprite)typeof(TV).GetField("screenOverlay", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
                if (sprite2 != null)
                {
                    sprite2.scale = GetScale(sprite2.sourceRect);
                    typeof(TV).GetField("screenOverlay", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, sprite2);
                    sprite2.update(Game1.currentGameTime);
                    sprite2.draw(spriteBatch, true, 0, 0, 1f);
                }
            }
        }

        private float GetScale(Rectangle sourceRect)
        {
            Vector2 phoneScreenSize = ModEntry.api.GetScreenSize();
            if (phoneScreenSize.X > phoneScreenSize.Y)
            {
                return phoneScreenSize.Y / sourceRect.Height;
            }
            else
            {
                return phoneScreenSize.X / sourceRect.Width;
            }
             
        }

        private void SetScreenScale()
        {
            Vector2 phoneScreenSize = ModEntry.api.GetScreenSize();
            if (phoneScreenSize.X > phoneScreenSize.Y)
            {
                screenScale = phoneScreenSize.Y / 28f;
                screenSize = new Vector2(42 * screenScale, phoneScreenSize.Y);
            }
            else
            {
                screenScale = phoneScreenSize.X / 42f;
                screenSize = new Vector2(phoneScreenSize.X, 28 * screenScale);
            }
        }
    }
}