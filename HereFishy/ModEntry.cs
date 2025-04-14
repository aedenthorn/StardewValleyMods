using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace HereFishy
{
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;

		internal static ModEntry context;

		private static readonly List<TemporaryAnimatedSprite> animations = new();
		private static bool beginnersRod;
		private static SoundEffect fishySound;
		private static SoundEffect weeSound;
		private static SparklingText sparklingText;
		private static bool caughtDoubleFish;
		private static Farmer lastUser;
		private static ObjectData objectData;
		private static Texture2D objectTexture;
		private static string whichFish;
		private static int fishSize;
		private static bool recordSize;
		private static bool perfect;
		private static int fishQuality;
		private static bool fishCaught;
		private static bool isBossFish;
		private static float fishDifficulty;
		private static bool canPerfect;
		private static bool hereFishying;


		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			try
			{
				fishySound = SoundEffect.FromStream(new FileStream(Path.Combine(Helper.DirectoryPath, "assets", "fishy.wav"), FileMode.Open));
				weeSound = SoundEffect.FromStream(new FileStream(Path.Combine(Helper.DirectoryPath, "assets", "wee.wav"), FileMode.Open));
			}
			catch(Exception e)
			{
				SMonitor.Log($"error loading fishy.wav: {e}");
			}

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
			Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
			Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
		}

		private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (!Config.EnableMod || !Context.IsWorldReady)
				return;

			if (e.Button == SButton.MouseRight)
			{
				if (!hereFishying)
				{
					if (Context.CanPlayerMove && (Game1.player.CurrentTool is FishingRod))
					{
						try
						{
							Vector2 mousePosition = Game1.currentCursorTile;

							if (Game1.player.currentLocation.waterTiles != null && Game1.player.currentLocation.waterTiles[(int)mousePosition.X, (int)mousePosition.Y])
							{
								SMonitor.Log($"here fishy fishy {mousePosition.X},{mousePosition.Y}");
								if (Game1.player.Stamina > 0f || Config.StaminaCost <= 0f)
								{
									beginnersRod = Game1.player.CurrentTool.UpgradeLevel == 1;
									HereFishyFishy(Game1.player, (int)mousePosition.X * 64, (int)mousePosition.Y * 64);
								}
								else
								{
									Game1.player.doEmote(36);
									Game1.staminaShakeTimer = 1000;
								}
							}
						}
						catch
						{
							SMonitor.Log($"error getting water tile");
						}
					}
				}
				else
				{
					if (canPerfect)
					{
						perfect = true;
						sparklingText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:BobberBar_Perfect"), Color.Yellow, Color.White, false, 0.1, 1500, -1, 500, 1f);
						Game1.playSound("jingle1");
					}
				}
			}
		}

		private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
		{
			if (!Config.EnableMod || !Context.IsWorldReady)
				return;

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
				Object @object = new(whichFish, caughtDoubleFish ? 2 : 1, false, -1, fishQuality);
				if (!lastUser.addItemToInventoryBool(@object))
				{
					Game1.createItemDebris(@object, lastUser.getStandingPosition(), 0, lastUser.currentLocation);
				}
				fishCaught = false;
			}
		}

		private void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
		{
			if (!Config.EnableMod || !Context.IsWorldReady)
				return;

			for (int i = animations.Count - 1; i >= 0; i--)
			{
				animations[i].draw(e.SpriteBatch, false, 0, 0, 1f);
			}
			if (sparklingText != null && lastUser != null)
			{
				sparklingText.draw(e.SpriteBatch, Game1.GlobalToLocal(Game1.viewport, lastUser.Position + new Vector2(-64f, -352f)));
			}
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is null)
				return;

			// register mod
			configMenu.Register(
				mod: ModManifest,
				reset: () => Config = new ModConfig(),
				save: () => Helper.WriteConfig(Config)
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
				getValue: () => Config.EnableMod,
				setValue: value => Config.EnableMod = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.PlaySound.Name"),
				getValue: () => Config.PlaySound,
				setValue: value => Config.PlaySound = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.StaminaCost.Name"),
				getValue: () => Config.StaminaCost,
				setValue: value => Config.StaminaCost = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.AllowMovement.Name"),
				getValue: () => Config.AllowMovement,
				setValue: value => Config.AllowMovement = value
			);
		}
	}
}
