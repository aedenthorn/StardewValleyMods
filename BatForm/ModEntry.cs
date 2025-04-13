using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.TerrainFeatures;

namespace BatForm
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

		internal const string batFormKey = "aedenthorn.BatForm";
		internal const int maxHeight = 50;
		internal static PerScreen<ICue> batSound = new();
		internal static PerScreen<int> height = new();
		internal static PerScreen<int> heightViewportLimit = new(() => maxHeight);
		internal static PerScreen<AnimatedSprite> batSprite = new();

		public enum BatForm
		{
			Inactive,
			SwitchingTo,
			SwitchingFrom,
			Active
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

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
			helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
			helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
			helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
			helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
			helper.Events.Player.Warped += Player_Warped;
			helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
			helper.Events.Display.RenderedWorld += Display_RenderedWorld;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.draw), new Type[] { typeof(SpriteBatch) }),
					prefix: new HarmonyMethod(typeof(Farmer_draw_Patch), nameof(Farmer_draw_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.PropertyGetter(typeof(Options), nameof(Options.zoomLevel)),
					postfix: new HarmonyMethod(typeof(Options_zoomLevel_Patch), nameof(Options_zoomLevel_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(FarmerSprite), "checkForFootstep"),
					prefix: new HarmonyMethod(typeof(FarmerSprite_checkForFootstep_Patch), nameof(FarmerSprite_checkForFootstep_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Grass), nameof(Grass.doCollisionAction)),
					prefix: new HarmonyMethod(typeof(Grass_doCollisionAction_Patch), nameof(Grass_doCollisionAction_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.takeDamage)),
					prefix: new HarmonyMethod(typeof(Farmer_takeDamage_Patch), nameof(Farmer_takeDamage_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Game1), nameof(Game1.pressActionButton)),
					prefix: new HarmonyMethod(typeof(Game1_pressActionButton_Patch), nameof(Game1_pressActionButton_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Game1), nameof(Game1.pressUseToolButton)),
					prefix: new HarmonyMethod(typeof(Game1_pressUseToolButton_Patch), nameof(Game1_pressUseToolButton_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
		{
			ResetBat();
		}

		private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
		{
			if (!Config.ModEnabled || e.Player != Game1.player || BatFormStatus(e.Player) == BatForm.Inactive)
				return;

			heightViewportLimit.Value = maxHeight;
			Game1.forceSnapOnNextViewportUpdate = true;
			Game1.game1.refreshWindowSettings();
			if (Config.OutdoorsOnly && !e.NewLocation.IsOutdoors)
			{
				ResetBat();
			}
			if (Game1.CurrentEvent != null)
			{
				ResetBat();
			}
		}

		private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
		{
			ResetBat();
		}

		private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
		{
			ResetBat();
		}

		private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
		{
			if (!Config.ModEnabled || BatFormStatus(Game1.player) != BatForm.Active || Config.StaminaUse <= 0)
				return;

			if (manaBarApi is not null && Config.UseMana)
			{
				if (manaBarApi.GetMana(Game1.player) < Config.StaminaUse)
				{
					TransformBat();
					return;
				}
				manaBarApi.AddMana(Game1.player, -Config.StaminaUse);
			}
			else
			{
				if (Game1.player.Stamina < Config.StaminaUse)
				{
					TransformBat();
					return;
				}
				Game1.player.Stamina = Math.Max(0.1f, Game1.player.Stamina - Config.StaminaUse);
			}
		}

		private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
		{
			if (!Config.ModEnabled || BatFormStatus(Game1.player) == BatForm.Inactive)
				return;

			batSprite.Value ??= new AnimatedSprite("Characters\\Monsters\\Bat");
			e.SpriteBatch.Draw(batSprite.Value.Texture, Game1.player.getLocalPosition(Game1.viewport) + new Vector2(32f, -height.Value * 8), new Rectangle?(batSprite.Value.SourceRect), Color.White, 0f, new Vector2(8f, 16f), (1 + height.Value / 50f) * 4f, SpriteEffects.None, Game1.player.StandingPixel.Y / 10000 + 0.05f + height.Value / 750f);
			batSprite.Value.Animate(Game1.currentGameTime, 0, 4, 80f);
			if (batSprite.Value.currentFrame % 3 == 0 && Game1.soundBank != null && (batSound.Value is null || !batSound.Value.IsPlaying) && Game1.player.currentLocation == Game1.currentLocation)
			{
				batSound.Value = Game1.soundBank.GetCue("batFlap");
				batSound.Value.Play();
			}
		}

		private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
		{
			if (!Config.ModEnabled || !Context.IsWorldReady)
				return;

			if (Game1.killScreen || Game1.player is null || Game1.player.health <= 0 || Game1.timeOfDay >= 2600 || Game1.eventUp || Game1.CurrentEvent != null)
			{
				ResetBat();
				return;
			}

			BatForm status = BatFormStatus(Game1.player);

			if (status != BatForm.Inactive)
			{
				EnforceMapBounds();
			}
			else
			{
				return;
			}
			if (status != BatForm.Active)
			{
				if (status == BatForm.SwitchingFrom)
				{
					height.Value = Math.Max(0, height.Value - 1);
					if (height.Value == 0)
					{
						PlayTransform();
						heightViewportLimit.Value = maxHeight;
						Game1.player.ignoreCollisions = false;
						Game1.player.buffs.Remove($"{SModManifest.UniqueID}.BatForm");
						Game1.player.modData[batFormKey] = BatForm.Inactive + "";
					}
				}
				else
				{
					Game1.player.ignoreCollisions = true;
					if (height.Value == 0)
					{
						PlayTransform();
						Game1.player.applyBuff(new Buff(
							id: $"{SModManifest.UniqueID}.BatForm",
							effects: new BuffEffects()
							{
								Speed = { Config.MoveSpeed }
							},
							duration: Buff.ENDLESS
						) { visible = false });
					}
					height.Value = Math.Min(maxHeight, height.Value + 1);
					if (height.Value == maxHeight)
					{
						Game1.player.modData[batFormKey] = BatForm.Active + "";
					}
				}
				Game1.forceSnapOnNextViewportUpdate = true;
				Game1.game1.refreshWindowSettings();
			}
		}

		private void Input_ButtonsChanged(object sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
		{
			if (!Config.ModEnabled || !Context.CanPlayerMove || !Config.TransformKey.JustPressed() || BatFormStatus(Game1.player) == BatForm.SwitchingFrom || BatFormStatus(Game1.player) == BatForm.SwitchingTo || (Config.NightOnly && Game1.timeOfDay < 1800) || (Config.OutdoorsOnly && !Game1.player.currentLocation.IsOutdoors) || (!Config.ActionsEnabled && Game1.player.isRidingHorse()))
				return;

			if (manaBarApi is not null && Config.UseMana)
			{
				if (manaBarApi.GetMana(Game1.player) >= Config.StaminaUse)
				{
					TransformBat();
				}
			}
			else
			{
				if (Game1.player.Stamina >= Config.StaminaUse)
				{
					TransformBat();
				}
			}
		}

		private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			if (CompatibilityUtility.IsManaBarLoaded)
			{
				manaBarApi = context.Helper.ModRegistry.GetApi<IManaBarApi>("spacechase0.ManaBar");
			}
			if (CompatibilityUtility.IsZoomLevelLoaded)
			{
				Config.ZoomOutEnabled = false;
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
					if (value == false)
					{
						ResetBat();
					}
					Config.ModEnabled = value;
				}
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.NightOnly.Name"),
				getValue: () => Config.NightOnly,
				setValue: value => {
					Config.NightOnly = value;
					if (value == true && Game1.timeOfDay < 1800)
					{
						Game1.player.modData[batFormKey] = BatForm.SwitchingFrom.ToString();
					}
				}
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.OutdoorsOnly.Name"),
				getValue: () => Config.OutdoorsOnly,
				setValue: value => {
					Config.OutdoorsOnly = value;
					if (value == true && !Game1.player.currentLocation.IsOutdoors)
					{
						Game1.player.modData[batFormKey] = BatForm.SwitchingFrom.ToString();
					}
				}
			);
			configMenu.AddKeybindList(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.TransformKey.Name"),
				getValue: () => Config.TransformKey,
				setValue: value => Config.TransformKey = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ActionsEnabled.Name"),
				getValue: () => Config.ActionsEnabled,
				setValue: value => {
					Config.ActionsEnabled = value;
					if (value == false && Game1.player.isRidingHorse())
					{
						Game1.player.modData[batFormKey] = BatForm.SwitchingFrom.ToString();
					}
				}
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MoveSpeed.Name"),
				getValue: () => Config.MoveSpeed,
				setValue: value => Config.MoveSpeed = value
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
				name: () => SHelper.Translation.Get("GMCM.TransformSound.Name"),
				getValue: () => Config.TransformSound,
				setValue: value => Config.TransformSound = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ZoomOutEnabled.Name"),
				getValue: () => Config.ZoomOutEnabled,
				setValue: value => Config.ZoomOutEnabled = value
			);
		}
	}
}
