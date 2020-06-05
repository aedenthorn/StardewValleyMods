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
							__instance.faceDirection((Game1.random.NextDouble() < 0.5) ? 2 : 0);
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
			return true;
		}


		public static void NPC_marriageDuties_Postfix(NPC __instance)
		{
			ModEntry.PMonitor.Log("marriage duties for " + __instance.Name);
			if (!(__instance.currentLocation is FarmHouse))
			{
				return;
			}
			ModEntry.PMonitor.Log("in farm house");
			__instance.shouldPlaySpousePatioAnimation.Value = false;

			Farmer spouse = __instance.getSpouse();
			FarmHouse farmHouse = Game1.getLocationFromName(spouse.homeLocation.Value) as FarmHouse;
			Vector2 spot = (farmHouse.upgradeLevel == 1) ? new Vector2(32f, 5f) : new Vector2(38f, 14f);

			if (ModEntry.kitchenSpouse == __instance.Name)
			{
				ModEntry.PMonitor.Log($"{__instance.Name} is in kitchen");
				__instance.setTilePosition(farmHouse.getKitchenStandingSpot());
				return;
			}
			else if (ModEntry.bedSpouse == __instance.Name)
			{
				ModEntry.PMonitor.Log($"{__instance.Name} is in bed");
				__instance.setTilePosition(farmHouse.getSpouseBedSpot(__instance.getSpouse().spouse));
				return;
			}

			if (!ModEntry.config.BuildAllSpousesRooms && spouse.spouse != __instance.Name)
			{
				__instance.setTilePosition(farmHouse.getRandomOpenPointInHouse(Game1.random));
				return;
			}

			ModEntry.ResetSpouses(spouse);

			int offset = 0;
			if (spouse.spouse != __instance.Name)
			{
				int idx = ModEntry.spouses.Keys.ToList().IndexOf(__instance.Name);
				offset = 7 * (idx + 1);
			}
			ModEntry.PMonitor.Log($"{__instance.Name} loc: {(spot.X + offset)},{spot.Y}");
			__instance.setTilePosition((int)spot.X + offset, (int)spot.Y);
			__instance.faceDirection(Game1.random.Next(0, 4));
		}

		public static bool NPC_isRoommate_Prefix(NPC __instance, ref bool __result)
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

		public static bool NPC_tryToReceiveActiveObject_Prefix(NPC __instance, ref Farmer who)
		{
			if (who.ActiveObject.ParentSheetIndex == 458)
			{
				if (who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].IsMarried())
				{
					who.spouse = __instance.Name;
					GameLocation l = Game1.getLocationFromName(Game1.player.homeLocation);
					l.playSound("dwop", NetAudio.SoundContext.NPC);
					if (l is FarmHouse)
					{
						(l as FarmHouse).showSpouseRoom();
						l.resetForPlayerEntry();
					}
					return false;
				}

				if (!__instance.datable)
				{
					if (Game1.random.NextDouble() < 0.5)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3955", __instance.displayName));
						return false;
					}
					__instance.CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3956") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3957"), __instance));
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
					if (__instance.datable && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < ModEntry.config.MinPointsToDate/2f)
					{
						__instance.CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3958") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3959"), __instance));
						Game1.drawDialogue(__instance);
						return false;
					}
					if (__instance.datable && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < ModEntry.config.MinPointsToDate)
					{
						__instance.CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3960") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3961"), __instance));
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
					__instance.CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3962") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3963"), __instance));
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
				if (!__instance.datable || __instance.isMarriedOrEngaged() || (who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < ModEntry.config.MinPointsToMarry*0.6f))
				{
					if (Game1.random.NextDouble() < 0.5)
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
						__instance.CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3972") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3973"), __instance));
						Game1.drawDialogue(__instance);
						who.changeFriendship(-20, __instance);
						who.friendshipData[__instance.Name].ProposalRejected = true;
						return false;
					}
					__instance.CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3974") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3975"), __instance));
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
					if (Game1.random.NextDouble() < 0.5)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", __instance.displayName));
						return false;
					}
					__instance.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3972"), __instance));
					Game1.drawDialogue(__instance);
					return false;
				}
			}
			else
			{
				return true;
			}
		}

	}
}
