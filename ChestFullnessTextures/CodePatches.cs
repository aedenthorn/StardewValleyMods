using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using StardewValley;
using StardewValley.Objects;
using System;

namespace ChestFullnessTextures
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Chest), nameof(Chest.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class Crop_draw_Patch
        {
            public static bool Prefix(Chest __instance, SpriteBatch spriteBatch, int x, int y, float alpha, int ___currentLidFrame)
            {
                if(!Config.ModEnabled || !__instance.playerChest.Value || !dataDict.TryGetValue(__instance.Name, out var dataList)) 
                    return true;
                ChestTextureData data = GetChestData(__instance, dataList);
                if (data is null)
                    return true;
                Texture2D texture = SHelper.GameContent.Load<Texture2D>(data.texturePath);
                float draw_x = (float)x;
                float draw_y = (float)y;
                if (__instance.localKickStartTile != null)
                {
                    draw_x = Utility.Lerp(__instance.localKickStartTile.Value.X, draw_x, __instance.kickProgress);
                    draw_y = Utility.Lerp(__instance.localKickStartTile.Value.Y, draw_y, __instance.kickProgress);
                }
                float base_sort_order = Math.Max(0f, ((draw_y + 1f) * 64f - 24f) / 10000f) + draw_x * 1E-05f;
                if (__instance.localKickStartTile != null)
                {
                    spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((draw_x + 0.5f) * 64f, (draw_y + 0.5f) * 64f)), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.Black * 0.5f, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.0001f);
                    draw_y -= (float)Math.Sin((double)__instance.kickProgress * 3.1415926535897931) * 0.5f;
                }
                int frame = 0;
                if(data.frames > 1 && ___currentLidFrame != __instance.startingLidFrame.Value)
                {
                    frame = Game1.ticks % data.frames;
                }
                int currentLidFrame = ___currentLidFrame - __instance.startingLidFrame.Value;
                spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)) + new Vector2(data.xOffset, data.yOffset), new Rectangle(frame * data.tileWidth, 0, data.tileWidth, data.tileHeight), __instance.Tint * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
                spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)) + new Vector2(data.xOffset, data.yOffset), new Rectangle(currentLidFrame * data.tileWidth, data.tileHeight, data.tileWidth, data.tileHeight), __instance.Tint * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
                return false;
            }
        }
    }
}