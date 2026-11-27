using System;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace WallTelevisions
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
			helper.Events.Content.AssetRequested += Content_AssetRequested;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Furniture), nameof(Furniture.canBePlacedHere)),
					prefix: new HarmonyMethod(typeof(Furniture_canBePlacedHere_Patch), nameof(Furniture_canBePlacedHere_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Furniture), nameof(Furniture.GetAdditionalFurniturePlacementStatus)),
					prefix: new HarmonyMethod(typeof(Furniture_GetAdditionalFurniturePlacementStatus_Patch), nameof(Furniture_GetAdditionalFurniturePlacementStatus_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Utility), nameof(Utility.playerCanPlaceItemHere)),
					prefix: new HarmonyMethod(typeof(Utility_playerCanPlaceItemHere_Patch), nameof(Utility_playerCanPlaceItemHere_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Furniture), nameof(Furniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
					prefix: new HarmonyMethod(typeof(Furniture_draw_Patch), nameof(Furniture_draw_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
		{
			if (e.NameWithoutLocale.IsEquivalentTo("aedenthorn.WallTelevisions/plasma"))
			{
				e.LoadFromModFile<Texture2D>("assets/plasma.png", AssetLoadPriority.Low);
			}
			else if(e.NameWithoutLocale.IsEquivalentTo("aedenthorn.WallTelevisions/tropical"))
			{
				e.LoadFromModFile<Texture2D>("assets/tropical.png", AssetLoadPriority.Low);
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
				getValue: () => Config.ModEnabled,
				setValue: value => Config.ModEnabled = value
			);
		}
	}
}
