using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FurnitureDisplayFramework
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
                var name = f.rotations.Value > 1 ? f.Name + ":" + f.currentRotation.Value : f.Name;

                if (!furnitureDisplayDict.ContainsKey(name))
                    continue;

                for(int i = 0; i < furnitureDisplayDict[name].slots.Length; i++)
                {
                    if (!f.modData.TryGetValue("aedenthorn.FurnitureDisplayFramework/" + i, out var slotString) || slotString.Length == 0)
                        continue;
                    Object obj;
                    if (slotString.Contains("{"))
                    {
                        obj = JsonConvert.DeserializeObject<Object>(slotString, new JsonSerializerSettings
                        {
                            Error = HandleDeserializationError
                        });
                    }
                    else
                    {
                        var currentItem = f.modData["aedenthorn.FurnitureDisplayFramework/" + i].Split(',');
                        obj = GetObjectFromID(currentItem[0], int.Parse(currentItem[1]), int.Parse(currentItem[2]));
                    }
                    if (obj == null)
                        continue;
                    float scale = 4;
                    var itemRect = new Rectangle(Utility.Vector2ToPoint(f.getLocalPosition(Game1.viewport) + new Vector2(furnitureDisplayDict[name].slots[i].itemRect.X, furnitureDisplayDict[name].slots[i].itemRect.Y) * scale), Utility.Vector2ToPoint(new Vector2(furnitureDisplayDict[name].slots[i].itemRect.Width, furnitureDisplayDict[name].slots[i].itemRect.Height) * scale));
                    var layerDepth = ((f.furniture_type.Value == 12) ? (2E-09f + f.TileLocation.Y / 100000f) : ((f.boundingBox.Value.Bottom - ((f.furniture_type.Value == 6 || f.furniture_type.Value == 17 || f.furniture_type.Value == 13) ? 48 : 8)) + 1) / 10000f);
                    if (obj.bigCraftable.Value)
                    {
                        b.Draw(Game1.bigCraftableSpriteSheet, itemRect, new Rectangle?(Object.getSourceRectForBigCraftable(obj.ParentSheetIndex)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                    }
                    else
                    {
                        b.Draw(Game1.objectSpriteSheet, itemRect, new Rectangle?(GameLocation.getSourceRectForObject(obj.ParentSheetIndex)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                    }
                }
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