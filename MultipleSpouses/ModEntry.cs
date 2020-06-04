using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using static Harmony.AccessTools;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace MultipleSpouses
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

		public static IMonitor PMonitor;
        public static ModConfig config;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
			PMonitor = Monitor;
			config = this.Helper.ReadConfig<ModConfig>();
			var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static Point GetClearPosition(FarmHouse location)
        {
            List<Point> tiles = new List<Point>();
            for (int x2 = (int)Math.Round(location.map.Layers[0].LayerWidth * 0.1f); x2 < (int)Math.Round(location.map.Layers[0].LayerWidth * 0.9f); x2++)
            {
                for (int y2 = (int)Math.Round(location.map.Layers[0].LayerHeight * 0.1f); y2 < (int)Math.Round(location.map.Layers[0].LayerHeight * 0.9f); y2++)
                {
                    Layer l = location.map.GetLayer("Paths");
                    if (l != null)
                    {
                        Tile t = l.Tiles[x2, y2];
                        if (t != null)
                        {
                            Vector2 tile2 = new Vector2((float)x2, (float)y2);
                            if (location.isTileLocationTotallyClearAndPlaceable(tile2))
                            {
                                tiles.Add(new Point(x2,y2));
                            }
                        }
                    }

                    if (tiles.Count == 0)
                    {
                        Tile t = location.map.Layers[0].Tiles[x2, y2];
                        if (t != null)
                        {
                            Vector2 tile2 = new Vector2((float)x2, (float)y2);
                            if (location.isTileLocationTotallyClearAndPlaceable(tile2))
                            {
                                tiles.Add(new Point(x2, y2));
                            }
                        }
                    }
                }
            }
            if (tiles.Count == 0)
            {
                return Point.Zero;
            }
            Point posT = tiles[Game1.random.Next(0, tiles.Count)];
            return posT;
        }

        public static void BuildSpouseRoom(FarmHouse farmHouse, string name, int count)
        {

			NPC spouse = Game1.getCharacterFromName(name);
			if (spouse != null)
			{
				int indexInSpouseMapSheet = -1;
				if (name == "Sam")
				{
					indexInSpouseMapSheet = 9;
				}
				else if (name == "Penny")
				{
					indexInSpouseMapSheet = 1;
				}
				else if (name == "Sebastian")
				{
					indexInSpouseMapSheet = 5;
				}
				else if (name == "Alex")
				{
					indexInSpouseMapSheet = 6;
				}
				else if (name == "Krobus")
				{
					indexInSpouseMapSheet = 12;
				}
				else if (name == "Maru")
				{
					indexInSpouseMapSheet = 4;
				}
				else if (name == "Haley")
				{
					indexInSpouseMapSheet = 3;
				}
				else if (name == "Harvey")
				{
					indexInSpouseMapSheet = 7;
				}
				else if (name == "Shane")
				{
					indexInSpouseMapSheet = 10;
				}
				else if (name == "Abigail")
				{
					indexInSpouseMapSheet = 0;
				}
				else if (name == "Emily")
				{
					indexInSpouseMapSheet = 11;
				}
				else if (name == "Elliott")
				{
					indexInSpouseMapSheet = 8;
				}
				else if (name == "Leah")
				{
					indexInSpouseMapSheet = 2;
				}

				PMonitor.Log($"Building {name}'s room", LogLevel.Debug);
				
				Microsoft.Xna.Framework.Rectangle areaToRefurbish = (farmHouse.upgradeLevel == 1) ? new Microsoft.Xna.Framework.Rectangle(36+(7*count), 1, 6, 9) : new Microsoft.Xna.Framework.Rectangle(42+(7 * count), 10, 6, 9);
				Map refurbishedMap = Game1.game1.xTileContent.Load<Map>("Maps\\spouseRooms");
				Point mapReader = new Point(indexInSpouseMapSheet % 5 * 6, indexInSpouseMapSheet / 5 * 9);
				farmHouse.map.Properties.Remove("DayTiles");
				farmHouse.map.Properties.Remove("NightTiles");

				List<Layer> layers = FieldRefAccess<Map, List<Layer>>(farmHouse.map, "m_layers");
				for(int i = 0; i < layers.Count; i++)
                {
					Tile[,] tiles = FieldRefAccess<Layer, Tile[,]>(layers[i], "m_tiles");
					Size size = FieldRefAccess<Layer, Size>(layers[i], "m_layerSize");
					size = new Size(size.Width + 7, size.Height);
					FieldRefAccess<Layer, Size>(layers[i], "m_layerSize") = size;
					FieldRefAccess<Map, List<Layer>>(farmHouse.map, "m_layers") = layers;

					Tile[,] newTiles = new Tile[tiles.GetLength(0) + 7, tiles.GetLength(1)];

					for (int k = 0; k < tiles.GetLength(0); k++)
					{
						for (int l = 0; l < tiles.GetLength(1); l++)
						{
							newTiles[k, l] = tiles[k, l];
						}
					}

					FieldRefAccess<Layer, Tile[,]>(layers[i], "m_tiles") = newTiles;
					FieldRefAccess<Layer, TileArray>(layers[i], "m_tileArray") = new TileArray(layers[i], newTiles);
				}
				FieldRefAccess<Map, List<Layer>>(farmHouse.map, "m_layers") = layers;

				int ox = 0;
				int oy = 0;
				if (farmHouse.upgradeLevel > 1)
				{
					ox = 6;
					oy = 9;
				}

				for (int i = 0; i < 7; i++)
				{
					farmHouse.setMapTileIndex(ox + 36 + i + (count*7), oy + 10, 165, "Front", 0);
					farmHouse.setMapTileIndex(ox + 36 + i + (count*7), oy + 11, 0, "Buildings", 0);
				}
				for (int i = 0; i < 3; i++)
				{
					farmHouse.setMapTileIndex(ox + 36 + (i*2) + (count*7), oy + 10, 53, "Back", 4);
					farmHouse.setMapTileIndex(ox + 36 + (i*2+1) + (count*7), oy + 10, 54, "Back", 4);
				}
				farmHouse.setMapTileIndex(ox + 42 + (count * 7), oy + 10, 54, "Back", 4);

				for (int i = 0; i < 6; i++)
				{
					farmHouse.setMapTileIndex(ox + 36 + i + (count * 7), oy + 0, 2, "Buildings", 0);
					farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 1 + i, 99, "Buildings", 4);
				}

				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 0, 87, "Buildings", 4);
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 7, 111, "Buildings", 4);
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 8, 123, "Buildings", 4);
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 9, 135, "Buildings", 4);
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 9, 54, "Back", 4);

				for (int x = 0; x < areaToRefurbish.Width; x++)
				{
					for (int y = 0; y < areaToRefurbish.Height; y++)
					{
						//PMonitor.Log($"x {x}, y {y}", LogLevel.Debug);
						if (refurbishedMap.GetLayer("Back").Tiles[mapReader.X + x, mapReader.Y + y] != null)
						{
							farmHouse.map.GetLayer("Back").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(farmHouse.map.GetLayer("Back"), farmHouse.map.GetTileSheet(refurbishedMap.GetLayer("Back").Tiles[mapReader.X + x, mapReader.Y + y].TileSheet.Id), BlendMode.Alpha, refurbishedMap.GetLayer("Back").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);
						}
						if (refurbishedMap.GetLayer("Buildings").Tiles[mapReader.X + x, mapReader.Y + y] != null)
						{
							farmHouse.map.GetLayer("Buildings").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(farmHouse.map.GetLayer("Buildings"), farmHouse.map.GetTileSheet(refurbishedMap.GetLayer("Buildings").Tiles[mapReader.X + x, mapReader.Y + y].TileSheet.Id), BlendMode.Alpha, refurbishedMap.GetLayer("Buildings").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);

							typeof(GameLocation).GetMethod("adjustMapLightPropertiesForLamp", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(farmHouse, new object[] { refurbishedMap.GetLayer("Buildings").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Buildings" });
						}
						else
						{
							farmHouse.map.GetLayer("Buildings").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = null;
						}
						if (y < areaToRefurbish.Height - 1 && refurbishedMap.GetLayer("Front").Tiles[mapReader.X + x, mapReader.Y + y] != null)
						{
							farmHouse.map.GetLayer("Front").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(farmHouse.map.GetLayer("Front"), farmHouse.map.GetTileSheet(refurbishedMap.GetLayer("Front").Tiles[mapReader.X + x, mapReader.Y + y].TileSheet.Id), BlendMode.Alpha, refurbishedMap.GetLayer("Front").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);
							typeof(GameLocation).GetMethod("adjustMapLightPropertiesForLamp", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(farmHouse, new object[] { refurbishedMap.GetLayer("Front").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Front" });
						}
						else if (y < areaToRefurbish.Height - 1)
						{
							farmHouse.map.GetLayer("Front").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = null;
						}
						if (x == 4 && y == 4)
						{
							farmHouse.map.GetLayer("Back").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y].Properties["NoFurniture"] = new PropertyValue("T");
						}
					}
				}
			}
		}
    }
}