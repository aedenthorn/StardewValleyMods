using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Object = StardewValley.Object;

namespace HereFishy
{
    public class ModEntry : Mod 
	{
		public static ModEntry context;

		public static ModConfig Config;
        private static SoundEffect fishySound;
		private static List<TemporaryAnimatedSprite> animations = new List<TemporaryAnimatedSprite>();
        private static SparklingText sparklingText;
        private static bool caughtDoubleFish;
        private static Farmer lastUser;
        private static int whichFish;
        private static int fishSize;
        private static bool recordSize;
        private static bool perfect;
        private static int fishQuality;
        private static bool fishCaught;
        private static bool bossFish;
        private static int fishDifficulty;


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
		{
			context = this;
			Config = Helper.ReadConfig<ModConfig>();
			if (!Config.EnableMod)
				return;

			HarmonyInstance harmony = HarmonyInstance.Create(Helper.ModRegistry.ModID);

			harmony.Patch(
			   original: AccessTools.Method(typeof(Pan), nameof(Pan.beginUsing)),
			   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Pan_beginUsing_prefix))
			);

            try
            {
				fishySound = SoundEffect.FromStream(new FileStream(Path.Combine(Helper.DirectoryPath, "assets", "fishy.wav"), FileMode.Open));
			}
            catch(Exception ex)
            {
				context.Monitor.Log($"error loading fishy.wav: {ex}");
			}

			Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
		}

        private void GameLoop_UpdateTicking(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
			for (int i = animations.Count - 1; i >= 0; i--)
			{
				if (animations[i].update(Game1.currentGameTime))
				{
					animations.RemoveAt(i);
				}
			}
			if (sparklingText != null && sparklingText.update(Game1.currentGameTime))
			{
				sparklingText = null;
			}
            if (fishCaught)
            {
				lastUser.addItemToInventoryBool(new Object(whichFish, caughtDoubleFish ? 2 : 1, false, -1, fishQuality), false);
				fishCaught = false;
			}
		}

		private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
			for (int i = animations.Count - 1; i >= 0; i--)
			{
				animations[i].draw(e.SpriteBatch, false, 0, 0, 1f);
			}
			if (sparklingText != null)
			{
				sparklingText.draw(e.SpriteBatch, Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(-64f, -352f)));
			}

		}

        private static bool Pan_beginUsing_prefix(Pan __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
        {
			context.Monitor.Log($"begin using pan");
			bool overrideCheck = false;
			Rectangle orePanRect = new Rectangle(location.orePanPoint.X * 64 - 64, location.orePanPoint.Y * 64 - 64, 256, 256);
			if (orePanRect.Contains(x, y) && Utility.distance((float)who.getStandingX(), (float)orePanRect.Center.X, (float)who.getStandingY(), (float)orePanRect.Center.Y) <= 192f)
			{
				overrideCheck = true;
			}
			if (!overrideCheck && (location.orePanPoint == null || location.orePanPoint.Equals(Point.Zero)))
			{
                try
                {
					Vector2 mousePos = Game1.currentCursorTile;
					if (location.waterTiles != null && location.waterTiles[(int)mousePos.X, (int)mousePos.Y])
					{
						context.Monitor.Log($"here fishy fishy {mousePos.X},{mousePos.Y}");
						who.forceCanMove();
						HereFishyFishy(who, (int)mousePos.X * 64, (int)mousePos.Y * 64);
						__result = true;
						return false;
					}

				}
                catch
                {
					context.Monitor.Log($"error getting water tile");
                }
			}
			return true;
		}

        private static async void HereFishyFishy(Farmer who, int x, int y)
        {
			if(fishySound != null)
            {
				fishySound.Play();
			}

			await System.Threading.Tasks.Task.Delay(Game1.random.Next(3000,4000));

			Object o = who.currentLocation.getFish(0, -1, 1, who, 0, new Vector2(x, y), who.currentLocation.Name);
			if (o == null || o.ParentSheetIndex <= 0)
			{
				o = new Object(Game1.random.Next(167, 173), 1, false, -1, 0);
			}
			pullFishFromWater(who, x, y, o.ParentSheetIndex);
			return;

		}

		private static void pullFishFromWater(Farmer who, int x, int y, int parentSheetIndex)
		{

			animations.Clear();
			float t;
			lastUser = who;
			whichFish = parentSheetIndex;
			Dictionary<int, string> data = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
			string[] datas = null;
			if (data.ContainsKey(whichFish))
			{
				datas = data[whichFish].Split('/');
			}
			float fs = 1f;
			int minimumSizeContribution = 1 + who.FishingLevel / 2;
			fs *= (float)Game1.random.Next(minimumSizeContribution, Math.Max(6, minimumSizeContribution)) / 5f;
			fs *= 1.2f;
			fs *= 1f + (float)Game1.random.Next(-10, 11) / 100f;
			fs = Math.Max(0f, Math.Min(1f, fs));
			if(datas != null)
            {
				int minFishSize = int.Parse(datas[3]);
				int maxFishSize = int.Parse(datas[4]);
				fishSize = (int)((float)minFishSize + (float)(maxFishSize - minFishSize) * fishSize);
				fishSize++;
				perfect = true;
				fishQuality = (((double)fishSize < 0.33) ? 0 : (((double)fishSize < 0.66) ? 2 : 4));
			}
			bossFish = FishingRod.isFishBossFish(whichFish);
			caughtDoubleFish = !bossFish && Game1.random.NextDouble() < 0.25 + Game1.player.DailyLuck / 2.0;

			context.Monitor.Log($"pulling fish {whichFish} {fishSize} {who.Name} {x},{y}");

			if (who.IsLocalPlayer)
			{
				if (datas != null)
				{
					fishDifficulty = int.Parse(datas[1]);

				}
				else
					fishDifficulty = 0;
				
				int experience = Math.Max(1, (fishQuality + 1) * 3 + fishDifficulty / 3);
				if (bossFish)
				{
					experience *= 5;
				}
				experience += (int)((float)experience * 1.4f); // perfect
				who.gainExperience(1, experience);
			}

			if (who.FacingDirection == 1 || who.FacingDirection == 3)
			{
				float distance = Vector2.Distance(new Vector2(x, y), who.Position);
				float gravity = 0.001f;
				float height = 128f - (who.Position.Y - y + 10f);
				double angle = 1.1423973285781066;
				float yVelocity = (float)((double)(distance * gravity) * Math.Tan(angle) / Math.Sqrt((double)(2f * distance * gravity) * Math.Tan(angle) - (double)(2f * gravity * height)));
				if (float.IsNaN(yVelocity))
				{
					yVelocity = 0.6f;
				}
				float xVelocity = (float)((double)yVelocity * (1.0 / Math.Tan(angle)));
				t = distance / xVelocity;
				animations.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, parentSheetIndex, 16, 16), t, 1, 0, new Vector2(x,y), false, false, y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
				{
					motion = new Vector2((float)((who.FacingDirection == 3) ? -1 : 1) * -xVelocity, -yVelocity),
					acceleration = new Vector2(0f, gravity),
					timeBasedMotion = true,
					endFunction = new TemporaryAnimatedSprite.endBehavior(playerCaughtFishEndFunction),
					extraInfoForEndBehavior = parentSheetIndex,
					endSound = "tinyWhip"
				});
				if (caughtDoubleFish)
				{
					distance = Vector2.Distance(new Vector2(x, y), who.Position);
					gravity = 0.0008f;
					height = 128f - (who.Position.Y - y + 10f);
					angle = 1.1423973285781066;
					yVelocity = (float)((double)(distance * gravity) * Math.Tan(angle) / Math.Sqrt((double)(2f * distance * gravity) * Math.Tan(angle) - (double)(2f * gravity * height)));
					if (float.IsNaN(yVelocity))
					{
						yVelocity = 0.6f;
					}
					xVelocity = (float)((double)yVelocity * (1.0 / Math.Tan(angle)));
					t = distance / xVelocity;
					animations.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, parentSheetIndex, 16, 16), t, 1, 0, new Vector2(x, y), false, false, y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
					{
						motion = new Vector2((float)((who.FacingDirection == 3) ? -1 : 1) * -xVelocity, -yVelocity),
						acceleration = new Vector2(0f, gravity),
						timeBasedMotion = true,
						endSound = "fishSlap",
						Parent = who.currentLocation
					});
				}
			}
			else
			{
				float distance2 = y - (float)(who.getStandingY() - 64);
				float height2 = Math.Abs(distance2 + 256f + 32f);
				if (who.FacingDirection == 0)
				{
					height2 += 96f;
				}
				float gravity2 = 0.003f;
				float velocity = (float)Math.Sqrt((double)(2f * gravity2 * height2));
				t = (float)(Math.Sqrt((double)(2f * (height2 - distance2) / gravity2)) + (double)(velocity / gravity2));
				float xVelocity2 = 0f;
				if (t != 0f)
				{
					xVelocity2 = (who.Position.X - x) / t;
				}
				animations.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, parentSheetIndex, 16, 16), t, 1, 0, new Vector2(x, y), false, false, y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
				{
					motion = new Vector2(xVelocity2, -velocity),
					acceleration = new Vector2(0f, gravity2),
					timeBasedMotion = true,
					endFunction = new TemporaryAnimatedSprite.endBehavior(playerCaughtFishEndFunction),
					extraInfoForEndBehavior = parentSheetIndex,
					endSound = "tinyWhip"
				});
				if (caughtDoubleFish)
				{
					distance2 = y - (float)(who.getStandingY() - 64);
					height2 = Math.Abs(distance2 + 256f + 32f);
					if (who.FacingDirection == 0)
					{
						height2 += 96f;
					}
					gravity2 = 0.004f;
					velocity = (float)Math.Sqrt((double)(2f * gravity2 * height2));
					t = (float)(Math.Sqrt((double)(2f * (height2 - distance2) / gravity2)) + (double)(velocity / gravity2));
					xVelocity2 = 0f;
					if (t != 0f)
					{
						xVelocity2 = (who.Position.X - x) / t;
					}
					animations.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, parentSheetIndex, 16, 16), t, 1, 0, new Vector2(x, y), false, false, y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
					{
						motion = new Vector2(xVelocity2, -velocity),
						acceleration = new Vector2(0f, gravity2),
						timeBasedMotion = true,
						endSound = "fishSlap",
						Parent = who.currentLocation
					});
				}
			}
		}
		public static void playerCaughtFishEndFunction(int extraData)
		{
			context.Monitor.Log($"caught fish end");
			fishCaught = true;
			fishQuality = Game1.random.Next(0, 5);
			lastUser.Halt();
			lastUser.armOffset = Vector2.Zero;

			recordSize = lastUser.caughtFish(whichFish, fishSize, false, caughtDoubleFish ? 2 : 1);
			lastUser.faceDirection(2);
			if (FishingRod.isFishBossFish(whichFish))
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14068"));
				string name = Game1.objectInformation[whichFish].Split(new char[]
				{
					'/'
				})[4];
				context.Helper.Reflection.GetField<Multiplayer>(Game1.game1, "multiplayer").GetValue().globalChatInfoMessage("CaughtLegendaryFish", new string[]
				{
					Game1.player.Name,
					name
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
		}
	}
}
