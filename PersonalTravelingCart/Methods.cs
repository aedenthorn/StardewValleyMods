using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.SaveMigrations;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace PersonalTravelingCart
{
	public partial class ModEntry
	{
		public static Dictionary<string, TravelingCart> LoadTravelingCarts()
		{
			travelingCartDictionary = SHelper.GameContent.Load<Dictionary<string, TravelingCart>>(dataPath);
			foreach (TravelingCart travelingCart in travelingCartDictionary.Values)
			{
				travelingCart.spriteSheet = !string.IsNullOrEmpty(travelingCart.spriteSheetPath) ? SHelper.GameContent.Load<Texture2D>(travelingCart.spriteSheetPath) : SHelper.ModContent.Load<Texture2D>("assets/cart.png");
				if(travelingCart.mapPath == null)
				{
					travelingCart.mapPath = SHelper.ModContent.GetInternalAssetName("assets/Cart.tmx").BaseName;
                }
			}
			SMonitor.Log($"Loaded {travelingCartDictionary.Count} custom traveling carts");
			travelingCartDictionary[defaultKey] = new TravelingCart
			{
				spriteSheet = SHelper.ModContent.Load<Texture2D>("assets/cart.png"),
				mapPath = SHelper.ModContent.GetInternalAssetName("assets/Cart.tmx").BaseName
			};
			return travelingCartDictionary;
		}

		private static List<(string, string)> GetSerializedLocations()
		{
			List<(string, string)> serializedLocations = new();

			Utility.ForEachLocation(location =>
			{
				if (location.NameOrUniqueName.StartsWith($"{SModManifest.UniqueID}/"))
				{
					string serialized = SerializeLocation(location);

					if (serialized is not null)
					{
						serializedLocations.Add((serialized, location.mapPath.Value));
					}
				}
				return true;
			});
			return serializedLocations;
		}

		private static string SerializeLocation(GameLocation location)
		{
			try
			{
				using StringWriter stringWriter = new();
				SaveGame.locationSerializer.Serialize(stringWriter, location);

				return stringWriter.ToString();
			}
			catch (Exception ex)
			{
				SMonitor.Log($"Failed to serialize {location.NameOrUniqueName}: {ex.Message}", LogLevel.Error);
				return null;
			}
		}

		private static void LoadSerializedLocations(List<(string, string)> serializedLocations)
		{
			if (serializedLocations is not null)
			{
				foreach ((string serializedLocation, string mapPath) in serializedLocations)
				{
					GameLocation location = DeserializeLocation(serializedLocation);

					if (location is not null)
					{
						DecoratableLocation travelingCartLocation = new(mapPath, location.NameOrUniqueName);

						ApplyLocationDataFromSource(travelingCartLocation, location, false);
						Game1.locations.Add(travelingCartLocation);
					}
				}
			}
		}

		private static GameLocation DeserializeLocation(string serializedLocation)
		{
			try
			{
				using StringReader stringReader = new(serializedLocation);
				GameLocation location = (GameLocation)SaveGame.locationSerializer.Deserialize(stringReader);

				return location;
			}
			catch (Exception ex)
			{
				SMonitor.Log($"Failed to deserialize location: {ex.Message}", LogLevel.Error);
				return null;
			}
		}

		private static void ApplyLocationDataFromSource(GameLocation location, GameLocation fromLocation, bool applySaveFixes = true)
		{
			location.TransferDataFromSavedLocation(fromLocation);
			location.animals.MoveFrom(fromLocation.animals);
			location.buildings.Set(fromLocation.buildings);
			location.characters.Set(fromLocation.characters);
			location.furniture.Set(fromLocation.furniture);
			location.largeTerrainFeatures.Set(fromLocation.largeTerrainFeatures);
			location.miniJukeboxCount.Value = fromLocation.miniJukeboxCount.Value;
			location.miniJukeboxTrack.Value = fromLocation.miniJukeboxTrack.Value;
			location.netObjects.Set(fromLocation.netObjects.Pairs);
			location.numberOfSpawnedObjectsOnMap = fromLocation.numberOfSpawnedObjectsOnMap;
			location.piecesOfHay.Value = fromLocation.piecesOfHay.Value;
			location.resourceClumps.Set(new List<ResourceClump>(fromLocation.resourceClumps));
			location.terrainFeatures.Set(fromLocation.terrainFeatures.Pairs);
			if (applySaveFixes && !SaveGame.loaded.HasSaveFix(SaveFixes.MigrateBuildingsToData))
			{
				SaveMigrator_1_6.ConvertBuildingsToData(location);
			}
			location.AddDefaultBuildings(load: false);
			foreach (Building building in location.buildings)
			{
				building.load();
				if (building.GetIndoorsType() == IndoorsType.Instanced)
				{
					building.GetIndoors()?.addLightGlows();
				}
			}
			foreach (FarmAnimal farmAnimal in location.animals.Values)
			{
				farmAnimal.reload((GameLocation)null);
			}
			foreach (Furniture furniture in location.furniture)
			{
				furniture.updateDrawPosition();
			}
			foreach (LargeTerrainFeature largeTerrainFeature in location.largeTerrainFeatures)
			{
				largeTerrainFeature.Location = location;
				largeTerrainFeature.loadSprite();
			}
			foreach (TerrainFeature terrainFeature in location.terrainFeatures.Values)
			{
				terrainFeature.Location = location;
				terrainFeature.loadSprite();
				if (terrainFeature is HoeDirt hoeDirt)
				{
					hoeDirt.updateNeighbors();
				}
			}
			foreach (KeyValuePair<Vector2, Object> pair in location.objects.Pairs)
			{
				pair.Value.initializeLightSource(pair.Key);
				pair.Value.reloadSprite();
			}
			location.addLightGlows();
		}

		private static bool TryUpdateCursorForHitchedHorse(Horse horse)
		{
			if (!horse.modData.ContainsKey(parkedKey))
			{
				string which = horse.modData.TryGetValue(whichCartKey, out string value) && travelingCartDictionary.ContainsKey(value) ? value : defaultKey;

				if (IsMouseInBoundingBox(horse, travelingCartDictionary[which]))
				{
					Game1.mouseCursor = Game1.cursor_grab;
					Game1.mouseCursorTransparency = IsPlayerNearby(horse, travelingCartDictionary[which]) ? 1f : 0.5f;
					return true;
				}
			}
			return false;
		}

		private static bool TryUpdateCursorForParkedTravelingCart(ParkedTravelingCart parkedTravelingCart)
		{
			TravelingCart data = GetCartData(parkedTravelingCart.whichCart);
			TravelingCart.DirectionData ddata = data.GetDirectionData(parkedTravelingCart.facingDirection);

			if (IsMouseInBoundingBox(parkedTravelingCart.position, ddata))
			{
				Game1.mouseCursor = Game1.cursor_grab;
				Game1.mouseCursorTransparency = IsPlayerNearby(parkedTravelingCart.position, ddata) ? 1f : 0.5f;
				return true;
			}
			return false;
		}

		private static bool IsMouseInBoundingBox(Horse horse, TravelingCart data)
		{
			return IsMouseInBoundingBox(horse.Position, GetDirectionData(data, horse.FacingDirection));
		}

		private static bool IsMouseInBoundingBox(Vector2 position, TravelingCart.DirectionData ddata)
		{
			Rectangle rectangle = new(Utility.Vector2ToPoint(position + ddata.cartOffset) + new Point(ddata.clickRect.Location.X * Game1.pixelZoom, ddata.clickRect.Location.Y * Game1.pixelZoom), new Point(ddata.clickRect.Width * Game1.pixelZoom, ddata.clickRect.Height * Game1.pixelZoom));

			return rectangle.Contains(Game1.viewport.X + Game1.getMouseX(), Game1.viewport.Y + Game1.getMouseY());
		}

		private static bool IsPlayerNearby(Horse horse, TravelingCart data)
		{
			return horse?.rider == Game1.player || IsPlayerNearby(horse.Position, GetDirectionData(data, horse.FacingDirection));
		}

		private static bool IsPlayerNearby(Vector2 position, TravelingCart.DirectionData ddata)
		{
			Rectangle rectangle = new(Utility.Vector2ToPoint(position + ddata.cartOffset) + new Point(ddata.clickRect.Location.X * Game1.pixelZoom - 2 * Game1.tileSize, ddata.clickRect.Location.Y * Game1.pixelZoom), new Point(ddata.clickRect.Width * Game1.pixelZoom + 4 * Game1.tileSize, ddata.clickRect.Height * Game1.pixelZoom + 2 * Game1.tileSize));

			return rectangle.Contains(Game1.player.GetBoundingBox());
		}

		private static bool IsActionOnTravelingCart(Horse horse, TravelingCart data)
		{
			return horse?.rider == Game1.player || IsActionOnTravelingCart(horse.Position, GetDirectionData(data, horse.FacingDirection));
		}

		private static bool IsActionOnTravelingCart(Vector2 position, TravelingCart.DirectionData ddata)
		{
			return IsActionOnTravelingCart(Game1.player.GetBoundingBox(), position, ddata);
		}

		private static bool IsActionOnTravelingCart(Rectangle boundingBox, Vector2 position, TravelingCart.DirectionData ddata)
		{
			Rectangle rectangle = new(Utility.Vector2ToPoint(position + ddata.cartOffset) + new Point(ddata.collisionRect.Location.X * Game1.pixelZoom - 8, ddata.collisionRect.Location.Y * Game1.pixelZoom - 8), new Point(ddata.collisionRect.Width * Game1.pixelZoom + 16, ddata.collisionRect.Height * Game1.pixelZoom + 16));
			Vector2 grabTile = Game1.player.GetGrabTile();
			Rectangle grabTileRectangle = new((int)grabTile.X * Game1.tileSize, (int)grabTile.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize);

			return rectangle.Intersects(boundingBox) && IsColliding(grabTileRectangle, position, ddata);
		}

		private static bool CheckActionForParkedTravelingCart(ParkedTravelingCart parkedTravelingCart, ref bool __result)
		{
			TravelingCart data = GetCartData(parkedTravelingCart.whichCart);
			TravelingCart.DirectionData ddata = data.GetDirectionData(parkedTravelingCart.facingDirection);

			if ((IsMouseInBoundingBox(parkedTravelingCart.position, ddata) && IsPlayerNearby(parkedTravelingCart.position, ddata)) || IsActionOnTravelingCart(parkedTravelingCart.position, ddata))
			{
				return TryWarpToTravelingCartLocation(parkedTravelingCart.location, new Point(data.entryTile.X, data.entryTile.Y), ref __result);
			}
			return false;
		}

		private static bool CheckActionForNonParkedTravelingCart(Horse horse, ref bool __result, bool flag)
		{
			if (!horse.Name.StartsWith("tractor/") && !horse.modData.ContainsKey(parkedKey))
			{
				string which = horse.modData.TryGetValue(whichCartKey, out string value) && travelingCartDictionary.ContainsKey(value) ? value : defaultKey;

				if ((horse.rider == Game1.player && IsMouseInBoundingBox(horse, travelingCartDictionary[which])) || (horse.rider != Game1.player && ((IsMouseInBoundingBox(horse, travelingCartDictionary[which]) && IsPlayerNearby(horse, travelingCartDictionary[which])) || IsActionOnTravelingCart(horse, travelingCartDictionary[which]))))
				{
					return TryWarpToTravelingCartLocation($"{SModManifest.UniqueID}/{horse.HorseId}", new Point(travelingCartDictionary[which].entryTile.X, travelingCartDictionary[which].entryTile.Y), ref __result, flag);
				}
			}
			return false;
		}

		private static bool TryWarpToTravelingCartLocation(string locationUniqueName, Point entryTile, ref bool __result, bool flag = true)
		{
			GameLocation location = Game1.getLocationFromName(locationUniqueName);

			__result = false;
			if (location is not null)
			{
				SMonitor.Log($"Warping to traveling cart {locationUniqueName}");
				LocationRequest locationRequest = new(locationUniqueName, false, location);

				ShouldDisplayFarmer = flag;
				locationRequest.OnWarp += () => ShouldDisplayFarmer = true;
				Game1.playSound("doorClose");
				UpdateOutdoorsInfos(location);
				Game1.warpFarmer(locationRequest, entryTile.X, entryTile.Y, 0);
				__result = true;
			}
			return __result;
		}

		private static bool IsColliding(Rectangle boundingBox, Horse horse, TravelingCart data)
		{
			return IsColliding(boundingBox, horse.Position, GetDirectionData(data, horse.FacingDirection));
		}

		private static bool IsColliding(Rectangle boundingBox, Vector2 position, TravelingCart.DirectionData ddata)
		{
			Rectangle rectangle = new(Utility.Vector2ToPoint(position + ddata.cartOffset) + new Point(ddata.collisionRect.Location.X * Game1.pixelZoom, ddata.collisionRect.Location.Y * Game1.pixelZoom), new Point(ddata.collisionRect.Width * Game1.pixelZoom, ddata.collisionRect.Height * Game1.pixelZoom));

			return rectangle.Intersects(boundingBox);
		}

		private static bool CheckCollidingPositionForParkedTravelingCart(Rectangle position, ParkedTravelingCart parkedTravelingCart, ref bool __result)
		{
			__result = IsColliding(position, parkedTravelingCart.position, GetCartData(parkedTravelingCart.whichCart).GetDirectionData(parkedTravelingCart.facingDirection));
			return __result;
		}

		private static bool CheckCollidingPositionForNonParkedTravelingCart(Rectangle position, Horse horse, ref bool __result)
		{
			__result = false;
			if (!horse.Name.StartsWith("tractor/") && !horse.modData.ContainsKey(parkedKey))
			{
				string which = horse.modData.TryGetValue(whichCartKey, out string value) && travelingCartDictionary.ContainsKey(value) ? value : defaultKey;

				__result = IsColliding(position, horse, travelingCartDictionary[which]);
			}
			return __result;
		}

		private static (GameLocation, Point, Point) GetOutdoorsInfos(GameLocation interiorLocation)
		{
			UpdateOutdoorsInfos(interiorLocation);
			if (Game1.getFarm().modData.TryGetValue($"{outdoorsInfosKey}/{interiorLocation.NameOrUniqueName}", out string outdoorsInfosAsJsonData))
			{
				Tuple<string, Point, Point> outdoorsInfosData = JsonConvert.DeserializeObject<Tuple<string, Point, Point>>(outdoorsInfosAsJsonData);

				if (outdoorsInfosData is not null)
				{
					return (Game1.getLocationFromName(outdoorsInfosData.Item1), outdoorsInfosData.Item2, outdoorsInfosData.Item3);
				}
			}
			return default;
		}

		private static void UpdateOutdoorsInfos(GameLocation interiorLocation)
		{
			if (TryFindHitchedHorse(interiorLocation, out (GameLocation, Point, Point) hitchedHorseData))
			{
				Game1.getFarm().modData[$"{outdoorsInfosKey}/{interiorLocation.NameOrUniqueName}"] = JsonConvert.SerializeObject((hitchedHorseData.Item1.NameOrUniqueName, hitchedHorseData.Item2, hitchedHorseData.Item3));
			}
			else if (TryFindParkedTravelingCart(interiorLocation, out (GameLocation, Point, Point) parkedTravelingCartData))
			{
				Game1.getFarm().modData[$"{outdoorsInfosKey}/{interiorLocation.NameOrUniqueName}"] = JsonConvert.SerializeObject((parkedTravelingCartData.Item1.NameOrUniqueName, parkedTravelingCartData.Item2, parkedTravelingCartData.Item3));
			}
		}

		private static bool TryFindHitchedHorse(GameLocation interiorLocation, out (GameLocation, Point, Point) locationData)
		{
			Horse hitchedHorse = null;

			locationData = default;
			Utility.ForEachLocation(location =>
			{
				foreach (NPC npc in location.characters)
				{
					if (npc is Horse horse && interiorLocation.NameOrUniqueName.Equals($"{SModManifest.UniqueID}/{horse.HorseId}") && !horse.modData.ContainsKey(parkedKey))
					{
						hitchedHorse = horse;
						return false;
					}
				}
				foreach (Farmer farmer in location.farmers)
				{
					if (farmer.isRidingHorse() && interiorLocation.NameOrUniqueName.Equals($"{SModManifest.UniqueID}/{farmer.mount.HorseId}") && !farmer.mount.modData.ContainsKey(parkedKey))
					{
						hitchedHorse = farmer.mount;
						return false;
					}
				}
				return true;
			});
			if (hitchedHorse is not null)
			{
				string which = hitchedHorse.modData.TryGetValue(whichCartKey, out string value) && travelingCartDictionary.ContainsKey(value) ? value : defaultKey;
				TravelingCart.DirectionData ddata = GetDirectionData(travelingCartDictionary[which], hitchedHorse.FacingDirection);
				Vector2 position = hitchedHorse.Position + ddata.cartOffset + ddata.backRect.Size.ToVector2() * 2;
				int positionX = (int)position.X - interiorLocation.Map.GetLayer("Back").LayerWidth * 32;
				int positionY = (int)position.Y - interiorLocation.Map.GetLayer("Back").LayerHeight * 32;
				Point outerTile = hitchedHorse.TilePoint;
				GameLocation parentLocation = hitchedHorse.currentLocation;

				hitchedHorse.modData[whichCartKey] = which;
				locationData = (parentLocation, outerTile, new Point(positionX, positionY));
				return true;
			}
			return false;
		}

		private static bool TryFindParkedTravelingCart(GameLocation interiorLocation, out (GameLocation, Point, Point) locationData)
		{
			ParkedTravelingCart parkedTravelingCart = null;
			GameLocation parentLocation = null;

			locationData = default;
			Utility.ForEachLocation(location =>
			{
				if (Game1.getFarm().modData.TryGetValue($"{parkedListKey}/{location.NameOrUniqueName}", out string parkedString))
				{
					List<ParkedTravelingCart> ptcs = JsonConvert.DeserializeObject<List<ParkedTravelingCart>>(parkedString);

					foreach (ParkedTravelingCart ptc in ptcs)
					{
						if (ptc.location == interiorLocation.NameOrUniqueName)
						{
							parkedTravelingCart = ptc;
							parentLocation = location;
							return false;
						}
					}
				}
				return true;
			});
			if (parkedTravelingCart is not null && parentLocation is not null)
			{
				TravelingCart data = GetCartData(parkedTravelingCart.whichCart);
				TravelingCart.DirectionData ddata = data.GetDirectionData(parkedTravelingCart.facingDirection);
				Vector2 position = parkedTravelingCart.position + ddata.cartOffset + ddata.backRect.Size.ToVector2() * 2;
				int positionX = (int)position.X - interiorLocation.Map.GetLayer("Back").LayerWidth * 32;
				int positionY = (int)position.Y - interiorLocation.Map.GetLayer("Back").LayerHeight * 32;
				Point outerTile = new((int)(parkedTravelingCart.position.X / Game1.tileSize) + (parkedTravelingCart.facingDirection == 1 ? 1 : 0), (int)(parkedTravelingCart.position.Y / Game1.tileSize));

				locationData = (parentLocation, outerTile, new Point(positionX, positionY));
				return true;
			}
			return false;
		}

		private static TravelingCart GetCartData(string which)
		{
			return travelingCartDictionary.TryGetValue(which, out TravelingCart value) ? value : travelingCartDictionary[defaultKey];
		}

		private static TravelingCart.DirectionData GetDirectionData(TravelingCart data, int facingDirection)
		{
			return facingDirection switch
			{
				0 => data.up,
				1 => data.right,
				2 => data.down,
				_ => data.left,
			};
		}

		private static void HandleHitchButton(Horse horse, string which)
		{
			bool hasParked = Game1.getFarm().modData.TryGetValue($"{parkedListKey}/{Game1.player.currentLocation.NameOrUniqueName}", out string parkedString);
			List<ParkedTravelingCart> parkedTravelingCarts = hasParked ? JsonConvert.DeserializeObject<List<ParkedTravelingCart>>(parkedString) : new List<ParkedTravelingCart>();

			if (!horse.modData.ContainsKey(parkedKey) || hasParked)
			{
				if (!horse.modData.ContainsKey(parkedKey))
				{
					TryParkTravelingCart(horse, which, parkedTravelingCarts);
				}
				else
				{
					TryHitchToTravelingCart(horse, parkedTravelingCarts);
				}
				UpdateParkedTravelingCarts(parkedTravelingCarts);
			}
		}

		private static bool TryParkTravelingCart(Horse horse, string which, List<ParkedTravelingCart> parkedTravelingCarts)
		{
			if (horse.currentLocation is null || !horse.currentLocation.IsOutdoors)
			{
				Game1.showRedMessage(SHelper.Translation.Get("CannotPark"));
				return false;
			}
			if (IsFestivalDay(horse.currentLocation, 0, true))
			{
				Game1.showRedMessage(SHelper.Translation.Get("CannotParkFestivalToday"));
				return false;
			}
			if (IsFestivalDay(horse.currentLocation, 1))
			{
				Game1.showRedMessage(SHelper.Translation.Get("CannotParkFestivalTomorrow"));
				return false;
			}

			ParkedTravelingCart parkedTravelingCart = new()
			{
				facingDirection = horse.FacingDirection,
				location = $"{SModManifest.UniqueID}/{horse.HorseId}",
				whichCart = which,
				position = Game1.player.Position
			};

			horse.modData[parkedKey] = "True";
			parkedTravelingCarts.Add(parkedTravelingCart);
			SMonitor.Log($"Parked traveling cart in {Game1.player.currentLocation}");
			return true;
		}

		private static bool TryHitchToTravelingCart(Horse horse, List<ParkedTravelingCart> parkedTravelingCarts)
		{
			for (int i = 0; i < parkedTravelingCarts.Count; i++)
			{
				ParkedTravelingCart parkedTravelingCart = parkedTravelingCarts[i];

				if (parkedTravelingCart.location == $"{SModManifest.UniqueID}/{horse.HorseId}")
				{
					if (CheckCartHitching(parkedTravelingCart, horse))
					{
						SMonitor.Log($"Hitching to traveling cart in {Game1.player.currentLocation}");
						Game1.player.Position = parkedTravelingCart.position;
						Game1.player.faceDirection(parkedTravelingCart.facingDirection);
						horse.SyncPositionToRider();
						horse.modData.Remove(parkedKey);
						parkedTravelingCarts.RemoveAt(i);
						return true;
					}
				}
			}
			return false;
		}

		private static bool CheckCartHitching(ParkedTravelingCart parkedTravelingCart, Horse horse)
		{
			TravelingCart cdata = GetCartData(parkedTravelingCart.whichCart);
			TravelingCart.DirectionData cddata = cdata.GetDirectionData(parkedTravelingCart.facingDirection);
			Rectangle hitchRectangle = new(Utility.Vector2ToPoint(parkedTravelingCart.position + cddata.cartOffset) + new Point(cddata.hitchRect.Location.X * Game1.pixelZoom, cddata.hitchRect.Location.Y * Game1.pixelZoom + Game1.tileSize), new Point(cddata.hitchRect.Size.X * Game1.pixelZoom, cddata.hitchRect.Size.Y * Game1.pixelZoom));

			return hitchRectangle.Intersects(horse.GetBoundingBox());
		}

		private static void UpdateParkedTravelingCarts(List<ParkedTravelingCart> parkedTravelingCarts)
		{
			foreach (ParkedTravelingCart parkedTravelingCart in parkedTravelingCarts)
			{
				parkedTravelingCart.data = null;
			}

			string serializedParkedTravelingCarts = JsonConvert.SerializeObject(parkedTravelingCarts);

			Game1.getFarm().modData[$"{parkedListKey}/{Game1.player.currentLocation.NameOrUniqueName}"] = serializedParkedTravelingCarts;
		}

		private static void SwitchCart(Horse horse, string which, int direction)
		{
			if (!horse.modData.ContainsKey(parkedKey))
			{
				List<string> keys = travelingCartDictionary.Keys.ToList();
				int index = keys.IndexOf(which);

				index = (index + direction + keys.Count) % keys.Count;

				SMonitor.Log($"Switching traveling cart to {keys[index]}");
				Game1.getLocationFromName($"{SModManifest.UniqueID}/{horse.HorseId}").mapPath.Value = travelingCartDictionary[keys[index]].mapPath;
				horse.modData[whichCartKey] = keys[index];
			}
		}

		private static void HandleDebug(string which, TravelingCart data, SButton button)
		{
			TravelingCart.DirectionData directionData = GetDirectionData(data, Game1.player.FacingDirection);

			if (button == SButton.F7)
			{
				SaveTravelingCartDataToJson();
			}
			else if (button == SButton.F5)
			{
				LoadTravelingCartDataFromJson(which);
			}
			else
			{
				AdjustCartOffsets(button, directionData);
				UpdateTravelingCartData(which, data, directionData);
			}
		}

		private static void SaveTravelingCartDataToJson()
		{
			SMonitor.Log("Saving to json file");
			File.WriteAllText(Path.Combine(SHelper.DirectoryPath, "assets", "cart_data.json"), JsonConvert.SerializeObject(travelingCartDictionary, Formatting.Indented));
		}

		private static void LoadTravelingCartDataFromJson(string which)
		{
			string jsonPath = Path.Combine(SHelper.DirectoryPath, "assets", "cart_data.json");

			if (!File.Exists(jsonPath))
			{
				SMonitor.Log("File not found");
				return;
			}
			travelingCartDictionary = JsonConvert.DeserializeObject<Dictionary<string, TravelingCart>>(File.ReadAllText(jsonPath));
			foreach (TravelingCart travelingCart in travelingCartDictionary.Values)
			{
				travelingCart.spriteSheet = !string.IsNullOrEmpty(travelingCart.spriteSheetPath) ? SHelper.GameContent.Load<Texture2D>(travelingCart.spriteSheetPath) : SHelper.ModContent.Load<Texture2D>("assets/cart.png");
			}
		}

		private static void AdjustCartOffsets(SButton button, TravelingCart.DirectionData directionData)
		{
			int multiplier = SHelper.Input.IsDown(SButton.LeftAlt) ? 4 : 1;

			if (SHelper.Input.IsDown(SButton.LeftShift))
			{
				switch (button)
				{
					case SButton.Left:
						directionData.playerOffset += new Vector2(Game1.pixelZoom * multiplier, 0);
						break;
					case SButton.Right:
						directionData.playerOffset -= new Vector2(Game1.pixelZoom * multiplier, 0);
						break;
					case SButton.Up:
						directionData.playerOffset += new Vector2(0, Game1.pixelZoom * multiplier);
						break;
					case SButton.Down:
						directionData.playerOffset -= new Vector2(0, Game1.pixelZoom * multiplier);
						break;
				}
			}
			else
			{
				switch (button)
				{
					case SButton.Left:
						directionData.cartOffset -= new Vector2(Game1.pixelZoom * multiplier, 0);
						break;
					case SButton.Right:
						directionData.cartOffset += new Vector2(Game1.pixelZoom * multiplier, 0);
						break;
					case SButton.Up:
						directionData.cartOffset -= new Vector2(0, Game1.pixelZoom * multiplier);
						break;
					case SButton.Down:
						directionData.cartOffset += new Vector2(0, Game1.pixelZoom * multiplier);
						break;
				}
			}
		}

		private static void UpdateTravelingCartData(string which, TravelingCart data, TravelingCart.DirectionData directionData)
		{
			switch (Game1.player.FacingDirection)
			{
				case 0:
					data.up = directionData;
					break;
				case 1:
					data.right = directionData;
					break;
				case 2:
					data.down = directionData;
					break;
				case 3:
					data.left = directionData;
					break;
			}
			travelingCartDictionary[which] = data;
		}

		private static bool IsFestivalDay(GameLocation location, int dayOffset, bool treatAsFestivalLocation = false)
		{
			int num = (Game1.Date.TotalDays + dayOffset) % (28 * 4);
			int dayOfMonth = num % 28 + 1;
			Season season = (Season)(num / 28);

			return IsActiveFestivalDay(location, dayOfMonth, season) || IsPassiveFestivalDay(location, dayOfMonth, season, treatAsFestivalLocation);
		}

		private static bool IsActiveFestivalDay(GameLocation location, int dayOfMonth, Season season)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new(0, 2);

			defaultInterpolatedStringHandler.AppendFormatted(Utility.getSeasonKey(season));
			defaultInterpolatedStringHandler.AppendFormatted(dayOfMonth);

			string text = defaultInterpolatedStringHandler.ToStringAndClear();

			if (DataLoader.Festivals_FestivalDates(Game1.temporaryContent).ContainsKey(text))
			{
				string locationContext = location.GetLocationContextId();

				if (locationContext is not null)
				{
					if (Event.tryToLoadFestivalData(text, out string _, out Dictionary<string, string> _, out string locationName, out int _, out int _))
					{
						if (location.Name == locationName)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		private static bool IsPassiveFestivalDay(GameLocation location, int dayOfMonth, Season season, bool treatAsFestivalLocation = false)
		{
			if (Utility.TryGetPassiveFestivalDataForDay(dayOfMonth, season, null, out string id, out PassiveFestivalData data) && data is not null)
			{
				if (data.MapReplacements is not null)
				{
					if (!treatAsFestivalLocation)
					{
						foreach (string key in data.MapReplacements.Keys)
						{
							if (key.Equals(location.Name))
							{
								return true;
							}
						}
					}
					else
					{
						foreach (string value in data.MapReplacements.Values)
						{
							if (value.Equals(location.Name))
							{
								return true;
							}
						}
					}
				}
				if (((id.Equals("TroutDerby") && location.Name.Equals("Forest")) || (id.Equals("SquidFest") && location.Name.Equals("Beach"))) && data.StartDay > Game1.dayOfMonth)
				{
					return true;
				}
			}
			return false;
		}

		private static bool IsPassiveFestivalLocation(GameLocation location)
		{
			Dictionary<string, PassiveFestivalData> PassiveFestivals = DataLoader.PassiveFestivals(Game1.content);

			foreach (PassiveFestivalData passiveFestivalData in PassiveFestivals.Values)
			{
				if (passiveFestivalData.MapReplacements is not null)
				{
					foreach (string value in passiveFestivalData.MapReplacements.Values)
					{
						if (value.Equals(location.Name))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
