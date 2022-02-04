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
        private static void Sign_draw_Postfix(Sign __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!Config.EnableMod || __instance.displayItem.Value == null || __instance.displayItem.Value is not Furniture || !__instance.displayItem.Value.modData.TryGetValue("aedenthorn.CustomPictureFrames/index", out string indexString) || indexString == "-1" || !int.TryParse(indexString, out int index))
                return;

            string key;
            if (pictureDict.ContainsKey(__instance.displayItem.Value.Name))
                key = __instance.displayItem.Value.Name;
            else if (__instance.displayItem.Value.Name.Contains("/") && pictureDict.ContainsKey(__instance.displayItem.Value.Name.Split('/')[1]))
                key = __instance.displayItem.Value.Name.Split('/')[1];
            else
                return;
            if (pictureDict[key].Count <= index)
            {
                __instance.displayItem.Value.modData["aedenthorn.CustomPictureFrames/index"] = "-1";
                return;
            }
            Texture2D texture = pictureDict[key][index];

            spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 - (__instance.displayItem.Value as Furniture).sourceRect.Width * 2, y * 64 - 8 - (__instance.displayItem.Value as Furniture).sourceRect.Height * 2)), new Rectangle(0, 0, texture.Width, texture.Height), Color.White * alpha, 0f, Vector2.Zero, 1f, (__instance.displayItem.Value as Furniture).Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.getBoundingBox(new Vector2(x, y)).Bottom + 1) / 10000f);
        }
        private static void GameLocation_draw_Postfix(GameLocation __instance, SpriteBatch b)
        {
            if (!Config.EnableMod)
                return;

            foreach (Furniture f in __instance.furniture)
            {
                if (!f.modData.ContainsKey("aedenthorn.CustomPictureFrames/index"))
                    continue;
                if (f.modData["aedenthorn.CustomPictureFrames/index"] == "-1" || !int.TryParse(f.modData["aedenthorn.CustomPictureFrames/index"], out int index))
                    continue;

                string key;
                if (pictureDict.ContainsKey(f.Name))
                    key = f.Name;
                else if (f.Name.Contains("/") && pictureDict.ContainsKey(f.Name.Split('/')[1]))
                    key = f.Name.Split('/')[1];
                else 
                    continue;
                if (pictureDict[key].Count <= index)
                {
                    f.modData["aedenthorn.CustomPictureFrames/index"] = "-1";
                    return;
                }
                Texture2D texture = pictureDict[key][index];

                b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, SHelper.Reflection.GetField<NetVector2>(f, "drawPosition").GetValue() + ((f.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), new Rectangle(0, 0, texture.Width, texture.Height), Color.White, 0f, Vector2.Zero, 1f, f.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (f.furniture_type.Value == 12) ? (2E-09f + f.TileLocation.Y / 100000f) : ((f.boundingBox.Value.Bottom - ((f.furniture_type.Value == 6 || f.furniture_type.Value == 17 || f.furniture_type.Value == 13) ? 48 : 8)) / 10000f));
            }
        } 

    }
}