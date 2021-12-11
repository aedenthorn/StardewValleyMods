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
        
        public static bool FarmHouse_loadSpouseRoom_Prefix(FarmHouse __instance, HashSet<string> ____appliedMapOverrides)
        {
			if (!Config.EnableMod)
				return true;
			try
            {
				ModEntry.currentRoomData.Clear();
				var allSpouses = Misc.GetSpouses(__instance.owner, -1).Keys.ToList();
				if (allSpouses.Count == 0)
					return true;

				if (ModEntry.customRoomData.Count == 0 && allSpouses.Count == 1) // single spouse, no customizations
					return true;

				GetSpouseRooms(__instance, allSpouses, out List<string> orderedSpouses, out List<string> customSpouses);

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
						srd = ModEntry.customRoomData[spouse];

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

        private static void GetSpouseRooms(FarmHouse fh, List<string> orderableSpouses, out List<string> orderedSpouses, out List<string> customSpouses)
		{
			customSpouses = new List<string>();
			for (int i = orderableSpouses.Count - 1; i >= 0; i--)
			{
				if (ModEntry.customRoomData.ContainsKey(orderableSpouses[i]) &&
					(ModEntry.customRoomData[orderableSpouses[i]].upgradeLevel == fh.upgradeLevel || ModEntry.customRoomData[orderableSpouses[i]].upgradeLevel < 0) &&
					ModEntry.customRoomData[orderableSpouses[i]].startPos.X > -1
				)
				{
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
					orderedSpouses.Add(s);
					orderableSpouses.Remove(s);
				}
			}
			foreach (string str in orderableSpouses)
			{
				orderedSpouses.Add(str);
				Config.SpouseRoomOrder += (Config.SpouseRoomOrder.Trim().Length > 0 ? "," : "") + str;
			}
			Helper.WriteConfig(Config);
		}

        private static void MakeSpouseRoom(FarmHouse fh, HashSet<string> appliedMapOverrides, SpouseRoomData srd, bool first = false)
        {


			Monitor.Log($"Loading spouse room for {srd.name}. shellStart {srd.startPos}, spouse offset {srd.spousePosOffset}. Type: {srd.shellType}");

			var corner = srd.startPos + new Point(1, 1);
			var spouse = srd.name;
			var shellPath = srd.shellType;
			var indexInSpouseMapSheet = srd.templateIndex;
			var spouseSpot = srd.startPos + srd.spousePosOffset;

            Rectangle shellAreaToRefurbish = new Rectangle(corner.X - 1, corner.Y - 1, 8, 12);
			Misc.ExtendMap(fh, shellAreaToRefurbish.X + shellAreaToRefurbish.Width, shellAreaToRefurbish.Y + shellAreaToRefurbish.Height);

			// load shell

			if (appliedMapOverrides.Contains("spouse_room_" + spouse + "_shell"))
			{
				appliedMapOverrides.Remove("spouse_room_" + spouse + "_shell");
			}

			fh.ApplyMapOverride(shellPath, "spouse_room_" + spouse + "_shell", new Rectangle?(new Rectangle(0, 0, shellAreaToRefurbish.Width, shellAreaToRefurbish.Height)), new Rectangle?(shellAreaToRefurbish));

			for (int x = 0; x < shellAreaToRefurbish.Width; x++)
			{
				for (int y = 0; y < shellAreaToRefurbish.Height; y++)
				{
					if (fh.map.GetLayer("Back").Tiles[shellAreaToRefurbish.X + x, shellAreaToRefurbish.Y + y] != null)
					{
						fh.map.GetLayer("Back").Tiles[shellAreaToRefurbish.X + x, shellAreaToRefurbish.Y + y].Properties["FloorID"] = "spouse_hall_" + (Config.DecorateHallsIndividually ? spouse : "floor");
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
				}
				else if (ModEntry.roomIndexes.ContainsKey(spouse))
				{
					indexInSpouseMapSheet = ModEntry.roomIndexes[spouse];
				}
				else
				{
					Monitor.Log($"Could not find spouse room map for {spouse}", LogLevel.Debug);
					return;
				}
			}
			int width = fh.GetSpouseRoomWidth();
			int height = fh.GetSpouseRoomHeight();

            Rectangle areaToRefurbish = new Rectangle(corner.X, corner.Y, width, height);
			Map refurbishedMap = Game1.game1.xTileContent.Load<Map>("Maps\\" + map_path);
			int columns = refurbishedMap.Layers[0].LayerWidth / width;
			int num2 = refurbishedMap.Layers[0].LayerHeight / height;
			Point mapReader = new Point(indexInSpouseMapSheet % columns * width, indexInSpouseMapSheet / columns * height);
			List<KeyValuePair<Point, Tile>> bottom_row_tiles = new List<KeyValuePair<Point, Tile>>();
			Layer front_layer = fh.map.GetLayer("Front");
			for (int x = areaToRefurbish.Left; x < areaToRefurbish.Right; x++)
			{
				Point point = new Point(x, areaToRefurbish.Bottom - 1);
				Tile tile = front_layer.Tiles[point.X, point.Y];
				if (tile != null)
				{
					bottom_row_tiles.Add(new KeyValuePair<Point, Tile>(point, tile));
				}
			}

			if (appliedMapOverrides.Contains("spouse_room_" + spouse))
			{
				appliedMapOverrides.Remove("spouse_room_" + spouse);
			}

			fh.ApplyMapOverride(map_path, "spouse_room_" + spouse, new Rectangle?(new Rectangle(mapReader.X, mapReader.Y, areaToRefurbish.Width, areaToRefurbish.Height)), new Rectangle?(areaToRefurbish));
			for (int x = 0; x < areaToRefurbish.Width; x++)
			{
				for (int y = 0; y < areaToRefurbish.Height; y++)
				{
					if (refurbishedMap.GetLayer("Buildings")?.Tiles[mapReader.X + x, mapReader.Y + y] != null)
					{
						Helper.Reflection.GetMethod(fh, "adjustMapLightPropertiesForLamp").Invoke(refurbishedMap.GetLayer("Buildings").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Buildings");
					}
					if (y < areaToRefurbish.Height - 1 && refurbishedMap.GetLayer("Front")?.Tiles[mapReader.X + x, mapReader.Y + y] != null)
					{
						Helper.Reflection.GetMethod(fh, "adjustMapLightPropertiesForLamp").Invoke(refurbishedMap.GetLayer("Front").Tiles[mapReader.X + x, mapReader.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Front");
					}
					/*
					if (fh.map.GetLayer("Back").Tiles[corner.X + x, corner.Y + y] != null)
					{
						fh.setTileProperty(corner.X + x, corner.Y + y, "Back", "FloorID", $"spouse_room_{spouse}");
					}
					*/
				}
			}
			fh.ReadWallpaperAndFloorTileData();
			bool spot_found = false;
			for (int x3 = areaToRefurbish.Left; x3 < areaToRefurbish.Right; x3++)
			{
				for (int y2 = areaToRefurbish.Top; y2 < areaToRefurbish.Bottom; y2++)
				{
					if (fh.getTileIndexAt(new Point(x3, y2), "Paths") == 7)
					{
						spot_found = true;
						if (first)
							fh.spouseRoomSpot = new Point(x3, y2);
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
			fh.setTileProperty(spouseSpot.X, spouseSpot.Y, "Back", "NoFurniture", "T");
			foreach (KeyValuePair<Point, Tile> kvp in bottom_row_tiles)
			{
				front_layer.Tiles[kvp.Key.X, kvp.Key.Y] = kvp.Value;
			}

			ModEntry.currentRoomData[srd.name] = srd;
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
			//Monitor.Log($"Checking spouse thing for {srd.name}");
			if (srd.name == "Emily" && (srd.templateName == "Emily" || srd.templateName == null || srd.templateName == "") && Game1.player.eventsSeen.Contains(463391))
			{
				fh.temporarySprites.RemoveAll((s) => s is EmilysParrot);

				Vector2 parrotSpot = Utility.PointToVector2(srd.startPos + new Point(4, 2)) * 64;
				parrotSpot += new Vector2(16, 32);
				ModEntry.PMonitor.Log($"Building Emily's parrot at {parrotSpot}");
				fh.temporarySprites.Add(new EmilysParrot(parrotSpot));
			}
			else if (srd.name == "Sebastian" && (srd.templateName == "Sebastian" || srd.templateName == null || srd.templateName == "") && Game1.netWorldState.Value.hasWorldStateID("sebastianFrogReal"))
			{
				Vector2 spot = Utility.PointToVector2(srd.startPos + new Point(2, 7));
				Monitor.Log($"building Sebastian's terrarium at {spot}");
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