using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Objects;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CustomPictureFrames
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private static void GameLocation_draw_Postfix(GameLocation __instance, SpriteBatch b)
        {
            if (!Config.EnableMod)
                return;

            foreach (Furniture f in __instance.furniture)
            {
                Texture2D texture;
                if (pictureDict.ContainsKey(f.Name))
                    texture = pictureDict[f.Name];
                else if (f.Name.Contains("/") && pictureDict.ContainsKey(f.Name.Split('/')[1]))
                    texture = pictureDict[f.Name.Split('/')[1]];
                else 
                    continue;
                b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, SHelper.Reflection.GetField<NetVector2>(f, "drawPosition").GetValue() + ((f.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero)), new Rectangle(0, 0, texture.Width, texture.Height), Color.White, 0f, Vector2.Zero, 1f, f.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (f.furniture_type.Value == 12) ? (2E-09f + f.TileLocation.Y / 100000f) : ((float)(f.boundingBox.Value.Bottom - ((f.furniture_type.Value == 6 || f.furniture_type.Value == 17 || f.furniture_type.Value == 13) ? 48 : 8)) / 10000f));
            }
        } 
        private static void Furniture_placementAction_Postfix(Furniture __instance)
        {
            if (!Config.EnableMod)
                return;
            SMonitor.Log($"furniture name {__instance.Name}");
        }
    }
}