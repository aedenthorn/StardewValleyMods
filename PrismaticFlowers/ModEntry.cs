using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;

namespace PrismaticFlowers
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
		public const string prismaticKey = "aedenthorn.PrismaticFlowers/prismatic";
		public const string dictPath = "aedenthorn.PrismaticFlowers/dict";
        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            Harmony harmony = new Harmony(ModManifest.UniqueID);
			harmony.PatchAll();
			foreach(var mi in new[] { 
				AccessTools.Method(typeof(ColoredObject), nameof(ColoredObject.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
                AccessTools.Method(typeof(ColoredObject), nameof(ColoredObject.drawInMenu), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }),
                AccessTools.Method(typeof(ColoredObject), nameof(ColoredObject.drawWhenHeld))
			})
			{
                harmony.Patch(
                    original: mi,
                    transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ColoredObject_Draw_Transpiler))
                );
            }
			foreach(var mi in new[] { 
				AccessTools.Method(typeof(Crop), nameof(Crop.draw)),
                AccessTools.Method(typeof(Crop), nameof(Crop.drawWithOffset))
			})
			{
                harmony.Patch(
                    original: mi,
                    transpiler: new HarmonyMethod(typeof(ModEntry), nameof(Crop_Draw_Transpiler))
                );
            }
        }
        public override object GetApi()
        {
			return new PrismaticFlowersAPI();
        }
        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
			if(!Config.ModEnabled) 
				return;
			if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
			{
				e.LoadFrom(() => new Dictionary<string, PrismaticData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
                if (Config.Debug)
                {
                    e.Edit((IAssetData data) =>
                    {
                        data.AsDictionary<string, PrismaticData>().Data["597"] = new PrismaticData() { Chance = 50, CropType = "Random", ObjectType = "Random", Speed = 4, Color1 = "#237FFF", Color2 = "#BFE4FF" };
                    });
                }
            }
        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			// Get Generic Mod Config Menu's API
			var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

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
                gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.PrismaticChance.Name"),
					getValue: () => Config.PrismaticChance,
					setValue: value => Config.PrismaticChance = value
				);
                gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ObjectSpeed.Name"),
					getValue: () => Config.PriceMultiplier +"",
					setValue: value => { if (float.TryParse(value, System.Globalization.NumberStyles.Any, null, out float f)) Config.PriceMultiplier = f; }
				);
                gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ObjectSpeed.Name"),
					getValue: () => Config.ObjectSpeed +"",
					setValue: value => { if (float.TryParse(value, System.Globalization.NumberStyles.Any, null, out float f)) Config.ObjectSpeed = f; }
				);
                gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.CropSpeed.Name"),
					getValue: () => Config.CropSpeed + "",
					setValue: value => { if (float.TryParse(value, System.Globalization.NumberStyles.Any, null, out float f)) Config.CropSpeed = f; }
				);
                gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ObjectPattern.Name"),
					getValue: () => Config.ObjectPattern +"",
					setValue: value => { if (Enum.TryParse(typeof(PrismaticPattern), value, true, out var e)) Config.ObjectPattern = (PrismaticPattern)e; }
				);
                gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.CropPattern.Name"),
					getValue: () => Config.CropPattern +"",
					setValue: value => { if (Enum.TryParse(typeof(PrismaticPattern), value, true, out var e)) Config.CropPattern = (PrismaticPattern)e; }
				);
            }
		}
	}

}
