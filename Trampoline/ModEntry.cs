using System;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace Trampoline
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;
		internal static ModEntry context;

		internal static int jumpHeight;
		internal static float jumpSpeed;
		internal static int jumpTicks;
		internal static double lastPoint;
		internal static bool isEmoting;
		internal static bool goingHigher;
		internal static bool goingLower;
		internal static bool goingSlower;
		internal static bool goingFaster;

		private const string trampolineKey = "aedenthorn.Trampoline/trampoline";
		private static Texture2D trampolineTexture;

		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
			helper.Events.Input.ButtonPressed += Input_ButtonPressed;
			helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Furniture), nameof(Furniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
					prefix: new HarmonyMethod(typeof(Furniture_draw_Patch), nameof(Furniture_draw_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Furniture), nameof(Furniture.GetSeatPositions)),
					prefix: new HarmonyMethod(typeof(Furniture_GetSeatPositions_Patch), nameof(Furniture_GetSeatPositions_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Furniture), nameof(Furniture.GetSeatCapacity)),
					prefix: new HarmonyMethod(typeof(Furniture_GetSeatCapacity_Patch), nameof(Furniture_GetSeatCapacity_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.ShowSitting)),
					prefix: new HarmonyMethod(typeof(Farmer_ShowSitting_Patch), nameof(Farmer_ShowSitting_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
		{
			if (!Config.EnableMod)
				return;

			if (IsOnTrampoline())
			{
				if (jumpTicks >= jumpHeight / jumpSpeed * 2 || jumpHeight < 64)
				{
					jumpTicks = 0;
					if (goingLower || Helper.Input.IsDown(Config.LowerKey))
					{
						jumpHeight -= 16;
						if (jumpHeight < 0)
							jumpHeight = 0;
						goingLower = false;
					}
					else if (goingHigher || Helper.Input.IsDown(Config.HigherKey))
					{
						jumpHeight += 16;
						goingHigher = false;
					}
					else if (goingSlower || Helper.Input.IsDown(Config.SlowerKey))
					{
						jumpSpeed -= 1;
						if (jumpSpeed < 1)
							jumpSpeed = 1;
						goingSlower = false;
					}
					else if (goingFaster || Helper.Input.IsDown(Config.FasterKey))
					{
						jumpSpeed += 1;
						goingFaster = false;
					}
				}
				if (jumpHeight < 64)
				{
					return;
				}

				double currentPoint = Math.Sin(jumpTicks / (jumpHeight / jumpSpeed) * Math.PI);

				Game1.player.yOffset = (int)Math.Round((currentPoint + 0.75) * (currentPoint > -1.7 ? jumpHeight : (int)Math.Round(Math.Sqrt(jumpHeight) * 8)));
				if (jumpTicks / (jumpHeight / jumpSpeed) > Math.PI / 2f && lastPoint < Math.PI / 2f)
				{
					Game1.currentLocation.playSound("bob");
				}
				lastPoint = jumpTicks / (jumpHeight / jumpSpeed);
				jumpTicks++;
				if (currentPoint < 0)
				{
					jumpTicks++;
				}
				if (jumpHeight >= 128 && currentPoint > -0.25f)
				{
					int facing = Game1.player.FacingDirection;
					int frame = 94;
					bool flipped = false;

					isEmoting = true;
					switch (facing)
					{
						case 0:
						case 2:
							break;
						case 1:
							frame = 97;
							break;
						case 3:
							frame = 97;
							flipped = true;
							break;
					}
					Game1.player.completelyStopAnimatingOrDoingAction();
					Game1.player.FarmerSprite.setCurrentAnimation(new FarmerSprite.AnimationFrame[]
						{
							new(frame, 1500, false, flipped, null, false)
						}
					);
					Game1.player.FarmerSprite.PauseForSingleAnimation = true;
					Game1.player.FarmerSprite.loop = true;
					Game1.player.FarmerSprite.loopThisAnimation = true;
					Game1.player.Sprite.currentFrame = 0;
				}
				else
				{
					isEmoting = false;
					Game1.player.completelyStopAnimatingOrDoingAction();
				}

			}
			else
			{
				if(isEmoting)
				{
					isEmoting = false;
					Game1.player.completelyStopAnimatingOrDoingAction();
				}
				jumpTicks = 0;
				jumpHeight = 128;
				jumpSpeed = Config.JumpSpeed;
			}
		}

		private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (!Config.EnableMod || !Context.IsPlayerFree)
				return;

			if (IsOnTrampoline())
			{
				KeyboardState currentKBState = Game1.GetKeyboardState();

				if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveUpButton))
				{
					Game1.player.faceDirection(0);
				}
				else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveDownButton))
				{
					Game1.player.faceDirection(2);
				}
				else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveRightButton))
				{
					Game1.player.faceDirection(1);
				}
				else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveLeftButton))
				{
					Game1.player.faceDirection(3);
				}
				if (e.Button == Config.HigherKey)
				{
					goingHigher = true;
				}
				else if (e.Button == Config.LowerKey)
				{
					goingLower = true;
				}
				else if (e.Button == Config.SlowerKey)
				{
					goingSlower = true;
				}
				else if (e.Button == Config.FasterKey)
				{
					goingFaster = true;
				}
			}
			else if(e.Button == Config.ConvertKey)
			{
				foreach (Furniture f in Game1.currentLocation.furniture)
				{
					if (f.isGroundFurniture() && !f.isPassable() && f.boundingBox.Width == 128 && f.boundingBox.Height == 128 && f.boundingBox.Value.Contains(Game1.viewport.X + Game1.getOldMouseX(), Game1.viewport.Y + Game1.getOldMouseY()))
					{
						if (f.modData.ContainsKey(trampolineKey))
						{
							f.modData.Remove(trampolineKey);
						}
						else
						{
							f.modData[trampolineKey] = "true";
						}
						Helper.Input.Suppress(e.Button);
						return;
					}
				}
			}
		}

		private static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
		{
			try
			{
				trampolineTexture = SHelper.GameContent.Load<Texture2D>("aedenthorn.Trampoline/trampoline");
				SMonitor.Log("Loaded custom pieces sheet");
			}
			catch
			{
				trampolineTexture = SHelper.ModContent.Load<Texture2D>("assets/trampoline.png");
				SMonitor.Log("Loaded default pieces sheet");
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
				setValue: value => {
					if (Config.EnableMod && !value)
					{
						if (Context.IsWorldReady && IsOnTrampoline(Game1.player))
						{
							Game1.player.sittingFurniture.RemoveSittingFarmer(Game1.player);
							Game1.player.sittingFurniture = null;
							Game1.player.isSitting.Value = false;
							Game1.player.completelyStopAnimatingOrDoingAction();
						}
					}
					Config.EnableMod = value;
				}
			);
			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.JumpSound.Name"),
				getValue: () => Config.JumpSound,
				setValue: value => Config.JumpSound = value
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ConvertKey.Name"),
				getValue: () => Config.ConvertKey,
				setValue: value => Config.ConvertKey = value
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.HigherKey.Name"),
				getValue: () => Config.HigherKey,
				setValue: value => Config.HigherKey = value
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.LowerKey.Name"),
				getValue: () => Config.LowerKey,
				setValue: value => Config.LowerKey = value
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.FasterKey.Name"),
				getValue: () => Config.FasterKey,
				setValue: value => Config.FasterKey = value
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.SlowerKey.Name"),
				getValue: () => Config.SlowerKey,
				setValue: value => Config.SlowerKey = value
			);
		}
	}
}
