using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json;
using xTile.Dimensions;
using xTile.Tiles;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Locations;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace PersonalTravelingCart
{
	public partial class ModEntry
	{
		internal static PerScreen<bool> shouldDisplayFarmer = new(() => true);

		internal static bool ShouldDisplayFarmer
		{
			get => shouldDisplayFarmer.Value;
			set => shouldDisplayFarmer.Value = value;
		}

		public class SaveGame_loadDataToLocations_Patch
		{
			public static void Prefix(ref List<GameLocation> fromLocations, ref List<GameLocation> __state)
			{
				if (!Config.ModEnabled)
					return;

				__state = fromLocations;
				fromLocations = fromLocations.Where(loc => !loc.NameOrUniqueName.StartsWith($"{SModManifest.UniqueID}/")).ToList();
			}

			public static void Postfix(List<GameLocation> __state)
			{
				if (!Config.ModEnabled)
					return;

				Dictionary<string, DecoratableLocation> travelingCartLocations = new();

				Utility.ForEachCharacter(character =>
				{
					if (character is Horse horse)
					{
						travelingCartLocations.Add($"{SModManifest.UniqueID}/{horse.HorseId}", new DecoratableLocation((horse.modData.TryGetValue(whichCartKey, out string which) && travelingCartDictionary.TryGetValue(which, out TravelingCart data) && data.mapPath is not null) ? data.mapPath : SHelper.ModContent.GetInternalAssetName("assets/Cart.tmx").Name, $"{SModManifest.UniqueID}/{horse.HorseId}"));
					}
					return true;
				});
				if (travelingCartLocations.Count > 0)
				{
					foreach (GameLocation fromLocation in __state)
					{
						if (travelingCartLocations.ContainsKey(fromLocation.NameOrUniqueName))
						{
							ApplyLocationDataFromSource(travelingCartLocations[fromLocation.NameOrUniqueName], fromLocation);
						}
					}
					foreach (DecoratableLocation travelingCartLocation in travelingCartLocations.Values)
					{
						Game1.locations.Add(travelingCartLocation);
					}
				}
			}
		}

		public class Horse_update_Patch
		{
			public static void Postfix(Horse __instance)
			{
				if (!Config.ModEnabled || __instance.Name.StartsWith("tractor/") || !__instance.mounting.Value || __instance.rider is null)
					return;

				__instance.rider.faceDirection(__instance.FacingDirection);
			}
		}

		public class Horse_draw_Patch
		{
			public static void Prefix(Horse __instance, SpriteBatch b)
			{
				if (!Config.ModEnabled || __instance.Name.StartsWith("tractor/") || __instance.currentLocation != Game1.currentLocation || __instance.modData.ContainsKey(parkedKey))
					return;

				string which = __instance.modData.TryGetValue(whichCartKey, out string value) && travelingCartDictionary.ContainsKey(value) ? value : defaultKey;
				TravelingCart.DirectionData ddata = GetDirectionData(travelingCartDictionary[which], __instance.FacingDirection);
				Texture2D spriteSheet = travelingCartDictionary[which].spriteSheet;
				Vector2 localPosition = Game1.GlobalToLocal(__instance.Position + ddata.cartOffset);
				Rectangle backRect = ddata.backRect;
				Rectangle middleRect = ddata.middleRect;
				Rectangle frontRect = ddata.frontRect;
				float layerDepth = (__instance.Position.Y + (__instance.FacingDirection == 0 ? ddata.clickRect.Height * Game1.pixelZoom : 0f) + Game1.tileSize) / 10000f;

				__instance.modData[whichCartKey] = which;
				if (ddata.frames > 0)
				{
					int frame = ((__instance.FacingDirection % 2 == 0) ? (int)__instance.Position.Y : (int)__instance.Position.X) / ddata.frameRate % ddata.frames;

					backRect.Location += new Point(backRect.Width * frame, 0);
					middleRect.Location += new Point(middleRect.Width * frame, 0);
					frontRect.Location += new Point(frontRect.Width * frame, 0);
				}
				b.Draw(spriteSheet, localPosition, backRect, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, layerDepth - 0.005f);
				b.Draw(spriteSheet, localPosition, middleRect, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, layerDepth - 0.0001f);
				b.Draw(spriteSheet, localPosition, frontRect, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, layerDepth + 0.0001f);
			}
		}

		public class Farmer_draw_Patch
		{
			public static bool Prefix(Farmer __instance, ref float[] __state)
			{
				if (!Config.ModEnabled)
					return true;

				if (__instance == Game1.player && !ShouldDisplayFarmer)
				{
					Game1.displayFarmer = false;
					return false;
				}
				if (__instance.isRidingHorse())
				{
					Horse horse = __instance.mount;

					if (!horse.Name.StartsWith("tractor/") && !horse.modData.ContainsKey(parkedKey))
					{
						string which = horse.modData.TryGetValue(whichCartKey, out string value) && travelingCartDictionary.ContainsKey(value) ? value : defaultKey;
						TravelingCart.DirectionData ddata = GetDirectionData(travelingCartDictionary[which], __instance.FacingDirection);

						horse.modData[whichCartKey] = which;
						__state = new float[] {
							__instance.xOffset,
							__instance.yOffset
						};
						__instance.xOffset += ddata.playerOffset.X;
						__instance.yOffset += ddata.playerOffset.Y;
					}
				}
				return true;
			}

			public static void Postfix(Farmer __instance, float[] __state)
			{
				if (!Config.ModEnabled || __state is null)
					return;

				__instance.xOffset = __state[0];
				__instance.yOffset = __state[1];
			}

			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				foreach (CodeInstruction instruction in instructions)
				{
					if (instruction.opcode.Equals(OpCodes.Ldc_R4) && instruction.operand.Equals(0.0016f))
					{
						instruction.opcode = OpCodes.Call;
						instruction.operand = typeof(Farmer_draw_Patch).GetMethod("GetLayerDepth", BindingFlags.Public | BindingFlags.Static);
					}
				}
				return instructions;
			}

			public static float GetLayerDepth()
			{
				if (!Config.ModEnabled)
					return 0.0016f;

				return 0.0016f + 0.0016f;
			}
		}

		public class Utility_canGrabSomethingFromHere_Patch
		{
			public static bool Prefix(ref bool __result)
			{
				if (!Config.ModEnabled || Game1.currentLocation is null || !Context.IsWorldReady)
					return true;

				foreach (NPC npc in Game1.currentLocation.characters)
				{
					if (npc is Horse horse && !npc.Name.StartsWith("tractor/"))
					{
						if (TryUpdateCursorForHitchedHorse(horse))
						{
							__result = true;
							return false;
						}
					}
				}
				foreach (Farmer farmer in Game1.currentLocation.farmers)
				{
					if (farmer.isRidingHorse() && farmer.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
					{
						if (TryUpdateCursorForHitchedHorse(farmer.mount))
						{
							__result = true;
							return false;
						}
					}
				}
				try
				{
                    if (Game1.getFarm().modData.TryGetValue($"{parkedListKey}/{Game1.currentLocation.NameOrUniqueName}", out string parkedString))
                    {
                        List<ParkedTravelingCart> parkedTravelingCarts = JsonConvert.DeserializeObject<List<ParkedTravelingCart>>(parkedString);

                        foreach (ParkedTravelingCart parkedTravelingCart in parkedTravelingCarts)
                        {
                            if (TryUpdateCursorForParkedTravelingCart(parkedTravelingCart))
                            {
                                __result = true;
                                return false;
                            }
                        }
                    }
                }
				catch { }
                return true;
			}
		}

		public class GameLocation_checkAction_Patch
		{
			public static bool Prefix(Farmer who, ref bool __result)
			{
				if (!Config.ModEnabled)
					return true;

				foreach (NPC npc in Game1.currentLocation.characters)
				{
					if (npc is Horse horse)
					{
						if (CheckActionForNonParkedTravelingCart(horse, ref __result, Game1.player != horse.rider))
						{
							return false;
						}
					}
				}
				foreach (Farmer farmer in Game1.currentLocation.farmers)
				{
					if (farmer.isRidingHorse() && farmer.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
					{
						if (CheckActionForNonParkedTravelingCart(farmer.mount, ref __result, Game1.player != who))
						{
							return false;
						}
					}
				}
				if (Game1.getFarm().modData.TryGetValue($"{parkedListKey}/{Game1.currentLocation.NameOrUniqueName}", out string parkedString))
				{
					List<ParkedTravelingCart> parkedTravelingCarts = JsonConvert.DeserializeObject<List<ParkedTravelingCart>>(parkedString);

					foreach (ParkedTravelingCart parkedTravelingCart in parkedTravelingCarts)
					{
						if (CheckActionForParkedTravelingCart(parkedTravelingCart, ref __result))
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		public class GameLocation_isCollidingPosition_Patch
		{
			public static void Postfix(GameLocation __instance, Rectangle position, ref bool __result)
			{
				if (!Config.ModEnabled || !Config.CollisionsEnabled || __result)
					return;

				foreach (NPC npc in Game1.currentLocation.characters)
				{
					if (npc is Horse horse)
					{
						if (CheckCollidingPositionForNonParkedTravelingCart(position, horse, ref __result))
						{
							return;
						}
					}
				}
				foreach (Farmer farmer in Game1.currentLocation.farmers)
				{
					if (farmer.isRidingHorse())
					{
						if (CheckCollidingPositionForNonParkedTravelingCart(position, farmer.mount, ref __result))
						{
							return;
						}
					}
				}
				if (Game1.getFarm().modData.TryGetValue($"{parkedListKey}/{__instance.NameOrUniqueName}", out string parkedString))
				{
					List<ParkedTravelingCart> parkedTravelingCarts = JsonConvert.DeserializeObject<List<ParkedTravelingCart>>(parkedString);

					foreach (ParkedTravelingCart parkedTravelingCart in parkedTravelingCarts)
					{
						if (CheckCollidingPositionForParkedTravelingCart(position, parkedTravelingCart, ref __result))
						{
							return;
						}
					}
				}
			}
		}

		public class GameLocation_draw_Patch
		{
			public static void Postfix(GameLocation __instance, SpriteBatch b)
			{
				if (!Config.ModEnabled || !Game1.getFarm().modData.TryGetValue($"{parkedListKey}/{__instance.NameOrUniqueName}", out string parkedString))
					return;

				List<ParkedTravelingCart> parkedTravelingCarts = JsonConvert.DeserializeObject<List<ParkedTravelingCart>>(parkedString);

				foreach (ParkedTravelingCart parkedTravelingCart in parkedTravelingCarts)
				{
					TravelingCart data = GetCartData(parkedTravelingCart.whichCart);
					TravelingCart.DirectionData ddata = data.GetDirectionData(parkedTravelingCart.facingDirection);
					Vector2 localPosition = Game1.GlobalToLocal(parkedTravelingCart.position + ddata.cartOffset);
					float layerDepth = (parkedTravelingCart.position.Y + (parkedTravelingCart.facingDirection == 0 ? ddata.clickRect.Height * Game1.pixelZoom : 0f) + ddata.cartOffset.Y + (ddata.hitchRect.Y + ddata.hitchRect.Height) * Game1.pixelZoom + Game1.tileSize) / 10000f;

					b.Draw(data.spriteSheet, localPosition, ddata.backRect, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, layerDepth - 0.005f);
					b.Draw(data.spriteSheet, localPosition, ddata.middleRect, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, layerDepth - 0.0001f);
					b.Draw(data.spriteSheet, localPosition, ddata.frontRect, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, layerDepth + 0.0001f);
				}
			}
		}

		public class GameLocation_drawBackground_Patch
		{
			internal static PerScreen<Tile> lastTile = new(() => null);

			internal static Tile LastTile
			{
				get => lastTile.Value;
				set => lastTile.Value = value;
			}

			public static void Postfix(GameLocation __instance)
			{
				if (!Config.ModEnabled || !__instance.NameOrUniqueName.StartsWith(SModManifest.UniqueID))
					return;

				(GameLocation parentLocation, Point outerTile, Point position) = GetOutdoorsInfos(__instance);
				Tile playerTile = __instance.Map.GetLayer("Back").PickTile(new Location((int)Game1.player.Position.X, (int)Game1.player.Position.Y), Game1.viewport.Size);

				if (playerTile is not null && playerTile != LastTile && playerTile.Properties.ContainsKey("Leave"))
				{
					if (parentLocation is null)
					{
						SMonitor.Log($"Warping to farm");
						Game1.warpFarmer("Farm", Game1.getFarm().GetMainFarmHouseEntry().X, Game1.getFarm().GetMainFarmHouseEntry().Y, false);
					}
					else
					{
						SMonitor.Log($"Warping to parent location {parentLocation.NameOrUniqueName}");
						Game1.warpFarmer(new LocationRequest(parentLocation.NameOrUniqueName, false, parentLocation), outerTile.X, outerTile.Y, 2);
					}
				}
				LastTile = playerTile;
				if (Config.DrawCartExterior && parentLocation is not null && deltaTime is not null && Game1.game1.screen is not null)
				{
					GameLocation currentLocation = Game1.currentLocation;
					xTile.Dimensions.Rectangle currentLocationViewport = Game1.viewport;

					drawingExterior = true;
					Game1.currentLocation = parentLocation;
					Game1.viewport = new xTile.Dimensions.Rectangle(currentLocationViewport.X + position.X, currentLocationViewport.Y + position.Y, currentLocationViewport.Width, currentLocationViewport.Height);
					try
					{
						if (!Context.IsMainPlayer)
						{
							if (parentLocation is BathHousePool bathHousePool)
							{
								typeof(BathHousePool).GetField("steamPosition", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(bathHousePool, new Vector2(-Game1.viewport.X, -Game1.viewport.Y));
								typeof(BathHousePool).GetField("steamAnimation", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(bathHousePool, Game1.temporaryContent.Load<Texture2D>("LooseSprites\\steamAnimation"));
								typeof(BathHousePool).GetField("swimShadow", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(bathHousePool, Game1.temporaryContent.Load<Texture2D>("LooseSprites\\swimShadow"));
							}
							if (parentLocation is BeachNightMarket beachNightMarket)
							{
								typeof(BeachNightMarket).GetField("shopClosedTexture", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(beachNightMarket, Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"));
								typeof(BeachNightMarket).GetField("paintingMailKey", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(beachNightMarket, "NightMarketYear" + Game1.year + "Day" + beachNightMarket.getDayOfNightMarket() + "_paintingSold");
							}
							if (parentLocation is MermaidHouse mermaidHouse)
							{
								typeof(MermaidHouse).GetField("mermaidSprites", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mermaidHouse, Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"));
								typeof(MermaidHouse).GetField("finalLeftMermaidAlpha", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mermaidHouse, 0f);
								typeof(MermaidHouse).GetField("finalRightMermaidAlpha", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mermaidHouse, 0f);
								typeof(MermaidHouse).GetField("finalBigMermaidAlpha", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mermaidHouse, 0f);
								typeof(MermaidHouse).GetField("blackBGAlpha", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mermaidHouse, 0f);
								typeof(MermaidHouse).GetField("bigMermaidAlpha", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mermaidHouse, 0f);
								typeof(MermaidHouse).GetField("oldStopWatchTime", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mermaidHouse, 0f);
								typeof(MermaidHouse).GetField("showTimer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mermaidHouse, 0f);
								typeof(MermaidHouse).GetField("curtainMovement", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mermaidHouse, 0f);
								typeof(MermaidHouse).GetField("curtainOpenPercent", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mermaidHouse, 0f);
								typeof(MermaidHouse).GetField("fairyTimer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mermaidHouse, 0f);
								typeof(MermaidHouse).GetField("stopWatch", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mermaidHouse, new Stopwatch());
								typeof(MermaidHouse).GetField("bubbles", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mermaidHouse, new List<Vector2>());
								typeof(MermaidHouse).GetField("sparkles", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mermaidHouse, new TemporaryAnimatedSpriteList());
								typeof(MermaidHouse).GetField("alwaysFrontTempSprites", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mermaidHouse, new TemporaryAnimatedSpriteList());
								typeof(MermaidHouse).GetField("lastFiveClamTones", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mermaidHouse, new List<int>());
								typeof(MermaidHouse).GetField("pearlRecipient", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mermaidHouse, null);
								typeof(MermaidHouse).GetField("mermaidFrames", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mermaidHouse, new int[93]
								{
									1, 0, 2, 0, 1, 0, 2, 0, 3, 3,
									3, 4, 3, 3, 3, 4, 3, 3, 3, 4,
									3, 3, 3, 4, 3, 3, 3, 4, 3, 3,
									4, 4, 3, 3, 3, 3, 0, 0, 0, 0,
									3, 3, 3, 4, 3, 3, 3, 4, 3, 3,
									3, 4, 3, 3, 3, 4, 3, 3, 3, 4,
									3, 3, 4, 4, 3, 3, 3, 3, 0, 0,
									0, 0, 3, 3, 3, 3, 4, 4, 4, 4,
									3, 3, 3, 3, 0, 0, 5, 6, 5, 6,
									7, 8, 8
								});
							}
							if (parentLocation is Submarine submarine)
							{
								typeof(Submarine).GetField("submarineSprites", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(submarine, Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"));
								typeof(Submarine).GetField("ambientLightTargetColor", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(submarine, Color.Black);
								typeof(Submarine).GetField("hasLitSubmergeLight", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(submarine, false);
								typeof(Submarine).GetField("curtainOpenPercent", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(submarine, 0f);
								typeof(Submarine).GetField("curtainMovement", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(submarine, 0f);
								typeof(Submarine).GetField("submergeTimer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(submarine, 0f);
								typeof(Submarine).GetField("hasLitAscendLight", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(submarine, false);
								typeof(Submarine).GetField("doneUntilReset", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(submarine, false);
								typeof(Submarine).GetField("localAscending", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(submarine, false);
							}
							if (parentLocation is BoatTunnel boatTunnel)
							{
								boatTunnel.critters = new List<Critter>();
								typeof(BoatTunnel).GetField("boatTexture", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(boatTunnel, Game1.temporaryContent.Load<Texture2D>("LooseSprites\\WillysBoat"));
								boatTunnel.ResetBoat();
							}
							if (parentLocation is Club club)
							{
								typeof(Club).GetField("coinBuffer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(club, (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh) ? "\u3000\u3000" : "  ");
							}
							if (parentLocation is DesertFestival desertFestival)
							{
								typeof(DesertFestival).GetField("eggMoneyDial", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(desertFestival, new MoneyDial(4, playSound: false));
								(typeof(DesertFestival).GetField("eggMoneyDial", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(desertFestival) as MoneyDial).currentValue = Game1.player.Items.CountId("CalicoEgg");
							}
							if (parentLocation is IslandLocation islandLocation)
							{
								islandLocation.parrotPlatforms.Clear();
								islandLocation.parrotPlatforms = ParrotPlatform.CreateParrotPlatformsForArea(islandLocation);
								foreach (ParrotUpgradePerch parrotUpgradePerch in islandLocation.parrotUpgradePerches)
								{
									parrotUpgradePerch.ResetForPlayerEntry();
								}
								if (islandLocation is Caldera caldera)
								{
									caldera.mapBaseTilesheet = Game1.temporaryContent.Load<Texture2D>(caldera.map.RequireTileSheet(0, "dungeon").ImageSource);
									caldera.waterColor.Value = Color.White;
								}
								if (islandLocation is IslandWest islandWest)
								{
									islandWest.sandDuggy = new NetRef<SandDuggy>();
									typeof(IslandWest).GetField("shippingBinLid", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(islandWest, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(134, 226, 30, 25), new Vector2(islandWest.shippingBinPosition.X, islandWest.shippingBinPosition.Y - 1) * 64f + new Vector2(2f, -7f) * 4f, flipped: false, 0f, Color.White));
									typeof(IslandWest).GetField("shippingBinLidOpenArea", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(islandWest, new Rectangle((islandWest.shippingBinPosition.X - 1) * 64, (islandWest.shippingBinPosition.Y - 1) * 64, 256, 192));
								}
								if (islandLocation is IslandFarmCave islandFarmCave)
								{
									islandFarmCave.gourmand = new NPC(new AnimatedSprite("Characters\\Gourmand", 0, 32, 32), new Vector2(4f, 4f) * 64f, "IslandFarmCave", 2, "Gourmand", datable: false, Game1.content.Load<Texture2D>("Portraits\\SafariGuy"))
									{
										AllowDynamicAppearance = false
									};
									typeof(IslandFarmCave).GetField("smokeTexture", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(islandFarmCave, Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"));
									islandFarmCave.waterColor.Value = new Color(10, 250, 120);
								}
								if (islandLocation is IslandForestLocation islandForestLocation)
								{
									typeof(IslandForestLocation).GetField("_raySeed", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(islandForestLocation, 14087);
									typeof(IslandForestLocation).GetField("_rayTexture", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(islandForestLocation, Game1.temporaryContent.Load<Texture2D>("LooseSprites\\LightRays"));
									typeof(IslandForestLocation).GetField("_ambientLightColor", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(islandForestLocation, new Color(150, 120, 50));
									islandForestLocation.ignoreOutdoorLighting.Value = false;
									if (islandLocation is IslandEast islandEast)
									{
										typeof(IslandEast).GetField("_parrotTextures", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(islandEast, Game1.temporaryContent.Load<Texture2D>("LooseSprites\\parrots"));
									}
								}
								if (islandLocation is IslandSouth islandSouth)
								{
									islandSouth.boatTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\WillysBoat");
									islandSouth.ResetBoat();
								}
								if (islandLocation is IslandSouthEast islandSouthEast)
								{
									islandSouthEast.mermaidSprites = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
								}
							}
							if (parentLocation is IslandFarmHouse islandFarmHouse)
							{
								islandFarmHouse.fridgePosition = islandFarmHouse.GetFridgePositionFromMap() ?? Point.Zero;
							}
							if (parentLocation is JojaMart jojaMart)
							{
								typeof(JojaMart).GetField("communityDevelopmentTexture", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(jojaMart, Game1.temporaryContent.Load<Texture2D>("LooseSprites\\JojaCDForm"));
							}
							if (parentLocation is Mountain mountain)
							{
								Season season = mountain.GetSeason();

								typeof(Mountain).GetField("boulderSourceRect", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mountain, new Rectangle(439 + ((season == Season.Winter) ? 39 : 0), 1385, 39, 48));
								typeof(Mountain).GetField("raildroadBlocksourceRect", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mountain, new Rectangle(640, (season == Season.Spring) ? 2176 : 1453, 64, 80));;
							}
							if (parentLocation is Sewer sewer)
							{
								typeof(Sewer).GetField("steamPosition", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(sewer, new Vector2(0f, 0f));
								typeof(Sewer).GetField("steamAnimation", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(sewer, Game1.temporaryContent.Load<Texture2D>("LooseSprites\\steamAnimation"));
							}
						}
						Game1.spriteBatch.End();
						Game1.game1.DrawWorld(deltaTime, Game1.game1.screen);
						Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
						Game1.updateWeather(deltaTime);
					}
					catch (Exception ex)
					{
						SMonitor.Log($"Error while drawing parent location {parentLocation.NameOrUniqueName}: {ex.Message}", LogLevel.Trace);
					}
					Game1.viewport = currentLocationViewport;
					Game1.currentLocation = currentLocation;
					drawingExterior = false;
				}
			}
		}

		public class Game1_drawWeather_Patch
		{
			public static bool Prefix(GameTime time)
			{
				if (drawingExterior && !Config.DrawCartExteriorWeather)
				{
					return false;
				}
				if (!drawingExterior)
				{
					deltaTime = time;
				}
				return true;
			}
		}

		public class GameLocation_CanPlaceThisFurnitureHere_Patch
		{
			public static bool Prefix(GameLocation __instance, ref bool __result)
			{
				if (!Config.ModEnabled || !__instance.NameOrUniqueName.StartsWith(SModManifest.UniqueID))
					return true;

				__result = true;
				return false;
			}
		}

		public class Stable_grabHorse_Patch
		{
			public static bool Prefix(Stable __instance)
			{
				if (!Config.ModEnabled || Config.WarpHorsesOnDayStart)
					return true;

				Horse horse = Utility.findHorse(__instance.HorseId);

				if (horse is not null)
				{
					if (IsPassiveFestivalLocation(horse.currentLocation))
					{
						return true;
					}
					return false;
				}
				return true;
			}
		}

		public class Game1_warpCharacter_Patch
		{
			public static void Postfix(NPC character)
			{
				if (!Config.ModEnabled)
					return;

				if (character is Horse horse && !horse.modData.ContainsKey(parkedKey))
				{
					GameLocation interiorLocation = Game1.getLocationFromName($"{SModManifest.UniqueID}/{horse.HorseId}");

					if (interiorLocation is not null)
					{
						string which = horse.modData.TryGetValue(whichCartKey, out string value) && travelingCartDictionary.ContainsKey(value) ? value : defaultKey;
						TravelingCart.DirectionData ddata = GetDirectionData(travelingCartDictionary[which], horse.FacingDirection);
						Vector2 position = horse.Position + ddata.cartOffset + ddata.backRect.Size.ToVector2() * 2;
						int positionX = (int)position.X - interiorLocation.Map.GetLayer("Back").LayerWidth * 32;
						int positionY = (int)position.Y - interiorLocation.Map.GetLayer("Back").LayerHeight * 32;
						Point outerTile = horse.TilePoint;
						GameLocation parentLocation = horse.currentLocation;

						horse.modData[whichCartKey] = which;
						Game1.getFarm().modData[$"{outdoorsInfosKey}/{interiorLocation.NameOrUniqueName}"] = JsonConvert.SerializeObject((parentLocation.NameOrUniqueName, outerTile, new Point(positionX, positionY)));
					}
				}
			}
		}

		public class GameLocation_startSleep_Patch
		{
			public static bool Prefix()
			{
				if (Game1.player.currentLocation is not null && Game1.player.currentLocation.NameOrUniqueName.StartsWith($"{SModManifest.UniqueID}/"))
				{
					(GameLocation parentLocation, Point _, Point _) = GetOutdoorsInfos(Game1.player.currentLocation);

					if (parentLocation is null || !parentLocation.IsOutdoors)
					{
						Game1.showRedMessage(SHelper.Translation.Get("CannotSleep"));
						return false;
					}
					if (IsFestivalDay(parentLocation, 0, true))
					{
						Game1.showRedMessage(SHelper.Translation.Get("CannotSleepFestivalToday"));
						return false;
					}
					if (IsFestivalDay(parentLocation, 1))
					{
						Game1.showRedMessage(SHelper.Translation.Get("CannotSleepFestivalTomorrow"));
						return false;
					}
				}
				return true;
			}
		}
	}
}
