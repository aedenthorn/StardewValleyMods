using System.IO;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace AllChestsMenu
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

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
			Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
		}

		public void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			if (Game1.activeClickableMenu is AllChestsMenu allChestsMenu)
			{
				if (Game1.options.snappyMenus && Game1.options.gamepadControls && e.Button == Config.SwitchButton)
				{
					Game1.playSound("shwip");
					if (!allChestsMenu.focusBottom)
					{
						allChestsMenu.lastTopSnappedCC = Game1.activeClickableMenu.currentlySnappedComponent;
					}
					allChestsMenu.focusBottom = !allChestsMenu.focusBottom;
					Game1.activeClickableMenu.currentlySnappedComponent = null;
					Game1.activeClickableMenu.snapToDefaultClickableComponent();
				}
				if ((allChestsMenu.locationText.Selected || allChestsMenu.renameBox.Selected) && e.Button.ToString().Length == 1)
				{
					SHelper.Input.Suppress(e.Button);
				}
			}
			if (e.Button == Config.MenuKey && (Config.ModKey == SButton.None || !Config.ModToOpen || Helper.Input.IsDown(Config.ModKey)))
			{
				OpenMenu();
			}
		}

		public void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			// Get Mobile Phone's API
			IMobilePhoneApi phoneAPI = Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");

			phoneAPI?.AddApp("aedenthorn.AllChestsMenu", "AllChestsMenu", OpenMenu, Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "icon.png")));

			// Get Generic Mod Config Menu's API
			IGenericModConfigMenuApi gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

			if (gmcm is not null)
			{
				// Register mod
				gmcm.Register(
					mod: ModManifest,
					reset: () => Config = new ModConfig(),
					save: () => Helper.WriteConfig(Config)
				);

				// Main section
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
					getValue: () => Config.ModEnabled,
					setValue: value => Config.ModEnabled = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.LimitToCurrentLocation.Name"),
					getValue: () => Config.LimitToCurrentLocation,
					setValue: value => Config.LimitToCurrentLocation = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncludeFridge.Name"),
					getValue: () => Config.IncludeFridge,
					setValue: value => Config.IncludeFridge = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncludeMiniFridges.Name"),
					getValue: () => Config.IncludeMiniFridges,
					setValue: value => Config.IncludeMiniFridges = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncludeShippingBin.Name"),
					getValue: () => Config.IncludeShippingBin,
					setValue: value => Config.IncludeShippingBin = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.UnrestrictedShippingBin.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.UnrestrictedShippingBin.Tooltip"),
					getValue: () => Config.UnrestrictedShippingBin,
					setValue: value => Config.UnrestrictedShippingBin = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncludeMiniShippingBins.Name"),
					getValue: () => Config.IncludeMiniShippingBins,
					setValue: value => Config.IncludeMiniShippingBins = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncludeJunimoChests.Name"),
					getValue: () => Config.IncludeJunimoChests,
					setValue: value => Config.IncludeJunimoChests = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncludeAutoGrabbers.Name"),
					getValue: () => Config.IncludeAutoGrabbers,
					setValue: value => Config.IncludeAutoGrabbers = value
				);
				gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SecondarySortingPriority.Name"),
					getValue: () => Config.SecondarySortingPriority,
					setValue: value => Config.SecondarySortingPriority = value,
					allowedValues: new string[] { "X", "Y" }
				);
				gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.MenuKey.Name"),
					getValue: () => Config.MenuKey,
					setValue: value => Config.MenuKey = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ModToOpen.Name"),
					getValue: () => Config.ModToOpen,
					setValue: value => Config.ModToOpen = value
				);
				gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ModKey.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.ModKey.Tooltip"),
					getValue: () => Config.ModKey,
					setValue: value => Config.ModKey = value
				);
				gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ModKey2.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.ModKey2.Tooltip"),
					getValue: () => Config.ModKey2,
					setValue: value => Config.ModKey2 = value
				);
				gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SwitchButton.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.SwitchButton.Tooltip"),
					getValue: () => Config.SwitchButton,
					setValue: value => Config.SwitchButton = value
				);
			}
		}
	}
}
