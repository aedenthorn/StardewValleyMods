using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;

namespace CraftableBalloons
{
    public partial class ModEntry : Mod
    {
        public static Color GetDisplayColor(string hex)
        {
            var color = GetColor(hex);
            if(color == new Color(6, 6, 6))
            {
                color = Utility.GetPrismaticColor(0, Config.PrismaticSpeed);
            }
            return color;
        }
        public static Color GetColor(string hex)
        {
            var color = hex.StartsWith("#") && hex.Length == 7 ? new Color(Convert.ToByte(hex.Substring(1, 2), 16), Convert.ToByte(hex.Substring(3, 2), 16), Convert.ToByte(hex.Substring(5, 2), 16)) : Color.White;
            return color;
        }
        public static string MakeColorString(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public static void DrawBalloon(SpriteBatch b, string id, Color color, Vector2 position, int facing, Point vel, int standing)
        {
            if (!TryGetBalloonTexture(id, out var texturePath))
                return;
            int up = 0;
            int right = 0;
            int max = 10;
            int div = 4;
            int height = 12;
            int segments = 1;
            if (vel != Point.Zero)
            {
                up = MathHelper.Clamp(vel.Y / div, -max, max) / 2;

                right = MathHelper.Clamp(vel.X / div, -max, max);
                height -= Math.Abs(up);
                height += (int)Math.Round(Math.Sin(Game1.ticks / 4f));
                segments = Math.Min(Math.Abs(right) + 1, height);
            }
            else
            {
                right += (int)Math.Round(Math.Sin(Game1.ticks / 50f));
            }

            var tex = SHelper.GameContent.Load<Texture2D>(texturePath);
            Vector2 offset = new Vector2(-18 - segments * 4 * Math.Sign(right), -128 + (12 - height) * 4);
            float drawOffset = (standing + (up <= 0 ? 1 : 0)) / 10000f;
            switch (facing)
            {
                case 1:
                    if (up >= 0)
                        drawOffset = (standing) / 10000f;
                    offset += new Vector2(0, -8);
                    break;
                case 3:
                    if(up >= 0)
                        drawOffset = (standing) / 10000f;
                    offset += new Vector2(32, -8);
                    break;
                case 2:
                    if (up >= 0)
                    {
                        drawOffset = (standing) / 10000f;
                    }
                    offset += new Vector2(32, -8);
                    break;
            }
            Vector2 pos = Game1.GlobalToLocal(position + offset);
            offset = new Vector2(32 - right * 2, 32 + Math.Abs(right * 2));
            Vector2 origin = new Vector2(8f, 8f);
            
            float rot = -right / 12f;
            b.Draw(tex, pos + offset, new Rectangle(0, 16, 16, 16), color, rot, origin, 4f, SpriteEffects.None, drawOffset);
            b.Draw(tex, pos + offset, new Rectangle(16, 0, 16, 16), Color.White, rot, origin, 4f, SpriteEffects.None, drawOffset + 1/10000f);
            b.Draw(tex, pos + new Vector2(0, 64), new Rectangle(16, 16, 16, 1), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, drawOffset - 1 / 10000f);

            int segmentHeight = (int)Math.Max(height / (float)segments, 1);
            for(int i = 0; i < segments; i++)
            {
                //b.DrawString(Game1.dialogueFont, $"{segments}, {segmentHeight}, {height}", Vector2.Zero, Color.White);
                b.Draw(tex, pos + new Vector2((i + 1) * 4 * Math.Sign(right), 68 + segmentHeight * i * 4), new Rectangle(16, 17, 16, segmentHeight), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, drawOffset - 1 / 10000f);
            }
        }
        public static bool TryGetBalloonTexture(string itemId, out string texturePath)
        {
            if (itemId is null)
            {
                texturePath = null;
                return false;
            }
            if(itemId == balloonKey)
            {
                texturePath = balloonPath;
                return true;
            }
            return SHelper.GameContent.Load<Dictionary<string, string>>(dictPath).TryGetValue(itemId, out texturePath);
        }
        public static bool TryGetBalloonFromCharacter(Character character, out string id, out Color color)
        {
            id = balloonKey;
            if (character is Farmer f && f.ActiveObject is ColoredObject c)
            {
                id = c.ItemId;
                color = c.color.Value;
                if (color == new Color(6, 6, 6))
                    color = Utility.GetPrismaticColor(0, Config.PrismaticSpeed);
            }
            else if (character.modData.TryGetValue(modKey, out var ct))
            {
                var split = ct.Split(' ');
                color = GetDisplayColor(split[0]);
                if(split.Length > 1)
                {
                    id = split[1];
                }
            }
            else
            {
                color = Color.White;
                return false;
            }
            return true;
        }
    }
}
