using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Security;

namespace PushInChairs
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class Furniture_draw_Patch
        {
            public static bool Prefix(Furniture __instance, SpriteBatch spriteBatch, int x, int y, float alpha, ref Dictionary<string, string> ____frontTextureName, NetInt ___sourceIndexOffset, NetVector2 ___drawPosition)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(modKey) || __instance.isTemporarilyInvisible || !Furniture.isDrawingLocationFurniture)
                    return true;
                if (__instance.sittingFarmers.Any())
                {
                    __instance.modData.Remove(modKey);
                    return true;
                }
                Rectangle drawn_source_rect = __instance.sourceRect.Value;
                drawn_source_rect.X += drawn_source_rect.Width * ___sourceIndexOffset.Value;
                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(__instance.QualifiedItemId);
                Texture2D texture = itemData.GetTexture();
                string textureName = itemData.TextureName;
                Vector2 actualDrawPosition = Game1.GlobalToLocal(Game1.viewport, ___drawPosition.Value + ((__instance.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero));
                SpriteEffects spriteEffects = (__instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
                Color color = Color.White * alpha;
                if (__instance.currentRotation.Value == 2)
                {
                    if (____frontTextureName == null)
                    {
                        ____frontTextureName = new Dictionary<string, string>();
                    }
                    string frontTexturePath;
                    if (!____frontTextureName.TryGetValue(textureName, out frontTexturePath))
                    {
                        frontTexturePath = textureName + "Front";
                        ____frontTextureName[textureName] = frontTexturePath;
                    }
                    Texture2D frontTexture = null;
                    try
                    {
                        frontTexture = SHelper.GameContent.Load<Texture2D>(frontTexturePath);
                    }
                    catch
                    {
                    }
                    if(frontTexture == null)
                    {
                        __instance.modData.Remove(modKey);
                        return true;
                    }
                    spriteBatch.Draw(frontTexture, actualDrawPosition + new Vector2(0, -48), new Rectangle?(drawn_source_rect), color, 0f, Vector2.Zero, 4f, spriteEffects, (float)(__instance.boundingBox.Value.Bottom - 56) / 10000f);
                    return false;
                }
                 
                switch(__instance.currentRotation.Value)
                {
                    case 1:
                        actualDrawPosition += new Vector2(48, 0);
                        drawn_source_rect.Width = 4;
                        break;
                    case 0:
                        actualDrawPosition += new Vector2(0, 48);
                        break;
                    case 3:
                        drawn_source_rect.Width = 4;
                        break;
                    default:
                        __instance.modData.Remove(modKey);
                        return true;
                }
                
                spriteBatch.Draw(texture, actualDrawPosition, new Rectangle?(drawn_source_rect), color, 0f, Vector2.Zero, 4f, spriteEffects, (__instance.furniture_type.Value == 12) ? (2E-09f + __instance.TileLocation.Y / 100000f) : ((float)(__instance.boundingBox.Value.Bottom - ((__instance.furniture_type.Value == 6 || __instance.furniture_type.Value == 17 || __instance.furniture_type.Value == 13) ? 48 : 8)) / 10000f));


                return false;
            }
        }

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.GetBoundingBoxAt))]
        public class Furniture_GetBoundingBoxAt_Patch
        {
            public static void Postfix(Furniture __instance, ref Rectangle __result)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(modKey))
                    return;
                switch(__instance.currentRotation.Value)
                {
                    case 2:
                        __result.Height = 16;
                        break;
                    case 1:
                        __result.X += 48;
                        __result.Width = 16;
                        break;
                    case 0:
                        __result.Y += 48;
                        __result.Height = 16;
                        break;
                    case 3:
                        __result.Width = 16;
                        break;
                    default:
                        __instance.modData.Remove(modKey);
                        return;
                }
                
            }
        }

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.OnRemoved))]
        public class Furniture_OnRemoved_Patch
        {
            public static void Postfix(Furniture __instance)
            {
                if (!Config.ModEnabled)
                    return;
                if (__instance.modData.ContainsKey(modKey))
                {
                    __instance.modData.Remove(modKey);
                }
                else if (IsTable(__instance))
                {
                    foreach(var f in Game1.currentLocation.furniture)
                    {
                        if (f.modData.ContainsKey(modKey))
                        {
                            CheckFurniture(f);
                        }
                    }
                }
            }
        }
    }
}