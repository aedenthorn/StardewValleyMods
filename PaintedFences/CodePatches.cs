using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.Fences;
using StardewValley.ItemTypeDefinitions;
using System;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace PaintedFences
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Fence), nameof(Fence.draw), new Type[] { typeof(SpriteBatch),typeof(int),typeof(int),typeof(float), })]
        public class Fence_draw_Patch
        {
            public static bool Prefix(Fence __instance, SpriteBatch b, int x, int y, float alpha)
            {
                if (!Config.EnableMod)
                    return true;
                var color = GetColor(__instance);
                if(color == Color.Transparent)
                    return true;
                if (!fenceTextureDict.TryGetValue(__instance.ItemId, out var texture))
                {
                    return true;
                }

                int sourceRectPosition = 1;
                FenceData data = __instance.GetData();
                if (data == null)
                {
                    IItemDataDefinition itemType = ItemRegistry.RequireTypeDefinition(__instance.TypeDefinitionId);
                    b.Draw(itemType.GetErrorTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.TileLocation.X * 64f, __instance.TileLocation.Y * 64f)), new Rectangle?(itemType.GetErrorSourceRect()), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-09f);
                    return false;
                }
                if (__instance.health.Value > 1f || __instance.repairQueued.Value)
                {
                    int drawSum = __instance.getDrawSum();
                    sourceRectPosition = Fence.fenceDrawGuide[drawSum];
                    if (__instance.isGate.Value)
                    {
                        Vector2 offset = new Vector2(0f, 0f);
                        if (drawSum <= 110)
                        {
                            if (drawSum == 10)
                            {
                                b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2((float)(x * 64 - 16), (float)(y * 64 - 128))), new Rectangle?(new Rectangle((__instance.gatePosition.Value == 88) ? 24 : 0, 192, 24, 48)), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32 + 1) / 10000f);
                                return false;
                            }
                            if (drawSum == 100)
                            {
                                b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2((float)(x * 64 - 16), (float)(y * 64 - 128))), new Rectangle?(new Rectangle((__instance.gatePosition.Value == 88) ? 24 : 0, 240, 24, 48)), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32 + 1) / 10000f);
                                return false;
                            }
                            if (drawSum == 110)
                            {
                                b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2((float)(x * 64 - 16), (float)(y * 64 - 64))), new Rectangle?(new Rectangle((__instance.gatePosition.Value == 88) ? 24 : 0, 128, 24, 32)), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32 + 1) / 10000f);
                                return false;
                            }
                        }
                        else
                        {
                            if (drawSum == 500)
                            {
                                b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2((float)(x * 64 + 20), (float)(y * 64 - 64 - 20))), new Rectangle?(new Rectangle((__instance.gatePosition.Value == 88) ? 24 : 0, 320, 24, 32)), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 96 - 1) / 10000f);
                                return false;
                            }
                            if (drawSum == 1000)
                            {
                                b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2((float)(x * 64 + 20), (float)(y * 64 - 64 - 20))), new Rectangle?(new Rectangle((__instance.gatePosition.Value == 88) ? 24 : 0, 288, 24, 32)), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 - 32 + 2) / 10000f);
                                return false;
                            }
                            if (drawSum == 1500)
                            {
                                b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2((float)(x * 64 + 20), (float)(y * 64 - 64 - 20))), new Rectangle?(new Rectangle((__instance.gatePosition.Value == 88) ? 16 : 0, 160, 16, 16)), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 - 32 + 2) / 10000f);
                                b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2((float)(x * 64 + 20), (float)(y * 64 - 64 + 44))), new Rectangle?(new Rectangle((__instance.gatePosition.Value == 88) ? 16 : 0, 176, 16, 16)), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 96 - 1) / 10000f);
                                return false;
                            }
                        }
                        sourceRectPosition = 17;
                    }
                    else if (__instance.heldObject.Value != null)
                    {
                        Vector2 offset2 = Vector2.Zero;
                        offset2 += data.HeldObjectDrawOffset;
                        if (drawSum != 10)
                        {
                            if (drawSum == 100)
                            {
                                offset2.X = data.LeftEndHeldObjectDrawX;
                            }
                        }
                        else
                        {
                            offset2.X = data.RightEndHeldObjectDrawX;
                        }
                        offset2 *= 4f;
                        __instance.heldObject.Value.draw(b, x * 64 + (int)offset2.X, y * 64 + (int)offset2.Y, (float)(y * 64 + 64) / 10000f, 1f);
                    }
                }
                b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64))), new Rectangle?(new Rectangle(sourceRectPosition * Fence.fencePieceWidth % texture.Bounds.Width, sourceRectPosition * Fence.fencePieceWidth / texture.Bounds.Width * Fence.fencePieceHeight, Fence.fencePieceWidth, Fence.fencePieceHeight)), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32) / 10000f);
                return false;
            }
        }
    }
}