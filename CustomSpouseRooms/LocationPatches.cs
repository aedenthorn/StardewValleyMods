using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CustomSpouseRooms
{
    public static class LocationPatches
    {
        public static IMonitor Monitor;
        public static ModConfig Config;
        public static IModHelper Helper;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, IModHelper helper, ModConfig config)
        {
            Monitor = monitor;
            Config = config;
            Helper = helper;
        }

        public static void FarmHouse_checkAction_Postfix(FarmHouse __instance, Location tileLocation)
        {
            try
            {
                if (__instance.map.GetLayer("Buildings").Tiles[tileLocation] != null)
                {
                    int tileIndex = __instance.map.GetLayer("Buildings").Tiles[tileLocation].TileIndex;
                    if (tileIndex == 2173 && Game1.player.eventsSeen.Contains(463391) && Game1.player.friendshipData.ContainsKey("Emily") && Game1.player.friendshipData["Emily"].IsMarried())
                    {
                        TemporaryAnimatedSprite t = __instance.getTemporarySpriteByID(5858585);
                        if (t != null && t is EmilysParrot)
                        {
                            (t as EmilysParrot).doAction();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(FarmHouse_checkAction_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        
        public static bool DecoratableLocation_IsFloorableOrWallpaperableTile_Prefix(DecoratableLocation __instance, int x, int y, ref bool __result)
		{
			if (!Config.EnableMod || !(__instance is FarmHouse))
				return true;
			foreach(var room in ModEntry.currentRoomData.Values)
            {
                Rectangle rect = new Rectangle(room.startPos.X + 1, room.startPos.Y + 1, (__instance as FarmHouse).GetSpouseRoomWidth(), (__instance as FarmHouse).GetSpouseRoomHeight());
				if (rect.Contains(x, y))
                {
					__result = true;
					return false;
                }
			}
			return true;
		}

		public static void FarmHouse_updateFarmLayout_Prefix(FarmHouse __instance, ref bool ___displayingSpouseRoom)
		{
			if (!Config.EnableMod)
				return;
			var allSpouses = Misc.GetSpouses(__instance.owner, -1).Keys.ToList();
			if (allSpouses.Count == 0)
				return;
			___displayingSpouseRoom = false;
		}
		
        public static bool FarmHouse_loadSpouseRoom_Prefix(FarmHouse __instance, HashSet<string> ____appliedMapOverrides)
        {
			if (!Config.EnableMod)
				return true;
			try
            {
				Monitor.Log("Loading spouse rooms, clearing current room data");
				ModEntry.currentRoomData.Clear();
				var allSpouses = Misc.GetSpouses(__instance.owner, -1).Keys.ToList();
				if (allSpouses.Count == 0)
					return true;

				if (ModEntry.customRoomData.Count == 0 && allSpouses.Count == 1)// single spouse, no customizations
				{
					Monitor.Log("Single uncustomized spouse room, letting vanilla game take over");
					return true;
				}

				GetFarmHouseSpouseRooms(__instance, allSpouses, out List<string> orderedSpouses, out List<string> customSpouses);

				__instance.reloadMap();

				if (____appliedMapOverrides.Contains("spouse_room"))
				{
					____appliedMapOverrides.Remove("spouse_room");
				}
				__instance.map.Properties.Remove("DayTiles");
				__instance.map.Properties.Remove("NightTiles");

				foreach(string spouse in customSpouses)
                {
					SpouseRoomData srd = ModEntry.customRoomData[spouse];
					MakeSpouseRoom(__instance, ____appliedMapOverrides, srd);
				}

				int xOffset = 7;

				for (int i = 0; i < orderedSpouses.Count; i++)
                {
					string spouse = orderedSpouses[i];

					SpouseRoomData srd = null;
					if (ModEntry.customRoomData.ContainsKey(spouse))
                    {
						srd = new SpouseRoomData(ModEntry.customRoomData[spouse]);
                    }

					Point corner = __instance.GetSpouseRoomCorner() + new Point(xOffset * i, 0);

					Point spouseOffset;
					int indexInSpouseMapSheet = -1;

					if (srd != null && (srd.upgradeLevel < 0 || srd.upgradeLevel == __instance.upgradeLevel))
					{
						spouseOffset = srd.spousePosOffset;
						indexInSpouseMapSheet = srd.templateIndex;
					}
					else
                    {
						spouseOffset = new Point(4, 5);
					}

					var shellStart = corner - new Point(1, 1);

					if (i == 0)
						__instance.spouseRoomSpot = shellStart;

					if(srd == null)
                    {
						srd = new SpouseRoomData()
						{
							name = spouse,
							spousePosOffset = spouseOffset,
							templateIndex = indexInSpouseMapSheet
						};

					}

					srd.shellType = i < orderedSpouses.Count - 1 ? "custom_spouse_room_open_right" : "custom_spouse_room_closed_right";
					if (i == 0 && __instance.upgradeLevel > 1)
					{
						srd.shellType += "_2";
					}
					Monitor.Log($"Using shell {srd.shellType} for {srd.name}");

					srd.startPos = shellStart;

					MakeSpouseRoom(__instance, ____appliedMapOverrides, srd, i == 0);
                }
				return false;
			}
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(FarmHouse_loadSpouseRoom_Prefix)}:\n{ex}", LogLevel.Error);
            }
			return true;
        }
		
        public static void DecoratableLocation_MakeMapModifications_Postfix(DecoratableLocation __instance, HashSet<string> ____appliedMapOverrides)
        {
			if (!Config.EnableMod || __instance is not IslandFarmHouse)
				return;
			try
            {
				Monitor.Log("Loading spouse rooms, clearing current room data");
				ModEntry.currentIslandRoomData.Clear();
				var allSpouses = Misc.GetSpouses(Game1.player, -1).Keys.ToList();
				if (allSpouses.Count == 0)
					return;

				GetIslandFarmHouseSpouseRooms(__instance as IslandFarmHouse, allSpouses, out List<string> customSpouses);

				__instance.reloadMap();

				if (____appliedMapOverrides.Contains("spouse_room"))
				{
					____appliedMapOverrides.Remove("spouse_room");
				}
				__instance.map.Properties.Remove("DayTiles");
				__instance.map.Properties.Remove("NightTiles");

				foreach(string spouse in customSpouses)
                {
					SpouseRoomData srd = ModEntry.customRoomData[spouse];
					MakeSpouseRoom(__instance, ____appliedMapOverrides, srd);
				}

			}
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(DecoratableLocation_MakeMapModifications_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        private static void GetFarmHouseSpouseRooms(FarmHouse fh, List<string> orderableSpouses, out List<string> orderedSpouses, out List<string> customSpouses)
		{
			Monitor.Log($"Getting {orderableSpouses.Count} spouse rooms");
			customSpouses = new List<string>();
			for (int i = orderableSpouses.Count - 1; i >= 0; i--)
			{
				if (ModEntry.customRoomData.TryGetValue(orderableSpouses[i], out SpouseRoomData srd) && !srd.islandFarmHouse && (srd.upgradeLevel == fh.upgradeLevel || srd.upgradeLevel < 0) && srd.startPos.X > -1)
				{
					Monitor.Log($"{orderableSpouses[i]} has custom spouse room");
					customSpouses.Add(orderableSpouses[i]);
					orderableSpouses.RemoveAt(i);
				}
			}

			orderedSpouses = new List<string>();
			string[] roomOrder = Config.SpouseRoomOrder.Split(',');
			foreach (string str in roomOrder)
			{
				string s = str.Trim();
				if (orderableSpouses.Contains(s))
				{
					Monitor.Log($"{s} has custom room order");
					orderedSpouses.Add(s);
					orderableSpouses.Remove(s);
				}
			}
			foreach (string str in orderableSpouses)
			{
				Monitor.Log($"{str} has no customization");
				orderedSpouses.Add(str);
				Config.SpouseRoomOrder += (Config.SpouseRoomOrder.Trim().Length > 0 ? "," : "") + str;
			}
			Helper.WriteConfig(Config);
		}
		
        private static void GetIslandFarmHouseSpouseRooms(IslandFarmHouse fh, List<string> orderableSpouses, out List<string> customSpouses)
		{
			Monitor.Log($"Getting {orderableSpouses.Count} island spouse rooms");
			customSpouses = new List<string>();
			for (int i = orderableSpouses.Count - 1; i >= 0; i--)
			{
				if (ModEntry.customRoomData.TryGetValue(orderableSpouses[i], out SpouseRoomData srd) && srd.islandFarmHouse && srd.startPos.X > -1)
				{
					Monitor.Log($"{orderableSpouses[i]} has custom island spouse room at {srd.startPos}");
					customSpouses.Add(orderableSpouses[i]);
				}
			}
		}

        private static void MakeSpouseRoom(DecoratableLocation location, HashSet<string> appliedMapOverrides, SpouseRoomData srd, bool first = false)
        {

			Monitor.Log($"Loading spouse room for {srd.name}. shellStart {srd.startPos}, spouse offset {srd.spousePosOffset}. Type: {srd.shellType}");

			var corner = srd.startPos + new Point(1, 1);
			var spouse = srd.name;
			var shellPath = srd.shellType;
			var indexInSpouseMapSheet = srd.templateIndex;
			var spouseSpot = srd.startPos + srd.spousePosOffset;

            Rectangle shellAreaToRefurbish = new Rectangle(corner.X - 1, corner.Y - 1, 8, 12);
			Misc.ExtendMap(location, shellAreaToRefurbish.X + shellAreaToRefurbish.Width, shellAreaToRefurbish.Y + shellAreaToRefurbish.Height);

			// load shell

			if (appliedMapOverrides.Contains("spouse_room_" + spouse + "_shell"))
			{
				appliedMapOverrides.Remove("spouse_room_" + spouse + "_shell");
			}

			location.ApplyMapOverride(shellPath, "spouse_room_" + spouse + "_shell", new Rectangle?(new Rectangle(0, 0, shellAreaToRefurbish.Width, shellAreaToRefurbish.Height)), new Rectangle?(shellAreaToRefurbish));

			for (int x = 0; x < shellAreaToRefurbish.Width; x++)
			{
				for (int y = 0; y < shellAreaToRefurbish.Height; y++)
				{
					if (location.map.GetLayer("Back").Tiles[shellAreaToRefurbish.X + x, shellAreaToRefurbish.Y + y] != null)
					{
						location.map.GetLayer("Back").Tiles[shellAreaToRefurbish.X + x, shellAreaToRefurbish.Y + y].Properties["FloorID"] = "spouse_hall_" + (Config.DecorateHallsIndividually ? spouse : "floor");
					}
				}
			}

			Dictionary<string, string> room_data = Game1.content.Load<Dictionary<string, string>>("Data\\SpouseRooms");
			string map_path = "spouseRooms";
			if (indexInSpouseMapSheet == -1 && room_data != null && srd.templateName != null && room_data.ContainsKey(srd.templateName))
			{
				try
				{
					string[] array = room_data[srd.templateName].Split('/', StringSplitOptions.None);
					map_path = array[0];
					indexInSpouseMapSheet = int.Parse(array[1]);
					Monitor.Log($"Got Data\\SpouseRooms room for template {srd.templateName}: room {map_path}, index {indexInSpouseMapSheet}");
				}
				catch (Exception)
				{
				}
			}
			if (indexInSpouseMapSheet == -1 && room_data != null && room_data.ContainsKey(spouse))
			{
				try
				{
					string[] array = room_data[spouse].Split('/', StringSplitOptions.None);
					map_path = array[0];
					indexInSpouseMapSheet = int.Parse(array[1]);
					Monitor.Log($"Got Data\\SpouseRooms room for spouse {spouse}: room {map_path}, index {indexInSpouseMapSheet}");
				}
				catch (Exception)
				{
				}
			}
			if (indexInSpouseMapSheet == -1)
			{
				if (srd.templateName != null && ModEntry.roomIndexes.ContainsKey(srd.templateName))
				{
					indexInSpouseMapSheet = ModEntry.roomIndexes[srd.templateName];
					Monitor.Log($"Got vanilla index for template {srd.templateName}: {indexInSpouseMapSheet}");
				}
				else if (ModEntry.roomIndexes.ContainsKey(spouse))
				{
					indexInSpouseMapSheet = ModEntry.roomIndexes[spouse];
					Monitor.Log($"Got vanilla index for spouse {spouse}: {indexInSpouseMapSheet}");
				}
				else
				{
					Monitor.Log($"Could not find spouse room map for {spouse}", LogLevel.Debug);
					return;
				}
			}
			int width = 6;
			int height = 9;

            Rectangle areaToRefurbish = new Rectangle(corner.X, corner.Y, width, height);
			Map refurbishedMap = Game1.game1.xTileContent.Load<Map>("Maps\\" + map_path);
			int columns = refurbishedMap.Layers[0].LayerWidth / width;
			int num2 = refurbishedMap.Layers[0].LayerHeight / height;
			Point mapReader = new Point(indexInSpouseMapSheet % columns * width, indexInSpouseMapSheet / columns * height);
			List<KeyValuePair<Point, Tile>> bottom_row_tiles = new List<KeyValuePair<Point, Tile>>();
			Layer front_layer = location.map.GetLayer("Front");
			for (int x = areaToRefurbish.Left; x < areaToRefurbish.Right; x++)
			{
				Point point = new Point(x, areaToRefurbish.Bottom - 1);
				Tile tile = front_layer?.Tiles[point.X, point.Y];
				if (tile != null)
				{
					bottom_row_tiles.Add(new KeyValuePair<Point, Tile>(point, tile));
				}
			}

			if (appliedMapOverrides.Contains("spouse_room_" + spouse))
			{
				appliedMapOverrides.Remove("spouse_room_" + spouse);
			}

			location.ApplyMapOverride(map_path, "spouse_room_" + spouse, new Rectangle?(new Rectangle(mapReader.X, mapReader.Y, areaToRefurbish.Width, areaToRefurbish.Height)), new Rectangle?(areaToRefurbish));
			for (int x = 0; x < areaToRefurbish.Width; x++)
			{
				for (int y = 0; y < areaToRefurbish.Height; y++)
				{
					if (refurbishedMap.GetLayer("Buildings")?.Tiles[mapReader.X + x, mapReader.Y + y] != null)
					{
						Helper.Reflection.GetMethod(location, "adjustMapLightPropertiesForLamp").Invoke(refurbishedMap.GetLayer("Buildings").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Buildings");
					}
					if (y < areaToRefurbish.Height - 1 && refurbishedMap.GetLayer("Front")?.Tiles[mapReader.X + x, mapReader.Y + y] != null)
					{
						Helper.Reflection.GetMethod(location, "adjustMapLightPropertiesForLamp").Invoke(refurbishedMap.GetLayer("Front").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Front");
					}
					/*
					if (fh.map.GetLayer("Back").Tiles[corner.X + x, corner.Y + y] != null)
					{
						fh.setTileProperty(corner.X + x, corner.Y + y, "Back", "FloorID", $"spouse_room_{spouse}");
					}
					*/
				}
			}
			location.ReadWallpaperAndFloorTileData();
			if(location.map.GetLayer("Paths") != null)
            {
				bool spot_found = false;
				for (int x3 = areaToRefurbish.Left; x3 < areaToRefurbish.Right; x3++)
				{
					for (int y2 = areaToRefurbish.Top; y2 < areaToRefurbish.Bottom; y2++)
					{
						if (location.getTileIndexAt(new Point(x3, y2), "Paths") == 7)
						{
							spot_found = true;
							if (first && location is FarmHouse)
                            {

								(location as FarmHouse).spouseRoomSpot = new Point(x3, y2);
                            }
							spouseSpot = new Point(x3, y2);
							srd.spousePosOffset = spouseSpot - srd.startPos;
							break;
						}
					}
					if (spot_found)
					{
						break;
					}
				}
			}
			location.setTileProperty(spouseSpot.X, spouseSpot.Y, "Back", "NoFurniture", "T");
			foreach (KeyValuePair<Point, Tile> kvp in bottom_row_tiles)
			{
				front_layer.Tiles[kvp.Key.X, kvp.Key.Y] = kvp.Value;
			}
			if(location is FarmHouse)
				ModEntry.currentRoomData[srd.name] = srd;
			else
				ModEntry.currentIslandRoomData[srd.name] = srd;
		}

        public static void FarmHouse_resetLocalState_Postfix(ref FarmHouse __instance)
        {
            try
            {
				if (!Config.EnableMod)
					return;

                Farmer f = __instance.owner;

                if (f == null)
                {
                    return;
                }

				foreach(var kvp in ModEntry.currentRoomData)
                {
					CheckSpouseThing(__instance, kvp.Value);
				}


				if (Misc.GetSpouses(f,0).ContainsKey("Sebastian") && Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
                {
                    if (Game1.random.NextDouble() < 0.1 && Game1.timeOfDay > 610)
                    {
                        DelayedAction.playSoundAfterDelay("croak", 1000, null, -1);
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(FarmHouse_resetLocalState_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void CheckSpouseThing(FarmHouse fh, SpouseRoomData srd)
        {
			Monitor.Log($"Checking spouse thing for {srd.name}");
			if (srd.name == "Emily" && (srd.templateName == "Emily" || srd.templateName == null || srd.templateName == "") && Game1.player.eventsSeen.Contains(463391))
			{
				fh.temporarySprites.RemoveAll((s) => s is EmilysParrot);

				Vector2 spot = Utility.PointToVector2(srd.startPos + new Point(4, 2)) * 64;
				spot += new Vector2(16, 32);
				ModEntry.PMonitor.Log($"Building Emily's parrot at {spot}");
				fh.temporarySprites.Add(new EmilysParrot(spot));
			}
			else if (srd.name == "Sebastian" && (srd.templateName == "Sebastian" || srd.templateName == null || srd.templateName == "") && Game1.netWorldState.Value.hasWorldStateID("sebastianFrogReal"))
			{
				Vector2 spot = Utility.PointToVector2(srd.startPos + new Point(2, 7));
				Monitor.Log($"building Sebastian's terrarium at {spot}");
				if(spot.X < 0 || spot.Y - 1 < 0 || spot.X + 2 >= fh.Map.GetLayer("Front").LayerWidth || spot.Y -1 >= fh.Map.GetLayer("Front").LayerHeight)
                {
					Monitor.Log("Spot is outside of map!");
					return;
				}
				fh.removeTile((int)spot.X, (int)spot.Y - 1, "Front");
				fh.removeTile((int)spot.X + 1, (int)spot.Y - 1, "Front");
				fh.removeTile((int)spot.X + 2, (int)spot.Y - 1, "Front");
				fh.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.mouseCursors,
					sourceRect = new Rectangle(641, 1534, 48, 37),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(641f, 1534f),
					interval = 5000f,
					totalNumberOfLoops = 9999,
					position = spot * 64f + new Vector2(0f, -5f) * 4f,
					scale = 4f,
					layerDepth = (spot.Y + 2f + 0.1f) * 64f / 10000f
				});
				if (Game1.random.NextDouble() < 0.85)
				{
					Texture2D crittersText2 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
					fh.TemporarySprites.Add(new SebsFrogs
					{
						texture = crittersText2,
						sourceRect = new Rectangle(64, 224, 16, 16),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(64f, 224f),
						interval = 100f,
						totalNumberOfLoops = 9999,
						position = spot * 64f + new Vector2((float)((Game1.random.NextDouble() < 0.5) ? 22 : 25), (float)((Game1.random.NextDouble() < 0.5) ? 2 : 1)) * 4f,
						scale = 4f,
						flipped = (Game1.random.NextDouble() < 0.5),
						layerDepth = (spot.Y + 2f + 0.11f) * 64f / 10000f,
						Parent = fh
					});
				}
				if (!Game1.player.activeDialogueEvents.ContainsKey("sebastianFrog2") && Game1.random.NextDouble() < 0.5)
				{
					Texture2D crittersText3 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
					fh.TemporarySprites.Add(new SebsFrogs
					{
						texture = crittersText3,
						sourceRect = new Rectangle(64, 240, 16, 16),
						animationLength = 1,
						sourceRectStartingPos = new Vector2(64f, 240f),
						interval = 150f,
						totalNumberOfLoops = 9999,
						position = spot * 64f + new Vector2(8f, 3f) * 4f,
						scale = 4f,
						layerDepth = (spot.Y + 2f + 0.11f) * 64f / 10000f,
						flipped = (Game1.random.NextDouble() < 0.5),
						pingPong = false,
						Parent = fh
					});
				}
			}
		}
    }
}