using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FireBreath
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		internal static IManaBarApi manaBarApi = null;

		private bool firing = false;
		private int ticks;

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
			Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
			helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
			Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
			Helper.Events.Input.ButtonReleased += Input_ButtonReleased;
		}

		private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			if (Game1.player?.currentLocation is not null && firing)
			{
				if(ticks % 120 == 0 && !string.IsNullOrEmpty(Config.FireSound))
				{
					Game1.player.currentLocation.playSound(Config.FireSound);
				}
				if(ticks++ % 3 != 0)
				{
					return;
				}

				float angle = 0f;
				float mouthOffset = 16f;
				Vector2 origin = new(Game1.player.GetBoundingBox().Center.X - 32f, (float)Game1.player.GetBoundingBox().Center.Y - 80f);

				switch (Game1.player.facingDirection.Value)
				{
					case 0:
						origin.Y -= mouthOffset;
						angle = 90f;
						break;
					case 1:
						origin.X += mouthOffset;
						angle = 0f;
						break;
					case 2:
						angle = 270f;
						origin.Y += mouthOffset;
						break;
					case 3:
						origin.X -= mouthOffset;
						angle = 180f;
						break;
				}
				angle += (float)Math.Sin((double)((float)ticks * 16 / 1000f * 180f) * 3.1415926535897931 / 180.0) * 25f;

				const float DegreesToRadians = (float)(Math.PI / 180.0);
				const float SpeedMultiplier = 10f;
				float FireMultiplier = Config.ScaleWithSkill ? (Game1.player.getEffectiveSkillLevel(4) + 1) / 10f : 1;
				Vector2 velocity = new Vector2((float)Math.Cos(angle * DegreesToRadians), -(float)Math.Sin(angle * DegreesToRadians)) * SpeedMultiplier;
				int damage = (int)Math.Round(Config.FireDamage * FireMultiplier);
				Fireball projectile = new(damage, 10, 0, 1, 0.196349546f, velocity.X, velocity.Y, origin, null, null, null, false, true, Game1.player.currentLocation, Game1.player, null);

				projectile.ignoreTravelGracePeriod.Value = true;
				projectile.maxTravelDistance.Value = (int)Math.Round(Config.FireDistance * FireMultiplier * 5f);
				Game1.player.currentLocation.projectiles.Add(projectile);
			}
			else
			{
				ticks = 0;
			}
		}

		private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
		{
			if (!Config.ModEnabled || !firing || Config.StaminaUse <= 0)
				return;

			if (manaBarApi is not null && Config.UseMana)
			{
				if (manaBarApi.GetMana(Game1.player) < Config.StaminaUse)
				{
					firing = false;
					return;
				}
				manaBarApi.AddMana(Game1.player, -Config.StaminaUse);
			}
			else
			{
				if (Game1.player.Stamina < Config.StaminaUse)
				{
					firing = false;
					return;
				}
				Game1.player.Stamina = Math.Max(0.1f, Game1.player.Stamina - Config.StaminaUse);
			}
		}

		private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			if (e.Button == Config.FireButton)
			{
				void Fire()
				{
					Monitor.Log($"Begin fire breath, skill level {Game1.player.getEffectiveSkillLevel(4)}");
					firing = true;
				}

				if (manaBarApi is not null && Config.UseMana)
				{
					if (manaBarApi.GetMana(Game1.player) >= Config.StaminaUse)
					{
						Fire();
					}
				}
				else
				{
					if (Game1.player.Stamina >= Config.StaminaUse)
					{
						Fire();
					}
				}
			}
		}

		private void Input_ButtonReleased(object sender, ButtonReleasedEventArgs e)
		{
			if(e.Button == Config.FireButton)
			{
				Monitor.Log("End fire breath");
				firing = false;
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
				setValue: value => Config.ModEnabled = value
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.FireButton.Name"),
				getValue: () => Config.FireButton,
				setValue: value => Config.FireButton = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ScaleWithSkill.Name"),
				getValue: () => Config.ScaleWithSkill,
				setValue: value => Config.ScaleWithSkill = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.FireDamage.Name"),
				getValue: () => Config.FireDamage,
				setValue: value => Config.FireDamage = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.FireDistance.Name"),
				getValue: () => Config.FireDistance,
				setValue: value => Config.FireDistance = value
			);
			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.FireSound.Name"),
				getValue: () => Config.FireSound,
				setValue: value => Config.FireSound = value
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
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.FireAnnoysNonMonsters.Name"),
				getValue: () => Config.FireAnnoysNonMonsters,
				setValue: value => Config.FireAnnoysNonMonsters = value
			);
		}
	}
}
