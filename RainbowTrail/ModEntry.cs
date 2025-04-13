using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace RainbowTrail
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		internal static IManaBarApi manaBarApi = null;

		const string rainbowTrailKey = "aedenthorn.RainbowTrail";
		const string buffId = "aedenthorn.RainbowTrail_RainbowTrailBuff";
		private static Texture2D rainbowTexture;
		private static readonly Dictionary<long, List<RainbowTrailElement>> trailDictionary = new();

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
			helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
			helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
			helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
			helper.Events.Player.Warped += Player_Warped;
			helper.Events.Content.AssetRequested += Content_AssetRequested;
			helper.Events.Content.AssetReady += Content_AssetReady;
			helper.Events.Input.ButtonPressed += Input_ButtonPressed;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.draw), new Type[] { typeof(SpriteBatch) }),
					prefix: new HarmonyMethod(typeof(Farmer_draw_Patch), nameof(Farmer_draw_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
			rainbowTexture = SHelper.GameContent.Load<Texture2D>(rainbowTrailKey);
		}

		private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
		{
			if (e.Name.IsEquivalentTo(rainbowTrailKey))
			{
				e.LoadFromModFile<Texture2D>("assets/rainbow.png", AssetLoadPriority.Low);
			}
		}

		private void Content_AssetReady(object sender, AssetReadyEventArgs e)
		{
			if (e.Name.IsEquivalentTo(rainbowTrailKey))
			{
				rainbowTexture = Game1.content.Load<Texture2D>(rainbowTrailKey);
			}
		}

		private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			DisableRainbowTrail(Game1.player);
		}

		private void Player_Warped(object sender, WarpedEventArgs e)
		{
			if (!Config.ModEnabled || e.Player != Game1.player || !IsRainbowTrailActive(e.Player))
				return;

			if (Game1.CurrentEvent is not null)
			{
				DisableRainbowTrail(e.Player);
			}
			ClearRainbowTrail(e.Player);
		}

		private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			ClearRainbowTrail(Game1.player);
		}

		private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			ReloadRainbowTrailTexture(Game1.player);
		}

		private void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
		{
			if (!Config.ModEnabled || !IsRainbowTrailActive(Game1.player) || Config.StaminaUse <= 0 || !Context.IsPlayerFree)
				return;

			if (manaBarApi is not null && Config.UseMana)
			{
				if (manaBarApi.GetMana(Game1.player) < Config.StaminaUse)
				{
					DisableRainbowTrail(Game1.player);
					return;
				}
				manaBarApi.AddMana(Game1.player, -Config.StaminaUse);
			}
			else
			{
				if (Game1.player.Stamina < Config.StaminaUse)
				{
					DisableRainbowTrail(Game1.player);
					return;
				}
				Game1.player.Stamina = Math.Max(0.1f, Game1.player.Stamina - Config.StaminaUse);
			}
		}

		private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (!Config.ModEnabled || !Context.CanPlayerMove)
				return;

			if (Config.ToggleKeys.Keybinds[0].Buttons.Any(button => button == e.Button) && Config.ToggleKeys.Keybinds[0].Buttons.All(button => SHelper.Input.IsDown(button) || SHelper.Input.IsSuppressed(button)))
			{
				if (manaBarApi is not null && Config.UseMana)
				{
					if (manaBarApi.GetMana(Game1.player) >= Config.StaminaUse)
					{
						ToggleRainbowTrail(Game1.player);
					}
				}
				else
				{
					if (Game1.player.Stamina >= Config.StaminaUse)
					{
						ToggleRainbowTrail(Game1.player);
					}
				}
				Config.ToggleKeys.Keybinds[0].Buttons.ToList().ForEach(button => SHelper.Input.Suppress(button));
			}
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			if (CompatibilityUtility.IsManaBarLoaded)
			{
				manaBarApi = context.Helper.ModRegistry.GetApi<IManaBarApi>("spacechase0.ManaBar");
			}

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
				getValue: () => Config.ModEnabled,
				setValue: value => {
					if (!value)
					{
						DisableRainbowTrail(Game1.player);
						ClearRainbowTrail(Game1.player);
					}
					Config.ModEnabled = value;
				}
			);
			configMenu.AddKeybindList(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ToggleKeys.Name"),
				getValue: () => Config.ToggleKeys,
				setValue: value => Config.ToggleKeys = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MoveSpeed.Name"),
				getValue: () => Config.MoveSpeed,
				setValue: value => {
					Config.MoveSpeed = value;
					if (IsRainbowTrailActive(Game1.player))
					{
						DisableRainbowTrail(Game1.player);
						EnableRainbowTrail(Game1.player);
					}
				},
				min: 1
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => CompatibilityUtility.IsManaBarLoaded ? SHelper.Translation.Get("GMCM.StaminaManaUse.Name") : SHelper.Translation.Get("GMCM.StaminaUse.Name"),
				getValue: () => Config.StaminaUse,
				setValue: value => Config.StaminaUse = value
			);

			if (CompatibilityUtility.IsManaBarLoaded)
			{
				configMenu.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.UseMana.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.UseMana.Tooltip"),
					getValue: () => Config.UseMana,
					setValue: value => Config.UseMana = value
				);
			}

			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.EnableSound.Name"),
				getValue: () => Config.EnableSound,
				setValue: value => Config.EnableSound = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MaxDuration.Name"),
				getValue: () => Config.MaxDuration,
				setValue: value => Config.MaxDuration = value,
				min: 0
			);
		}
	}
}
