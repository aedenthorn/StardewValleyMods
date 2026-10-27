using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sickhead.Engine.Util;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.Characters;
using StardewValley.Locations;
using StardewValley.Monsters;
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
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {


        private static void MakeSpouseRoom(DecoratableLocation location, HashSet<string> appliedMapOverrides, SpouseRoomData srd, bool first = false)
        {
            try
            {
                SMonitor.Log($"Loading spouse room for {srd.name}. template {srd.templateName}, index {srd.templateIndex}, shellStart {srd.startPos}, spouse offset {srd.spousePosOffset}. Type: {srd.shellType}");

                var corner = srd.startPos + new Point(1, 1);
                var spouse = srd.name;
                var shellPath = srd.shellType;
                var indexInSpouseMapSheet = srd.templateIndex;
                var spouseSpot = srd.startPos + srd.spousePosOffset;

                Rectangle shellAreaToRefurbish = new Rectangle(corner.X - 1, corner.Y - 1, 8, 12);
                //ExtendMap(location.Map, shellAreaToRefurbish.X + shellAreaToRefurbish.Width, shellAreaToRefurbish.Y + shellAreaToRefurbish.Height);

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

                CharacterSpouseRoomData roomData = (NPC.TryGetData(spouse, out var spouseData) ? ((spouseData != null) ? spouseData.SpouseRoom : null) : null);
                string assetName = ((roomData != null) ? roomData.MapAsset : null) ?? "spouseRooms";
                Rectangle sourceArea = ((roomData != null) ? roomData.MapSourceRect : CharacterSpouseRoomData.DefaultMapSourceRect);

                SMonitor.Log("Map Path: |" + assetName + "|");

                Map refurbishedMap = Game1.game1.xTileContent.Load<Map>("Maps\\" + assetName);

                Rectangle areaToRefurbish = new Rectangle(corner.X, corner.Y, sourceArea.Width, sourceArea.Height);
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

                location.ApplyMapOverride(assetName, "spouse_room_" + spouse, sourceArea, new Rectangle?(areaToRefurbish));
                for (int x = 0; x < areaToRefurbish.Width; x++)
                {
                    for (int y = 0; y < areaToRefurbish.Height; y++)
                    {
                        if (refurbishedMap.GetLayer("Buildings")?.Tiles[sourceArea.X + x, sourceArea.Y + y] != null)
                        {
                            SHelper.Reflection.GetMethod(location, "adjustMapLightPropertiesForLamp").Invoke(refurbishedMap.GetLayer("Buildings").Tiles[sourceArea.X + x, sourceArea.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Buildings");
                        }
                        if (y < areaToRefurbish.Height - 1 && refurbishedMap.GetLayer("Front")?.Tiles[sourceArea.X + x, sourceArea.Y + y] != null)
                        {
                            SHelper.Reflection.GetMethod(location, "adjustMapLightPropertiesForLamp").Invoke(refurbishedMap.GetLayer("Front").Tiles[sourceArea.X + x, sourceArea.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Front");
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
                if (location.map.GetLayer("Paths") != null)
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
                if (location is FarmHouse)
                    currentRoomData[srd.name] = srd;
                else
                    currentIslandRoomData[srd.name] = srd;
                SMonitor.Log("Finished loading spouse room.");
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Failed in {nameof(MakeSpouseRoom)}:\n{ex}", LogLevel.Error);
            }

        }
        public static Dictionary<string, object> GetSpouses(Farmer farmer, int all)
        {
            Dictionary<string, object> spouses = new Dictionary<string, object>();
            if (all < 0)
            {
                NPC ospouse = farmer.getSpouse();
                if (ospouse != null)
                {
                    spouses[ospouse.Name] = ospouse;
                }
            }
            foreach (string friend in farmer.friendshipData.Keys)
            {
                if (Game1.getCharacterFromName(friend, true) != null && farmer.friendshipData[friend].IsMarried() && (all > 0 || friend != farmer.spouse))
                {
                    spouses[friend] = Game1.getCharacterFromName(friend, true);
                }
            }
			foreach(var pair in farmer.team.friendshipData.Pairs)
            {
				if (!pair.Value.IsMarried())
					continue;
				long id = pair.Key.Farmer1 == farmer.UniqueMultiplayerID ? pair.Key.Farmer2 : pair.Key.Farmer1;
				Farmer spouse = Game1.GetPlayer(id);
				if (spouse != null)
                {
					spouses[spouse.Name] = spouse;
                }
            }
            return spouses;
        }

        public static bool ExtendMap(Map map, int w, int h)
        {
            bool resized = false;
            foreach (Layer layer in map.Layers)
            {
                if (layer.LayerWidth < w || layer.LayerHeight < h)
                {
                    var name = layer.Id;
                    resized = true;
                    int width = Math.Max(w, layer.LayerWidth);
                    int height = Math.Max(h, layer.LayerHeight);
                    Tile[,] tiles = new Tile[width, height];
                    for (int x = 0; x < layer.LayerWidth; x++)
                    {
                        for (int y = 0; y < layer.LayerHeight; y++)
                        {
                            tiles[x, y] = layer.Tiles[x, y];
                        }
                    }
                    AccessTools.Field(typeof(Layer), "m_tiles").SetValue(layer, tiles);
                    AccessTools.Field(typeof(Layer), "m_tileArray").SetValue(layer, new TileArray(layer, tiles));
					layer.LayerHeight = height;
					layer.LayerWidth = width;
                }
            }
            if (resized)
            {
				AccessTools.Method(typeof(Map), "UpdateDisplaySize").Invoke(map, Array.Empty<object>());
            }
			return resized;
        }
        public static void CheckSpouseThing(FarmHouse fh, SpouseRoomData srd)
		{
			SMonitor.Log($"Checking spouse thing for {srd.name}");
			if (srd.name == "Emily" && (srd.templateName == "Emily" || srd.templateName == null || srd.templateName == ""))
			{
				List<TemporaryAnimatedSprite> emilyParrot = new List<TemporaryAnimatedSprite>();
				foreach(TemporaryAnimatedSprite sprite in fh.TemporarySprites.Where<TemporaryAnimatedSprite>((x) => x is EmilysParrot))
				{
					emilyParrot.Add(sprite);
				}

				foreach(TemporaryAnimatedSprite sprite in emilyParrot)
				{
					fh.TemporarySprites.Remove(sprite);
				}
				Vector2 spot = Utility.PointToVector2(srd.startPos + new Point(4, 2)) * 64;
				spot += new Vector2(16, 32);
				SMonitor.Log($"Building Emily's parrot at {spot}");
				fh.temporarySprites.Add(new EmilysParrot(spot));
			}
			else if (srd.name == "Sebastian" && (srd.templateName == "Sebastian" || srd.templateName == null || srd.templateName == "") && Game1.netWorldState.Value.hasWorldStateID("sebastianFrogReal"))
			{
				Vector2 spot = Utility.PointToVector2(srd.startPos + new Point(2, 7));
				SMonitor.Log($"building Sebastian's terrarium at {spot}");
				if (spot.X < 0 || spot.Y - 1 < 0 || spot.X + 2 >= fh.Map.GetLayer("Front").LayerWidth || spot.Y - 1 >= fh.Map.GetLayer("Front").LayerHeight)
				{
					SMonitor.Log("Spot is outside of map!");
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