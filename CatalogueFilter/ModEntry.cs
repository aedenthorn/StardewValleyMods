using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Menus;

namespace CatalogueFilter
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

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Constructor(typeof(ShopMenu), new Type[] { typeof(string), typeof(ShopData), typeof(ShopOwnerData), typeof(NPC), typeof(ShopMenu.OnPurchaseDelegate), typeof(Func<ISalable, bool>), typeof(bool) }),
					postfix: new HarmonyMethod(typeof(ShopMenu_Patch), nameof(ShopMenu_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.drawCurrency)),
					postfix: new HarmonyMethod(typeof(ShopMenu_drawCurrency_Patch), nameof(ShopMenu_drawCurrency_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.receiveLeftClick)),
					postfix: new HarmonyMethod(typeof(ShopMenu_receiveLeftClick_Patch), nameof(ShopMenu_receiveLeftClick_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.receiveKeyPress)),
					prefix: new HarmonyMethod(typeof(ShopMenu_receiveKeyPress_Patch), nameof(ShopMenu_receiveKeyPress_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.performHoverAction)),
					postfix: new HarmonyMethod(typeof(ShopMenu_performHoverAction_Patch), nameof(ShopMenu_performHoverAction_Patch.Postfix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
				getValue: () => Config.ModEnabled,
				setValue: value => Config.ModEnabled = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ShowLabel.Name"),
				getValue: () => Config.ShowLabel,
				setValue: value => Config.ShowLabel = value
			);
			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.LabelColor.Name"),
				getValue: () => $"#{Config.LabelColor.R:X2}{Config.LabelColor.G:X2}{Config.LabelColor.B:X2}",
				setValue: value => Config.LabelColor = Utility.StringToColor(value) ?? Color.White
			);
		}
	}
}
