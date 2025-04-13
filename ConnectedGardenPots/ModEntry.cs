using System;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace ConnectedGardenPots
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		private const string alternativeTextureOwnerKey = "AlternativeTextureOwner";
		private const string alternativeTextureOwnerStardewDefaultValue = "Stardew.Default";
		private const string wallPlantersOffsetKey = "aedenthorn.WallPlanters/offset";
		private const string disconnectedKey = "aedenthorn.ConnectedGardenPots/disconnected";
		private static Texture2D gardenPotspriteSheet;

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
			helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
			helper.Events.Input.ButtonPressed += Input_ButtonPressed;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(IndoorPot), nameof(IndoorPot.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
					prefix: new HarmonyMethod(typeof(IndoorPot_draw_Patch), nameof(IndoorPot_draw_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(IndoorPot), nameof(IndoorPot.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
					postfix: new HarmonyMethod(typeof(IndoorPot_draw_Patch), nameof(IndoorPot_draw_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(ParsedItemData), nameof(ParsedItemData.GetTexture)),
					prefix: new HarmonyMethod(typeof(ParsedItemData_GetTexture_Patch), nameof(ParsedItemData_GetTexture_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(ParsedItemData), nameof(ParsedItemData.GetSourceRect), new Type[] { typeof(int), typeof(int) }),
					prefix: new HarmonyMethod(typeof(ParsedItemData_GetSourceRect_Patch), nameof(ParsedItemData_GetSourceRect_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (!Config.EnableMod || !Context.IsWorldReady)
				return;

			if (Config.DisconnectKeys.Keybinds[0].Buttons.All(button => SHelper.Input.IsDown(button) || SHelper.Input.IsSuppressed(button)))
			{
				if (Game1.currentLocation.objects.TryGetValue(Game1.currentCursorTile, out Object obj) && obj is IndoorPot && !obj.modData.ContainsKey(wallPlantersOffsetKey) && (!obj.modData.ContainsKey(alternativeTextureOwnerKey) || obj.modData[alternativeTextureOwnerKey] == alternativeTextureOwnerStardewDefaultValue))
				{
					if (obj.modData.ContainsKey(disconnectedKey))
					{
						obj.modData.Remove(disconnectedKey);
					}
					else
					{
						obj.modData.Add(disconnectedKey, "T");
					}
				}
			}
		}

		private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
		{
			try
			{
				gardenPotspriteSheet = Game1.content.Load<Texture2D>("aedenthorn.ConnectedGardenPots/sprite_sheet");
			}
			catch
			{
				gardenPotspriteSheet = Helper.ModContent.Load<Texture2D>("assets/sprite_sheet.png");
			}
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
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
					getValue: () => Config.EnableMod,
					setValue: value => Config.EnableMod = value
				);
				gmcm.AddKeybindList(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.DisconnectKeys.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.DisconnectKeys.Tooltip"),
					getValue: () => Config.DisconnectKeys,
					setValue: value => Config.DisconnectKeys = value
				);
			}
		}
	}
}
