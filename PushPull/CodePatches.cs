using xTile.Dimensions;
using xTile.Tiles;
using StardewValley;
using StardewValley.Locations;
using StardewModdingAPI;
using System.Globalization;
using System.Linq;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Object = StardewValley.Object;
using StardewValley.ItemTypeDefinitions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;
using StardewValley.Objects;
using StardewValley.Extensions;

namespace PushPull
{
	public partial class ModEntry
    {
        internal static bool Farmer_FacingDirection_Prefix(Farmer __instance, ref int value)
        {
            if (!Config.ModEnabled || Game1.eventUp || __instance.currentLocation is null || PullingTicks.Value <= 0)
                return true;
            __instance.facingDirection.Value = PullingFace.Value; 
            return false;
        }
        internal static void Farmer_MovePosition_Prefix(Farmer __instance, ref Vector2[] __state)
        {
            if (!Config.ModEnabled || Game1.eventUp || __instance.currentLocation is null)
                return;
            var tileLoc = __instance.Tile;
            __state = [__instance.Position, tileLoc];
        }

        internal static void Farmer_MovePosition_Postfix(Farmer __instance, ref Vector2[] __state)
        {
            if (!Config.ModEnabled || Game1.eventUp || __state is null || __instance.currentLocation is null)
                return;
            var f = __instance;
            var bbc = f.GetBoundingBox().Center;
            var tilePos = new Vector2(bbc.X / 64, bbc.Y / 64);

            if (f.movementDirections.Any() && __state[0] == f.Position)
            {
                Vector2 startTile = new(f.GetBoundingBox().Center.X / 64, f.GetBoundingBox().Center.Y / 64);
                var dir = GetNextTile(f.FacingDirection);
                startTile += dir;
                if (__instance.currentLocation.objects.TryGetValue(startTile, out var obj) && !__instance.currentLocation.objects.ContainsKey(startTile + dir))
                {
                    var destination = startTile + dir;
                    if (movingObjects.TryGetValue(obj, out var d))
                    {
                        f.Position += d.position * d.direction;

                    }
                    else if (IsAllowed(f.currentLocation, obj, destination, true))
                    {
                        if(PushingTile.Value != startTile)
                        {
                            PushingTile.Value = startTile;
                        }
                        else if (PushingTicks.Value++ >= Config.Delay)
                        {
                            PushingTicks.Value = 0;
                            MoveObject(obj, new MovementData()
                            {
                                position = 0,
                                destination = destination,
                                location = f.currentLocation,
                                direction = dir
                            });
                        }
                    }
                }
                else
                {
                    PushingTile.Value = new(-1,-1);
                    PushingTicks.Value = 0;
                }
                
            }
            else
            {
                PushingTile.Value = new(-1, -1);
                PushingTicks.Value = 0;
            }

            if (!CheckPull())
            {
                PullingTile.Value = new(-1, -1);
                PullingTicks.Value = 0;
            }
        }

        private static bool IsAllowed(GameLocation l, Object obj, Vector2 dest, bool push)
        {
            return l.CanItemBePlacedHere(dest, collisionMask: ~CollisionMask.Farmers) && (push || Config.Pull) && ((Config.Constructs && (obj.HasTypeBigCraftable() || obj is not Object)) || Config.Rocks && obj.Name == "Stone" || Config.Sticks && obj.Name == "Twig");
        }

        internal static bool Object_draw_Prefix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!Config.ModEnabled || !movingObjects.TryGetValue(__instance, out var d))
                return true;
            if (__instance.bigCraftable.Value)
            {
                Vector2 scaleFactor = __instance.getScale();
                scaleFactor *= 4f;
                Vector2 position2 = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64))) + d.position * d.direction;
                Rectangle destination = new Rectangle((int)(position2.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position2.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
                int offset = 0;
                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(__instance.QualifiedItemId);
                spriteBatch.Draw(itemData.GetTexture(), destination, new Rectangle?(itemData.GetSourceRect(offset, new int?(__instance.ParentSheetIndex))), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
            }
            else
            {
                Rectangle bounds = __instance.GetBoundingBoxAt(x, y);

                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(__instance.QualifiedItemId);
                spriteBatch.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(y * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)))) + d.position * d.direction, new Rectangle?(itemData.GetSourceRect(0, null)), Color.White * alpha, 0f, new Vector2(8f, 8f), (__instance.scale.Y > 1f) ? __instance.getScale().Y : 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(__instance.isPassable() ? bounds.Top : bounds.Center.Y) / 10000f);
            }
            return false;
        }

        public static IEnumerable<CodeInstruction> Chest_draw_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Chest_draw");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Stloc_0)
                {
                    SMonitor.Log("Adjusting draw_x");
                    codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.AdjustDrawX))));
                    codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    i += 2;
                }
                else if (codes[i].opcode == OpCodes.Stloc_1)
                {
                    SMonitor.Log("Adjusting draw_y");
                    codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.AdjustDrawY))));
                    codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    break;
                }
            }

            return codes.AsEnumerable();
        }

        public static float AdjustDrawX(float val, Chest chest)
        {
            if (!Config.ModEnabled || !movingObjects.TryGetValue(chest, out var d))
                return val;
            val += d.position * d.direction.X / 64f;
            return val;
        }
        public static float AdjustDrawY(float val, Chest chest)
        {
            if (!Config.ModEnabled || !movingObjects.TryGetValue(chest, out var d))
                return val;
            val += d.position * d.direction.Y / 64f;
            return val;
        }
    }
}
