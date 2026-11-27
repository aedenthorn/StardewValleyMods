using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using Netcode;
using StardewValley;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace LawnGrass
{
    public partial class ModEntry
    {
        public static void InvalidateLawn()
        {
            foreach (Season s in Enum.GetValues(typeof(Season)))
                SHelper.GameContent.InvalidateCache(lawnPath + Utility.getSeasonKey(s));
        }

        /// <summary>Keep the lawn sprite's shape (alpha) and fill it with the map's grass tile, so the lawn matches whatever outdoor tilesheet is loaded (vanilla or a recolour).</summary>
        public static void PaintLawnFromMap(IAssetData asset, string season)
        {
            var device = Game1.graphics?.GraphicsDevice;
            if (device is null)
                return;
            Texture2D sheet;
            try
            {
                sheet = SHelper.GameContent.Load<Texture2D>($"Maps/{season}_outdoorsTileSheet");
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Couldn't load the {season} outdoor tilesheet, leaving the lawn as-is: {ex.Message}", StardewModdingAPI.LogLevel.Warn);
                return;
            }
            int columns = sheet.Width / 16;
            int tileX = columns > 0 ? Config.MapGrassTileIndex % columns * 16 : -1;
            int tileY = columns > 0 ? Config.MapGrassTileIndex / columns * 16 : -1;
            if (Config.MapGrassTileIndex < 0 || tileX < 0 || tileX + 16 > sheet.Width || tileY + 16 > sheet.Height)
            {
                SMonitor.Log($"Map Grass Tile {Config.MapGrassTileIndex} is outside the {season} outdoor tilesheet ({sheet.Width}x{sheet.Height}), leaving the lawn as-is.", StardewModdingAPI.LogLevel.Warn);
                return;
            }
            var grass = new Color[16 * 16];
            sheet.GetData(0, new Rectangle(tileX, tileY, 16, 16), grass, 0, grass.Length);

            var image = asset.AsImage();
            var lawn = image.Data;
            var shape = new Color[lawn.Width * lawn.Height];
            lawn.GetData(shape);
            var output = new Color[shape.Length];
            for (int y = 0; y < lawn.Height; y++)
            {
                for (int x = 0; x < lawn.Width; x++)
                {
                    int i = y * lawn.Width + x;
                    byte alpha = shape[i].A;
                    if (alpha == 0)
                        continue;
                    var source = grass[y % 16 * 16 + x % 16];
                    float scale = alpha / 255f; // textures are premultiplied
                    output[i] = new Color((byte)(source.R * scale), (byte)(source.G * scale), (byte)(source.B * scale), alpha);
                }
            }
            var patched = new Texture2D(device, lawn.Width, lawn.Height);
            patched.SetData(output);
            image.PatchImage(patched);
        }

        public static bool IsLawn(Grass grass)
        {
            return (Config.AllGrassIsLawn && grass.grassType.Value == 1) || grass.modData.ContainsKey(lawnKey);
        }
        public static Rectangle? SetSourceRect(Rectangle? sourceRect, Grass grass)
        {
            if (!Config.ModEnabled || grass.numberOfWeeds.Value == 4)
                return sourceRect;
            var height = (int)Math.Round(sourceRect.Value.Height * (grass.numberOfWeeds.Value / 4f));
            return new Rectangle(sourceRect.Value.X, sourceRect.Value.Y, sourceRect.Value.Width, height);
        }
        public static Vector2 SetPos(Vector2 pos, Grass grass)
        {
            if (!Config.ModEnabled || grass.numberOfWeeds.Value == 4)
                return pos;
            pos.Y += 20 * (4 - grass.numberOfWeeds.Value);
            return pos;
        }
        public static bool CheckForGrass(NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>> terrainFeatures, Vector2 vec)
        {
            if (!Config.ModEnabled || !Config.TrufflesInGrass)
                return terrainFeatures.ContainsKey(vec);
            return terrainFeatures.TryGetValue(vec, out var tf) && tf is not Grass;
        }
        public static string GetWhichSeason(GameLocation location)
        {
            string season = location.GetSeasonKey();
            if(!location.IsOutdoors && season == "winter")
            {
                season = "spring";
            }
            return season;
        }
        public static void OnAdded(Grass grass, GameLocation loc, Vector2 tilePos)
        {
            grass.Location = loc;
            grass.Tile = tilePos;
            UpdateNeighbors(grass);
        }

        public static void OnRemoved(Grass grass, GameLocation location)
        {
            if (grass.Location == null)
            {
                grass.Location = location;
            }
            List<Neighbor> list = GatherNeighbors(grass);
            grass.modData[maskKey] = "0";
            foreach (Neighbor i in list)
            {
                OnNeighborRemoved(i.feature, i.invDirection);
                UpdateDrawSums(i.feature);
            }
            UpdateDrawSums(grass);
        }

        public static void UpdateNeighbors(Grass grass)
        {
            List<Neighbor> list = GatherNeighbors(grass);
            byte neighborMask = 0;
            foreach (Neighbor i in list)
            {
                neighborMask |= i.direction;
                OnNeighborAdded(i.feature, i.invDirection);
                UpdateDrawSums(i.feature);
            }
            grass.modData[maskKey] = neighborMask.ToString();
            UpdateDrawSums(grass);
        }
        public static void OnNeighborAdded(Grass grass, byte direction)
        {
            if (!grass.modData.TryGetValue(maskKey, out var str) || !byte.TryParse(str, out byte neighborMask))
                neighborMask = 0;
            neighborMask |= direction;
            grass.modData[maskKey] = neighborMask.ToString();
        }
        public static void OnNeighborRemoved(Grass grass, byte direction)
        {
            if (!grass.modData.TryGetValue(maskKey, out var str) || !byte.TryParse(str, out byte neighborMask))
                neighborMask = 0;
            neighborMask &= (byte)~direction;
            grass.modData[maskKey] = neighborMask.ToString();

        }
        private static List<Neighbor> GatherNeighbors(Grass grass)
        {
            List<Neighbor> results = new();
            GameLocation location = grass.Location;
            Vector2 tilePos = grass.Tile;
            NetVector2Dictionary<TerrainFeature,NetRef<TerrainFeature>> terrainFeatures = location.terrainFeatures;
            foreach (NeighborLoc item in _offsets)
            {
                Vector2 tile = tilePos + item.Offset;
                TerrainFeature feature;
                if (terrainFeatures.TryGetValue(tile, out feature) && feature is Grass g && (IsLawn(g) || Config.ProtectNonLawn))
                {
                    Neighbor i = new(g, item.Direction, item.InvDirection);
                    results.Add(i);
                }
            }
            return results;
        }
        public static void UpdateDrawSums(Grass grass)
        {
            if (!grass.modData.TryGetValue(maskKey, out var str) || !byte.TryParse(str, out byte neighborMask))
                neighborMask = 0;
            byte drawSum = (byte)(neighborMask & 15);
            if (drawGuide is null)
                PopulateDrawGuide();
            grass.modData[posKey] = drawGuide[drawSum].ToString();
        }
        private struct NeighborLoc
        {
            public NeighborLoc(Vector2 a, byte b, byte c)
            {
                Offset = a;
                Direction = b;
                InvDirection = c;
            }

            public readonly Vector2 Offset;

            public readonly byte Direction;

            public readonly byte InvDirection;
        }
        private struct Neighbor
        {
            public Neighbor(Grass a, byte b, byte c)
            {
                feature = a;
                direction = b;
                invDirection = c;
            }

            public readonly Grass feature;

            public readonly byte direction;

            public readonly byte invDirection;
        }
        private static readonly NeighborLoc[] _offsets = new NeighborLoc[]
        {
            new (HoeDirt.N_Offset, 1, 4),
            new (HoeDirt.S_Offset, 4, 1),
            new (HoeDirt.E_Offset, 2, 8),
            new (HoeDirt.W_Offset, 8, 2)
        };
        public static Dictionary<byte, int> drawGuide;
        public static void PopulateDrawGuide()
        {
            Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
            dictionary[0] = 0;
            dictionary[8] = 15;
            dictionary[2] = 13;
            dictionary[1] = 12;
            dictionary[4] = 4;
            dictionary[9] = 11;
            dictionary[3] = 9;
            dictionary[5] = 8;
            dictionary[6] = 1;
            dictionary[12] = 3;
            dictionary[10] = 14;
            dictionary[7] = 5;
            dictionary[15] = 6;
            dictionary[13] = 7;
            dictionary[11] = 10;
            dictionary[14] = 2;
            drawGuide = dictionary;
        }
    }
}