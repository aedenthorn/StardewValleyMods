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

				if (!(__instance.currentLocation is FarmHouse))
				{
					return;
				}
				ModEntry.PMonitor.Log("in farm house");
				__instance.shouldPlaySpousePatioAnimation.Value = false;

				Farmer spouse = __instance.getSpouse();
				FarmHouse farmHouse = Game1.getLocationFromName(spouse.homeLocation.Value) as FarmHouse;
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
				if (__result && typeof(FarmHouse) == __instance.currentLocation.GetType())
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
					if (who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].IsMarried())
					{
						who.spouse = __instance.Name;
						GameLocation l = Game1.getLocationFromName(Game1.player.homeLocation);
						l.playSound("dwop", NetAudio.SoundContext.NPC);
						Utility.getHomeOfFarmer(who).showSpouseRoom();
						if (l is FarmHouse)
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
