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
			MigrateLegacyConfig();

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
			bool keyboardOpen = e.Button == Config.KeyboardMenuKey
				&& (!Config.KeyboardRequireModifierToOpen || Config.KeyboardOpenModifierKey == SButton.None || Helper.Input.IsDown(Config.KeyboardOpenModifierKey));
			bool controllerOpen = e.Button == Config.ControllerMenuButton
				&& (!Config.ControllerRequireModifierToOpen || Config.ControllerOpenModifierButton == SButton.None || Helper.Input.IsDown(Config.ControllerOpenModifierButton));
			if (keyboardOpen || controllerOpen)
			{
				OpenMenu();
			}
		}

		private void MigrateLegacyConfig()
		{
			bool changed = false;

			if (Config.MenuKey != SButton.None)
			{
				if (Config.MenuKey.ToString().StartsWith("Controller"))
				{
					if (Config.ControllerMenuButton == SButton.None)
					{
						Config.ControllerMenuButton = Config.MenuKey;
						changed = true;
					}
				}
				else
				{
					if (Config.KeyboardMenuKey == SButton.None || Config.KeyboardMenuKey == SButton.F2)
					{
						Config.KeyboardMenuKey = Config.MenuKey;
						changed = true;
					}
				}
			}

			if (Config.ModToOpen)
			{
				if (!Config.KeyboardRequireModifierToOpen)
				{
					Config.KeyboardRequireModifierToOpen = true;
					changed = true;
				}
				if (!Config.ControllerRequireModifierToOpen)
				{
					Config.ControllerRequireModifierToOpen = true;
					changed = true;
				}
			}

			if (Config.KeyboardOpenModifierKey == SButton.LeftShift && Config.ModKey != SButton.LeftShift)
			{
				Config.KeyboardOpenModifierKey = Config.ModKey;
				changed = true;
			}

			if (Config.MenuKey != SButton.None)
			{
				Config.MenuKey = SButton.None;
				changed = true;
			}

			if (changed)
				Helper.WriteConfig(Config);
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

				// ==================== GENERAL SECTION ====================
				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.Section.General.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.Section.General.Desc")
				);

				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.ModEnabled.Tooltip"),
					getValue: () => Config.ModEnabled,
					setValue: value => Config.ModEnabled = value
				);

				// ==================== CONTAINERS SECTION ====================
				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.Section.Containers.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.Section.Containers.Desc")
				);

				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.LimitToCurrentLocation.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.LimitToCurrentLocation.Tooltip"),
					getValue: () => Config.LimitToCurrentLocation,
					setValue: value => Config.LimitToCurrentLocation = value
				);

				gmcm.AddParagraph(
					mod: ModManifest,
					text: () => ""
				); // spacer

				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncludeFridge.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.IncludeFridge.Tooltip"),
					getValue: () => Config.IncludeFridge,
					setValue: value => Config.IncludeFridge = value
				);

				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncludeMiniFridges.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.IncludeMiniFridges.Tooltip"),
					getValue: () => Config.IncludeMiniFridges,
					setValue: value => Config.IncludeMiniFridges = value
				);

				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncludeShippingBin.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.IncludeShippingBin.Tooltip"),
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
					tooltip: () => SHelper.Translation.Get("GMCM.IncludeMiniShippingBins.Tooltip"),
					getValue: () => Config.IncludeMiniShippingBins,
					setValue: value => Config.IncludeMiniShippingBins = value
				);

				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncludeJunimoChests.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.IncludeJunimoChests.Tooltip"),
					getValue: () => Config.IncludeJunimoChests,
					setValue: value => Config.IncludeJunimoChests = value
				);

				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.IncludeAutoGrabbers.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.IncludeAutoGrabbers.Tooltip"),
					getValue: () => Config.IncludeAutoGrabbers,
					setValue: value => Config.IncludeAutoGrabbers = value
				);



				// ==================== SORTING SECTION ====================
				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.Section.Sorting.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.Section.Sorting.Desc")
				);

				gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SecondarySortingPriority.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.SecondarySortingPriority.Tooltip"),
					getValue: () => Config.SecondarySortingPriority,
					setValue: value => Config.SecondarySortingPriority = value,
					allowedValues: new string[] { "X", "Y" },
					formatAllowedValue: value => SHelper.Translation.Get($"GMCM.SecondarySortingPriority.{value}")
				);

				// ==================== CONTROLS SECTION ====================
				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.Section.Controls.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.Section.Controls.Desc")
				);

				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.Section.KeyboardControls.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.Section.KeyboardControls.Desc")
				);

				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.KeyboardRequireModifierToOpen.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.KeyboardRequireModifierToOpen.Tooltip"),
					getValue: () => Config.KeyboardRequireModifierToOpen,
					setValue: value => Config.KeyboardRequireModifierToOpen = value
				);

				gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.KeyboardOpenModifierKey.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.KeyboardOpenModifierKey.Tooltip"),
					getValue: () => Config.KeyboardOpenModifierKey,
					setValue: value => Config.KeyboardOpenModifierKey = value
				);

				gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.KeyboardMenuKey.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.KeyboardMenuKey.Tooltip"),
					getValue: () => Config.KeyboardMenuKey,
					setValue: value => Config.KeyboardMenuKey = value
				);

				gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.TransferModifierKey.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.TransferModifierKey.Tooltip"),
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

				gmcm.AddSectionTitle(
					mod: ModManifest,
					text: () => SHelper.Translation.Get("GMCM.Section.ControllerControls.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.Section.ControllerControls.Desc")
				);

				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ControllerRequireModifierToOpen.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.ControllerRequireModifierToOpen.Tooltip"),
					getValue: () => Config.ControllerRequireModifierToOpen,
					setValue: value => Config.ControllerRequireModifierToOpen = value
				);

				gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ControllerOpenModifierButton.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.ControllerOpenModifierButton.Tooltip"),
					getValue: () => Config.ControllerOpenModifierButton,
					setValue: value => Config.ControllerOpenModifierButton = value
				);

				gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ControllerMenuButton.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.ControllerMenuButton.Tooltip"),
					getValue: () => Config.ControllerMenuButton,
					setValue: value => Config.ControllerMenuButton = value
				);

				gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SwitchButton.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.SwitchButton.Tooltip"),
					getValue: () => Config.SwitchButton,
					setValue: value => Config.SwitchButton = value
				);

				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.EnableControllerKeyboard.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.EnableControllerKeyboard.Tooltip"),
					getValue: () => Config.EnableControllerKeyboard,
					setValue: value => Config.EnableControllerKeyboard = value
				);
			}
		}
	}
}
