using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using System.Collections.Generic;
using System.Globalization;
using Object = StardewValley.Object;

namespace PortableFurnace
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static string texturePath = "aedenthorn.PortableFurnace/furnace";
        public static string timeKey = "aedenthorn.PortableFurnace/time";
        public static string repeatKey = "aedenthorn.PortableFurnace/repeat";
        public static string itemKey = "aedenthorn.PortableFurnace/item";
        public static string amountKey = "aedenthorn.PortableFurnace/amount";

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();


            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.Content.AssetRequested += Content_AssetRequested;
            
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.Left)
            {
                List<string> playerRecipes = new List<string>();

                using (Dictionary<string, string>.KeyCollection.Enumerator enumerator = CraftingRecipe.craftingRecipes.Keys.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        string key = enumerator.Current;
                        if (Game1.player.craftingRecipes.ContainsKey(key))
                        {
                            playerRecipes.Add(key);
                        }
                    }
                }
                foreach (string text in playerRecipes)
                {
                    if (text.StartsWith("aedenthorn"))
                    {
                        CraftingRecipe recipe = new CraftingRecipe(text, false);
                        ParsedItemData itemData = recipe.GetItemData(true);
                        Texture2D texture = itemData.GetTexture();
                    }
                }
            }
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            for(int i = 0; i < Game1.player.Items.Count; i++)
            {
                Item furnace = Game1.player.Items[i];
                float speed = GetFurnaceSpeed(furnace);
                if (speed <= 0)
                    continue;
                if(!furnace.modData.TryGetValue(itemKey, out var itemID))
                {
                    continue;
                }
                if(!furnace.modData.TryGetValue(amountKey, out var amountString) || !int.TryParse(amountString, out var amount))
                {
                    continue;
                }
                if(!furnace.modData.TryGetValue(timeKey, out var timeString) || !int.TryParse(timeString, out var timeLeft))
                {
                    continue;
                }
                int timeChange = Utility.CalculateMinutesBetweenTimes(e.OldTime, e.NewTime);
                timeLeft -= Utility.CalculateMinutesBetweenTimes(e.OldTime, e.NewTime);
                if (timeLeft < 0)
                {
                    FinishProcess(furnace, itemID, amount, speed);
                }
                else
                {
                    furnace.modData[timeKey] = timeLeft.ToString();
                    if (!string.IsNullOrEmpty(Config.WorkSound) && Game1.random.NextDouble() < Config.WorkSoundChance)
                    {
                        Game1.playSound(Config.WorkSound);
                    }
                }
            }
        }
        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(texturePath))
            {
                e.LoadFromModFile<Texture2D>("assets/furnace.png", StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(delegate (IAssetData data)
                {
                    var dict = data.AsDictionary<string, ObjectData>();
                    dict.Data["aedenthorn.PortableFurnace_CopperFurnace"] = new ObjectData()
                    {
                        Name = "aedenthorn.PortableFurnace_CopperFurnace",
                        DisplayName = SHelper.Translation.Get("Copper.name"),
                        Description = SHelper.Translation.Get("Copper.desc"),
                        Type = "Crafting",
                        Category = Object.CraftingCategory,
                        Price = 20,
                        Texture = texturePath,
                        SpriteIndex = 0,
                        ContextTags = new List<string>() { "not_placeable" }
                    };
                    dict.Data["aedenthorn.PortableFurnace_IronFurnace"] = new ObjectData()
                    {
                        Name = "aedenthorn.PortableFurnace_IronFurnace",
                        DisplayName = SHelper.Translation.Get("Iron.name"),
                        Description = SHelper.Translation.Get("Iron.desc"),
                        Type = "Crafting",
                        Category = Object.CraftingCategory,
                        Price = 100,
                        Texture = texturePath,
                        SpriteIndex = 2,
                        ContextTags = new List<string>() { "not_placeable" }
                    };
                    dict.Data["aedenthorn.PortableFurnace_GoldFurnace"] = new ObjectData()
                    {
                        Name = "aedenthorn.PortableFurnace_GoldFurnace",
                        DisplayName = SHelper.Translation.Get("Gold.name"),
                        Description = SHelper.Translation.Get("Gold.desc"),
                        Type = "Crafting",
                        Category = Object.CraftingCategory,
                        Price = 250,
                        Texture = texturePath,
                        SpriteIndex = 4,
                        ContextTags = new List<string>() { "not_placeable" }
                    };
                    dict.Data["aedenthorn.PortableFurnace_IridiumFurnace"] = new ObjectData()
                    {
                        Name = "aedenthorn.PortableFurnace_IridiumFurnace",
                        DisplayName = SHelper.Translation.Get("Iridium.name"),
                        Description = SHelper.Translation.Get("Iridium.desc"),
                        Type = "Crafting",
                        Category = Object.CraftingCategory,
                        Price = 500,
                        Texture = texturePath,
                        SpriteIndex = 6,
                        ContextTags = new List<string>() { "not_placeable" }
                    };
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                e.Edit(delegate (IAssetData data)
                {
                    var dict = data.AsDictionary<string, string>();
                    dict.Data["aedenthorn.PortableFurnace_CopperFurnace"] = $"378 20 390 25/Home/aedenthorn.PortableFurnace_CopperFurnace/false/{Config.SkillCopper}/";
                    dict.Data["aedenthorn.PortableFurnace_IronFurnace"] = $"380 20 390 25/Home/aedenthorn.PortableFurnace_IronFurnace/false/{Config.SkillIron}/";
                    dict.Data["aedenthorn.PortableFurnace_GoldFurnace"] = $"384 20 390 25/Home/aedenthorn.PortableFurnace_GoldFurnace/false/{Config.SkillGold}/";
                    dict.Data["aedenthorn.PortableFurnace_IridiumFurnace"] = $"386 20 390 25/Home/aedenthorn.PortableFurnace_IridiumFurnace/false/{Config.SkillIridium}/";
                });
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
                name: () => "Mod Enabled",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            
            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => "Toggle Key",
                getValue: () => Config.ToggleButton,
                setValue: value => Config.ToggleButton = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Copper Time Mult",
                getValue: () => Config.TimeMultCopper + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.TimeMultCopper = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Iron Time Mult",
                getValue: () => Config.TimeMultIron + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.TimeMultIron = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Gold Time Mult",
                getValue: () => Config.TimeMultGold + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.TimeMultGold = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Iridium Time Mult",
                getValue: () => Config.TimeMultIridium + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.TimeMultIridium = f; } }
            );
        }
    }
}
