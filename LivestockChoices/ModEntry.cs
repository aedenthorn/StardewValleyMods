using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace LivestockChoices
{
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;
		internal static ModEntry context;

		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Constructor(typeof(PurchaseAnimalsMenu), new Type[] { typeof(List<Object>), typeof(GameLocation) }),
					postfix: new HarmonyMethod(typeof(PurchaseAnimalsMenu_Patch), nameof(PurchaseAnimalsMenu_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.draw), new Type[] { typeof(SpriteBatch) }),
					transpiler: new HarmonyMethod(typeof(PurchaseAnimalsMenu_draw_Patch), nameof(PurchaseAnimalsMenu_draw_Patch.Transpiler))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.performHoverAction)),
					postfix: new HarmonyMethod(typeof(PurchaseAnimalsMenu_performHoverAction_Patch), nameof(PurchaseAnimalsMenu_performHoverAction_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.GetShopDescription), new Type[] { typeof(string) }),
					prefix: new HarmonyMethod(typeof(FarmAnimal_GetShopDescription_Patch), nameof(FarmAnimal_GetShopDescription_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.receiveLeftClick)),
					prefix: new HarmonyMethod(typeof(PurchaseAnimalsMenu_receiveLeftClick_Patch), nameof(PurchaseAnimalsMenu_receiveLeftClick_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.receiveLeftClick)),
					postfix: new HarmonyMethod(typeof(PurchaseAnimalsMenu_receiveLeftClick_Patch), nameof(PurchaseAnimalsMenu_receiveLeftClick_Patch.Postfix))
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
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is null)
				return;

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
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.BlueChickenPrice.Name"),
				getValue: () => Config.BlueChickenPrice,
				setValue: value => Config.BlueChickenPrice = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.VoidChickenPrice.Name"),
				getValue: () => Config.VoidChickenPrice,
				setValue: value => Config.VoidChickenPrice = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.GoldenChickenPrice.Name"),
				getValue: () => Config.GoldenChickenPrice,
				setValue: value => Config.GoldenChickenPrice = value
			);
		}
	}
}
