using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;

namespace LetterBlocks
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
        public const string stoneBlockKey = "aedenthorn.LetterBlocks_stone_block";
        public const string woodBlockKey = "aedenthorn.LetterBlocks_wood_block";
        public const string deluxeStoneBlockKey = "aedenthorn.LetterBlocks_stone_block_delux";
        public const string deluxeWoodBlockKey = "aedenthorn.LetterBlocks_wood_block_delux";
        public const string blockPath = "aedenthorn.LetterBlocks/block";
        public const string dictPath = "aedenthorn.LetterBlocks/dict";
        public const string letterKey = "aedenthorn.LetterBlocks/letter";
        public const string colorKey = "aedenthorn.LetterBlocks/color";

        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Input.MouseWheelScrolled += Input_MouseWheelScrolled;

            Harmony harmony = new Harmony(ModManifest.UniqueID);
			harmony.PatchAll();
        }

        private void Input_MouseWheelScrolled(object sender, StardewModdingAPI.Events.MouseWheelScrolledEventArgs e)
        {
			if (!Config.ModEnabled || !Context.IsPlayerFree)
				return;
			if(Game1.currentLocation.Objects.TryGetValue(Game1.currentCursorTile, out var obj) && TryGetBlockData(obj.ItemId, out var data))
			{
                if (!obj.modData.TryGetValue(letterKey, out string letterData))
                {
                    letterData = $"0,0,{data.DefaultColor}";
                }
                var split = letterData.Split(',');
				int row = Math.Min(int.Parse(split[0]), data.Letters.Length - 1);
                int letter = Math.Min(int.Parse(split[1]), data.Letters[row].Length - 1);
                int color = Math.Min(int.Parse(split[2]), 20);
				if (SHelper.Input.IsDown(Config.ShiftKey))
				{
                    if (data.Letters.Length == 1)
                        return;
                    letter = 0;
                    row -= Math.Sign(e.Delta);
                    if (row < 0)
                    {
                        row = data.Letters.Length - 1;
                    }
                    else if (row > data.Letters.Length - 1)
                    {
                        row = 0;
                    }
                    Game1.playSound("shwip", null);
                }
				else if (SHelper.Input.IsDown(Config.ColorKey))
				{
                    if (data.Colors?.Length == 1)
                        return;
                    int max = data.Colors != null ? data.Colors.Length - 1 : 20;
                    color += Math.Sign(e.Delta);
                    if (color < 0)
                    {
                        color = max;
                    }
                    else if (color > max)
                    {
                        color = 0;
                    }
                    Game1.playSound("toolSwap", null);
                }
                else
				{
                    letter -= Math.Sign(e.Delta);
                    if (letter < 0)
                    {
                        letter = data.Letters[row].Length - 1;
                    }
                    else if (letter > data.Letters[row].Length - 1)
                    {
                        letter = 0;
                    }
                    Game1.playSound("shiny4", null);
                }
                obj.modData[letterKey] = $"{row},{letter},{color}";
                SHelper.Input.SuppressScrollWheel();
            }
        }


        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
				e.Edit(asset =>
				{
					var dict = asset.AsDictionary<string, ObjectData>().Data;
					dict[stoneBlockKey] = new ObjectData()
					{
                        Name = stoneBlockKey,
                        DisplayName = SHelper.Translation.Get("stone-block"),
                        Description = SHelper.Translation.Get("stone-block-desc"),
                        Price = 100,
                        Texture = blockPath,
                        SpriteIndex = 0,
                        Type = "Crafting",
                        Category = 0,
                        Edibility = -300,
                    };
					dict[woodBlockKey] = new ObjectData()
					{
                        Name = woodBlockKey,
                        DisplayName = SHelper.Translation.Get("wood-block"),
                        Description = SHelper.Translation.Get("wood-block-desc"),
                        Price = 100,
                        Texture = blockPath,
                        SpriteIndex = 1,
                        Type = "Crafting",
                        Category = 0,
                        Edibility = -300,
                    };
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                e.Edit(asset =>
                {
                    var dict = asset.AsDictionary<string, string>();
                    dict.Data[stoneBlockKey] = $"390 10 80 1/Home/{stoneBlockKey}/false/default/";
                    dict.Data[woodBlockKey] = $"388 10 382 1/Home/{woodBlockKey}/false/default/";
                    dict.Data[deluxeStoneBlockKey] = $"335 10 769 1/Home/{deluxeStoneBlockKey}/false/default/";
                    dict.Data[deluxeWoodBlockKey] = $"709 10 768 1/Home/{deluxeWoodBlockKey}/false/default/";
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(blockPath))
            {
                e.LoadFromModFile<Texture2D>("assets/Block.png", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, BlockData>()
				{
					{ 
						stoneBlockKey, new BlockData()
						{
                            DefaultColor = 20,
                            SpriteText = true
                        }
					},
					{ 
						woodBlockKey, new BlockData()
						{
                            DefaultColor = 0,
                            SpriteText = true
                        }
					}
				}, StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
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
                gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ShiftKey.Name"),
					getValue: () => Config.ShiftKey,
					setValue: value => Config.ShiftKey = value
				);
                gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ColorKey.Name"),
					getValue: () => Config.ColorKey,
					setValue: value => Config.ColorKey = value
				);
            }
		}
	}

}
