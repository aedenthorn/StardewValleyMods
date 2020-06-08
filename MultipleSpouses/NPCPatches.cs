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
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;

namespace MultipleSpouses
{
	public static class NPCPatches
	{
		private static IMonitor Monitor;

		// call this method from your Entry class
		public static void Initialize(IMonitor monitor)
		{
			Monitor = monitor;
		}

		public static string[] csMarriageDialoguesReplace = new string[]
		{
			"NPC.cs.4406",
			"NPC.cs.4431",
			"NPC.cs.4427",
			"NPC.cs.4429",
			"NPC.cs.4439",
			"NPC.cs.4442",
			"NPC.cs.4443",
			"NPC.cs.4446",
			"NPC.cs.4449",
			"NPC.cs.4452",
			"NPC.cs.4455",
			"NPC.cs.4462",
			"NPC.cs.4470",
			"NPC.cs.4474",
			"NPC.cs.4481",
			"NPC.cs.4488",
			"NPC.cs.4489",
			"NPC.cs.4490",
			"NPC.cs.4496",
			"NPC.cs.4497",
			"NPC.cs.4498",
			"NPC.cs.4499",
			"NPC.cs.4440",
			"NPC.cs.4441",
			"NPC.cs.4444",
			"NPC.cs.4445",
			"NPC.cs.4447",
			"NPC.cs.4448",
			"NPC.cs.4463",
			"NPC.cs.4465",
			"NPC.cs.4466",
			"NPC.cs.4486",
			"NPC.cs.4488",
			"NPC.cs.4489",
			"NPC.cs.4490",
			"NPC.cs.4496",
			"NPC.cs.4497",
			"NPC.cs.4498",
			"NPC.cs.4499",
			"NPC.cs.4500",
		};

		public static string[][] csMarriageDialoguesChoose = new string[][]
		{
			new string[]
			{
				"NPC.cs.4420",
				"NPC.cs.4421",
				"NPC.cs.4422",
				"NPC.cs.4423",
				"NPC.cs.4424",
				"NPC.cs.4425",
				"NPC.cs.4426",
				"NPC.cs.4432",
				"NPC.cs.4433",
			},
			new string[]
			{
				"NPC.cs.4434",
				"NPC.cs.4435",
				"NPC.cs.4436",
				"NPC.cs.4437",
				"NPC.cs.4438",
			},
		};

		public static string[][] marriageDialogues = new string[][]
		{
			new string[]{
				"Indoor_Day_0",
				"Indoor_Day_1",
				"Indoor_Day_2",
				"Indoor_Day_3",
				"Indoor_Day_4",
			},
			new string[]{
				"OneKid_0",
				"OneKid_1",
				"OneKid_2",
				"OneKid_3",
			},
			new string[]{
				"Outdoor_0",
				"Outdoor_1",
				"Outdoor_2",
				"Outdoor_3",
				"Outdoor_4",
			},
			new string[]{
				"Rainy_Day_0",
				"Rainy_Day_1",
				"Rainy_Day_2",
				"Rainy_Day_3",
				"Rainy_Day_4",
			},
			new string[]{
				"TwoKids_0",
				"TwoKids_1",
				"TwoKids_2",
				"TwoKids_3",
			},
			new string[]{
				"Good_0",
				"Good_1",
				"Good_2",
				"Good_3",
				"Good_4",
				"Good_5",
				"Good_6",
				"Good_7",
				"Good_8",
				"Good_9",
			},
			new string[]{
				"Neutral_0",
				"Neutral_1",
				"Neutral_2",
				"Neutral_3",
				"Neutral_4",
				"Neutral_5",
				"Neutral_6",
				"Neutral_7",
				"Neutral_8",
				"Neutral_9",
			},
			new string[]{
				"Bad_0",
				"Bad_1",
				"Bad_2",
				"Bad_3",
				"Bad_4",
				"Bad_5",
				"Bad_6",
				"Bad_7",
				"Bad_8",
				"Bad_9",
			},
		};

		public static bool NPC_setUpForOutdoorPatioActivity_Prefix(NPC __instance)
		{
			if (ModEntry.outdoorSpouse != __instance.Name)
			{
				return false;
			}
			ModEntry.PMonitor.Log("is outdoor spouse: " + __instance.Name);
			return true;
		}

		public static bool NPC_checkAction_Prefix(ref NPC __instance, ref Farmer who, ref bool __result)
		{
			try
			{
				ModEntry.ResetSpouses(who);

				if ((__instance.Name.Equals(who.spouse) || ModEntry.spouses.ContainsKey(__instance.Name)) && who.IsLocalPlayer)
				{
					int timeOfDay = Game1.timeOfDay;
					if (__instance.Sprite.CurrentAnimation == null)
					{
						__instance.faceDirection(-3);
					}
					if (__instance.Sprite.CurrentAnimation == null && who.friendshipData.ContainsKey(__instance.name) && who.friendshipData[__instance.name].Points >= 3125 && !who.mailReceived.Contains("CF_Spouse"))
					{
						__instance.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString(Game1.player.isRoommate(who.spouse) ? "Strings\\StringsFromCSFiles:Krobus_Stardrop" : "Strings\\StringsFromCSFiles:NPC.cs.4001"), __instance));
						Game1.player.addItemByMenuIfNecessary(new StardewValley.Object(Vector2.Zero, 434, "Cosmic Fruit", false, false, false, false), null);
						__instance.shouldSayMarriageDialogue.Value = false;
						__instance.currentMarriageDialogue.Clear();
						who.mailReceived.Add("CF_Spouse");
						__result = true;
						return false;
					}
					if (__instance.Sprite.CurrentAnimation == null && !__instance.hasTemporaryMessageAvailable() && __instance.currentMarriageDialogue.Count == 0 && __instance.CurrentDialogue.Count == 0 && Game1.timeOfDay < 2200 && !__instance.isMoving() && who.ActiveObject == null)
					{
						__instance.faceGeneralDirection(who.getStandingPosition(), 0, false);
						who.faceGeneralDirection(__instance.getStandingPosition(), 0, false);
						if (__instance.FacingDirection == 3 || __instance.FacingDirection == 1)
						{
							int spouseFrame = 28;
							bool facingRight = true;
							string name = __instance.Name;
							if (name == "Sam")
							{
								spouseFrame = 36;
								facingRight = true;
							}
							else if (name == "Penny")
							{
								spouseFrame = 35;
								facingRight = true;
							}
							else if (name == "Sebastian")
							{
								spouseFrame = 40;
								facingRight = false;
							}
							else if (name == "Alex")
							{
								spouseFrame = 42;
								facingRight = true;
							}
							else if (name == "Krobus")
							{
								spouseFrame = 16;
								facingRight = true;
							}
							else if (name == "Maru")
							{
								spouseFrame = 28;
								facingRight = false;
							}
							else if (name == "Emily")
							{
								spouseFrame = 33;
								facingRight = false;
							}
							else if (name == "Harvey")
							{
								spouseFrame = 31;
								facingRight = false;
							}
							else if (name == "Shane")
							{
								spouseFrame = 34;
								facingRight = false;
							}
							else if (name == "Elliott")
							{
								spouseFrame = 35;
								facingRight = false;
							}
							else if (name == "Leah")
							{
								spouseFrame = 25;
								facingRight = true;
							}
							else if (name == "Abigail")
							{
								spouseFrame = 33;
								facingRight = false;
							}
							bool flip = (facingRight && __instance.FacingDirection == 3) || (!facingRight && __instance.FacingDirection == 1);
							if (who.getFriendshipHeartLevelForNPC(__instance.Name) > 9)
							{
								int delay = Game1.IsMultiplayer ? 1000 : 10;
								__instance.movementPause = delay;
								__instance.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
							{
								new FarmerSprite.AnimationFrame(spouseFrame, delay, false, flip, new AnimatedSprite.endOfAnimationBehavior(__instance.haltMe), true)
							});
								if (!__instance.hasBeenKissedToday.Value)
								{
									who.changeFriendship(10, __instance);
								}
								if (who.hasCurrentOrPendingRoommate())
								{
									ModEntry.mp.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite("LooseSprites\\emojis", new Microsoft.Xna.Framework.Rectangle(0, 0, 9, 9), 2000f, 1, 0, new Vector2((float)__instance.getTileX(), (float)__instance.getTileY()) * 64f + new Vector2(16f, -64f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
										{
											motion = new Vector2(0f, -0.5f),
											alphaFade = 0.01f
										}
									});
								}
								else
								{
									ModEntry.mp.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(211, 428, 7, 6), 2000f, 1, 0, new Vector2((float)__instance.getTileX(), (float)__instance.getTileY()) * 64f + new Vector2(16f, -64f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
										{
											motion = new Vector2(0f, -0.5f),
											alphaFade = 0.01f
										}
									});
								}
								if (ModEntry.config.RealKissSound && ModEntry.kissEffect != null)
								{
									ModEntry.kissEffect.Play();
								}
								else
								{
									__instance.currentLocation.playSound("dwop", NetAudio.SoundContext.NPC);
								}
								who.exhausted.Value = false;
								__instance.hasBeenKissedToday.Value = true;
								__instance.Sprite.UpdateSourceRect();
							}
							else
							{
								__instance.faceDirection((ModEntry.myRand.NextDouble() < 0.5) ? 2 : 0);
								__instance.doEmote(12, true);
							}
							int playerFaceDirection = 1;
							if ((facingRight && !flip) || (!facingRight && flip))
							{
								playerFaceDirection = 3;
							}
							who.PerformKiss(playerFaceDirection);
							who.CanMove = false;
							who.FarmerSprite.PauseForSingleAnimation = false;
							who.FarmerSprite.animateOnce(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(101, 1000, 0, false, who.FacingDirection == 3, null, false, 0),
							new FarmerSprite.AnimationFrame(6, 1, false, who.FacingDirection == 3, new AnimatedSprite.endOfAnimationBehavior(Farmer.completelyStopAnimating), false)
						}.ToArray(), null);
							__result = true;
							return false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(NPC_checkAction_Prefix)}:\n{ex}", LogLevel.Error);
			}
			return true;
		}


		public static void NPC_marriageDuties_Prefix(NPC __instance, string ___nameOfTodaysSchedule, Dictionary<int, SchedulePathDescription> ___schedule)
        {
			try
			{
				if (!Game1.newDay && Game1.gameMode != 6)
				{
					return;
				}
				Farmer spouse = __instance.getSpouse();
				if (spouse != null)
				{
					__instance.shouldSayMarriageDialogue.Value = true;
					__instance.DefaultMap = spouse.homeLocation.Value;
					FarmHouse farmHouse = Utility.getHomeOfFarmer(spouse);
					Random r = new Random((int)(Game1.stats.DaysPlayed + (uint)((int)Game1.uniqueIDForThisGame / 2) + (uint)((int)spouse.UniqueMultiplayerID)));
					int heartsWithSpouse = spouse.getFriendshipHeartLevelForNPC(__instance.Name);
					if (Game1.IsMasterGame && (__instance.currentLocation == null || !__instance.currentLocation.Equals(farmHouse)))
					{
						Game1.warpCharacter(__instance, spouse.homeLocation.Value, farmHouse.getBedSpot());
					}
					if (Game1.isRaining)
					{
						__instance.marriageDefaultDialogue.Value = new MarriageDialogueReference("MarriageDialogue", "Rainy_Day_" + r.Next(5), false, new string[0]);
					}
					else
					{
						__instance.marriageDefaultDialogue.Value = new MarriageDialogueReference("MarriageDialogue", "Indoor_Day_" + r.Next(5), false, new string[0]);
					}
					__instance.currentMarriageDialogue.Add(new MarriageDialogueReference(__instance.marriageDefaultDialogue.Value.DialogueFile, __instance.marriageDefaultDialogue.Value.DialogueKey, __instance.marriageDefaultDialogue.Value.IsGendered, __instance.marriageDefaultDialogue.Value.Substitutions));
					if (spouse.GetSpouseFriendship().DaysUntilBirthing == 0)
					{
						__instance.setTilePosition(farmHouse.getKitchenStandingSpot());
						__instance.currentMarriageDialogue.Clear();
						return;
					}
					if (__instance.daysAfterLastBirth >= 0)
					{
						__instance.daysAfterLastBirth--;
						int kids = __instance.getSpouse().getChildrenCount();
						if (kids == 1)
						{
							__instance.setTilePosition(farmHouse.getKitchenStandingSpot());
							if (!__instance.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false, new string[0]), farmHouse, false))
							{
								__instance.currentMarriageDialogue.Clear();
								__instance.addMarriageDialogue("MarriageDialogue", "OneKid_" + r.Next(4), false, new string[0]);
							}
							return;
						}
						if (kids == 2)
						{
							__instance.setTilePosition(farmHouse.getKitchenStandingSpot());
							if (!__instance.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false, new string[0]), farmHouse, false))
							{
								__instance.currentMarriageDialogue.Clear();
								__instance.addMarriageDialogue("MarriageDialogue", "TwoKids_" + r.Next(4), false, new string[0]);
							}
							return;
						}
					}
					__instance.setTilePosition(farmHouse.getKitchenStandingSpot());
					if (__instance.tryToGetMarriageSpecificDialogueElseReturnDefault(Game1.currentSeason + "_" + Game1.dayOfMonth, "").Length > 0)
					{
						if (spouse != null)
						{
							__instance.currentMarriageDialogue.Clear();
							__instance.addMarriageDialogue("MarriageDialogue", Game1.currentSeason + "_" + Game1.dayOfMonth, false, new string[0]);
						}
						return;
					}
					if (___schedule != null)
					{
						if (___nameOfTodaysSchedule.Equals("marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
						{
							__instance.currentMarriageDialogue.Clear();
							__instance.addMarriageDialogue("MarriageDialogue", "funLeave_" + __instance.Name, false, new string[0]);
							return;
						}
						if (___nameOfTodaysSchedule.Equals("marriageJob"))
						{
							__instance.currentMarriageDialogue.Clear();
							__instance.addMarriageDialogue("MarriageDialogue", "jobLeave_" + __instance.Name, false, new string[0]);
						}
						return;
					}
					else
					{
						if (!Game1.isRaining && !Game1.IsWinter && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat") && spouse == Game1.MasterPlayer && !__instance.Name.Equals("Krobus"))
						{
							__instance.setUpForOutdoorPatioActivity();
							return;
						}
						if (spouse.GetDaysMarried() >= 1 && r.NextDouble() < (double)(1f - (float)Math.Max(1, heartsWithSpouse) / 12f))
						{
							Furniture f = farmHouse.getRandomFurniture(r);
							if (f != null && f.isGroundFurniture())
							{
								Point p = new Point((int)f.tileLocation.X - 1, (int)f.tileLocation.Y);
								if (farmHouse.isTileLocationTotallyClearAndPlaceable(p.X, p.Y))
								{
									__instance.setTilePosition(p);
									__instance.faceDirection(1);
									switch (r.Next(10))
									{
										case 0:
											__instance.currentMarriageDialogue.Clear();
											__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4420", false, new string[0]);
											return;
										case 1:
											__instance.currentMarriageDialogue.Clear();
											__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4421", false, new string[0]);
											return;
										case 2:
											__instance.currentMarriageDialogue.Clear();
											__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4422", true, new string[0]);
											return;
										case 3:
											__instance.currentMarriageDialogue.Clear();
											__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4423", false, new string[0]);
											return;
										case 4:
											__instance.currentMarriageDialogue.Clear();
											__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4424", false, new string[0]);
											return;
										case 5:
											__instance.currentMarriageDialogue.Clear();
											__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4425", false, new string[0]);
											return;
										case 6:
											__instance.currentMarriageDialogue.Clear();
											__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4426", false, new string[0]);
											return;
										case 7:
											if (__instance.Gender != 1)
											{
												__instance.currentMarriageDialogue.Clear();
												__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4431", false, new string[0]);
												return;
											}
											if (r.NextDouble() < 0.5)
											{
												__instance.currentMarriageDialogue.Clear();
												__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4427", false, new string[0]);
												return;
											}
											__instance.currentMarriageDialogue.Clear();
											__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4429", false, new string[0]);
											return;
										case 8:
											__instance.currentMarriageDialogue.Clear();
											__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4432", false, new string[0]);
											return;
										case 9:
											__instance.currentMarriageDialogue.Clear();
											__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4433", false, new string[0]);
											return;
										default:
											return;
									}
								}
							}
							switch (r.Next(5))
							{
								case 0:
									new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4434", false, new string[0]);
									break;
								case 1:
									new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4435", false, new string[0]);
									break;
								case 2:
									new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4436", false, new string[0]);
									break;
								case 3:
									new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4437", true, new string[0]);
									break;
								case 4:
									new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4438", false, new string[0]);
									break;
							}
							__instance.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false, new string[0]), farmHouse, true);
							return;
						}
						Friendship friendship = spouse.GetSpouseFriendship();
						if (friendship.DaysUntilBirthing != -1 && friendship.DaysUntilBirthing <= 7 && r.NextDouble() < 0.5)
						{
							if (__instance.isGaySpouse())
							{
								__instance.setTilePosition(farmHouse.getKitchenStandingSpot());
								if (!__instance.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4439", false, new string[0]), farmHouse, false))
								{
									if (r.NextDouble() < 0.5)
									{
										__instance.currentMarriageDialogue.Clear();
									}
									if (r.NextDouble() < 0.5)
									{
										__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4440", false, new string[]
										{
										__instance.getSpouse().displayName
										});
										return;
									}
									__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4441", false, new string[]
									{
									"%endearment"
									});
									return;
								}
							}
							else if (__instance.Gender == 1)
							{
								__instance.setTilePosition(farmHouse.getKitchenStandingSpot());
								if (!__instance.spouseObstacleCheck((r.NextDouble() < 0.5) ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4442", false, new string[0]) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4443", false, new string[0]), farmHouse, false))
								{
									if (r.NextDouble() < 0.5)
									{
										__instance.currentMarriageDialogue.Clear();
									}
									__instance.currentMarriageDialogue.Add((r.NextDouble() < 0.5) ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4444", false, new string[]
									{
									__instance.getSpouse().displayName
									}) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4445", false, new string[]
									{
									"%endearment"
									}));
									return;
								}
							}
							else
							{
								__instance.setTilePosition(farmHouse.getKitchenStandingSpot());
								if (!__instance.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4446", true, new string[0]), farmHouse, false))
								{
									if (r.NextDouble() < 0.5)
									{
										__instance.currentMarriageDialogue.Clear();
									}
									__instance.currentMarriageDialogue.Add((r.NextDouble() < 0.5) ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4447", true, new string[]
									{
									__instance.getSpouse().displayName
									}) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4448", false, new string[]
									{
									"%endearment"
									}));
								}
							}
							return;
						}
						if (r.NextDouble() < 0.07)
						{
							int kids2 = __instance.getSpouse().getChildrenCount();
							if (kids2 == 1)
							{
								__instance.setTilePosition(farmHouse.getKitchenStandingSpot());
								if (!__instance.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4449", true, new string[0]), farmHouse, false))
								{
									__instance.currentMarriageDialogue.Clear();
									__instance.addMarriageDialogue("MarriageDialogue", "OneKid_" + r.Next(4), false, new string[0]);
								}
								return;
							}
							if (kids2 == 2)
							{
								__instance.setTilePosition(farmHouse.getKitchenStandingSpot());
								if (!__instance.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4452", true, new string[0]), farmHouse, false))
								{
									__instance.currentMarriageDialogue.Clear();
									__instance.addMarriageDialogue("MarriageDialogue", "TwoKids_" + r.Next(4), false, new string[0]);
								}
								return;
							}
						}
						Farm farm = Game1.getFarm();
						if (__instance.currentMarriageDialogue.Count > 0 && __instance.currentMarriageDialogue[0].IsItemGrabDialogue(__instance))
						{
							__instance.setTilePosition(farmHouse.getKitchenStandingSpot());
							__instance.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4455", true, new string[0]), farmHouse, false);
							return;
						}
						if (!Game1.isRaining && r.NextDouble() < 0.4 && !NPC.checkTileOccupancyForSpouse(farm, Utility.PointToVector2(farmHouse.getPorchStandingSpot()), "") && !__instance.Name.Equals("Krobus"))
						{
							bool filledBowl = false;
							if (!farm.petBowlWatered.Value && !NPC.hasSomeoneFedThePet)
							{
								filledBowl = true;
								farm.petBowlWatered.Set(true);
								NPC.hasSomeoneFedThePet = true;
							}
							if (r.NextDouble() < 0.6 && !Game1.currentSeason.Equals("winter") && !NPC.hasSomeoneWateredCrops)
							{
								Vector2 origin = Vector2.Zero;
								int tries = 0;
								bool foundWatered = false;
								while (tries < Math.Min(50, farm.terrainFeatures.Count()) && origin.Equals(Vector2.Zero))
								{
									int index = r.Next(farm.terrainFeatures.Count());
									if (farm.terrainFeatures.Pairs.ElementAt(index).Value is HoeDirt)
									{
										if ((farm.terrainFeatures.Pairs.ElementAt(index).Value as HoeDirt).needsWatering())
										{
											origin = farm.terrainFeatures.Pairs.ElementAt(index).Key;
										}
										else if ((farm.terrainFeatures.Pairs.ElementAt(index).Value as HoeDirt).crop != null)
										{
											foundWatered = true;
										}
									}
									tries++;
								}
								if (!origin.Equals(Vector2.Zero))
								{
									Microsoft.Xna.Framework.Rectangle wateringArea = new Microsoft.Xna.Framework.Rectangle((int)origin.X - 30, (int)origin.Y - 30, 60, 60);
									Vector2 currentPosition = default(Vector2);
									for (int x = wateringArea.X; x < wateringArea.Right; x++)
									{
										for (int y = wateringArea.Y; y < wateringArea.Bottom; y++)
										{
											currentPosition.X = (float)x;
											currentPosition.Y = (float)y;
											if (farm.isTileOnMap(currentPosition) && farm.terrainFeatures.ContainsKey(currentPosition) && farm.terrainFeatures[currentPosition] is HoeDirt && Game1.IsMasterGame && (farm.terrainFeatures[currentPosition] as HoeDirt).needsWatering())
											{
												(farm.terrainFeatures[currentPosition] as HoeDirt).state.Value = 1;
											}
										}
									}
									__instance.faceDirection(2);
									__instance.currentMarriageDialogue.Clear();
									__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4462", true, new string[0]);
									if (filledBowl)
									{
										__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, new string[]
										{
										Game1.player.getPetDisplayName()
										});
									}
									__instance.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false, new string[0]);
									NPC.hasSomeoneWateredCrops = true;
								}
								else
								{
									__instance.faceDirection(2);
									if (foundWatered)
									{
										__instance.currentMarriageDialogue.Clear();
										if (Game1.gameMode == 6)
										{
											if (r.NextDouble() < 0.5)
											{
												__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4465", false, new string[]
												{
												"%endearment"
												});
											}
											else
											{
												__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4466", false, new string[]
												{
												"%endearment"
												});
												__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4462", true, new string[0]);
												if (filledBowl)
												{
													__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, new string[]
													{
													Game1.player.getPetDisplayName()
													});
												}
											}
										}
										else
										{
											__instance.currentMarriageDialogue.Clear();
											__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4470", true, new string[0]);
										}
									}
									else
									{
										__instance.currentMarriageDialogue.Clear();
										__instance.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false, new string[0]);
									}
								}
							}
							else if (r.NextDouble() < 0.6 && !NPC.hasSomeoneFedTheAnimals)
							{
								bool fedAnything = false;
								foreach (Building b in farm.buildings)
								{
									if ((b is Barn || b is Coop) && b.daysOfConstructionLeft <= 0)
									{
										if (Game1.IsMasterGame)
										{
											(b.indoors.Value as AnimalHouse).feedAllAnimals();
										}
										fedAnything = true;
									}
								}
								__instance.faceDirection(2);
								if (fedAnything)
								{
									NPC.hasSomeoneFedTheAnimals = true;
									__instance.currentMarriageDialogue.Clear();
									__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4474", true, new string[0]);
									if (filledBowl)
									{
										__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, new string[]
										{
										Game1.player.getPetDisplayName()
										});
									}
									__instance.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false, new string[0]);
								}
								else
								{
									__instance.currentMarriageDialogue.Clear();
									__instance.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false, new string[0]);
								}
								if (Game1.IsMasterGame)
								{
									farm.petBowlWatered.Set(true);
								}
							}
							else if (!NPC.hasSomeoneRepairedTheFences)
							{
								int tries2 = 0;
								__instance.faceDirection(2);
								Vector2 origin2 = Vector2.Zero;
								while (tries2 < Math.Min(50, farm.objects.Count()) && origin2.Equals(Vector2.Zero))
								{
									int index2 = r.Next(farm.objects.Count());
									if (farm.objects.Pairs.ElementAt(index2).Value is Fence)
									{
										origin2 = farm.objects.Pairs.ElementAt(index2).Key;
									}
									tries2++;
								}
								if (!origin2.Equals(Vector2.Zero))
								{
									Microsoft.Xna.Framework.Rectangle wateringArea2 = new Microsoft.Xna.Framework.Rectangle((int)origin2.X - 10, (int)origin2.Y - 10, 20, 20);
									Vector2 currentPosition2 = default(Vector2);
									for (int x2 = wateringArea2.X; x2 < wateringArea2.Right; x2++)
									{
										for (int y2 = wateringArea2.Y; y2 < wateringArea2.Bottom; y2++)
										{
											currentPosition2.X = (float)x2;
											currentPosition2.Y = (float)y2;
											if (farm.isTileOnMap(currentPosition2) && farm.objects.ContainsKey(currentPosition2) && farm.objects[currentPosition2] is Fence && Game1.IsMasterGame)
											{
												(farm.objects[currentPosition2] as Fence).repair();
											}
										}
									}
									__instance.currentMarriageDialogue.Clear();
									__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4481", true, new string[0]);
									if (filledBowl)
									{
										__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, new string[]
										{
										Game1.player.getPetDisplayName()
										});
									}
									__instance.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false, new string[0]);
									NPC.hasSomeoneRepairedTheFences = true;
								}
								else
								{
									__instance.currentMarriageDialogue.Clear();
									__instance.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false, new string[0]);
								}
							}
							Game1.warpCharacter(__instance, "Farm", farmHouse.getPorchStandingSpot());
							__instance.popOffAnyNonEssentialItems();
							__instance.faceDirection(2);
							return;
						}
						if (__instance.Name.Equals("Krobus") && Game1.isRaining && r.NextDouble() < 0.4 && !NPC.checkTileOccupancyForSpouse(farm, Utility.PointToVector2(farmHouse.getPorchStandingSpot()), ""))
						{
							__instance.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5), false, new string[0]);
							Game1.warpCharacter(__instance, "Farm", farmHouse.getPorchStandingSpot());
							__instance.popOffAnyNonEssentialItems();
							__instance.faceDirection(2);
							return;
						}
						if (spouse.GetDaysMarried() >= 1 && r.NextDouble() < 0.045)
						{
							if (r.NextDouble() < 0.75)
							{
								Point spot = farmHouse.getRandomOpenPointInHouse(r, 1, 30);
								Furniture new_furniture = null;
								try
								{
									new_furniture = new Furniture(Utility.getRandomSingleTileFurniture(r), new Vector2((float)spot.X, (float)spot.Y));
								}
								catch (Exception)
								{
									new_furniture = null;
								}
								if (new_furniture == null || spot.X <= 0 || !farmHouse.isTileLocationOpen(new Location(spot.X - 1, spot.Y)))
								{
									__instance.setTilePosition(farmHouse.getKitchenStandingSpot());
									__instance.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4490", false, new string[0]), farmHouse, false);
									return;
								}
								farmHouse.furniture.Add(new_furniture);
								__instance.setTilePosition(spot.X - 1, spot.Y);
								__instance.faceDirection(1);
								__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4486", false, new string[]
								{
								"%endearmentlower"
								});
								if (Game1.random.NextDouble() < 0.5)
								{
									__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4488", true, new string[0]);
									return;
								}
								__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4489", false, new string[0]);
								return;
							}
							else
							{
								Point p2 = farmHouse.getRandomOpenPointInHouse(r, 0, 30);
								if (p2.X > 0)
								{
									__instance.setTilePosition(p2.X, p2.Y);
									__instance.faceDirection(0);
									if (r.NextDouble() < 0.5)
									{
										int wall = farmHouse.getWallForRoomAt(p2);
										if (wall != -1)
										{
											int style = r.Next(112);
											List<int> styles = new List<int>();
											string name = __instance.Name;
											if (name == "Alex")
											{
												styles.AddRange(new int[]
												{
																6
												});
											}
											else if (name == "Krobus")
											{
												styles.AddRange(new int[]
												{
														23,
														24
												});
											}
											else if (name == "Sebastian")
											{
												styles.AddRange(new int[]
												{
													3,
													4,
													12,
													14,
													30,
													46,
													47,
													56,
													58,
													59,
													107
												});
											}
											if (name == "Haley")
											{
												styles.AddRange(new int[]
												{
															1,
															7,
															10,
															35,
															49,
															84,
															99
												});
											}
											else if (name == "Shane")
											{
												styles.AddRange(new int[]
												{
													6,
													21,
													60
												});
											}
											if (name == "Leah")
											{
												styles.AddRange(new int[]
												{
														44,
														108,
														43,
														45,
														92,
														37,
														29
												});
											}
											else if (name == "Abigail")
											{
												styles.AddRange(new int[]
												{
												2,
												13,
												23,
												26,
												46,
												45,
												64,
												77,
												106,
												107
												});
											}
											if (styles.Count > 0)
											{
												style = styles[r.Next(styles.Count)];
											}
											farmHouse.setWallpaper(style, wall, true);
											__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4496", false, new string[0]);
											return;
										}
									}
									else
									{
										int floor = farmHouse.getFloorAt(p2);
										if (floor != -1)
										{
											farmHouse.setFloor(r.Next(40), floor, true);
											__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4497", false, new string[0]);
											return;
										}
									}
								}
							}
						}
						else
						{
							if (Game1.isRaining && r.NextDouble() < 0.08 && heartsWithSpouse < 11)
							{
								foreach (Furniture f2 in farmHouse.furniture)
								{
									if (f2.furniture_type == 13 && farmHouse.isTileLocationTotallyClearAndPlaceable((int)f2.tileLocation.X, (int)f2.tileLocation.Y + 1))
									{
										__instance.setTilePosition((int)f2.tileLocation.X, (int)f2.tileLocation.Y + 1);
										__instance.faceDirection(0);
										__instance.currentMarriageDialogue.Clear();
										__instance.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4498", true, new string[0]);
										return;
									}
								}
								__instance.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4499", false, new string[0]), farmHouse, true);
								return;
							}
							if (r.NextDouble() < 0.45)
							{
								Vector2 spot2 = (farmHouse.upgradeLevel == 1) ? new Vector2(32f, 5f) : new Vector2(38f, 14f);
								__instance.setTilePosition((int)spot2.X, (int)spot2.Y);
								__instance.faceDirection(0);
								__instance.setSpouseRoomMarriageDialogue();
								if (__instance.name == "Sebastian" && Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
								{
									__instance.setTilePosition((farmHouse.upgradeLevel == 1) ? 31 : 37, (farmHouse.upgradeLevel == 1) ? 6 : 15);
									__instance.faceDirection(2);
									return;
								}
							}
							else
							{
								__instance.setTilePosition(farmHouse.getKitchenStandingSpot());
								__instance.faceDirection(0);
								if (r.NextDouble() < 0.2)
								{
									__instance.setRandomAfternoonMarriageDialogue(Game1.timeOfDay, farmHouse, false);
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(NPC_marriageDuties_Prefix)}:\n{ex}", LogLevel.Error);
			}
		}

		public static void NPC_marriageDuties_Postfix(NPC __instance)
		{
			try
			{

				if (ModEntry.spouseRolesDate < new WorldDate().TotalDays)
				{
					ModEntry.PMonitor.Log("Resetting spouse roles");
					ModEntry.ResetSpouseRoles();
				}
				ModEntry.PMonitor.Log("marriage duties for " + __instance.Name);

				if (ModEntry.outdoorSpouse == __instance.Name && !Game1.isRaining && !Game1.IsWinter && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat") && !__instance.Name.Equals("Krobus"))
				{
					ModEntry.PMonitor.Log("going to outdoor patio");
					__instance.setUpForOutdoorPatioActivity();
					return;
				}

				Farmer spouse = __instance.getSpouse();
				FarmHouse farmHouse = Utility.getHomeOfFarmer(spouse);
				if (__instance.currentLocation != farmHouse)
				{
					return;
				}
				ModEntry.PMonitor.Log("in farm house");
				__instance.shouldPlaySpousePatioAnimation.Value = false;

				Vector2 spot = (farmHouse.upgradeLevel == 1) ? new Vector2(32f, 5f) : new Vector2(38f, 14f);
				Point spot2 = farmHouse.getSpouseBedSpot(__instance.Name);

				if (ModEntry.bedSpouse != null)
				{
					foreach (NPC character in farmHouse.characters)
					{
						if (character.isVillager() && ModEntry.GetRandomSpouses(true).ContainsKey(character.Name) && character.position == new Vector2(spot2.X * 64f, spot2.Y * 64f))
						{
							ModEntry.PMonitor.Log($"{character.Name} is already in bed");
							ModEntry.bedSpouse = character.Name;
							break;
						}
					}
				}

				if (ModEntry.kitchenSpouse == __instance.Name)
				{
					ModEntry.PMonitor.Log($"{__instance.Name} is in kitchen");
					__instance.setTilePosition(farmHouse.getKitchenStandingSpot());
					ModEntry.bedSpouse = null;
				}
				else if (ModEntry.bedSpouse == __instance.Name)
				{
					ModEntry.PMonitor.Log($"{__instance.Name} is in bed");
					__instance.setTilePosition(farmHouse.getSpouseBedSpot(__instance.Name));
					ModEntry.bedSpouse = null;
				}
				else if (!ModEntry.config.BuildAllSpousesRooms && spouse.spouse != __instance.Name)
				{
					__instance.setTilePosition(farmHouse.getRandomOpenPointInHouse(ModEntry.myRand));
				}
				else
				{
					ModEntry.ResetSpouses(spouse);

					int offset = 0;
					if (spouse.spouse != __instance.Name)
					{
						int idx = ModEntry.spouses.Keys.ToList().IndexOf(__instance.Name);
						offset = 7 * (idx + 1);
					}
					ModEntry.PMonitor.Log($"{__instance.Name} loc: {(spot.X + offset)},{spot.Y}");
					__instance.setTilePosition((int)spot.X + offset, (int)spot.Y);
					__instance.faceDirection(ModEntry.myRand.Next(0, 4));
				}


				// custom dialogues


				// dialogues

				if (__instance.currentMarriageDialogue == null || __instance.currentMarriageDialogue.Count == 0)
					return;

				bool gotDialogue = false;

				for (int i = 0; i < __instance.currentMarriageDialogue.Count; i++)
				{
					MarriageDialogueReference mdr = __instance.currentMarriageDialogue[i];

					if (mdr.DialogueFile == "Strings\\StringsFromCSFiles")
					{
						foreach (string[] array in csMarriageDialoguesChoose)
						{
							string key = array[ModEntry.myRand.Next(0, array.Length)];
							if (array.Contains(key))
							{
								Dictionary<string, string> marriageDialogues = null;
								try
								{
									marriageDialogues = ModEntry.PHelper.Content.Load<Dictionary<string, string>>("Characters\\Dialogue\\MarriageDialogue" + __instance.Name, ContentSource.GameContent);
								}
								catch (Exception)
								{
								}
								MarriageDialogueReference mdrn;
								if (marriageDialogues != null && marriageDialogues.ContainsKey(key))
								{
									mdrn = new MarriageDialogueReference("Characters\\Dialogue\\MarriageDialogue" + __instance.Name, key, mdr.IsGendered, mdr.Substitutions.ToArray());
								}
								else
								{
									mdrn = new MarriageDialogueReference("Characters\\Dialogue\\MarriageDialogue" + __instance.Name, key, mdr.IsGendered, mdr.Substitutions.ToArray());
								}
								__instance.currentMarriageDialogue[i] = mdrn;
								gotDialogue = true;
								break;
							}
						}
						if (!gotDialogue)
						{
							if (csMarriageDialoguesReplace.Contains(mdr.DialogueKey))
							{
								Dictionary<string, string> marriageDialogues = null;
								try
								{
									marriageDialogues = ModEntry.PHelper.Content.Load<Dictionary<string, string>>("Characters\\Dialogue\\MarriageDialogue" + __instance.Name, ContentSource.GameContent);
								}
								catch (Exception)
								{
								}
								if (marriageDialogues != null && marriageDialogues.ContainsKey(mdr.DialogueKey))
								{
									MarriageDialogueReference mdrn = new MarriageDialogueReference("Characters\\Dialogue\\MarriageDialogue" + __instance.Name, mdr.DialogueKey, mdr.IsGendered, mdr.Substitutions.ToArray());

									__instance.currentMarriageDialogue[i] = mdrn;
									break;
								}

							}
						}

					}
					else if (mdr.DialogueFile == "MarriageDialogue")
					{
						foreach (string[] array in csMarriageDialoguesChoose)
						{
							string key = array[ModEntry.myRand.Next(0, array.Length)];
							if (array.Contains(key))
							{
								Dictionary<string, string> marriageDialogues = null;
								try
								{
									marriageDialogues = ModEntry.PHelper.Content.Load<Dictionary<string, string>>("Characters\\Dialogue\\MarriageDialogue" + __instance.Name, ContentSource.GameContent);
								}
								catch (Exception)
								{
								}
								if (marriageDialogues != null && marriageDialogues.ContainsKey(key))
								{
									MarriageDialogueReference mdrn = new MarriageDialogueReference("Characters\\Dialogue\\MarriageDialogue" + __instance.Name, key, mdr.IsGendered, mdr.Substitutions.ToArray());
									__instance.currentMarriageDialogue[i] = mdrn;
									break;
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(NPC_marriageDuties_Postfix)}:\n{ex}", LogLevel.Error);
			}
		}

		public static void NPC_spouseObstacleCheck_Postfix(NPC __instance, bool __result)
		{
			try
			{
				if (__result && __instance.getSpouse() != null && __instance.currentLocation == Utility.getHomeOfFarmer(__instance.getSpouse()))
				{
					Farmer spouse = __instance.getSpouse();
					ModEntry.ResetSpouses(spouse);

					int offset = 0;
					if (spouse.spouse != __instance.Name)
					{
						int idx = ModEntry.spouses.Keys.ToList().IndexOf(__instance.Name);
						offset = 7 * (idx + 1);
					}
					Vector2 spot = ((__instance.currentLocation as FarmHouse).upgradeLevel == 1) ? new Vector2(32f, 5f) : new Vector2(38f, 14f);
					__instance.setTilePosition((int)spot.X + offset, (int)spot.Y);
					__instance.faceDirection(ModEntry.myRand.Next(0, 4));
				}
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(NPC_spouseObstacleCheck_Postfix)}:\n{ex}", LogLevel.Error);
			}
		}
		public static void NPC_engagementResponse_Postfix(NPC __instance, Farmer who)
		{
			ModEntry.ResetSpouses(who);
			Friendship friendship = who.friendshipData[__instance.Name];
			WorldDate weddingDate = new WorldDate(Game1.Date);
			weddingDate.TotalDays += Math.Max(1,ModEntry.config.DaysUntilMarriage);
			while (!Game1.canHaveWeddingOnDay(weddingDate.DayOfMonth, weddingDate.Season))
			{
				weddingDate.TotalDays++;
			}
			friendship.WeddingDate = weddingDate;

			ModEntry.BuildSpouseRoom(Utility.getHomeOfFarmer(who), "", -1);
		}

		public static bool NPC_isRoommate_Prefix(NPC __instance, ref bool __result)
		{
			try
			{

				if (!__instance.isVillager())
				{
					__result = false;
					return false;
				}
				foreach (Farmer f in Game1.getAllFarmers())
				{
					if (f.isRoommate(__instance.Name))
					{
						__result = true;
						return false;
					}
				}
				__result = false;
				return false;
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(NPC_isRoommate_Prefix)}:\n{ex}", LogLevel.Error);
				return true; // run original logic
			}
		}
		
		public static bool NPC_getSpouse_Prefix(NPC __instance, ref Farmer __result)
		{
			foreach (Farmer f in Game1.getAllFarmers())
			{
				if (f.friendshipData.ContainsKey(__instance.Name) && f.friendshipData[__instance.Name].IsMarried())
				{
					__result = f;
					return false;
				}
			}
			return true;
		}

		public static bool NPC_isMarried_Prefix(NPC __instance, ref bool __result)
		{
			__result = false;
			if (!__instance.isVillager())
			{
				return false;
			}
			foreach (Farmer f in Game1.getAllFarmers())
			{
				if (f.friendshipData.ContainsKey(__instance.Name) && f.friendshipData[__instance.Name].IsMarried())
				{
					__result = true;
					return false;
				}
			}
			return true;
		}

		public static bool NPC_isMarriedOrEngaged_Prefix(NPC __instance, ref bool __result)
		{
			__result = false;
			if (!__instance.isVillager())
			{
				return false;
			}
			foreach (Farmer f in Game1.getAllFarmers())
			{
				if (f.friendshipData.ContainsKey(__instance.Name) && (f.friendshipData[__instance.Name].IsMarried() || f.friendshipData[__instance.Name].IsEngaged()))
				{
					__result = true;
					return false;
				}
			}
			return true;
		}

		public static bool NPC_tryToReceiveActiveObject_Prefix(NPC __instance, ref Farmer who, ref string __state)
		{
			try
			{
				if (who.ActiveObject.ParentSheetIndex == 458)
				{
					if (who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].IsMarried() && !who.isEngaged())
					{
						who.spouse = __instance.Name;
						GameLocation l = Game1.getLocationFromName(Game1.player.homeLocation);
						l.playSound("dwop", NetAudio.SoundContext.NPC);
						Utility.getHomeOfFarmer(who).showSpouseRoom();
						if (l == Utility.getHomeOfFarmer(who))
						{
							l.resetForPlayerEntry();
						}
						return false;
					}

					if (!__instance.datable)
					{
						if (ModEntry.myRand.NextDouble() < 0.5)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3955", __instance.displayName));
							return false;
						}
						__instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3956") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3957"), __instance));
						Game1.drawDialogue(__instance);
						return false;
					}
					else
					{
						if (__instance.datable && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].IsDating())
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:AlreadyDatingBouquet", __instance.displayName));
							return false;
						}
						if (__instance.datable && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].IsDivorced())
						{
							__instance.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Divorced_bouquet"), __instance));
							Game1.drawDialogue(__instance);
							return false;
						}
						if (__instance.datable && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < ModEntry.config.MinPointsToDate / 2f)
						{
							__instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3958") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3959"), __instance));
							Game1.drawDialogue(__instance);
							return false;
						}
						if (__instance.datable && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < ModEntry.config.MinPointsToDate)
						{
							__instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3960") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3961"), __instance));
							Game1.drawDialogue(__instance);
							return false;
						}
						Friendship friendship = who.friendshipData[__instance.Name];
						if (!friendship.IsDating())
						{
							friendship.Status = FriendshipStatus.Dating;
							Multiplayer mp = ModEntry.PHelper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
							mp.globalChatInfoMessage("Dating", new string[]
							{
									Game1.player.Name,
									__instance.displayName
							});
						}
						__instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3962") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3963"), __instance));
						who.changeFriendship(25, __instance);
						who.reduceActiveItemByOne();
						who.completelyStopAnimatingOrDoingAction();
						__instance.doEmote(20, true);
						Game1.drawDialogue(__instance);
						return false;
					}
				}

				if (who.ActiveObject.ParentSheetIndex == 460)
				{
					if (who.isEngaged())
					{
						__instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3965") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3966"), __instance));
						Game1.drawDialogue(__instance);
						return false;
					}
					if (!__instance.datable || __instance.isMarriedOrEngaged() || (who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < ModEntry.config.MinPointsToMarry * 0.6f))
					{
						if (ModEntry.myRand.NextDouble() < 0.5)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", __instance.displayName));
							return false;
						}
						__instance.CurrentDialogue.Push(new Dialogue((__instance.Gender == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3970") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3971"), __instance));
						Game1.drawDialogue(__instance);
						return false;
					}
					else if (__instance.datable && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < ModEntry.config.MinPointsToMarry)
					{
						if (!who.friendshipData[__instance.Name].ProposalRejected)
						{
							__instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3972") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3973"), __instance));
							Game1.drawDialogue(__instance);
							who.changeFriendship(-20, __instance);
							who.friendshipData[__instance.Name].ProposalRejected = true;
							return false;
						}
						__instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3974") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3975"), __instance));
						Game1.drawDialogue(__instance);
						who.changeFriendship(-50, __instance);
						return false;
					}
					else
					{
						if (!__instance.datable || who.houseUpgradeLevel >= 1)
						{
							typeof(NPC).GetMethod("engagementResponse", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { who, false });
							return false;
						}
						if (ModEntry.myRand.NextDouble() < 0.5)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", __instance.displayName));
							return false;
						}
						__instance.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3972"), __instance));
						Game1.drawDialogue(__instance);
						return false;
					}
				}
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(NPC_tryToReceiveActiveObject_Prefix)}:\n{ex}", LogLevel.Error);
			}
			return true;
		}
		public static void NPC_tryToReceiveActiveObject_Postfix(NPC __instance, ref Farmer who, ref string __state)
		{
			try
			{
				if (who.spouse != __state)
				{
					who.spouse = __state;
				}
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(NPC_tryToReceiveActiveObject_Postfix)}:\n{ex}", LogLevel.Error);
			}
		}
	}
}
