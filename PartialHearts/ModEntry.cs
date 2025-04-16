using System;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;

namespace PartialHearts
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;
		internal static ModEntry context;
		private static Texture2D heartTexture;
		private static Harmony harmony;

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

			harmony = new(ModManifest.UniqueID);

			// Load Harmony patches
			try
			{
				harmony.Patch(
					original: AccessTools.Method(typeof(SocialPage), "drawNPCSlot"),
					postfix: new HarmonyMethod(typeof(SocialPage_drawNPCSlot_Patch), nameof(SocialPage_drawNPCSlot_Patch.Postfix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}

			heartTexture = Helper.ModContent.Load<Texture2D>("assets/heart.png");
		}

		public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			if (Helper.ModRegistry.IsLoaded("CJBok.CheatsMenu"))
			{
				try
				{
					Monitor.Log($"patching CJBok.CheatsMenu");
					harmony.Patch(
						original: AccessTools.Method(Type.GetType("CJBCheatsMenu.Framework.Components.CheatsOptionsNpcSlider, CJBCheatsMenu"), "draw"),
						postfix: new HarmonyMethod(typeof(CheatsOptionsNpcSlider_draw_Patch), nameof(CheatsOptionsNpcSlider_draw_Patch.Postfix))
					);
				}
				catch
				{
					Monitor.Log($"Failed patching CJBok.CheatsMenu", LogLevel.Debug);
				}
			}

			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is not null)
			{
				// register mod
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
				configMenu.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Granular.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.Granular.Tooltip"),
					getValue: () => Config.Granular,
					setValue: value => Config.Granular = value
				);
			}
		}
	}
}
