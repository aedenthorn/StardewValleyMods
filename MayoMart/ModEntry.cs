using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MayoMart
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
					original: AccessTools.Method(typeof(Dialogue), "parseDialogueString"),
					postfix: new HarmonyMethod(typeof(Dialogue_parseDialogueString_Patch), nameof(Dialogue_parseDialogueString_Patch.Prefix))
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
			if (!Config.ModEnabled)
				return;

			if (Config.ReplaceTexts)
			{
				if (e.NameWithoutLocale.StartsWith("Characters/Dialogue/") || e.NameWithoutLocale.StartsWith("Data/Events/") || (e.NameWithoutLocale.StartsWith("Strings/") && !e.NameWithoutLocale.IsEquivalentTo("Strings/credits")) || e.NameWithoutLocale.IsEquivalentTo("Data/ExtraDialogue") || e.NameWithoutLocale.IsEquivalentTo("Data/Festivals/winter25") || e.NameWithoutLocale.IsEquivalentTo("Data/mail"))
				{
					e.Edit(ReplaceJojaWithMayo);
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Data/hats"))
				{
					e.Edit((IAssetData data) => ReplaceJojaWithMayoStringDataFormat(data, new int[] { 1, 5 }));
				}
			}
			if (Config.ReplaceTextures)
			{
				if (e.NameWithoutLocale.IsEquivalentTo("Maps/spring_town"))
				{
					Texture2D texture = Helper.ModContent.Load<Texture2D>(e.Name switch
					{
						IAssetName name when name.IsEquivalentTo("Maps/spring_town.fr-FR") => "assets/Maps/spring_town.fr-FR.png",
						_ => "assets/Maps/spring_town.png"
					});

					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(0, 836, 512, 156), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Maps/summer_town"))
				{
					Texture2D texture = Helper.ModContent.Load<Texture2D>(e.Name switch
					{
						IAssetName name when name.IsEquivalentTo("Maps/summer_town.fr-FR") => "assets/Maps/summer_town.fr-FR.png",
						_ => "assets/Maps/summer_town.png"
					});

					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(0, 836, 512, 156), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Maps/fall_town"))
				{
					Texture2D texture = Helper.ModContent.Load<Texture2D>(e.Name switch
					{
						IAssetName name when name.IsEquivalentTo("Maps/fall_town.fr-FR") => "assets/Maps/fall_town.fr-FR.png",
						_ => "assets/Maps/fall_town.png"
					});

					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(0, 836, 512, 156), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Maps/winter_town"))
				{
					Texture2D texture = Helper.ModContent.Load<Texture2D>(e.Name switch
					{
						IAssetName name when name.IsEquivalentTo("Maps/winter_town.fr-FR") => "assets/Maps/winter_town.fr-FR.png",
						_ => "assets/Maps/winter_town.png"
					});

					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(0, 836, 512, 156), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Maps/townInterior"))
				{
					Texture2D texture = Helper.ModContent.Load<Texture2D>(e.Name switch
					{
						IAssetName name when name.IsEquivalentTo("Maps/townInterior.fr-FR") => "assets/Maps/townInterior.fr-FR.png",
						_ => "assets/Maps/townInterior.png"
					});

					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(96, 928, 320, 128), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Maps/JojaRuins_TileSheet"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/Maps/JojaRuins_TileSheet.png"), null, new Rectangle(112, 59, 80, 165), PatchMode.Overlay));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Maps/MovieTheaterJoja_TileSheet"))
				{
					Texture2D texture = Helper.ModContent.Load<Texture2D>(e.Name switch
					{
						IAssetName name when name.IsEquivalentTo("Maps/MovieTheaterJoja_TileSheet.ru-RU") => "assets/Maps/MovieTheaterJoja_TileSheet.ru-RU.png",
						_ => "assets/Maps/MovieTheaterJoja_TileSheet.png"
					});

					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(6, 1, 167, 191), PatchMode.Overlay));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Maps/MovieTheaterJoja_TileSheet_international"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/Maps/MovieTheaterJoja_TileSheet_international.png"), null, new Rectangle(6, 1, 167, 191), PatchMode.Overlay));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Maps/island_tilesheet_1"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/Maps/island_tilesheet_1.png"), null, new Rectangle(336, 352, 16, 32), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Maps/walls_and_floors"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/Maps/walls_and_floors.png"), null, new Rectangle(80, 48, 16, 48), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Minigames/jojacorps"))
				{
					Texture2D texture = Helper.ModContent.Load<Texture2D>(e.Name switch
					{
						IAssetName name when name.IsEquivalentTo("Minigames/jojacorps.es-ES") => "assets/Minigames/jojacorps.es-ES.png",
						IAssetName name when name.IsEquivalentTo("Minigames/jojacorps.fr-FR") => "assets/Minigames/jojacorps.fr-FR.png",
						IAssetName name when name.IsEquivalentTo("Minigames/jojacorps.hu-HU") => "assets/Minigames/jojacorps.hu-HU.png",
						IAssetName name when name.IsEquivalentTo("Minigames/jojacorps.ja-JP") => "assets/Minigames/jojacorps.ja-JP.png",
						IAssetName name when name.IsEquivalentTo("Minigames/jojacorps.ko-KR") => "assets/Minigames/jojacorps.ko-KR.png",
						IAssetName name when name.IsEquivalentTo("Minigames/jojacorps.pt-BR") => "assets/Minigames/jojacorps.pt-BR.png",
						IAssetName name when name.IsEquivalentTo("Minigames/jojacorps.ru-RU") => "assets/Minigames/jojacorps.ru-RU.png",
						IAssetName name when name.IsEquivalentTo("Minigames/jojacorps.zh-CN") => "assets/Minigames/jojacorps.zh-CN.png",
						_ => "assets/Minigames/jojacorps.png"
					});

					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, null, PatchMode.Overlay));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Minigames/spring_boatJourneyMap") || e.NameWithoutLocale.IsEquivalentTo("Minigames/summer_boatJourneyMap") || e.NameWithoutLocale.IsEquivalentTo("Minigames/fall_boatJourneyMap") || e.NameWithoutLocale.IsEquivalentTo("Minigames/winter_boatJourneyMap"))
				{
					Texture2D texture = Helper.ModContent.Load<Texture2D>("assets/Minigames/boatJourneyMap.png");

					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(717, 427, 16, 3), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(749, 427, 16, 3), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("TileSheets/Craftables"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/TileSheets/Craftables.png"), null, new Rectangle(80, 448, 16, 32), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("TileSheets/furniture"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/TileSheets/furniture.png"), null, new Rectangle(144, 803, 48, 23), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("TileSheets/joja_furniture"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/TileSheets/joja_furniture.png"), null, null, PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("TileSheets/joja_furnitureFront"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/TileSheets/joja_furnitureFront.png"), null, null, PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("LooseSprites/JojaCDForm"))
				{
					Texture2D texture = Helper.ModContent.Load<Texture2D>(e.Name switch
					{
						IAssetName name when name.IsEquivalentTo("LooseSprites/JojaCDForm.de-DE") => "assets/LooseSprites/JojaCDForm.de-DE.png",
						IAssetName name when name.IsEquivalentTo("LooseSprites/JojaCDForm.es-ES") => "assets/LooseSprites/JojaCDForm.es-ES.png",
						IAssetName name when name.IsEquivalentTo("LooseSprites/JojaCDForm.fr-FR") => "assets/LooseSprites/JojaCDForm.fr-FR.png",
						IAssetName name when name.IsEquivalentTo("LooseSprites/JojaCDForm.hu-HU") => "assets/LooseSprites/JojaCDForm.hu-HU.png",
						IAssetName name when name.IsEquivalentTo("LooseSprites/JojaCDForm.it-IT") => "assets/LooseSprites/JojaCDForm.it-IT.png",
						IAssetName name when name.IsEquivalentTo("LooseSprites/JojaCDForm.ja-JP") => "assets/LooseSprites/JojaCDForm.ja-JP.png",
						IAssetName name when name.IsEquivalentTo("LooseSprites/JojaCDForm.ko-KR") => "assets/LooseSprites/JojaCDForm.ko-KR.png",
						IAssetName name when name.IsEquivalentTo("LooseSprites/JojaCDForm.pt-BR") => "assets/LooseSprites/JojaCDForm.pt-BR.png",
						IAssetName name when name.IsEquivalentTo("LooseSprites/JojaCDForm.ru-RU") => "assets/LooseSprites/JojaCDForm.ru-RU.png",
						IAssetName name when name.IsEquivalentTo("LooseSprites/JojaCDForm.tr-TR") => "assets/LooseSprites/JojaCDForm.tr-TR.png",
						IAssetName name when name.IsEquivalentTo("LooseSprites/JojaCDForm.zh-CN") => "assets/LooseSprites/JojaCDForm.zh-CN.png",
						_ => "assets/LooseSprites/JojaCDForm.png"
					});
					Rectangle targetArea = e.Name switch
					{
						IAssetName name when name.IsEquivalentTo("LooseSprites/JojaCDForm.es-ES") => new Rectangle(1, 1, 103, 50),
						IAssetName name when name.IsEquivalentTo("LooseSprites/JojaCDForm.ru-RU") => new Rectangle(1, 1, 141, 50),
						_ => new Rectangle(1, 1, 133, 50)
					};

					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, targetArea, PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("LooseSprites/LetterBG"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/LooseSprites/LetterBG.png"), null, new Rectangle(198, 210, 116, 28), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("LooseSprites/Cursors"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/LooseSprites/Cursors.png"), null, new Rectangle(187, 587, 408, 1116), PatchMode.Overlay));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("LooseSprites/Cursors_1_6"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/LooseSprites/Cursors_1_6.png"), null, new Rectangle(363, 251, 55, 78), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("LooseSprites/emojis"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/LooseSprites/emojis.png"), null, new Rectangle(72, 27, 9, 9), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Characters/Farmer/hats"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/Characters/Farmer/hats.png"), null, new Rectangle(0, 640, 20, 80), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Characters/Farmer/shirts"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/Characters/Farmer/shirts.png"), null, new Rectangle(72, 192, 8, 32), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Portraits/Sam_JojaMart"))
				{
					Texture2D texture = Helper.ModContent.Load<Texture2D>("assets/Portraits/Sam_JojaMart.png");

					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(39, 60, 10, 4), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(103, 60, 10, 4), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(39, 124, 10, 4), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(103, 124, 10, 4), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(39, 188, 10, 4), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(103, 188, 10, 4), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(103, 252, 10, 4), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(39, 316, 10, 4), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Portraits/Shane"))
				{
					Texture2D texture = Helper.ModContent.Load<Texture2D>("assets/Portraits/Shane.png");

					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(42, 57, 5, 6), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(105, 57, 5, 6), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(42, 121, 5, 6), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(106, 121, 5, 6), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(42, 185, 5, 6), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(106, 185, 5, 6), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(42, 249, 5, 6), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(42, 377, 5, 6), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Portraits/Shane_Winter"))
				{
					Texture2D texture = Helper.ModContent.Load<Texture2D>("assets/Portraits/Shane_Winter.png");

					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(39, 59, 5, 5), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(103, 59, 5, 5), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(39, 123, 5, 5), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(103, 123, 5, 5), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(39, 187, 5, 5), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(103, 187, 5, 5), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(39, 251, 5, 5), PatchMode.Replace));
					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(39, 379, 5, 5), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Portraits/Shane_JojaMart"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/Portraits/Shane_JojaMart.png"), null, new Rectangle(28, 7, 82, 376), PatchMode.Overlay));
				}
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
					save: () => {
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.StartsWith("Characters/Dialogue/"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.StartsWith("Data/Events/"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.StartsWith("Strings/") && !asset.NameWithoutLocale.IsEquivalentTo("Strings/credits"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Data/ExtraDialogue"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Data/Festivals/winter25"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Data/mail"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Data/hats"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Maps/spring_town"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Maps/summer_town"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Maps/fall_town"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Maps/winter_town"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Maps/townInterior"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Maps/JojaRuins_TileSheet"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Maps/MovieTheaterJoja_TileSheet"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Maps/MovieTheaterJoja_TileSheet_international"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Maps/island_tilesheet_1"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Maps/walls_and_floors"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Minigames/jojacorps"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Minigames/spring_boatJourneyMap"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Minigames/summer_boatJourneyMap"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Minigames/fall_boatJourneyMap"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Minigames/winter_boatJourneyMap"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("TileSheets/Craftables"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("TileSheets/furniture"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("TileSheets/joja_furniture"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("TileSheets/joja_furnitureFront"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("LooseSprites/JojaCDForm"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("LooseSprites/LetterBG"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("LooseSprites/JojaCDForm"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("LooseSprites/Cursors"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("LooseSprites/Cursors_1_6"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("LooseSprites/emojis"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Characters/Farmer/hats"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Characters/Farmer/shirts"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Portraits/Sam_JojaMart"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Portraits/Shane"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Portraits/Shane_Winter"));
						SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Portraits/Shane_JojaMart"));
						Helper.WriteConfig(Config);
					}
				);

				// Main section
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => Helper.Translation.Get("GMCM.ModEnabled.Name"),
					getValue: () => Config.ModEnabled,
					setValue: value => Config.ModEnabled = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => Helper.Translation.Get("GMCM.ReplaceTexts.Name"),
					getValue: () => Config.ReplaceTexts,
					setValue: value => Config.ReplaceTexts = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => Helper.Translation.Get("GMCM.ReplaceTextures.Name"),
					getValue: () => Config.ReplaceTextures,
					setValue: value => Config.ReplaceTextures = value
				);
			}
		}
	}
}
