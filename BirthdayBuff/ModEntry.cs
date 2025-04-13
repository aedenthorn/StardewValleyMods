using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace BirthdayBuff
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		internal const string BuffFrameworkKey = "aedenthorn.BuffFramework/dictionary";
		internal static object HappyBirthdayAPI;
		internal static IBuffFrameworkAPI BuffFrameworkAPI;
		internal static bool cachedResult = false;

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
			Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
			Helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
		}

		private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			cachedResult = IsBirthdayDay();
			AddBirthdayBuff();
		}

		private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
		{
			cachedResult = false;
		}

		private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
		{
			cachedResult = false;
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			HappyBirthdayAPI = SHelper.ModRegistry.GetApi("Omegasis.HappyBirthday");
			BuffFrameworkAPI = SHelper.ModRegistry.GetApi<IBuffFrameworkAPI>("aedenthorn.BuffFramework");

			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

			if (configMenu is not null)
			{
				// register mod
				configMenu.Register(
					mod: ModManifest,
					reset: () => Config = new ModConfig(),
					save: () => {
						RemoveBirthdayBuff();
						if (Config.ModEnabled)
						{
							cachedResult = IsBirthdayDay();
							AddBirthdayBuff();
						}
						BuffFrameworkAPI.UpdateBuffs();
						Helper.WriteConfig(Config);
					}
				);

				configMenu.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
					getValue: () => Config.ModEnabled,
					setValue: value => Config.ModEnabled = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.480").Trim(),
					getValue: () => Config.Farming,
					setValue: value => Config.Farming = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.486").Trim(),
					getValue: () => Config.Mining,
					setValue: value => Config.Mining = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.492").Trim(),
					getValue: () => Config.Foraging,
					setValue: value => Config.Foraging = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.483").Trim(),
					getValue: () => Config.Fishing,
					setValue: value => Config.Fishing = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.504").Trim(),
					getValue: () => Config.Attack,
					setValue: value => Config.Attack = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.501").Trim(),
					getValue: () => Config.Defense,
					setValue: value => Config.Defense = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.507").Trim(),
					getValue: () => Config.Speed,
					setValue: value => Config.Speed = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.498").Trim(),
					getValue: () => Config.MagneticRadius,
					setValue: value => Config.MagneticRadius = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.489").Trim(),
					getValue: () => Config.Luck,
					setValue: value => Config.Luck = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.495").Trim(),
					getValue: () => Config.MaxStamina,
					setValue: value => Config.MaxStamina = value
				);
				configMenu.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Sound.Name"),
					getValue: () => Config.Sound,
					setValue: value => Config.Sound = value
				);
			}
		}
	}
}
