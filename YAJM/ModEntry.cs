using System;
using System.IO;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;

namespace YetAnotherJumpMod
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry: Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		private static readonly PerScreen<float>	velX = new();
		private static readonly PerScreen<float>	velY = new();
		private static readonly PerScreen<float>	lastYJumpVelocity = new();
		private static readonly PerScreen<bool>		playerJumpingWithHorse = new();
		private static readonly PerScreen<bool>		blockedJump = new();
		private static Texture2D horseShadow;

		internal static float VelX
		{
			get => velX.Value;
			set => velX.Value = value;
		}

		internal static float VelY
		{
			get => velY.Value;
			set => velY.Value = value;
		}

		internal static float LastYJumpVelocity
		{
			get => lastYJumpVelocity.Value;
			set => lastYJumpVelocity.Value = value;
		}

		internal static bool PlayerJumpingWithHorse
		{
			get => playerJumpingWithHorse.Value;
			set => playerJumpingWithHorse.Value = value;
		}

		internal static bool BlockedJump
		{
			get => blockedJump.Value;
			set => blockedJump.Value = value;
		}

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
			Helper.Events.Content.AssetRequested += Content_AssetRequested;
			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Horse), nameof(Horse.draw), new Type[] { typeof(SpriteBatch) }),
					prefix: new HarmonyMethod(typeof(Horse_draw_Patch), nameof(Horse_draw_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Character), nameof(Character.getLocalPosition), new Type[] { typeof(xTile.Dimensions.Rectangle) }),
					postfix: new HarmonyMethod(typeof(Character_getLocalPosition_Patch), nameof(Character_getLocalPosition_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getDrawLayer)),
					prefix: new HarmonyMethod(typeof(Farmer_getDrawLayer_Patch), nameof(Farmer_getDrawLayer_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
			horseShadow = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "horse_shadow.png"));
		}

		private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (!Config.ModEnabled || !Context.IsPlayerFree || Game1.player.IsSitting() || Game1.player.swimming.Value || Game1.currentMinigame is not null || Game1.player.yJumpVelocity != 0)
				return;

			if (e.Button == Config.JumpButton)
			{
				TryToJump();
			}
		}

		private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			if (Game1.player.yJumpVelocity == 0f && LastYJumpVelocity < 0f)
			{
				PlayerJumpingWithHorse = false;
				BlockedJump = false;
				Game1.player.canMove = true;
				Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
				return;
			}
			Game1.player.position.X += VelX;
			Game1.player.position.Y += VelY;
			LastYJumpVelocity = Game1.player.yJumpVelocity;
		}

		private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			if (e.NameWithoutLocale.IsEquivalentTo("Animals/horse"))
			{
				e.LoadFromModFile<Texture2D>(Path.Combine("assets", "horse.png"), AssetLoadPriority.Medium);
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
				name: () => Helper.Translation.Get("GMCM.ModEnabled.Name"),
				getValue: () => Config.ModEnabled,
				setValue: value => Config.ModEnabled = value
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.JumpButton.Name"),
				getValue: () => Config.JumpButton,
				setValue: value => Config.JumpButton = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.PlayJumpSound.Name"),
				getValue: () => Config.PlayJumpSound,
				setValue: value => Config.PlayJumpSound = value
			);
			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.JumpSound.Name"),
				getValue: () => Config.JumpSound,
				setValue: value => Config.JumpSound = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.MaxJumpDistance.Name"),
				getValue: () => Config.MaxJumpDistance,
				setValue: value => Config.MaxJumpDistance = value,
				min: 3
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.OrdinaryJumpHeight.Name"),
				getValue: () => Config.OrdinaryJumpHeight,
				setValue: value => Config.OrdinaryJumpHeight = value,
				min: 0
			);
		}
	}
}
