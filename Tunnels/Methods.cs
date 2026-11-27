using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Audio;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Network;
using StardewValley.Objects;
using System;
using System.Linq;
using Object = StardewValley.Object;

namespace Tunnels
{
    public partial class ModEntry
    {

        public static void DrawColoredObject(ColoredObject obj, SpriteBatch spriteBatch, int xNonTile, int yNonTile, float alpha)
        {
            if (obj.bigCraftable.Value)
            {
                Vector2 scaleFactor = obj.getScale();
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile, yNonTile - 64));
                Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f), (int)(position.Y - scaleFactor.Y / 2f), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                int indexOffset = 0;
                if (obj.showNextIndex.Value)
                {
                    indexOffset = 1;
                }
                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(obj.QualifiedItemId);
                Texture2D texture = itemData.GetTexture();
                if (!obj.ColorSameIndexAsParentSheetIndex)
                {
                    Rectangle coloredSourceRect = itemData.GetSourceRect(indexOffset + 1, new int?(obj.ParentSheetIndex));
                    spriteBatch.Draw(texture, destination, new Rectangle?(itemData.GetSourceRect(indexOffset, new int?(obj.ParentSheetIndex))), Color.White, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)(yNonTile + 64 - 1) / 10000f));
                    spriteBatch.Draw(texture, destination, new Rectangle?(coloredSourceRect), obj.color.Value, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (yNonTile + 64) / 10000f));
                }
                else
                {
                    spriteBatch.Draw(texture, destination, new Rectangle?(itemData.GetSourceRect(0, new int?(obj.ParentSheetIndex))), obj.color.Value, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (yNonTile + 64 - 1) / 10000f));
                }
            }
            else if (!Game1.eventUp || obj.Location.IsFarm)
            {
                ParsedItemData itemData2 = ItemRegistry.GetDataOrErrorItem(obj.QualifiedItemId);
                Texture2D texture2 = itemData2.GetTexture();
                int bottom = yNonTile;
                if (!obj.ColorSameIndexAsParentSheetIndex)
                {
                    Rectangle coloredSourceRect2 = itemData2.GetSourceRect(1, new int?(obj.ParentSheetIndex));
                    spriteBatch.Draw(texture2, Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile + 32, yNonTile + 32)), new Rectangle?(itemData2.GetSourceRect(0, new int?(obj.ParentSheetIndex))), Color.White, 0f, new Vector2(8f, 8f), (obj.scale.Y > 1f) ? obj.getScale().Y : 4f, obj.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, bottom / 10000f);
                    spriteBatch.Draw(texture2, Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile + 32 + ((obj.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), yNonTile + 32 + ((obj.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), new Rectangle?(coloredSourceRect2), obj.color.Value, 0f, new Vector2(8f, 8f), (obj.scale.Y > 1f) ? obj.getScale().Y : 4f, obj.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (bottom + 1) / 10000f);
                    return;
                }
                spriteBatch.Draw(texture2, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(xNonTile + 32 + ((obj.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(yNonTile * 64 + 32 + ((obj.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)))), new Rectangle?(itemData2.GetSourceRect(0, new int?(obj.ParentSheetIndex))), obj.color.Value, 0f, new Vector2(8f, 8f), (obj.scale.Y > 1f) ? obj.getScale().Y : 4f, obj.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, bottom / 10000f);
            }
        }
    }
}