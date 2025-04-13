using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace WitcherMod
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
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

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			helper.Events.Content.AssetRequested += Content_AssetRequested;
		}

		private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			if (Config.EnableGeralt)
			{
				if (e.NameWithoutLocale.IsEquivalentTo("Characters/Elliott") || e.NameWithoutLocale.IsEquivalentTo("Characters/Elliott_Winter"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/Characters/Elliott.png"), null, null, PatchMode.Replace));
				}
				else if (Config.EnableDialogueChanges && e.NameWithoutLocale.IsEquivalentTo("Characters/Dialogue/Elliott"))
				{
					Dictionary<string, string> dialogues = Helper.ModContent.Load<Dictionary<string, string>>(e.Name switch
					{
						IAssetName name when name.IsEquivalentTo("Characters/Dialogue/Elliott.fr-FR") => "assets/Characters/Dialogue/Elliott.fr-FR.json",
						_ => "assets/Characters/Dialogue/Elliott.json"
					});

					e.Edit((IAssetData data) => ReplaceDialogues(data, dialogues));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Portraits/Elliott") || e.NameWithoutLocale.IsEquivalentTo("Portraits/Elliott_Winter"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/Portraits/Elliott.png"), null, null, PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Strings/NPCNames"))
				{
					e.Edit((IAssetData data) => ReplaceNPCNames(data, "Elliott", SHelper.Translation.Get("Geralt")));
				}
			}
			if (Config.EnableYennefer)
			{
				if (e.NameWithoutLocale.IsEquivalentTo("Characters/Abigail") || e.NameWithoutLocale.IsEquivalentTo("Characters/Abigail_Winter"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/Characters/Abigail.png"), null, null, PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Portraits/Abigail") || e.NameWithoutLocale.IsEquivalentTo("Portraits/Abigail_Winter"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/Portraits/Abigail.png"), null, null, PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Strings/NPCNames"))
				{
					e.Edit((IAssetData data) => ReplaceNPCNames(data, "Abigail", SHelper.Translation.Get("Yennefer")));
				}
			}
			if (Config.EnableTriss)
			{
				if (e.NameWithoutLocale.IsEquivalentTo("Characters/Penny") || e.NameWithoutLocale.IsEquivalentTo("Characters/Penny_Winter"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/Characters/Penny.png"), null, null, PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Portraits/Penny") || e.NameWithoutLocale.IsEquivalentTo("Portraits/Penny_Winter"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/Portraits/Penny.png"), null, null, PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Strings/NPCNames"))
				{
					e.Edit((IAssetData data) => ReplaceNPCNames(data, "Penny", SHelper.Translation.Get("Triss")));
				}
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
				save: () => {
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Characters/Elliott"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Characters/Elliott_Winter"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Characters/Dialogue/Elliott"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Portraits/Elliott"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Portraits/Elliott_Winter"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Characters/Abigail"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Characters/Abigail_Winter"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Portraits/Abigail"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Portraits/Abigail_Winter"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Characters/Penny"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Characters/Penny_Winter"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Portraits/Penny"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Portraits/Penny_Winter"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Strings/NPCNames"));
					Helper.WriteConfig(Config);
				}
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
				getValue: () => Config.ModEnabled,
				setValue: value => Config.ModEnabled = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.EnableGeralt.Name"),
				getValue: () => Config.EnableGeralt,
				setValue: value => Config.EnableGeralt = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.EnableYennefer.Name"),
				getValue: () => Config.EnableYennefer,
				setValue: value => Config.EnableYennefer = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.EnableTriss.Name"),
				getValue: () => Config.EnableTriss,
				setValue: value => Config.EnableTriss = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.EnableDialogueChanges.Name"),
				getValue: () => Config.EnableDialogueChanges,
				setValue: value => Config.EnableDialogueChanges = value
			);
		}
	}
}
