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
using System.Linq;

namespace MultipleSpouses
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

		public static IMonitor PMonitor;
		public static IModHelper PHelper;
        public static ModConfig config;

        public static Dictionary<string,NPC> spouses = new Dictionary<string, NPC>();
        internal static string outdoorSpouse = null;
        internal static string kitchenSpouse = null;
        internal static string bedSpouse = null;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
			PMonitor = Monitor;
			PHelper = helper;
			config = Helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;

			var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

		private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
			ResetSpouseRoles();
		}

		public static void ResetSpouseRoles()
        {
			outdoorSpouse = null;
			kitchenSpouse = null;
			bedSpouse = null;
			ResetSpouses(Game1.player);
			List<NPC> allSpouses = spouses.Values.ToList();
			PMonitor.Log("num spouses: " + allSpouses.Count);
			if(Game1.player.getSpouse() != null)
            {
				allSpouses.Add(Game1.player.getSpouse()); 
			}
			foreach(NPC spouse in allSpouses)
            {
				int maxType = 4;


				int type = Game1.random.Next(0, maxType);

				switch (type)
                {
					case 1:
						if (bedSpouse == null)
                        {
							PMonitor.Log("made bed spouse: " + spouse.Name);
							bedSpouse = spouse.Name;
						}
						break;
					case 2:
						if (kitchenSpouse == null)
                        {
							PMonitor.Log("made kitchen spouse: " + spouse.Name);
							kitchenSpouse = spouse.Name;
						}
						break;
					case 3:
						if (outdoorSpouse == null)
                        {
							PMonitor.Log("made outdoor spouse: " + spouse.Name);
							outdoorSpouse = spouse.Name;
						}
						break;
					default:
						break;
                }
			}
		}

        internal static bool SpotHasSpouse(Vector2 position, GameLocation location)
        {
			foreach(NPC spouse in ModEntry.spouses.Values)
			{
				if (spouse.currentLocation == location)
				{
					Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle((int)position.X + 1, (int)position.Y + 1, 62, 62);
					if(spouse.GetBoundingBox().Intersects(rect))
						return true;
				}
			}
			return false;
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

        internal static void ResetSpouses(Farmer f)
        {
			spouses.Clear();
			foreach (string name in f.friendshipData.Keys)
			{
				if (f.friendshipData[name].IsMarried() && f.spouse != name)
				{
					spouses.Add(name,Game1.getCharacterFromName(name));
				}
			}
			if(spouses.Count > 0)
            {
				//PMonitor.Log("got a spouse", LogLevel.Error);
            }
		}

		public static void BuildSpouseRoom(FarmHouse farmHouse, string name, int count)
        {

			NPC spouse = Game1.getCharacterFromName(name);
			string back = "Back";
			string buildings = "Buildings";
			string front = "Front";
			if (spouse != null)
			{
				Map refurbishedMap = PHelper.Content.Load<Map>("Maps\\spouseRooms", ContentSource.GameContent);
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
				else if(name == "Victor")
                {
					back = "BackSpouse";
					buildings = "BuildingsSpouse";
					front = "FrontSpouse";
					refurbishedMap = PHelper.Content.Load<Map>("../[TMX] Stardew Valley Expanded/assets/VictorsRoom.tmx", ContentSource.ModFolder);
					indexInSpouseMapSheet = 0;
				}
				else if(name == "Olivia")
                {
					back = "BackSpouse";
					buildings = "BuildingsSpouse";
					front = "FrontSpouse";
					refurbishedMap = PHelper.Content.Load<Map>("../[TMX] Stardew Valley Expanded/assets/OliviasRoom.tmx", ContentSource.ModFolder);
					indexInSpouseMapSheet = 0;
				}
				else if(name == "Sophia")
                {
					back = "BackSpouse";
					buildings = "BuildingsSpouse";
					front = "FrontSpouse";
					refurbishedMap = PHelper.Content.Load<Map>("../[TMX] Stardew Valley Expanded/assets/SophiasRoom.tmx", ContentSource.ModFolder);
					indexInSpouseMapSheet = 0;
				}

				PMonitor.Log($"Building {name}'s room", LogLevel.Debug);
				
				Microsoft.Xna.Framework.Rectangle areaToRefurbish = (farmHouse.upgradeLevel == 1) ? new Microsoft.Xna.Framework.Rectangle(36+(7*count), 1, 6, 9) : new Microsoft.Xna.Framework.Rectangle(42+(7 * count), 10, 6, 9);

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


				Point mapReader = new Point(indexInSpouseMapSheet % 5 * 6, indexInSpouseMapSheet / 5 * 9);
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
					farmHouse.setMapTileIndex(ox + 36 + i + (count*7), oy + 10, 165, "Front", 0);
					farmHouse.setMapTileIndex(ox + 36 + i + (count*7), oy + 11, 0, "Buildings", 0);
				}
				for (int i = 0; i < 3; i++)
				{
					farmHouse.setMapTileIndex(ox + 36 + (i*2) + (count*7), oy + 10, 53, "Back", untitled);
					farmHouse.setMapTileIndex(ox + 36 + (i*2+1) + (count*7), oy + 10, 54, "Back", untitled);
				}
				farmHouse.setMapTileIndex(ox + 42 + (count * 7), oy + 10, 54, "Back", 4);

				for (int i = 0; i < 6; i++)
				{
					farmHouse.setMapTileIndex(ox + 36 + i + (count * 7), oy + 0, 2, "Buildings", 0);
					farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 1 + i, 99, "Buildings", untitled);
				}

				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 0, 87, "Buildings", untitled);
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 7, 111, "Buildings", untitled);
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 8, 123, "Buildings", untitled);
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 9, 135, "Buildings", untitled);
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 9, 54, "Back", untitled);

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
                            catch(Exception ex)
                            {
								PMonitor.Log(ex.ToString());
                            }
						}
					}
				}
			}
		}
    }
}