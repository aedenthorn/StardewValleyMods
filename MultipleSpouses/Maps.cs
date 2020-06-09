using Harmony;
using static Harmony.AccessTools;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Reflection;
using xTile.Dimensions;
using System.IO;
using StardewValley.BellsAndWhistles;
using xTile.Tiles;
using System.Linq;
using xTile;
using xTile.Layers;
using xTile.ObjectModel;

namespace MultipleSpouses
{
	public static class Maps
	{
		private static IMonitor Monitor;
        private static ModConfig config;
        private static IModHelper PHelper;
        public static Dictionary<string, Map> tmxSpouseRooms = new Dictionary<string, Map>();

		// call this method from your Entry class
		public static void Initialize(IMonitor monitor)
		{
			Monitor = monitor;
			config = ModEntry.config;
			PHelper = ModEntry.PHelper;
		}

		public static void BuildSpouseRooms(FarmHouse farmHouse)
		{
			try
			{
				Farmer f = farmHouse.owner;
				if (f == null)
					return;

				ModEntry.LoadTMXSpouseRooms();

				if (ModEntry.spouses.ContainsKey("Emily") && f.spouse != "Emily" && Game1.player.eventsSeen.Contains(463391))
				{
					int offset = (ModEntry.spouses.Keys.ToList().IndexOf("Emily") + 1) * 7 * 64;
					Vector2 parrotSpot = new Vector2(2064f + offset, 160f);
					int upgradeLevel = farmHouse.upgradeLevel;
					if (upgradeLevel - 2 <= 1)
					{
						parrotSpot = new Vector2(2448f + offset, 736f);
					}
					farmHouse.temporarySprites.Add(new EmilysParrot(parrotSpot));
				}

				List<NPC> mySpouses = new List<NPC>();

				foreach (NPC spouse in ModEntry.spouses.Values)
				{
					string name = spouse.Name;
					mySpouses.Add(spouse);
				}

				if (farmHouse.upgradeLevel > 3 || mySpouses.Count == 0)
				{
					return;
				}

				int untitled = 0;
				List<string> sheets = new List<string>();
				for (int i = 0; i < farmHouse.map.TileSheets.Count; i++)
				{
					sheets.Add(farmHouse.map.TileSheets[i].Id);
				}
				untitled = sheets.IndexOf("untitled tile sheet");


				int ox = 0;
				int oy = 0;
				if (farmHouse.upgradeLevel > 1)
				{
					ox = 6;
					oy = 9;
				}

				for (int i = 0; i < 7; i++)
				{
					farmHouse.setMapTileIndex(ox + 29 + i, oy + 11, 0, "Buildings", 0);
					farmHouse.removeTile(ox + 29 + i, oy + 9, "Front");
					farmHouse.removeTile(ox + 29 + i, oy + 10, "Buildings");
					farmHouse.setMapTileIndex(ox + 28 + i, oy + 10, 165, "Front", 0);
					farmHouse.removeTile(ox + 29 + i, oy + 10, "Back");
				}
				for (int i = 0; i < 8; i++)
				{
					farmHouse.setMapTileIndex(ox + 28 + i, oy + 10, 165, "Front", 0);
				}
				for (int i = 0; i < 10; i++)
				{
					farmHouse.removeTile(ox + 35, oy + 0 + i, "Buildings");
					farmHouse.removeTile(ox + 35, oy + 0 + i, "Front");
				}
				for (int i = 0; i < 7; i++)
				{
					// horiz hall
					farmHouse.setMapTileIndex(ox + 29 + i, oy + 10, (i % 2 == 0 ? ModEntry.config.HallTileOdd: ModEntry.config.HallTileEven), "Back", 0);
				}


				for (int i = 0; i < 7; i++)
				{
					//farmHouse.removeTile(ox + 28, oy + 4 + i, "Back");
					//farmHouse.setMapTileIndex(ox + 28, oy + 4 + i, (i % 2 == 0 ? ModEntry.config.HallTileOdd : ModEntry.config.HallTileEven), "Back", 0);
				}


				farmHouse.removeTile(ox + 28, oy + 9, "Front");
				farmHouse.removeTile(ox + 28, oy + 10, "Buildings");
				
				if(farmHouse.upgradeLevel > 1) 
					farmHouse.setMapTileIndex(ox + 28, oy + 10, 163, "Front", 0);
				farmHouse.removeTile(ox + 35, oy + 0, "Front");
				farmHouse.removeTile(ox + 35, oy + 0, "Buildings");



				int count = 0;

				if (f.isMarried() && tmxSpouseRooms.ContainsKey(f.spouse))
				{
					BuildOneSpouseRoom(farmHouse, f.spouse, -1);
				}

				// remove and rebuild spouse rooms
				for (int j = 0; j < mySpouses.Count; j++)
				{
					farmHouse.removeTile(ox + 35 + (7 * count), oy + 0, "Buildings");
					for (int i = 0; i < 10; i++)
					{
						farmHouse.removeTile(ox + 35 + (7 * count), oy + 1 + i, "Buildings");
					}
					BuildOneSpouseRoom(farmHouse, mySpouses[j].Name, count++);
				}

				// far wall
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 0, 11, "Buildings", 0);
				for (int i = 0; i < 10; i++)
				{
					farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 1 + i, 68, "Buildings", 0);
				}
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 10, 130, "Front", 0);
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(BuildSpouseRooms)}:\n{ex}", LogLevel.Error);
			}

		}

		public static void BuildOneSpouseRoom(FarmHouse farmHouse, string name, int count)
		{

			NPC spouse = Game1.getCharacterFromName(name);
			string back = "Back";
			string buildings = "Buildings";
			string front = "Front";
			if (spouse != null || name == "")
			{
				Map refurbishedMap;
				if (name == "")
				{
					refurbishedMap = PHelper.Content.Load<Map>("Maps\\" + farmHouse.Name + ((farmHouse.upgradeLevel == 0) ? "" : ((farmHouse.upgradeLevel == 3) ? "2" : string.Concat(farmHouse.upgradeLevel))) + "_marriage", ContentSource.GameContent);
				}
				else
				{
					refurbishedMap = PHelper.Content.Load<Map>("Maps\\spouseRooms", ContentSource.GameContent);
				}
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
				else if (tmxSpouseRooms.ContainsKey(name))
				{
					back = "BackSpouse";
					buildings = "BuildingsSpouse";
					front = "FrontSpouse";

					refurbishedMap = tmxSpouseRooms[name];
					if (refurbishedMap == null)
					{
						ModEntry.PMonitor.Log($"Couldn't load TMX spouse room for spouse {name}", LogLevel.Error);
						return;
					}
					if (refurbishedMap.GetLayer(back) == null)
					{
						back = "Back";
						buildings = "Buildings";
						front = "Front";
					}
					if (refurbishedMap.GetLayer(back) == null)
					{
						ModEntry.PMonitor.Log($"Couldn't load TMX spouse room for spouse {name}", LogLevel.Error);
						return;
					}

					indexInSpouseMapSheet = 0;
				}


				Monitor.Log($"Building {name}'s room", LogLevel.Debug);

				Microsoft.Xna.Framework.Rectangle areaToRefurbish = (farmHouse.upgradeLevel == 1) ? new Microsoft.Xna.Framework.Rectangle(36 + (7 * count), 1, 6, 9) : new Microsoft.Xna.Framework.Rectangle(42 + (7 * count), 10, 6, 9);

				List<Layer> layers = FieldRefAccess<Map, List<Layer>>(farmHouse.map, "m_layers");
				for (int i = 0; i < layers.Count; i++)
				{
					Tile[,] tiles = FieldRefAccess<Layer, Tile[,]>(layers[i], "m_tiles");
					Size size = FieldRefAccess<Layer, Size>(layers[i], "m_layerSize");
					if (size.Width >= areaToRefurbish.X + 7)
						continue;
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


				Point mapReader;
				if (name == "")
				{
					mapReader = new Point(areaToRefurbish.X, areaToRefurbish.Y);
				}
				else
				{
					mapReader = new Point(indexInSpouseMapSheet % 5 * 6, indexInSpouseMapSheet / 5 * 9);
				}
				farmHouse.map.Properties.Remove("DayTiles");
				farmHouse.map.Properties.Remove("NightTiles");


				int untitled = 0;
				for (int i = 0; i < farmHouse.map.TileSheets.Count; i++)
				{
					if (farmHouse.map.TileSheets[i].Id == "untitled tile sheet")
						untitled = i;
				}


				int ox = 0;
				int oy = 0;
				if (farmHouse.upgradeLevel > 1)
				{
					ox = 6;
					oy = 9;
				}

				for (int i = 0; i < 7; i++)
				{
					// bottom wall
					farmHouse.setMapTileIndex(ox + 36 + i + (count * 7), oy + 10, 165, "Front", 0);
					farmHouse.setMapTileIndex(ox + 36 + i + (count * 7), oy + 11, 0, "Buildings", 0);

					// horiz hall
					farmHouse.setMapTileIndex(ox + 36 + i + (count * 7), oy + 10, (i % 2 == 0 ? ModEntry.config.HallTileOdd : ModEntry.config.HallTileEven), "Back", 0);

					if(count > -1)
                    {
						// vert hall
						farmHouse.removeTile(ox + 35 + (7 * count), oy + 4 + i, "Back");
						farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 4 + i, (i % 2 == 0 ? config.HallTileOdd : config.HallTileEven), "Back", 0);
					}
				}

				for (int i = 0; i < 6; i++)
				{
					// top wall
					farmHouse.setMapTileIndex(ox + 36 + i + (count * 7), oy + 0, 2, "Buildings", 0);
				}

				// vert wall
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 0, 87, "Buildings", untitled);
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 1, 99, "Buildings", untitled);
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 2, 111, "Buildings", untitled);
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 3, 123, "Buildings", untitled);
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 4, 135, "Buildings", untitled);


				for (int x = 0; x < areaToRefurbish.Width; x++)
				{
					for (int y = 0; y < areaToRefurbish.Height; y++)
					{
						//PMonitor.Log($"x {x}, y {y}", LogLevel.Debug);
						if (refurbishedMap.GetLayer(back).Tiles[mapReader.X + x, mapReader.Y + y] != null)
						{
							farmHouse.map.GetLayer("Back").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(farmHouse.map.GetLayer("Back"), farmHouse.map.GetTileSheet(refurbishedMap.GetLayer(back).Tiles[mapReader.X + x, mapReader.Y + y].TileSheet.Id), BlendMode.Alpha, refurbishedMap.GetLayer(back).Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);
						}
						if (refurbishedMap.GetLayer(buildings).Tiles[mapReader.X + x, mapReader.Y + y] != null)
						{
							farmHouse.map.GetLayer("Buildings").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(farmHouse.map.GetLayer("Buildings"), farmHouse.map.GetTileSheet(refurbishedMap.GetLayer(buildings).Tiles[mapReader.X + x, mapReader.Y + y].TileSheet.Id), BlendMode.Alpha, refurbishedMap.GetLayer(buildings).Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);

							typeof(GameLocation).GetMethod("adjustMapLightPropertiesForLamp", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(farmHouse, new object[] { refurbishedMap.GetLayer(buildings).Tiles[mapReader.X + x, mapReader.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Buildings" });
						}
						else
						{
							farmHouse.map.GetLayer("Buildings").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = null;
						}
						if (y < areaToRefurbish.Height - 1 && refurbishedMap.GetLayer(front).Tiles[mapReader.X + x, mapReader.Y + y] != null)
						{
							farmHouse.map.GetLayer("Front").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(farmHouse.map.GetLayer("Front"), farmHouse.map.GetTileSheet(refurbishedMap.GetLayer(front).Tiles[mapReader.X + x, mapReader.Y + y].TileSheet.Id), BlendMode.Alpha, refurbishedMap.GetLayer(front).Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);
							typeof(GameLocation).GetMethod("adjustMapLightPropertiesForLamp", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(farmHouse, new object[] { refurbishedMap.GetLayer(front).Tiles[mapReader.X + x, mapReader.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Front" });
						}
						else if (y < areaToRefurbish.Height - 1)
						{
							farmHouse.map.GetLayer("Front").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = null;
						}
						if (x == 4 && y == 4)
						{
							try
							{
								farmHouse.map.GetLayer("Back").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y].Properties["NoFurniture"] = new PropertyValue("T");
							}
							catch (Exception ex)
							{
								Monitor.Log(ex.ToString());
							}
						}
					}
				}
			}
		}

		public static void ReplaceBed()
		{
			try
			{
				FarmHouse fh = Utility.getHomeOfFarmer(Game1.player);
				if (fh == null || fh.map == null)
					return;

				// bed

				FarmHouse farmHouse = fh;

				int untitled = 0;
				List<string> sheets = new List<string>();
				for (int i = 0; i < farmHouse.map.TileSheets.Count; i++)
				{
					sheets.Add(farmHouse.map.TileSheets[i].Id);
				}
				untitled = sheets.IndexOf("untitled tile sheet");


				int ox = 0;
				int oy = 0;
				if (farmHouse.upgradeLevel > 1)
				{
					ox = 6;
					oy = 9;
				}

				int bedWidth = ModEntry.GetBedWidth(farmHouse);
				int width = bedWidth - 1;
				int start = 21 - (farmHouse.upgradeLevel > 1 ? (bedWidth / 2) - 1 : 0);

				List<int> backIndexes = new List<int>();
				List<int> frontIndexes = new List<int>();
				List<int> buildIndexes = new List<int>();
				List<int> backSheets = new List<int>();
				List<int> frontSheets = new List<int>();
				List<int> buildSheets = new List<int>();

				for (int i = 0; i < 12; i++)
				{
					backIndexes.Add(farmHouse.getTileIndexAt(ox + 21 + (i % 3), oy + 2 + (i / 3), "Back"));
					backSheets.Add(sheets.IndexOf(farmHouse.getTileSheetIDAt(ox + 21 + (i % 3), oy + 2 + (i / 3), "Back")));
					frontIndexes.Add(farmHouse.getTileIndexAt(ox + 21 + (i % 3), oy + 2 + (i / 3), "Front"));
					frontSheets.Add(sheets.IndexOf(farmHouse.getTileSheetIDAt(ox + 21 + (i % 3), oy + 2 + (i / 3), "Front")));
					buildIndexes.Add(farmHouse.getTileIndexAt(ox + 21 + (i % 3), oy + 2 + (i / 3), "Buildings"));
					buildSheets.Add(sheets.IndexOf(farmHouse.getTileSheetIDAt(ox + 21 + (i % 3), oy + 2 + (i / 3), "Buildings")));
				}


				setupTile(0, 2, 0, 0, farmHouse, start, ox, oy, frontIndexes, frontSheets, 1);
				setupTile(0, 3, 0, 1, farmHouse, start, ox, oy, frontIndexes, frontSheets, 0);
				setupTile(0, 3, 0, 1, farmHouse, start, ox, oy, buildIndexes, buildSheets, 2);
				setupTile(0, 4, 0, 2, farmHouse, start, ox, oy, frontIndexes, frontSheets, 0);
				setupTile(0, 5, 0, 3, farmHouse, start, ox, oy, buildIndexes, buildSheets, 1);

				farmHouse.removeTile(ox + start, oy + 3, "Buildings");
				for (int i = 1; i < width; i++)
				{
					farmHouse.removeTile(ox + start + i, oy + 3, "Buildings");

					setupTile(i, 2, 1, 0, farmHouse, start, ox, oy, frontIndexes, frontSheets, 1);
					setupTile(i, 3, 1, 1, farmHouse, start, ox, oy, frontIndexes, frontSheets, 0);
					setupTile(i, 3, 1, 1, farmHouse, start, ox, oy, buildIndexes, buildSheets, 2);
					setupTile(i, 4, 1, 2, farmHouse, start, ox, oy, frontIndexes, frontSheets, 0);
					setupTile(i, 5, 1, 3, farmHouse, start, ox, oy, buildIndexes, buildSheets, 1);
				}
				farmHouse.removeTile(ox + start + width, oy + 3, "Buildings");

				setupTile(width, 2, 2, 0, farmHouse, start, ox, oy, frontIndexes, frontSheets, 1);
				setupTile(width, 3, 2, 1, farmHouse, start, ox, oy, frontIndexes, frontSheets, 0);
				setupTile(width, 3, 2, 1, farmHouse, start, ox, oy, buildIndexes, buildSheets, 2);
				setupTile(width, 4, 2, 2, farmHouse, start, ox, oy, frontIndexes, frontSheets, 0);
				setupTile(width, 5, 2, 3, farmHouse, start, ox, oy, buildIndexes, buildSheets, 1);


				farmHouse.removeTile(ox + 21, oy + 2, "Front");
				farmHouse.removeTile(ox + 22, oy + 2, "Front");
				farmHouse.removeTile(ox + 23, oy + 2, "Front");


			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(ReplaceBed)}:\n{ex}", LogLevel.Error);
			}

		}


		private static void setupTile(int v1, int v2, int x, int y, FarmHouse farmHouse, int start, int ox, int oy, List<int> indexes, List<int> sheets, int sheetNo)
        {
			string[] sheetNames = {
				"Front",
				"Buildings",
				"Back"
			};
			int idx = y * 3 + x;
			try
			{
				farmHouse.removeTile(ox + start + v1, oy + v2, sheetNames[sheetNo]);
				farmHouse.setMapTileIndex(ox + start + v1, oy + v2, indexes[idx], sheetNames[sheetNo], sheets[idx]);
			}
            catch(Exception ex)
            {
				Monitor.Log("v1: "+v1);
				Monitor.Log("v2: "+v2);
				Monitor.Log("x: "+x);
				Monitor.Log("y: "+y);
				Monitor.Log("sheet: "+sheetNo);
				Monitor.Log("index: "+ idx);
				Monitor.Log("Exception: "+ex, LogLevel.Error);
            }
		}
	}
}