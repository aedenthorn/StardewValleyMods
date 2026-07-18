using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FurnitureDisplayFramework
{
    public partial class ModEntry
    {
        private static void GameLocation_draw_Postfix(GameLocation __instance, SpriteBatch b)
        {
            if (!Config.EnableMod)
                return;

            foreach (Furniture f in __instance.furniture)
            {
                var name = f.Name;
                if(!TryGetFurnitureData(f, out var data))
                    continue;
                for(int i = 0; i < data.slots.Length; i++)
                {
                    if (!f.modData.TryGetValue("aedenthorn.FurnitureDisplayFramework/" + i, out var slotString) || slotString.Length == 0)
                        continue;
                    Object obj;
                    var currentItem = f.modData["aedenthorn.FurnitureDisplayFramework/" + i];
                    obj = GetObjectFromSlot(currentItem);
                    
                    if (obj == null)
                        continue;
                    float scale = 4;
                    var tempRect = data.slots[i].itemRect;
                    var itemRect = new Rectangle(Utility.Vector2ToPoint(f.getLocalPosition(Game1.viewport) + new Vector2(tempRect.X, tempRect.Y) * scale), Utility.Vector2ToPoint(new Vector2(tempRect.Width, tempRect.Height) * scale));
                    var layerDepth = ((f.furniture_type.Value == 12) ? (2E-09f + f.TileLocation.Y / 100000f) : ((f.boundingBox.Value.Bottom - ((f.furniture_type.Value == 6 || f.furniture_type.Value == 17 || f.furniture_type.Value == 13) ? 48 : 8)) + 1) / 10000f);

                    ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(obj.QualifiedItemId);
                    Rectangle sourceRect = itemData.GetSourceRect(obj is Mannequin ? 2 : 0, new int?(obj.ParentSheetIndex));
                    var texture = itemData.GetTexture();
                    if (texture != null)
                    {
                        b.Draw(texture, itemRect, sourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                    }
                }
            }
        } 
    }
}