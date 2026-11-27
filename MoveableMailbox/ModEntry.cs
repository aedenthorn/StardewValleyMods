using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Buildings;
using Object = StardewValley.Object;

namespace MoveableMailbox
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static ModConfig Config;
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModEntry context;

		public const string ownerKey = "aedenthorn.MoveableMailbox_Owner";

		internal static List<Object> mailboxes = new();

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			SMonitor = Monitor;
			SHelper = helper;
			context = this;

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
			helper.Events.Content.AssetRequested += Content_AssetRequested;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Farm), nameof(Farm.GetMainMailboxPosition)),
					postfix: new HarmonyMethod(typeof(Farm_GetMainMailboxPosition_Patch), nameof(Farm_GetMainMailboxPosition_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Object), nameof(Object.placementAction)),
					postfix: new HarmonyMethod(typeof(Object_placementAction_Patch), nameof(Object_placementAction_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Object), nameof(Object.performRemoveAction)),
					postfix: new HarmonyMethod(typeof(Object_performRemoveAction_Patch), nameof(Object_performRemoveAction_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Object), nameof(Object.checkForAction)),
					prefix: new HarmonyMethod(typeof(Object_checkForAction_Patch), nameof(Object_checkForAction_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Object), nameof(Object.hoverAction)),
					postfix: new HarmonyMethod(typeof(Object_hoverAction_Patch), nameof(Object_hoverAction_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.draw)),
					postfix: new HarmonyMethod(typeof(Gamelocation_draw_Patch), nameof(Gamelocation_draw_Patch.Postfix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private static void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
		{
			if (e.Name.IsEquivalentTo("Data/Buildings"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, BuildingData> data = asset.AsDictionary<string, BuildingData>().Data;
					List<BuildingPlacementTile> placementTilesToRemove = new();
					List<BuildingActionTile> actionTilesToRemove = new();
					List<BuildingDrawLayer> drawLayersToRemove = new();

					data["Farmhouse"].CollisionMap = data["Farmhouse"].CollisionMap.Replace(" ", "");
					data["Farmhouse"].CollisionMap = data["Farmhouse"].CollisionMap.Remove(data["Farmhouse"].CollisionMap.Length - 2, 1);
					foreach (BuildingPlacementTile placementTile in data["Farmhouse"].AdditionalPlacementTiles)
					{
						if (placementTile.TileArea == new Rectangle(9, 4, 1, 1) && !placementTile.OnlyNeedsToBePassable)
						{
							placementTilesToRemove.Add(placementTile);
						}
					}
					foreach (BuildingActionTile actionTile in data["Farmhouse"].ActionTiles)
					{
						if (actionTile.Id.Equals("Default_Mailbox"))
						{
							actionTilesToRemove.Add(actionTile);
						}
					}
					foreach (BuildingDrawLayer drawLayer in data["Farmhouse"].DrawLayers)
					{
						if (drawLayer.Id.Equals("Default_Mailbox"))
						{
							drawLayersToRemove.Add(drawLayer);
						}
					}
					foreach (BuildingPlacementTile placementTile in placementTilesToRemove)
					{
						data["Farmhouse"].AdditionalPlacementTiles.Remove(placementTile);
					}
					foreach (BuildingActionTile actionTile in actionTilesToRemove)
					{
						data["Farmhouse"].ActionTiles.Remove(actionTile);
					}
					foreach (BuildingDrawLayer drawLayer in drawLayersToRemove)
					{
						data["Farmhouse"].DrawLayers.Remove(drawLayer);
					}
				});
			}
			if (e.Name.IsEquivalentTo("Data/CraftingRecipes"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

					data.Add("aedenthorn.MoveableMailbox_Mailbox", "388 20 335 10/Home/aedenthorn.MoveableMailbox_Mailbox/true/default/");
				});
			}
			if (e.Name.IsEquivalentTo("Data/BigCraftables"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, BigCraftableData> data = asset.AsDictionary<string, BigCraftableData>().Data;

					data.Add("aedenthorn.MoveableMailbox_Mailbox", new BigCraftableData()
					{
						Name = "Mailbox",
						DisplayName = "[aedenthorn.MoveableMailbox_i18n item.mailbox.name]",
						Description = "[aedenthorn.MoveableMailbox_i18n item.mailbox.description]",
						Texture = "Buildings\\Mailbox",
					});
				});
			}
		}

		private static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
		{
			Farm farm = Game1.getFarm();

			InitMailBoxesList();
			if (!AnyOwnedMailbox(Game1.MasterPlayer.UniqueMultiplayerID.ToString()))
			{
				Building farmhouse = farm.GetMainFarmHouse();

				if (farmhouse is not null)
				{
					Vector2 tileLocation = new(farmhouse.tileX.Value + farmhouse.tilesWide.Value, farmhouse.tileY.Value + farmhouse.tilesHigh.Value - 1);
					Object mailbox = new(tileLocation, "aedenthorn.MoveableMailbox_Mailbox");

					mailbox.modData[ownerKey] = Game1.MasterPlayer.UniqueMultiplayerID.ToString();
					farm.objects.Add(tileLocation, mailbox);
					mailboxes.Add(mailbox);
				}
			}
			farm.mapMainMailboxPosition = new Point(-1, -1);
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			TokensUtility.Register();

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
				name: () => SHelper.Translation.Get("GMCM.MultipleMailboxes.Name"),
				getValue: () => Config.MultipleMailboxes,
				setValue: value => Config.MultipleMailboxes = value
			);
		}
	}
}
