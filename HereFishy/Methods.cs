using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;
using Object = StardewValley.Object;

namespace HereFishy
{
	public partial class ModEntry
	{
		private static async void HereFishyFishy(Farmer who, int x, int y)
		{
			List<FarmerSprite.AnimationFrame> animationFrames = new()
			{
				new FarmerSprite.AnimationFrame(94, 100, false, who.FacingDirection == 3, null, false).AddFrameAction(delegate (Farmer f)
				{
					f.jitterStrength = 2f;
				})
			};

			hereFishying = true;
			if (Config.PlaySound)
			{
				fishySound?.Play();
			}
			who.completelyStopAnimatingOrDoingAction();
			who.CanMove = Config.AllowMovement;
			who.forceTimePass = true;
			who.jitterStrength = 2f;
			who.FarmerSprite.setCurrentAnimation(animationFrames.ToArray());
			who.FarmerSprite.PauseForSingleAnimation = true;
			who.FarmerSprite.loop = true;
			who.FarmerSprite.loopThisAnimation = true;
			who.Sprite.currentFrame = 94;

			await System.Threading.Tasks.Task.Delay(1793);

			canPerfect = true;
			perfect = false;

			who.synchronizedJump(8f);

			float stamina = who.Stamina;

			who.Stamina = Math.Max(0, who.Stamina - Config.StaminaCost);
			who.checkForExhaustion(stamina);

			await System.Threading.Tasks.Task.Delay(100);

			canPerfect = false;

			await System.Threading.Tasks.Task.Delay(900);

			who.stopJittering();
			who.completelyStopAnimatingOrDoingAction();
			who.forceCanMove();
			who.CanMove = Config.AllowMovement;
			who.forceTimePass = true;
			hereFishying = false;

			await System.Threading.Tasks.Task.Delay(Game1.random.Next(500, 1000));

			Item item = who.currentLocation.getFish(0, "-1", 1, who, 0, new Vector2(x, y), who.currentLocation.Name);

			if (item == null || item.ParentSheetIndex <= 0)
			{
				item = new Object(Game1.random.Next(167, 173).ToString(), 1);
			}
			animations.Clear();
			lastUser = who;
			whichFish = item.ItemId;
			Game1.objectData.TryGetValue(whichFish, out objectData);
			objectTexture = objectData is null ? Game1.mouseCursors : objectData.Texture is null ? Game1.objectSpriteSheet : Game1.content.Load<Texture2D>(objectData.Texture);

			Dictionary<string, string> data = Game1.content.Load<Dictionary<string, string>>("Data\\Fish");
			string[] array = null;

			if (data.ContainsKey(whichFish))
			{
				array = data[whichFish].Split('/');
			}

			bool nonfish = false;

			if (item is Furniture)
			{
				nonfish = true;
			}
			else if (item.HasContextTag("fish_nonfish"))
			{
				nonfish = true;
			}
			else if (Utility.IsNormalObjectAtParentSheetIndex(item, item.ItemId) && data.ContainsKey(item.ItemId))
			{
				if (!int.TryParse(data[item.ItemId].Split('/')[1], out _))
				{
					nonfish = true;
				}
			}
			else
			{
				nonfish = true;
			}
			fishSize = 0;
			fishQuality = 0;
			fishDifficulty = 0;
			if(array != null && !nonfish)
			{
				try
				{
					float fishDifficulty = Convert.ToInt32(array[1]);
					int minFishSize = Convert.ToInt32(array[3]);
					int maxFishSize = Convert.ToInt32(array[3]);
					int minimumSizeContribution = 1 + who.FishingLevel / 2;

					float fFishSize = 1f;
					fFishSize *= Game1.random.Next(minimumSizeContribution, Math.Max(6, minimumSizeContribution)) / 5f;
					if (item is Object @object && @object.scale.X == 1f)
					{
						fFishSize *= 1.2f;
					}
					fFishSize *= 1f + Game1.random.Next(-10, 11) / 100f;
					fFishSize = Math.Max(0f, Math.Min(1f, fFishSize));
					fishSize = (int)(minFishSize + (maxFishSize - minFishSize) * fFishSize);
					fishSize++;
					fishQuality = (fFishSize < 0.33) ? 0 : ((fFishSize < 0.66) ? 1 : 2);
					fishQuality = fishQuality switch
					{
						>= 4 => 4,
						>= 2 => perfect ? 4 : 2,
						>= 1 => perfect ? 2 : 1,
						_ => 0
					};
					if (beginnersRod)
					{
						fishQuality = 0;
						fishSize = minFishSize;
					}
				}
				catch
				{
					context.Monitor.Log($"Error getting fish size from {data[whichFish]}", LogLevel.Error);
				}
			}
			isBossFish = item.TryGetTempData("IsBossFish", out isBossFish);
			caughtDoubleFish = !isBossFish && Game1.random.NextDouble() < 0.1 + Game1.player.DailyLuck / 2.0;
			context.Monitor.Log($"pulling fish {whichFish} {fishSize} {who.Name} {x},{y}");
			if (who.IsLocalPlayer)
			{
				if (array != null && !nonfish)
				{
					float experience = (fishQuality + 1) * 3 + (int)fishDifficulty / 3;

					if(perfect)
					{
						experience *= 2.4f;
					}
					if (isBossFish)
					{
						experience *= 5f;
					}
					who.gainExperience(1, (int)experience);
				}
				else
				{
					who.gainExperience(1, 3);
				}
			}
			if (Config.PlaySound)
			{
				weeSound?.Play();
			}
			CreateAnimation(objectTexture, objectData is null ? 1384 : item.ParentSheetIndex, x, y, who, 0.002f, "tinyWhip", PlayerCaughtFishEndFunction);
			if (caughtDoubleFish)
			{
				CreateAnimation(objectTexture, objectData is null ? 1384 : item.ParentSheetIndex, x, y, who, 0.0016f, "fishSlap");
			}
		}

		static void CreateAnimation(Texture2D texture, int parentSheetIndex, float x, float y, Farmer who, float gravity, string endSound, Action<int> endFunction = null)
		{
			float distance = y - (who.StandingPixel.Y - 64);
			float height = Math.Abs(distance + 256f + 32f);
			float velocity = (float)Math.Sqrt(2f * gravity * height);
			float t = (float)(Math.Sqrt(2f * (height - distance) / gravity) + (velocity / gravity));
			float xVelocity = t != 0f ? (who.Position.X - x) / t : 0f;

			animations.Add(new TemporaryAnimatedSprite(texture.Name, Game1.getSourceRectForStandardTileSheet(texture, parentSheetIndex, 16, 16), t, 1, 0, new Vector2(x, y), false, false, y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
			{
				motion = new Vector2(xVelocity, -velocity),
				acceleration = new Vector2(0f, gravity),
				timeBasedMotion = true,
				endFunction = endFunction is not null ? new TemporaryAnimatedSprite.endBehavior(endFunction) : null,
				endSound = endSound
			});
		}

		public static void PlayerCaughtFishEndFunction(int _)
		{
			context.Monitor.Log($"caught fish end");
			fishCaught = true;
			lastUser.Halt();
			lastUser.armOffset = Vector2.Zero;
			recordSize = lastUser.caughtFish(whichFish, fishSize, false, caughtDoubleFish ? 2 : 1);
			lastUser.faceDirection(2);
			if (isBossFish)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14068"));
				Game1.Multiplayer.globalChatInfoMessage("CaughtLegendaryFish", new string[]
				{
					Game1.player.Name,
					objectData is not null ? TokenParser.ParseText(objectData.DisplayName) : whichFish
				});
				return;
			}
			if (recordSize)
			{
				sparklingText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14069"), Color.LimeGreen, Color.Azure, false, 0.1, 2500, -1, 500, 1f);
				lastUser.currentLocation.localSound("newRecord");
				return;
			}
			lastUser.currentLocation.localSound("fishSlap");
			lastUser.CanMove = true;
		}
	}
}
