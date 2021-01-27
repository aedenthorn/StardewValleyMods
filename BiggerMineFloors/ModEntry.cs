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

                                        // squares
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
                                            else if (w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 15;
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
                                        case 184:
                                        case 219:
                                        case 258:
                                            if (w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 1;
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

                                        //corner floor

                                        // TR corner
                                        case 151:
                                            if (w == mult - 1 && h == 0)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            }
                                            else
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 183;
                                            }
                                            break;
                                        // BR corner
                                        case 150:
                                            if (w == mult - 1 && h == mult - 1)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            }
                                            else
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 183;
                                            }
                                            break;
                                        // TL corner
                                        case 152:
                                            if (w + h == 0)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            }
                                            else
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 183;
                                            }
                                            break;
                                        // BL corner
                                        case 149:
                                            if (w == 0 &&  h == mult - 1)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            }
                                            else
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 183;
                                            }
                                            break;

                                        // wooden wall
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
                                        case 20:
                                        case 36:
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

                                        // vines

                                        case 7:
                                        case 8:
                                            if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 7 + w % 2;
                                            else 
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 7 + 16 + (16 * ((h - 1) % 2)) + w % 2;
                                            break;
                                        case 23:
                                        case 39:
                                        case 24:
                                        case 40:
                                            newTiles[basePos.X + w, basePos.Y + h].TileIndex = 23 + 16 * h % 2 + w % 2;
                                            break;

                                        // jungle
                                        case 9:
                                            newTiles[basePos.X + w, basePos.Y + h].TileIndex = 8 + w % 7 + (16 * h % 4);
                                            break;

                                        // jungle bottom
                                        case 55:
                                        case 56:
                                            if(h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 55 + w % 2;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 40 - 16 * h % 2 - w % 2;
                                            break;
                                        case 65:
                                            if(w == mult - 1 && h == mult - 1)
                                            {
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
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
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
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = (w % 2 == 0 ? 85  : 110);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = (w % 2 == 0 ? 85 : 110) + 16 + 16 * (h % 2);
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
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = (w % 2 == 0 ? 85 : 110) + 32 - 16 * (h % 2);
                                            break;
                                        case 133:
                                            if (h == mult - 1 && w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16 - 16 * (h % 2);
                                            else if (h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = (w % 2 == 0 ? 133 : 158);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = (w % 2 == 0 ? 133 : 158) - 16 - 16 * (h % 2);
                                            break;

                                        case 110:
                                            if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = (w % 2 == 0 ? 110 : 85);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = (w % 2 == 0 ? 110 : 85) + 16 + 16 * (h % 2);
                                            break;
                                        case 126:
                                            if(w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = (w % 2 == 0 ? 110 : 85) + 16 + 16 * (h % 2);
                                            break;
                                        case 142:
                                            if(w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = (w % 2 == 0 ? 110 : 85) + 32 - 16 * (h % 2);
                                            break;
                                        case 158:
                                            if (h == mult - 1 && w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16 - 16 * (h % 2);
                                            else if (h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = (w % 2 == 0 ? 158 : 133);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = (w % 2 == 0 ? 158 : 133) - 16 - 16 * (h % 2);
                                            break;

                                        // top wall slant left
                                        case 70:
                                            if (w == mult - 1 && h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            else if (h == 1 && w < mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 85;
                                            else if (w == mult - 1)
                                                newTiles[basePos.X +w, basePos.Y + h].TileIndex = (h % 2 == 0 ? 86 : 102);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = (h % 2 == 0 ? 117 : 101);
                                            break;
                                        case 86:
                                            if (w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16 * (h % 2);
                                            else if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 85;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = (h % 2 == 0 ? 117 : 101);
                                            break;
                                        case 102:
                                            if (w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = (h % 2 == 0 ? 117 : 101);
                                            break;
                                        case 118:
                                            if (w == mult - 1 && h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w % 2 == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 101 + 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 109 + 16 * (h % 2);
                                            break;

                                        // top wall slant right
                                        case 93:
                                            if (w == 0 && h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            else if (h == 1 && w > 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 110;
                                            else if (w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = (h % 2 == 0 ? 109 : 125);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = (h % 2 == 0 ? 142 : 126);
                                            break;
                                        case 109:
                                            if (w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16 * (h % 2);
                                            else if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 110;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = (h % 2 == 0 ? 142 : 126);
                                            break;
                                        case 125:
                                            if (w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = (h % 2 == 0 ? 142 : 126);
                                            break;
                                        case 141:
                                            if (w == 0 && h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w % 2 == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 109 + 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 101 + 16 * (h % 2);
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
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 73 + w % 4;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 89 + 16 * h % 2 + w % 4;
                                            break;
                                        case 89:
                                        case 90:
                                        case 91:
                                        case 92:
                                            newTiles[basePos.X + w, basePos.Y + h].TileIndex = 89 + 16 * h % 2 + w % 4;
                                            break;
                                        case 105:
                                        case 106:
                                        case 107:
                                        case 108:
                                            newTiles[basePos.X + w, basePos.Y + h].TileIndex = 89 + 16 * h % 2 + w % 4;
                                            break;
                                        case 121:
                                        case 122:
                                        case 123:
                                        case 124:
                                            if (h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 121 + w % 4;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 89 + 16 * h % 2 + w % 4;
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
                                                newTiles[basePos.X + w, basePos.Y + h] = null;
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
                                                newTiles[basePos.X + w, basePos.Y + h] = null;

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
                                                newTiles[basePos.X + w, basePos.Y + h] = null;
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
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            }
                                            else
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h] = null;
                                            }
                                            break;
                                        // bottom wall
                                        case 214:
                                        case 215:
                                            if (h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 214 + w % 2;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            break;
                                        // left wall
                                        case 132:
                                        case 148:
                                        case 164:
                                        case 180:
                                            if(w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 132 + 16 * (h % 4);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            break;
                                        // right wall
                                        case 175:
                                        case 191:
                                        case 207:
                                        case 223:
                                            if(w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 175 + 16 * (h % 4);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            break;
                                        // TL inner corner
                                        case 116:
                                            if (w == mult - 1 && h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 32 + 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            break;
                                        // TL inner corner wall
                                        case 68:
                                            if (w == mult - 1 && h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16 + 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            break;
                                        case 100:
                                            if (w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            break;
                                        case 84:
                                            if (w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            break;
                                        // TR inner corner
                                        case 159:
                                            if (w == 0 && h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 32 + 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            break;
                                        // TR inner corner wall
                                        case 111:
                                            if (w == 0 && h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16 + 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            break;
                                        case 127:
                                            if (w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            break;
                                        case 143:
                                            if (w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + 16 * (h % 2);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            break;

                                        // slanted left wall
                                        case 216:
                                            if (w == mult - 1 && h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w == h)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 197;
                                            else if (w > h)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h] = null;
                                            }
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            break;
                                        case 232:
                                            if (w == mult - 1 && h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 132 + 16 * (h % 4);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            break;
                                        // slanted right wall
                                        case 220:
                                            if (w == 0 && h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w + h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 205;
                                            else if (w + h == mult)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 221;
                                            }
                                            else if (w + h < mult - 1)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h] = null;
                                            }
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            break;
                                        case 236:
                                            if (w == 0 && h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 175 + 16 * (h % 4);
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            break;

                                        // bottom slanted wall
                                        case 230:
                                            if (h == mult - 1)
                                            {
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex + w % 2;
                                            }
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h] = null;
                                            break;
                                        case 231:
                                            if (h == mult - 1)
                                            {
                                                if (mult % 2 == 1)
                                                    newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - w % 2;
                                                else
                                                    newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex - 1 + w % 2;
                                            }
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h] = null;
                                            break;

                                        // BR Corner Insides
                                        case 206:
                                        case 221:
                                            if (h == 0 && w == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            break;

                                        // BL Corner Insides
                                        case 196:
                                        case 213:
                                            if (h == 0 && w == mult -1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 79;
                                            break;


                                        //tracks
                                        case 226:
                                            if (w + h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w + h < mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 210;
                                            else 
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 209;
                                            break;
                                        case 227:
                                            if (w + h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else if (w + h < mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 209;
                                            else 
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = 210;
                                            break;

                                        // mine cart
                                        case 178:
                                        case 179:
                                            if (w == mult - 1 && h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h]= null;
                                            break;
                                        case 194:
                                        case 195:
                                            if (w == mult - 1 && h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h]= null;
                                            break;
                                        case 208:
                                            if (w == 0 && h == mult - 1)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h]= null;
                                            break;
                                        case 224:
                                            if (w == 0 && h == 0)
                                                newTiles[basePos.X + w, basePos.Y + h].TileIndex = tile.TileIndex;
                                            else
                                                newTiles[basePos.X + w, basePos.Y + h]= null;
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
            //__instance.loadLights();
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
