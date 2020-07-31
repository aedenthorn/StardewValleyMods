using Harmony;
using static Harmony.AccessTools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.IO;
using xTile;
using xTile.Layers;
using xTile.Tiles;
using xTile.Dimensions;
using System.Threading;

namespace BiggerMineFloors
{
    public class ModEntry : Mod 
	{
		public static ModEntry context;

		public static ModConfig Config;

        public static Dictionary<int, string> specialTiles = new Dictionary<int, string>()
        {
{ 0, "Unique" },
{ 1, "TopLeft" },
{ 2, "TopMid" },
{ 3, "TopRight" },
{ 4, "TopLeft" },
{ 5, "TopMid" },
{ 6, "TopLeft" },
{ 7, "TopMid" },
{ 8, "TopMid" },
{ 9, "TopLeft" },
{ 10, "TopRight" },
{ 11, "TopLeft" },
{ 12, "TopRight" },
{ 13, "TopLeft" },
{ 14, "TopRight" },
{ 15, "TopMid" },
{ 16, "Unique" },
{ 17, "MidLeft" },
{ 18, "Copy" },
{ 19, "MidRight" },
{ 20, "Unique" },
{ 21, "Unique" },
{ 22, "Unique" },
{ 23, "Copy" },
{ 24, "Copy" },
{ 25, "BottomLeft" },
{ 26, "BottomRight" },
{ 27, "BottomLeft" },
{ 28, "BottomRight" },
{ 29, "BottomLeft" },
{ 30, "BottomRight" },
{ 31, "BottomMid" },
{ 32, "Unique" },
{ 33, "BottomLeft" },
{ 34, "BottomMid" },
{ 35, "BottomRight" },
{ 36, "Unique" },
{ 37, "Unique" },
{ 38, "Unique" },
{ 39, "Copy" },
{ 40, "Copy" },
{ 41, "TopLeft" },
{ 42, "TopRight" },
{ 43, "TopLeft" },
{ 44, "TopRight" },
{ 45, "TopLeft" },
{ 46, "TopRight" },
{ 47, "TopMid" },
{ 48, "Unique" },
{ 49, "Copy" },
{ 50, "Unique" },
{ 51, "Unique" },
            { 52, "BottomLeft" },
            { 53, "BottomMid" },
            { 54, "BottomRight" },
            { 55, "BottomMid" },
            { 56, "BottomMid" },
{ 57, "BottomLeft" },
{ 58, "BottomRight" },
{ 59, "BottomLeft" },
{ 60, "BottomRight" },
{ 61, "BottomLeft" },
{ 62, "BottomRight" },
{ 63, "BottomMid" },
{ 64, "Unique" },
{ 65, "Unique" },
{ 66, "Unique" },
{ 67, "Unique" },
{ 68, "type" },
{ 69, "type" },
{ 70, "type" },
{ 71, "type" },
{ 72, "type" },
{ 73, "type" },
{ 74, "type" },
{ 75, "type" },
{ 76, "type" },
{ 77, "type" },
{ 78, "type" },
{ 79, "type" },
{ 80, "type" },
{ 81, "type" },
{ 82, "type" },
{ 83, "type" },
{ 84, "type" },
{ 85, "type" },
{ 86, "type" },
{ 87, "type" },
{ 88, "type" },
{ 89, "type" },
{ 90, "type" },
{ 91, "type" },
{ 92, "type" },
{ 93, "type" },
{ 94, "type" },
{ 95, "type" },
{ 96, "Unique" },
{ 97, "Unique" },
{ 98, "type" },
{ 99, "type" },
{ 100, "type" },
{ 101, "type" },
{ 102, "type" },
{ 103, "type" },
{ 104, "type" },
{ 105, "type" },
{ 106, "type" },
{ 107, "type" },
{ 108, "type" },
{ 109, "type" },
{ 110, "type" },
{ 111, "type" },
{ 112, "Unique" },
{ 113, "Unique" },
{ 114, "type" },
{ 115, "type" },
{ 116, "type" },
{ 117, "type" },
{ 118, "type" },
{ 119, "type" },
{ 120, "type" },
            { 121, "copyRight" },
            { 122, "copyRight" },
            { 123, "copyRight" },
            { 124, "copyRight" },

{ 125, "type" },
{ 126, "type" },
{ 127, "type" },
{ 128, "type" },
{ 129, "type" },
{ 130, "type" },
{ 131, "type" },
{ 132, "type" },
{ 133, "type" },
{ 134, "type" },
{ 135, "type" },
{ 136, "type" },
{ 137, "type" },
{ 138, "type" },
{ 139, "type" },
{ 140, "type" },
{ 141, "type" },
{ 142, "type" },
{ 143, "type" },
{ 144, "type" },
{ 145, "type" },
{ 146, "type" },
{ 147, "type" },
{ 148, "type" },
{ 149, "type" },
{ 150, "type" },
{ 151, "type" },
{ 152, "type" },
{ 153, "type" },
{ 154, "type" },
{ 155, "type" },
{ 156, "type" },
{ 157, "type" },
            { 158, "copyRight" },
{ 159, "type" },
{ 160, "type" },
{ 161, "type" },
            { 162, "copyRight" },
            { 163, "copyRight" },
{ 164, "type" },
{ 165, "type" },
{ 166, "type" },
{ 167, "type" },
{ 168, "type" },
{ 169, "type" },
{ 170, "type" },
{ 171, "type" },
{ 172, "type" },
{ 173, "Unique" },
{ 174, "type" },
{ 175, "type" },
{ 176, "type" },
{ 177, "type" },
{ 178, "type" },
{ 179, "type" },
{ 180, "type" },
{ 181, "type" },
{ 182, "type" },
{ 183, "type" },
{ 184, "type" },
{ 185, "type" },
{ 186, "type" },
{ 187, "type" },
{ 188, "type" },
{ 189, "type" },
{ 190, "type" },
{ 191, "type" },
{ 192, "type" },
            { 193, "copyRight" },

{ 194, "type" },
{ 195, "type" },
{ 196, "type" },
{ 197, "type" },
{ 198, "type" },
{ 199, "type" },
{ 200, "type" },
{ 201, "type" },
{ 202, "type" },
{ 203, "type" },
{ 204, "Unique" },
{ 205, "type" },
{ 206, "type" },
{ 207, "type" },
{ 208, "type" },
{ 209, "type" },

            { 210, "copyRight" },
            { 211, "copyRight" },
            { 212, "copyRight" },
{ 213, "type" },
{ 214, "type" },
{ 215, "type" },
{ 216, "type" },
{ 217, "type" },
{ 218, "type" },
{ 219, "type" },
{ 220, "type" },
{ 221, "type" },
{ 222, "type" },
{ 223, "type" },
{ 224, "type" },
            { 225, "copyRight" },
{ 226, "type" },
{ 227, "type" },
{ 228, "type" },
{ 229, "type" },
{ 230, "type" },
{ 231, "type" },
{ 232, "type" },
{ 233, "type" },
{ 234, "type" },
{ 235, "type" },
{ 236, "type" },
{ 237, "Unique" },
{ 238, "Unique" },
{ 239, "Unique" },
{ 240, "type" },
{ 241, "type" },
{ 242, "type" },
{ 243, "type" },
{ 244, "type" },
{ 245, "type" },
{ 246, "type" },
{ 247, "type" },
{ 248, "type" },
{ 249, "type" },
{ 250, "type" },
{ 251, "type" },
{ 252, "type" },
{ 253, "type" },
{ 254, "type" },
{ 255, "type" },
{ 256, "type" },
{ 257, "type" },
{ 258, "type" },
{ 259, "type" },
{ 260, "type" },
{ 261, "type" },
{ 262, "type" },
{ 263, "type" },
{ 264, "type" },
{ 265, "type" },
{ 266, "type" },
{ 267, "type" },
{ 268, "type" },
{ 269, "type" },
{ 270, "type" },
{ 271, "type" },
{ 272, "type" },
{ 273, "type" },
{ 274, "type" },
{ 275, "type" },
{ 276, "type" },
{ 277, "type" },
{ 278, "type" },
{ 279, "type" },
{ 280, "type" },
{ 281, "type" },
{ 282, "type" },




        };

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
		{
			context = this;
			Config = Helper.ReadConfig<ModConfig>();
			if (!Config.EnableMod)
				return;

            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(MineShaft), nameof(GameLocation.loadMap)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.loadMap_postfix))
            );

        }

        private static void loadMap_postfix(GameLocation __instance)
        {
            if (!(__instance is MineShaft))
                return;
            Size oldSize = __instance.map.Layers[0].LayerSize; ;
            Map map = __instance.map;
            context.ResizeMap(ref map);
            Size newSize = __instance.map.Layers[0].LayerSize; ;
            int mult = Config.FloorSizeMult;
            context.Monitor.Log($"resized map {__instance.Name} original size {oldSize.Width},{oldSize.Height} new size {newSize.Width},{newSize.Height}");

            for (int i = 0; i < __instance.map.Layers.Count; i++)
            {
                Tile[,] newTiles = new Tile[newSize.Width, newSize.Height];
                for (int x = 0; x < oldSize.Width; x++)
                {
                    for (int y = 0; y < oldSize.Height; y++)
                    {
                        Tile tile = __instance.map.Layers[i].PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                        if (tile != null)
                        {
                            Point basePos = new Point(x * mult, y * mult);
                            for (int w = 0; w < mult; w++)
                            {
                                for (int h = 0; h < mult; h++)
                                {
                                    newTiles[basePos.X + w, basePos.Y + h] = new StaticTile(__instance.map.Layers[i], tile.TileSheet, tile.BlendMode, tile.TileIndex);
                                }
                            }
                            for (int w = 0; w < mult; w++)
                            {
                                for (int h = 0; h < mult; h++)
                                {
                                    int plus;
                                    switch (BaseTileIndex(tile.TileIndex))
                                    {
                                        case 1:
                                        case 166:
                                        case 201:
                                        case 240:
                                        case 246:
                                            if (w + h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 1;
                                            else if (w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 17;
                                            break;
                                        case 2:
                                        case 167:
                                        case 202:
                                        case 241:
                                        case 247:
                                            if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16;
                                            break;
                                        case 3:
                                        case 168:
                                        case 203:
                                        case 242:
                                        case 248:
                                            if (w == mult - 1 && h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 1;
                                            else if (w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 15;
                                            break;
                                        case 4:
                                            plus = w == 0 ? 0 : 1;
                                            if(h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + plus;
                                            else if (h == 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16 + plus;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 32 + plus;
                                            break;
                                        case 5:
                                            if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (h == 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 32;
                                            break;
                                        case 6:
                                            plus = w == mult - 1 ? 0 : -1;
                                            if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + plus;
                                            else if (h == 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16 + plus;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 32 + plus;
                                            break;
                                        case 7:
                                        case 8:
                                            if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 7 + w % 2;
                                            else 
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 7 + 16 + (16 * ((h - 1) % 2)) + w % 2;
                                            break;
                                        case 9:
                                            newTiles[basePos.X + w, basePos.Y + h].TileIndex = 8 + w % 7 + (16 * h % 4);
                                            break;
                                        case 17:
                                        case 182:
                                        case 217:
                                        case 256:
                                        case 262:
                                            if (w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 1;
                                            break;
                                        case 19:
                                        case 183:
                                        case 218:
                                        case 257:
                                        case 263:
                                            if (w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 1;
                                            break;
                                        case 20:
                                        case 36:
                                        case 184:
                                        case 219:
                                        case 258:
                                        case 264:
                                            plus = w == 0 ? 0 : 1;
                                            newTiles[basePos.X + w, basePos.Y + h].TileIndex = 36 + plus;
                                            break;
                                        case 21:
                                            newTiles[basePos.X + w, basePos.Y + h].TileIndex = 37;
                                            break;
                                        case 22:
                                        case 38:
                                            plus = w == mult - 1 ? 0 : -1;
                                            newTiles[basePos.X + w, basePos.Y + h].TileIndex = 38 + plus;
                                            break;
                                        case 23:
                                        case 39:
                                        case 24:
                                        case 40:
                                            newTiles[basePos.X + w, basePos.Y + h].TileIndex = 23 + 16 * h % 2 + w % 2;
                                            break;
                                        case 33:
                                        case 198:
                                        case 233:
                                        case 272:
                                        case 278:
                                            if (w == 0 && h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 1;
                                            else if (w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 15;
                                            break;
                                        case 34:
                                        case 199:
                                        case 234:
                                        case 273:
                                        case 279:
                                            if (h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16;
                                            break;
                                        case 35:
                                        case 200:
                                        case 235:
                                        case 274:
                                        case 280:
                                            if (w == mult - 1 && h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 1;
                                            else if (w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 17;
                                            break;
                                        case 52:
                                            plus = w == 0 ? 0 : 1;
                                            if (h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + plus;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16 + plus;
                                            break;
                                        case 53:
                                            if (h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16;
                                            break;
                                        case 54:
                                            plus = w == 0 ? 0 : -1;
                                            if (h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + plus;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16 + plus;
                                            break;
                                        case 55:
                                        case 56:
                                            if(h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 55 + w % 2;
                                            else
                                                newTiles[basePos.X + mult - 1 - w, basePos.Y + mult - 1 - h].TileIndex = 40 - 16 * h % 2 - w % 2;
                                            break;
                                        case 65:
                                            context.Monitor.Log($"index {tile.TileIndex} {basePos.X},{basePos.Y} {w},{h} mult {mult}");
                                            if(w == mult - 1 && h == mult - 1)
                                            {
                                                context.Monitor.Log($"tileIndex");
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            }
                                            else
                                            {
                                                context.Monitor.Log($"{108 - 16 * h % 2 - w % 4}");
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 108 - 16 * h % 2 - w % 4;
                                            }
                                            break;
                                        case 66:
                                            if(w == 0 && h == mult - 1)
                                                newTiles[basePos.X, basePos.Y + mult - 1].TileIndex = tile.TileIndex;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 108 - 16 * h % 2 - w % 2;
                                            break;
                                        case 81:
                                            if (w == mult - 1 && h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 108 - 16 * h % 2 - w % 4;
                                            break;
                                        case 82:
                                            if (w == 0 && h == 0)
                                                newTiles[basePos.X, basePos.Y + mult - 1].TileIndex = tile.TileIndex;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 108 - 16 * h % 2 - w % 2;
                                            break;
                                        
                                        // ladder
                                        case 67:
                                            if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16 + 16 * h % 2;
                                            break;
                                        case 115:
                                            if (h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 32 + 16 * h % 2;
                                            break;
                                        
                                        // top wall
                                        case 85:
                                            if (h == 0 && w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16 + 16 * (h % 2);
                                            else if (h == 0)
                                                newTiles[basePos.X + mult - 1 - w, basePos.Y + h].TileIndex = (w % 2 == 0 ? 85  : 110);
                                            else
                                                newTiles[basePos.X + mult - 1 - w, basePos.Y + h].TileIndex = (w % 2 == 0 ? 85 : 110) + 16 + 16 * (h % 2);
                                            break;
                                        case 101:
                                            if(w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + mult - 1 - w, basePos.Y + h].TileIndex = (w % 2 == 0 ? 85 : 110) + 16 + 16 * (h % 2);
                                            break;
                                        case 117:
                                            if(w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + mult - 1 - w, basePos.Y + h].TileIndex = (w % 2 == 0 ? 85 : 110) + 32 - 16 * (h % 2);
                                            break;
                                        case 133:
                                            if (h == mult - 1 && w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16 - 16 * (h % 2);
                                            else if (h == mult - 1)
                                                newTiles[basePos.X + mult - 1 - w, basePos.Y + h].TileIndex = (w % 2 == 0 ? 133 : 158);
                                            else
                                                newTiles[basePos.X + mult - 1 - w, basePos.Y + h].TileIndex = (w % 2 == 0 ? 133 : 158) - 16 - 16 * (h % 2);
                                            break;

                                        // left face
                                        case 71:
                                            if (w == mult - 1 && h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16 + 16 * (h % 2);
                                            else if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 73 + w % 4;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 73 + w % 4 + 16 + 16 * (h % 2);
                                            break;
                                        case 87:
                                            if (w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 1 + w % 4 + 16 * (h % 2);
                                            break;
                                        case 103:
                                            if (w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 1 + w % 4 - 16 * (h % 2);
                                            break;
                                        case 119:
                                            if (w + h == (mult - 1) * 2)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 32 + 16 * (h % 2);
                                            else if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 1 + w % 4;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 1 + w % 4 - 32 + 16 * (h % 2);
                                            break;

                                        // right face
                                        case 72:
                                            if (w + h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16 + 16 * (h % 2);
                                            else if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 1 + w % 4;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 1 + w % 4 + 16 + 16 * (h % 2);
                                            break;
                                        case 88:
                                            if (w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 1 + w % 4 + 16 * (h % 2);
                                            break;
                                        case 104:
                                            if (w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 1 + w % 4 - 16 * (h % 2);
                                            break;
                                        case 120:
                                            if (w == 0 && h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 32 + 16 * (h % 2);
                                            else if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + (w + 1) % 4;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + (w + 1) % 4 - 32 + 16 * (h % 2);
                                            break;

                                        // top walls
                                        case 73:
                                        case 74:
                                        case 75:
                                        case 76:
                                            if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 73 + (w + tile.TileIndex - 73) % 4;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 89 + 16 * h % 2 + (w + tile.TileIndex - 73) % 4;
                                            break;
                                        case 89:
                                        case 90:
                                        case 91:
                                        case 92:
                                            newTiles[basePos.X + w, basePos.Y + h].TileIndex = 89 + 16 * h % 2 + (w + tile.TileIndex - 89) % 4;
                                            break;
                                        case 105:
                                        case 106:
                                        case 107:
                                        case 108:
                                            newTiles[basePos.X + w, basePos.Y + h].TileIndex = 89 + 16 * h % 2 + (w + tile.TileIndex - 105) % 4;
                                            break;
                                        case 121:
                                        case 122:
                                        case 123:
                                        case 124:
                                            if (h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 121 + (w + tile.TileIndex - 121) % 4;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 89 + 16 * h % 2 + (w + tile.TileIndex - 121) % 4;
                                            break;

                                        // TR corner
                                        case 157:
                                            if(w == h)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            }
                                            else if(w == h + 1)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16;
                                            }
                                            else if(w > h + 1)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 15;
                                            }
                                            else
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = -1;
                                            }
                                            break;
                                        // BR corner
                                        case 205:
                                            if(w + h == mult - 1)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            }
                                            else if(w + h == mult)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16;
                                            }
                                            else if(w + h > mult)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            }
                                            else
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = -1;
                                            }
                                            break;
                                        // TL corner
                                        case 134:
                                            if(w + h == mult - 1)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            }
                                            else if(w + h == mult - 2)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16;
                                            }
                                            else if(w + h < mult - 2)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 17;
                                            }
                                            else
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = -1;
                                            }
                                            break;
                                        // BL corner
                                        case 197:
                                            if (w == h)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            }
                                            else if (w == h - 1)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16;
                                            }
                                            else if (w < h - 1)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex = 79;
                                            }
                                            else
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = -1;
                                            }
                                            break;
                                        default:
                                            try
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h] = tile;
                                            }
                                            catch (Exception ex)
                                            {
                                                context.Monitor.Log($"Error trying to add tile at {basePos.X + basePos.Y + h} to map {__instance.Name}: {ex}", LogLevel.Error);
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                FieldRefAccess<Layer, Tile[,]>(__instance.map.Layers[i], "m_tiles") = newTiles;
                FieldRefAccess<Layer, TileArray>(__instance.map.Layers[i], "m_tileArray") = new TileArray(__instance.map.Layers[i], newTiles);

            }
            __instance.map = map;
            __instance.loadLights();
        }

        private static int BaseTileIndex(int tileIndex)
        {
            int x = tileIndex % 16;
            int y = tileIndex / 16;
            if(x > 8 && y < 4) // bushes
            {
                return 9;
            }

            return tileIndex;
        }

        private void ResizeMap(ref Map map)
        {
            int mult = Config.FloorSizeMult;
			Monitor.Log($"Multiplying map size by {mult}x");
			Point newSize = new Point(map.Layers[0].LayerWidth * mult, map.Layers[0].LayerHeight * mult);
            Monitor.Log($"old size {map.Layers[0].LayerWidth},{map.Layers[0].LayerHeight} new size {newSize.X},{newSize.Y}");
			for (int i = 0; i < map.Layers.Count; i++)
			{
				FieldRefAccess<Layer, Size>(map.Layers[i], "m_layerSize") = new Size(newSize.X, newSize.Y);
                Tile[,] tiles = FieldRefAccess<Layer, Tile[,]>(map.Layers[i], "m_tiles");

                Tile[,] newTiles = new Tile[newSize.X, newSize.Y];
                for (int k = 0; k < tiles.GetLength(0); k++)
                {
                    for (int l = 0; l < tiles.GetLength(1); l++)
                    {
                        newTiles[k, l] = tiles[k, l];
                    }
                }

                FieldRefAccess<Layer, Tile[,]>(map.Layers[i], "m_tiles") = newTiles;
				FieldRefAccess<Layer, TileArray>(map.Layers[i], "m_tileArray") = new TileArray(map.Layers[i], newTiles);
			}
            Monitor.Log($"map new size {map.Layers[0].LayerWidth},{map.Layers[0].LayerHeight}");
        }
    }
}
