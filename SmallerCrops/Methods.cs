using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace SmallerCrops
{
    public partial class ModEntry
    {
        private static int GetMouseIndex(int tileX, int tileY, bool inTile = true)
        {
            var mousePos = Game1.getMousePosition();
            mousePos = new Point(mousePos.X + Game1.viewport.X, mousePos.Y + Game1.viewport.Y);
            if (inTile) 
            { 
                var box = new Rectangle(tileX * 64, tileY * 64, 64, 64);
                if(!box.Contains(mousePos))
                    return -1;
            }

            var x = mousePos.X % 64;
            var y = mousePos.Y % 64;
            if (x < 32 && y < 32)
                return 0;
            int idx = 1;
            if (y > 32)
            {
                if (x > 32)
                {
                    idx = 3;
                }
                else
                {
                    idx = 2;
                }
            }
            return idx;
        }
        private static float GetCropScale(Crop __instance)
        {
            if (!Config.ModEnabled || __instance.forageCrop.Value)
                return 4f;
            return 2f;
        }
        private static float GetGiantCropScale()
        {
            if (!Config.ModEnabled)
                return 4f;
            return 1f;
        }
        private static float GetPlacementScale(Object __instance)
        {
            if (!Config.ModEnabled || (__instance.Category != -74 && __instance.Category != -19))
                return 4f;
            return 2f;
        }
        private static int GetPlacementX(Object __instance)
        {
            if (!Config.ModEnabled || (__instance.Category != -74 && __instance.Category != -19))
                return Game1.viewport.X;
            int idx = GetMouseIndex(0, 0, false);
            return Game1.viewport.X - 32 * (idx % 2);
        }
        private static int GetPlacementY(Object __instance)
        {
            if (!Config.ModEnabled || (__instance.Category != -74 && __instance.Category != -19))
                return Game1.viewport.Y;
            int idx = GetMouseIndex(0, 0, false);
            return Game1.viewport.Y - (idx > 1 ? 32 : 0);
        }
        private static float GetScale()
        {
            if (!Config.ModEnabled)
                return 4f;
            return 2f;
        }
        private static double GetGiantCropDouble(ulong a, ulong b, ulong c, ulong d)
        {
            if (!Config.ModEnabled)
                return (double)AccessTools.Method("StardewValley.OneTimeRandom:GetDouble").Invoke(null, new object[] { a, b, c, d });
            return 1;
        }
        private struct NeighborLoc
        {
            public NeighborLoc(Vector2 a, byte b, byte c)
            {
                this.Offset = a;
                this.Direction = b;
                this.InvDirection = c;
            }

            public readonly Vector2 Offset;

            public readonly byte Direction;

            public readonly byte InvDirection;
        }
        private static object[] GetNeigbourOffsets(GameLocation loc, Vector2 tilePos)
        {
            if (!Config.ModEnabled)
                return (object[])AccessTools.Field(typeof(HoeDirt), "_offsets").GetValue(null);
            return new object[] {
            };
            if (tilePos.X >= tileOffset * 3)
            {
                 return new object[] {
                    new NeighborLoc(new Vector2(-tileOffset * 2, -tileOffset * 2), 1, 4),
                    new NeighborLoc(new Vector2(-tileOffset * 2, -tileOffset * 2 + 1), 4, 1),
                    new NeighborLoc(new Vector2(-tileOffset + 1, -tileOffset), 2, 8),
                    new NeighborLoc(new Vector2(-tileOffset, -tileOffset), 8, 2)
                };
            }
            else if (tilePos.X >= tileOffset * 2)
            {
                 return new object[] {
                    new NeighborLoc(new Vector2(-tileOffset * 2, -tileOffset * 2), 1, 4),
                    new NeighborLoc(new Vector2(-tileOffset * 2, -tileOffset * 2 + 1), 4, 1),
                    new NeighborLoc(new Vector2(tileOffset, tileOffset), 2, 8),
                    new NeighborLoc(new Vector2(tileOffset - 1, tileOffset), 8, 2)
                };
            }
            else if (tilePos.X >= tileOffset)
            {
                 return new object[] {
                    new NeighborLoc(new Vector2(tileOffset * 2, tileOffset * 2 - 1), 1, 4),
                    new NeighborLoc(new Vector2(tileOffset * 2, tileOffset * 2), 4, 1),
                    new NeighborLoc(new Vector2(-tileOffset + 1, -tileOffset), 2, 8),
                    new NeighborLoc(new Vector2(-tileOffset, -tileOffset), 8, 2)
                };
            }
            else
            {
                 return new object[] {
                    new NeighborLoc(new Vector2(tileOffset * 2, tileOffset * 2 - 1), 1, 4),
                    new NeighborLoc(new Vector2(tileOffset * 2, tileOffset * 2), 4, 1),
                    new NeighborLoc(new Vector2(tileOffset, tileOffset), 2, 8),
                    new NeighborLoc(new Vector2(tileOffset - 1, tileOffset), 8, 2)
                };
            }
        }
    }
}