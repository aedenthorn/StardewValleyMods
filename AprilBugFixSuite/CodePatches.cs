using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace AprilBugFixSuite
{
	public partial class ModEntry
	{
		public class InventoryMenu_draw
		{
			private static int lastSlotNumber = -1;
			private static int avoidTicks = 0;

			public static void Prefix(InventoryMenu __instance)
			{
				if (!IsModEnabled() || !Config.InventoryEnabled)
					return;

				avoidTicks++;
				foreach (ClickableComponent clickableComponent in __instance.inventory)
				{
					int slotNumber = Convert.ToInt32(clickableComponent.name);
					if (clickableComponent.containsPoint(Game1.getMouseX(), Game1.getMouseY()) && slotNumber < __instance.actualInventory.Count && (__instance.actualInventory[slotNumber] == null || __instance.highlightMethod(__instance.actualInventory[slotNumber])) && slotNumber < __instance.actualInventory.Count && __instance.actualInventory[slotNumber] != null)
					{
						if (slotNumber == lastSlotNumber || avoidTicks < 30)
							return;

						avoidTicks = 0;
						lastSlotNumber = slotNumber;
						if (Game1.random.NextDouble() < 0.5)
							return;

						MoveToRandomSlot(ref __instance.actualInventory, slotNumber);
					}
				}
			}

			private static void MoveToRandomSlot(ref IList<Item> inv, int slot)
			{
				List<int> slots = new() { slot - 1, slot + 1, slot - 12, slot + 12 };

				foreach(var i in slots){
					if(i > 0 && i < inv.Count && inv[i] == null)
					{
						inv[i] = inv[slot];
						inv[slot] = null;
						Game1.playSound("dwop");
						return;
					}
				}
			}
		}

		private static int screamTicks = 0;

		public class Tree_performToolAction
		{
			public static bool Prefix(Tree __instance, Tool t, Vector2 tileLocation)
			{
				if (!IsModEnabled() || !Config.TreeScreamEnabled)
					return true;

				GameLocation location = __instance.Location;

				if (!__instance.stump.Value && !__instance.falling.Value && Game1.random.NextDouble() < 0.25 && t is Axe)
				{
					List<Vector2> surroundingTiles = Utility.getSurroundingTileLocationsArray(tileLocation).ToList();

					ShuffleList(surroundingTiles);
					foreach (Vector2 tile in surroundingTiles)
					{
						if (t.getLastFarmerToUse().GetBoundingBox().Intersects(new Rectangle((int)tile.X * Game1.tileSize,(int)tile.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize)))
							continue;
						if (!location.IsTileBlockedBy(tile))
						{
							location.terrainFeatures.Remove(tileLocation);
							location.terrainFeatures.Add(tile, __instance);
							Game1.playSound("leafrustle");
							return false;
						}
					}
				}
				return true;
			}

			public static void Postfix(Tree __instance)
			{
				if (!IsModEnabled() || !Config.TreeScreamEnabled)
					return;

				if (!__instance.stump.Value && !__instance.falling.Value)
				{
					if(screamTicks >= 120)
					{
						screamTicks = 0;
						__instance.modData["aedenthorn.AprilBugFixSuite/speech"] = SHelper.Translation.Get("tree-chop-" + Game1.random.Next(1, 8));
						__instance.modData["aedenthorn.AprilBugFixSuite/timer"] = "60/60";
					}
				}
			}
		}

		public class Tree_draw
		{
			public static void Postfix(Tree __instance, SpriteBatch spriteBatch)
			{
				if (!IsModEnabled() || !Config.TreeScreamEnabled)
					return;

				if(screamTicks < 120)
					screamTicks++;

				__instance.modData.TryGetValue("aedenthorn.AprilBugFixSuite/speech", out string speech);
				if (!__instance.falling.Value && speech == null)
					return;

				string lastTimerMax = __instance.modData["aedenthorn.AprilBugFixSuite/timer"]?.Split('/')[1];

				if (__instance.falling.Value && (speech == null || lastTimerMax != "120"))
				{
					speech = SHelper.Translation.Get("tree-fall-" + Game1.random.Next(1, 5));
					__instance.modData["aedenthorn.AprilBugFixSuite/speech"] = speech;
					__instance.modData["aedenthorn.AprilBugFixSuite/timer"] = "120/120";
				}

				Vector2 local = Game1.GlobalToLocal(__instance.Tile * 64 + new Vector2(32, __instance.falling.Value || __instance.stump.Value ? -128 : -192));
				string[] timerString = __instance.modData["aedenthorn.AprilBugFixSuite/timer"].Split('/');
				int timer = int.Parse(timerString[0]);
				int timerMax = int.Parse(timerString[1]);

				timer--;
				if (timer <= 0 && timerMax != 120)
				{
					__instance.modData.Remove("aedenthorn.AprilBugFixSuite/speech");
					return;
				}
				__instance.modData["aedenthorn.AprilBugFixSuite/timer"] = $"{timer}/{timerMax}";

				float alpha = 1;

				if (timer < 5)
					alpha = timer / 5f;
				else if (timer > timerMax - 5)
					alpha = timerMax - timer / 5f;
				SpriteText.drawStringWithScrollCenteredAt(spriteBatch, speech, (int)local.X, (int)local.Y, "", alpha);
			}
		}

		public static class Farmer_MovePosition_Patch
		{
			public static void Prefix(Farmer __instance, ref Vector2 __state)
			{
				if (!IsModEnabled() || !Config.BackwardsEnabled)
					return;

				if (backwardsFarmer)
				{
					__state = __instance.Position;
				}
			}

			public static void Postfix(Farmer __instance, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation, ref Vector2 __state)
			{
				if (!IsModEnabled())
					return;

				if (backwardsFarmer)
				{
					Vector2 dest = __state - (__instance.Position - __state);
					int width = __instance.GetSpriteWidthForPositioning() * 4 * 3 / 4;
					Rectangle destRectFloor = new((int)Math.Floor(dest.X) - 8, (int)Math.Floor(dest.Y) - 16, width, 32);
					Rectangle destRectCeil = new((int)Math.Ceiling(dest.X) + 8, (int)Math.Ceiling(dest.Y) + 16, width, 32);
					Rectangle destRect = Rectangle.Union(destRectCeil, destRectFloor);

					if (!currentLocation.isCollidingPosition(destRect, viewport, true, -1, false, __instance))
						__instance.Position = dest;
				}
			}
		}

		public static class NPC_draw_Patch
		{
			public static void Prefix(NPC __instance)
			{
				if (!IsModEnabled() || !Config.GiantEnabled || !__instance.IsVillager)
					return;

				__instance.Scale = !gianting ? 1 : 1 + (float)new Random((int)Game1.stats.DaysPlayed + __instance.Name.GetHashCode()).NextDouble() * 2;
			}
		}

		public static class Farmer_Update_Patch
		{
			public static void Prefix(Farmer __instance, GameTime time)
			{
				if (!IsModEnabled() || !Config.SlimeEnabled|| !slimeFarmer || slime == null)
					return;

				slime.Sprite.AnimateDown(time, 0, "");
				if (__instance.isMoving())
				{
					slime.Sprite.interval = 100f;
				}
				else
				{
					slime.Sprite.interval = 200f;
				}
			}
		}

		public static class Farmer_draw_Patch
		{
			public static bool Prefix(Farmer __instance, SpriteBatch b)
			{
				if (!IsModEnabled() || !Config.SlimeEnabled || !slimeFarmer)
					return true;

				slime.Position = __instance.Position;
				b.Draw(slime.Sprite.Texture, slime.getLocalPosition(Game1.viewport) + new Vector2(56f, (float)(16 + slime.yJumpOffset)), new Rectangle?(slime.Sprite.SourceRect), Utility.GetPrismaticColor(348 + 50, 5f), slime.rotation, new Vector2(16f, 16f), Math.Max(0.2f, slime.Scale) * 4f, slime.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, slime.drawOnTop ? 0.991f : ((float)slime.StandingPixel.Y / 10000f)));
				return false;
			}
		}

		public static class Character_update_Patch
		{
			public static void Postfix(Character __instance, GameTime time)
			{
				if (!IsModEnabled() || !Config.EnableAnimalTalk || speakingAnimals is null || __instance is not FarmAnimal)
					return;

				if(speakingAnimals.aID == (__instance as FarmAnimal).myID.Value)
				{
					if (speakingAnimals.atextAboveHeadPreTimer > 0)
					{
						speakingAnimals.atextAboveHeadPreTimer -= time.ElapsedGameTime.Milliseconds;
					}
					else
					{
						speakingAnimals.atextAboveHeadTimer -= time.ElapsedGameTime.Milliseconds;
						if (speakingAnimals.atextAboveHeadTimer > 500)
						{
							speakingAnimals.atextAboveHeadAlpha = Math.Min(1f, speakingAnimals.atextAboveHeadAlpha + 0.1f);
						}
						else
						{
							speakingAnimals.atextAboveHeadAlpha = Math.Max(0f, speakingAnimals.atextAboveHeadAlpha - 0.04f);
						}
					}
					if (speakingAnimals.bTextAboveHeadPreTimer > 0)
					{
						speakingAnimals.bTextAboveHeadPreTimer -= time.ElapsedGameTime.Milliseconds;
					}
					else
					{
						speakingAnimals.bTextAboveHeadTimer -= time.ElapsedGameTime.Milliseconds;
						if (speakingAnimals.bTextAboveHeadTimer > 500)
						{
							speakingAnimals.bTextAboveHeadAlpha = Math.Min(1f, speakingAnimals.bTextAboveHeadAlpha + 0.1f);
						}
						else
						{
							speakingAnimals.bTextAboveHeadAlpha = Math.Max(0f, speakingAnimals.bTextAboveHeadAlpha - 0.04f);
						}
					}
					if (speakingAnimals.bTextAboveHeadTimer < 0)
						speakingAnimals = null;
				}
			}
		}

		public static class GameLocation_drawAboveAlwaysFrontLayer_Patch
		{
			public static void Postfix(SpriteBatch b)
			{
				if (!IsModEnabled() || !Config.EnableAnimalTalk || speakingAnimals is null || (Game1.currentLocation is not Farm && Game1.currentLocation is not AnimalHouse && Game1.currentLocation is not Forest))
					return;

				FarmAnimal[] animals;

				if (Game1.currentLocation is AnimalHouse)
					animals = (Game1.currentLocation as AnimalHouse).animals.Values.ToArray();
				else if (Game1.currentLocation is Farm)
					animals = (Game1.currentLocation as Farm).animals.Values.ToArray();
				else if (Game1.currentLocation is Forest)
					animals = (Game1.currentLocation as Forest).marniesLivestock.ToArray();
				else
					return;

				foreach (FarmAnimal a in animals)
				{
					if (speakingAnimals.aID == a.myID.Value)
					{
						if (speakingAnimals.atextAboveHeadTimer > 0 && speakingAnimals.atextAboveHead != null)
						{
							Vector2 local = Game1.GlobalToLocal(new Vector2(a.StandingPixel.X, a.StandingPixel.Y - a.Sprite.SpriteHeight * 4 - 64 + a.yJumpOffset));

							if (a.shouldShadowBeOffset)
							{
								local += a.drawOffset;
							}
							SpriteText.drawStringWithScrollCenteredAt(b, speakingAnimals.atextAboveHead, (int)local.X, (int)local.Y, "", speakingAnimals.atextAboveHeadAlpha, speakingAnimals.atextAboveHeadColor, 1, (float)(a.Tile.Y * 64) / 10000f + 0.001f + a.Tile.X / 10000f, false);
						}
					}
					else if (speakingAnimals.bID == a.myID.Value)
					{
						if (speakingAnimals.bTextAboveHeadTimer > 0 && speakingAnimals.bTextAboveHead != null)
						{
							Vector2 local = Game1.GlobalToLocal(new Vector2(a.StandingPixel.X, a.StandingPixel.Y - a.Sprite.SpriteHeight * 4 - 64 + a.yJumpOffset));

							if (a.shouldShadowBeOffset)
							{
								local += a.drawOffset;
							}
							SpriteText.drawStringWithScrollCenteredAt(b, speakingAnimals.bTextAboveHead, (int)local.X, (int)local.Y, "", speakingAnimals.bTextAboveHeadAlpha, speakingAnimals.bTextAboveHeadColor, 1, (float)(a.Tile.Y * 64) / 10000f + 0.001f + a.Tile.X / 10000f, false);
						}
					}
				}
			}
		}
	}
}
