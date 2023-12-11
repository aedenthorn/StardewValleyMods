using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using xTile.Dimensions;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Swim
{
    public class SwimHelperEvents
    {

        private static IMonitor Monitor;
        private static ModConfig Config;
        private static IModHelper Helper;

        public static readonly PerScreen<bool> isJumping = new PerScreen<bool>(() => false);
        public static readonly PerScreen<Vector2> startJumpLoc = new PerScreen<Vector2>();
        public static readonly PerScreen<Vector2> endJumpLoc = new PerScreen<Vector2>();
        public static readonly PerScreen<ulong> lastJump = new PerScreen<ulong>(() => 0);
        public static readonly PerScreen<ulong> lastProjectile = new PerScreen<ulong>(() => 0);
        public static readonly PerScreen<int> abigailTicks = new PerScreen<int>();
        public static readonly PerScreen<SoundEffect> breatheEffect = new PerScreen<SoundEffect>(() => null);
        public static readonly PerScreen<int> ticksUnderwater = new PerScreen<int>(() => 0);
        public static readonly PerScreen<int> ticksWearingScubaGear = new PerScreen<int>(() => 0);
        public static readonly PerScreen<int> bubbleOffset = new PerScreen<int>(() => 0);
        //public static readonly PerScreen<SButton[]> abigailShootButtons = new PerScreen<SButton[]>(() =>
        //   new SButton[] {
        //        SButton.Left,
        //        SButton.Right,
        //        SButton.Up,
        //        SButton.Down
        //   }
        //);
        public static SButton[] abigailShootButtons = new SButton[] {
            SButton.Left,
            SButton.Right,
            SButton.Up,
            SButton.Down
        };

        internal static Texture2D bubbleTexture;

        private static readonly PerScreen<int> lastBreatheSound = new PerScreen<int>();
        private static readonly PerScreen<bool> surfacing = new PerScreen<bool>();

        public static void Initialize(IMonitor monitor, IModHelper helper, ModConfig config)
        {
            Monitor = monitor;
            Config = config;
            Helper = helper;
        }

        public static void Player_Warped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation.Name == "Custom_ScubaAbigailCave")
            {
                abigailTicks.Value = 0;
                e.NewLocation.characters.Clear();


                Game1.player.changeOutOfSwimSuit();
                if (Game1.player.hat.Value != null && Game1.player.hat.Value.ParentSheetIndex != 0)
                    Game1.player.addItemToInventory(Game1.player.hat.Value);
                Game1.player.hat.Value = new Hat("0");
                Game1.player.doEmote(9);
            }
            if (Game1.player.swimming.Value)
            {
                //SwimMaps.SwitchToWaterTiles(e.NewLocation);
            }
        }

        public static void Player_InventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (e.Player != Game1.player)
                return;

            if (!Game1.player.mailReceived.Contains("ScubaTank") && ModEntry.scubaTankID.Value != -1 && e.Added != null && e.Added.Count() > 0 && e.Added.FirstOrDefault() != null && e.Added.FirstOrDefault().GetType() == typeof(Clothing) && e.Added.FirstOrDefault().ParentSheetIndex == ModEntry.scubaTankID.Value)
            {
                Monitor.Log("Player found scuba tank");
                Game1.player.mailReceived.Add("ScubaTank");
            }
            if (!Game1.player.mailReceived.Contains("ScubaMask") && ModEntry.scubaMaskID.Value != -1 && e.Added != null && e.Added.Count() > 0 && e.Added.FirstOrDefault() != null && e.Added.FirstOrDefault().GetType() == typeof(Hat) && (e.Added.FirstOrDefault() as Hat).ItemId == ModEntry.scubaMaskID.Value + "")
            {
                Monitor.Log("Player found scuba mask");
                Game1.player.mailReceived.Add("ScubaMask");
            }
            if (!Game1.player.mailReceived.Contains("ScubaFins") && ModEntry.scubaFinsID.Value != -1 && e.Added != null && e.Added.Count() > 0 && e.Added.FirstOrDefault() != null && e.Added.FirstOrDefault().GetType() == typeof(Boots) && e.Added.FirstOrDefault().ParentSheetIndex == ModEntry.scubaFinsID.Value)
            {
                Monitor.Log("Player found scuba fins");
                Game1.player.mailReceived.Add("ScubaFins");
            }
        }

        public static void GameLoop_Saving(object sender, SavingEventArgs e)
        {
            foreach (var l in Game1.locations)
            {
                for (int i = l.characters.Count - 1; i >= 0; i--)
                {
                    if (l.characters[i] is Fishie || l.characters[i] is BigFishie || l.characters[i] is SeaCrab || l.characters[i] is AbigailMetalHead)
                        l.characters.RemoveAt(i);
                }
            }
        }

        public static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // load scuba gear ids

            if (ModEntry.JsonAssets != null)
            {
                ModEntry.scubaMaskID.Value = ModEntry.JsonAssets.GetHatId("Scuba Mask");
                ModEntry.scubaTankID.Value = ModEntry.JsonAssets.GetClothingId("Scuba Tank");

                if (ModEntry.scubaMaskID.Value == -1)
                {
                    Monitor.Log("Can't get ID for Swim mod item #1. Some functionality will be lost.");
                }
                else
                {
                    Monitor.Log(string.Format("Swim mod item #1 ID is {0}.", ModEntry.scubaMaskID.Value));
                }

                if (ModEntry.scubaTankID.Value == -1)
                {
                    Monitor.Log("Can't get ID for Swim mod item #2. Some functionality will be lost.");
                }
                else
                {
                    Monitor.Log(string.Format("Swim mod item #2 ID is {0}.", ModEntry.scubaTankID.Value));
                }

                try
                {
                    ModEntry.scubaFinsID.Value = Helper.GameContent.Load<Dictionary<int, string>>(@"Data/Boots").First(x => x.Value.StartsWith("Scuba Fins")).Key;
                }
                catch
                {
                    Monitor.Log("Can't get ID for Swim mod item #3. Some functionality will be lost.");
                }
                if (ModEntry.scubaFinsID.Value != -1)
                {
                    Monitor.Log(string.Format("Swim mod item #3 ID is {0}.", ModEntry.scubaFinsID.Value));
                    if (Game1.player.boots.Value != null && Game1.player.boots.Value != null && Game1.player.boots.Value.Name == "Scuba Fins" && Game1.player.boots.Value.ParentSheetIndex != ModEntry.scubaFinsID.Value)
                    {
                        Game1.player.boots.Value = new Boots(ModEntry.scubaFinsID.Value + "");
                    }
                }
            }

            // load dive maps

            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                try
                {
                    Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
                    DiveMapData data = contentPack.ReadJsonFile<DiveMapData>("content.json");
                    SwimUtils.ReadDiveMapData(data);
                }
                catch
                {
                    Monitor.Log($"couldn't read content.json in content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                }
            }

            Monitor.Log($"Reading content pack from assets/swim-map-content.json");

            try
            {
                DiveMapData myData = Helper.Data.ReadJsonFile<DiveMapData>("assets/swim-map-content.json");
                SwimUtils.ReadDiveMapData(myData);
            }
            catch (Exception ex)
            {
                Monitor.Log($"assets/swim-map-content.json file read error. Exception: {ex}", LogLevel.Warn);
            }

            if (!SwimUtils.IsWearingScubaGear() && Config.SwimSuitAlways && !Config.NoAutoSwimSuit)
                Game1.player.changeIntoSwimsuit();

            bubbleTexture = Helper.GameContent.Load<Texture2D>("LooseSprites/temporary_sprites_1");
        }





        public static void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (ModEntry.isUnderwater.Value && SwimUtils.IsMapUnderwater(Game1.player.currentLocation.Name))
            {
                if ((ticksUnderwater.Value % 100 / Math.Min(100, Config.BubbleMult)) - bubbleOffset.Value == 0)
                {
                    Game1.playSound("tinyWhip");
                    ModEntry.bubbles.Value.Add(new Vector2(Game1.player.position.X + Game1.random.Next(-24, 25), Game1.player.position.Y - 96));
                    if (ModEntry.bubbles.Value.Count > 100)
                    {
                        ModEntry.bubbles.Value = ModEntry.bubbles.Value.Skip(1).ToList();
                    }
                    bubbleOffset.Value = Game1.random.Next(30 / Math.Min(100, Config.BubbleMult));
                }

                for (int k = 0; k < ModEntry.bubbles.Value.Count; k++)
                {
                    ModEntry.bubbles.Value[k] = new Vector2(ModEntry.bubbles.Value[k].X, ModEntry.bubbles.Value[k].Y - 2);
                }

                foreach (Vector2 v in ModEntry.bubbles.Value)
                {
                    e.SpriteBatch.Draw(bubbleTexture, v + new Vector2((float)Math.Sin(ticksUnderwater.Value / 20f) * 10f - Game1.viewport.X, -Game1.viewport.Y), new Rectangle?(new Rectangle(132, 20, 8, 8)), new Color(1, 1, 1, 0.5f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
                }
                ticksUnderwater.Value++;
            }
            else
            {
                ticksUnderwater.Value = 0;
            }
        }

        public static void Display_RenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (Game1.player.currentLocation.Name == "Custom_ScubaAbigailCave")
            {
                if (abigailTicks.Value > 0 && abigailTicks.Value < 30 * 5)
                {
                    e.SpriteBatch.Draw(Game1.mouseCursors, new Vector2(Game1.viewport.Width, Game1.viewport.Height) / 2 - new Vector2(78, 31) / 2, new Rectangle?(new Rectangle(353, 1649, 78, 31)), new Color(255, 255, 255, abigailTicks.Value > 30 * 3 ? (int)Math.Round(255 * (abigailTicks.Value - 90) / 60f) : 255), 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.99f);
                }
                if (abigailTicks.Value > 0 && abigailTicks.Value < 80000 / 16 && Config.ShowOxygenBar)
                    SwimUtils.MakeOxygenBar((80000 / 16) - abigailTicks.Value, 80000 / 16);
                e.SpriteBatch.Draw(ModEntry.OxygenBarTexture.Value, new Vector2((int)Math.Round(Game1.viewport.Width * 0.13f), 100), Color.White);
                return;
            }
            int maxOx = SwimUtils.MaxOxygen();
            if (ModEntry.oxygen.Value < maxOx && Config.ShowOxygenBar)
            {
                SwimUtils.MakeOxygenBar(ModEntry.oxygen.Value, maxOx);
                e.SpriteBatch.Draw(ModEntry.OxygenBarTexture.Value, new Vector2((int)Math.Round(Game1.viewport.Width * 0.13f), 100), Color.White);
            }
        }

        public static void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // load scuba gear

            ModEntry.JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            bool flag = ModEntry.JsonAssets == null;
            if (flag)
            {
                Monitor.Log("Can't load Json Assets API for scuba gear");
            }
            else
            {
                ModEntry.JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets/json-assets"));
            }

            // fix dive maps

            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                try
                {
                    Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
                    DiveMapData data = contentPack.ReadJsonFile<DiveMapData>("content.json");
                    foreach (DiveMap map in data.Maps)
                    {
                        if (map.Features.Contains("FixWaterTiles") && !ModEntry.changeLocations.ContainsKey(map.Name))
                        {
                            ModEntry.changeLocations.Add(map.Name, false);
                        }
                    }
                }
                catch
                {
                    Monitor.Log($"couldn't read content.json in content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                }
            }

            // load breath audio

            if (Config.BreatheSound)
            {
                LoadBreatheSound();
            }

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null)
            {
                // Register mod.
                configMenu.Register(
                    mod: ModEntry.context.ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                #region Region: Basic Options.

                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "Mod Enabled?",
                    tooltip: () => "Enables and Disables mod.",
                    getValue: () => Config.EnableMod,
                    setValue: value => Config.EnableMod = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "Auto-Swim enabled?",
                    tooltip: () => "Allow character to jump to the water automatically, when you walk to land edge.",
                    getValue: () => Config.ReadyToSwim,
                    setValue: value => Config.ReadyToSwim = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "ShowOxygenBar",
                    tooltip: () => "Define, will oxygen bar draw or not, when you dive to the water.",
                    getValue: () => Config.ShowOxygenBar,
                    setValue: value => Config.ShowOxygenBar = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "SwimSuitAlways",
                    tooltip: () => "If set's true, your character will always wear a swimsuit.",
                    getValue: () => Config.SwimSuitAlways,
                    setValue: value => Config.SwimSuitAlways = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "NoAutoSwimSuit",
                    tooltip: () => "If set's false, character will NOT wear a swimsuit automatically, when you enter the water.",
                    getValue: () => Config.NoAutoSwimSuit,
                    setValue: value => Config.NoAutoSwimSuit = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "AllowActionsWhileInSwimsuit",
                    tooltip: () => "Allow you to use items, while you're swimming (may cause some visual bugs).",
                    getValue: () => Config.AllowActionsWhileInSwimsuit,
                    setValue: value => Config.AllowActionsWhileInSwimsuit = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "AllowRunningWhileInSwimsuit",
                    tooltip: () => "Allow you to run, while you're swimming (may cause some visual bugs).",
                    getValue: () => Config.AllowRunningWhileInSwimsuit,
                    setValue: value => Config.AllowRunningWhileInSwimsuit = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "EnableClickToSwim",
                    tooltip: () => "Enables or Disables possibility to manual jump to the water (by clicking certain key).",
                    getValue: () => Config.EnableClickToSwim,
                    setValue: value => Config.EnableClickToSwim = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "SwimRestoresVitals",
                    tooltip: () => "If set's true, your HP and Energy will restore, while you're swimming (like in Bath).",
                    getValue: () => Config.SwimRestoresVitals,
                    setValue: value => Config.SwimRestoresVitals = value
                );
                #endregion

                #region Region: Key Binds.

                configMenu.AddKeybind(
                    mod: ModEntry.context.ModManifest,
                    name: () => "Enable Auto-Swimming",
                    tooltip: () => "Enables and Disables auto-swimming option.",
                    getValue: () => Config.SwimKey,
                    setValue: value => Config.SwimKey = value
                );
                configMenu.AddKeybind(
                    mod: ModEntry.context.ModManifest,
                    name: () => "Toggle Swimsuit",
                    tooltip: () => "Change character cloth to swimsuit and vice versa.",
                    getValue: () => Config.SwimSuitKey,
                    setValue: value => Config.SwimSuitKey = value
                );
                configMenu.AddKeybind(
                    mod: ModEntry.context.ModManifest,
                    name: () => "Dive",
                    tooltip: () => "Change character cloth to swimsuit and vice versa.",
                    getValue: () => Config.DiveKey,
                    setValue: value => Config.DiveKey = value
                );
                configMenu.AddKeybind(
                    mod: ModEntry.context.ModManifest,
                    name: () => "Manual Jump",
                    tooltip: () => "Allow you to jump into the water by clicking a certain key.",
                    getValue: () => Config.ManualJumpButton,
                    setValue: value => Config.ManualJumpButton = value
                );
                #endregion

                #region Region: Advanced Tweaks.

                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "JumpTimeInMilliseconds",
                    tooltip: () => "Sets jumping animation time.",
                    getValue: () => Config.JumpTimeInMilliseconds,
                    setValue: value => Config.JumpTimeInMilliseconds = value
                );

                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "OxygenMult",
                    tooltip: () => "Sets oxygen multiplier (Energy * Mult = O2).",
                    getValue: () => Config.OxygenMult,
                    setValue: value => Config.OxygenMult = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "BubbleMult",
                    tooltip: () => "Set's quantity multiplier of bubbles.",
                    getValue: () => Config.BubbleMult,
                    setValue: value => Config.BubbleMult = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "AddFishies",
                    tooltip: () => "Allow fishes to spawn in underwater.",
                    getValue: () => Config.AddFishies,
                    setValue: value => Config.AddFishies = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "AddCrabs",
                    tooltip: () => "Allow crabs to spawn in underwater.",
                    getValue: () => Config.AddCrabs,
                    setValue: value => Config.AddCrabs = value
                );
                configMenu.AddBoolOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "BreatheSound",
                    tooltip: () => "If sets true, while you're underwater you will hear breathe sound.",
                    getValue: () => Config.BreatheSound,
                    setValue: value => Config.BreatheSound = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "MineralPerThousandMin",
                    tooltip: () => "Sets minimal quantity, that can be meet underwater.",
                    getValue: () => Config.MineralPerThousandMin,
                    setValue: value => Config.MineralPerThousandMin = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "MineralPerThousandMax",
                    tooltip: () => "Sets maximal quantity, that can be meet underwater.",
                    getValue: () => Config.MineralPerThousandMax,
                    setValue: value => Config.MineralPerThousandMax = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "CrabsPerThousandMin",
                    tooltip: () => "Sets minimal quantity, that can be meet underwater.",
                    getValue: () => Config.CrabsPerThousandMin,
                    setValue: value => Config.CrabsPerThousandMin = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "CrabsPerThousandMax",
                    tooltip: () => "Sets maximal quantity, that can be meet underwater.",
                    getValue: () => Config.CrabsPerThousandMax,
                    setValue: value => Config.CrabsPerThousandMax = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "PercentChanceCrabIsMimic",
                    tooltip: () => "Sets chance to change crab by the mimic one.",
                    getValue: () => Config.PercentChanceCrabIsMimic,
                    setValue: value => Config.PercentChanceCrabIsMimic = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "MinSmolFishies",
                    tooltip: () => "Sets minimal quantity, that can be meet underwater.",
                    getValue: () => Config.MinSmolFishies,
                    setValue: value => Config.MinSmolFishies = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "MaxSmolFishies",
                    tooltip: () => "Sets maximal quantity, that can be meet underwater.",
                    getValue: () => Config.MaxSmolFishies,
                    setValue: value => Config.MaxSmolFishies = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "BigFishiesPerThousandMin",
                    tooltip: () => "Sets minimal quantity, that can be meet underwater.",
                    getValue: () => Config.BigFishiesPerThousandMin,
                    setValue: value => Config.BigFishiesPerThousandMin = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "BigFishiesPerThousandMax",
                    tooltip: () => "Sets maximal quantity, that can be meet underwater.",
                    getValue: () => Config.BigFishiesPerThousandMax,
                    setValue: value => Config.BigFishiesPerThousandMax = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "OceanForagePerThousandMin",
                    tooltip: () => "Sets minimal quantity, that can be meet underwater.",
                    getValue: () => Config.OceanForagePerThousandMin,
                    setValue: value => Config.OceanForagePerThousandMin = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "OceanForagePerThousandMax",
                    tooltip: () => "Sets maximal quantity, that can be meet underwater.",
                    getValue: () => Config.OceanForagePerThousandMax,
                    setValue: value => Config.OceanForagePerThousandMax = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "MinOceanChests",
                    tooltip: () => "Sets minimal quantity, that can be meet in underwater biome ocean.",
                    getValue: () => Config.MinOceanChests,
                    setValue: value => Config.MinOceanChests = value
                );
                configMenu.AddNumberOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "MaxOceanChests",
                    tooltip: () => "Sets maximal quantity, that can be meet in underwater biome ocean",
                    getValue: () => Config.MaxOceanChests,
                    setValue: value => Config.MaxOceanChests = value
                );
                configMenu.AddTextOption(
                    mod: ModEntry.context.ModManifest,
                    name: () => "JumpDistanceMult",
                    tooltip: () => "Multiply jump sensitivity by this amount",
                    getValue: () => Config.TriggerDistanceMult +"",
                    setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f)) { Config.TriggerDistanceMult = f; } } 
                );
                #endregion
            }
        }

        private static void LoadBreatheSound()
        {
            string filePath = Path.Combine(Helper.DirectoryPath, "assets", "breathe.wav");
            if (File.Exists(filePath))
            {
                breatheEffect.Value = SoundEffect.FromStream(new FileStream(filePath, FileMode.Open));
                Monitor.Log("Loaded breathing sound.");
            }
        }

        public static void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            foreach (KeyValuePair<string, DiveMap> kvp in ModEntry.diveMaps)
            {
                GameLocation location = Game1.getLocationFromName(kvp.Key);
                if (location == null)
                {
                    Monitor.Log($"GameLocation {kvp.Key} not found in day started loop");
                    continue;
                }
                if (kvp.Value.Features.Contains("OceanTreasure") || kvp.Value.Features.Contains("OceanResources") || kvp.Value.Features.Contains("Minerals"))
                {
                    Monitor.Log($"Clearing overlay objects from GameLocation {location.Name} ");
                    location.overlayObjects.Clear();
                }
                if (kvp.Value.Features.Contains("OceanTreasure"))
                {
                    Monitor.Log($"Adding ocean treasure to GameLocation {location.Name} ");
                    SwimMaps.AddOceanTreasure(location);
                }
                if (kvp.Value.Features.Contains("OceanResources"))
                {
                    Monitor.Log($"Adding ocean forage to GameLocation {location.Name} ");
                    SwimMaps.AddOceanForage(location);
                }
                if (kvp.Value.Features.Contains("Minerals"))
                {
                    Monitor.Log($"Adding minerals to GameLocation {location.Name} ");
                    SwimMaps.AddMinerals(location);
                }
                if (kvp.Value.Features.Contains("SmolFishies") || kvp.Value.Features.Contains("BigFishies") || kvp.Value.Features.Contains("Crabs"))
                {
                    Monitor.Log($"Clearing characters from GameLocation {location.Name} ");
                    location.characters.Clear();
                }
                if (kvp.Value.Features.Contains("SmolFishies"))
                {
                    Monitor.Log($"Adding smol fishies to GameLocation {location.Name} ");
                    SwimMaps.AddFishies(location);
                }
                if (kvp.Value.Features.Contains("BigFishies"))
                {
                    Monitor.Log($"Adding big fishies to GameLocation {location.Name} ");
                    SwimMaps.AddFishies(location, false);
                }
                if (kvp.Value.Features.Contains("Crabs"))
                {
                    Monitor.Log($"Adding crabs to GameLocation {location.Name} ");
                    SwimMaps.AddCrabs(location);
                }
                if (kvp.Value.Features.Contains("WaterTiles"))
                {
                    Monitor.Log($"Adding water tiles to GameLocation {location.Name} ");
                    SwimMaps.AddWaterTiles(location);
                }
                if (kvp.Value.Features.Contains("Underwater"))
                {
                    Monitor.Log($"Removing water tiles from GameLocation {location.Name} ");
                    SwimMaps.RemoveWaterTiles(location);
                }
            }
            if (Game1.getLocationFromName("Custom_ScubaCave") != null && !Game1.player.mailReceived.Contains("ScubaMask"))
            {
                SwimMaps.AddScubaChest(Game1.getLocationFromName("Custom_ScubaCave"), new Vector2(10, 14), "ScubaMask");
            }
            ModEntry.marinerQuestionsWrongToday.Value = false;
            ModEntry.oxygen.Value = SwimUtils.MaxOxygen();
        }

        public static void Input_ButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (Game1.player == null)
            {
                ModEntry.myButtonDown.Value = false;
                return;
            }
            SwimUtils.CheckIfMyButtonDown();
        }


        public static void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Game1.player == null || Game1.player.currentLocation == null)
            {
                ModEntry.myButtonDown.Value = false;
                return;
            }

            if (false && e.Button == SButton.Q)
            {
                SwimUtils.SeaMonsterSay("The quick brown fox jumped over the slow lazy dog.");
            }

            if (Game1.activeClickableMenu != null && Game1.player.currentLocation.Name == "Custom_ScubaCrystalCave" && Game1.player.currentLocation.lastQuestionKey?.StartsWith("SwimMod_Mariner_") == true)
            {
                IClickableMenu menu = Game1.activeClickableMenu;
                if (menu == null || menu.GetType() != typeof(DialogueBox))
                    return;

                DialogueBox db = menu as DialogueBox;
                int resp = db.selectedResponse;
                List<Response> resps = db.responses.ToList();

                if (resp < 0 || resps == null || resp >= resps.Count || resps[resp] == null)
                    return;
                Game1.player.currentLocation.lastQuestionKey = "";

                SwimDialog.OldMarinerDialogue(resps[resp].responseKey);
                return;
            }

            if (false && e.Button == SButton.Q)
            {
                var v1 = Game1.game1;
                return;
                //Game1.player.currentLocation.overlayObjects[Game1.player.getTileLocation() + new Vector2(0, 1)] = new Chest(0, new List<Item>() { Helper.Value.Input.IsDown(SButton.LeftShift) ? (Item)(new StardewValley.Object(434, 1)) : (new Hat(ModEntry.scubaMaskID.Value)) }, Game1.player.getTileLocation() + new Vector2(0, 1), false, 0);
            }

            if (e.Button == Config.DiveKey && Game1.activeClickableMenu == null && !Game1.player.UsingTool && ModEntry.diveMaps.ContainsKey(Game1.player.currentLocation.Name) && ModEntry.diveMaps[Game1.player.currentLocation.Name].DiveLocations.Count > 0)
            {
                Point pos = Game1.player.TilePoint;
                Location loc = new Location(pos.X, pos.Y);

                if (!SwimUtils.IsInWater())
                {
                    return;
                }

                DiveMap dm = ModEntry.diveMaps[Game1.player.currentLocation.Name];
                DiveLocation diveLocation = null;
                foreach (DiveLocation dl in dm.DiveLocations)
                {
                    if (dl.GetRectangle().X == -1 || dl.GetRectangle().Contains(loc))
                    {
                        diveLocation = dl;
                        break;
                    }
                }

                if (diveLocation == null)
                {
                    Monitor.Log($"No dive destination for this point on this map", LogLevel.Debug);
                    return;
                }

                if (Game1.getLocationFromName(diveLocation.OtherMapName) == null)
                {
                    Monitor.Log($"Can't find destination map named {diveLocation.OtherMapName}", LogLevel.Warn);
                    return;
                }

                Monitor.Log($"warping to {diveLocation.OtherMapName}", LogLevel.Debug);
                SwimUtils.DiveTo(diveLocation);
                return;
            }

            if (e.Button == Config.SwimKey && Game1.activeClickableMenu == null && (!Game1.player.swimming.Value || !Config.ReadyToSwim) && !isJumping.Value)
            {
                Config.ReadyToSwim = !Config.ReadyToSwim;
                Helper.WriteConfig(Config);
                Monitor.Log($"Ready to swim: {Config.ReadyToSwim}");
                return;
            }

            if (e.Button == Config.SwimSuitKey && Game1.activeClickableMenu == null)
            {
                Config.SwimSuitAlways = !Config.SwimSuitAlways;
                Helper.WriteConfig(Config);
                if (!Game1.player.swimming.Value)
                {
                    if (!Config.SwimSuitAlways)
                        Game1.player.changeOutOfSwimSuit();
                    else
                        Game1.player.changeIntoSwimsuit();
                }
                return;
            }

        }

        public static void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.player.currentLocation == null || Game1.player == null || !Game1.displayFarmer || Game1.player.position == null)
                return;

            ModEntry.isUnderwater.Value = SwimUtils.IsMapUnderwater(Game1.player.currentLocation.Name);

            if (Game1.player.currentLocation.Name == "Custom_ScubaAbigailCave")
            {
                AbigailCaveTick();
            }

            if (Game1.activeClickableMenu == null)
            {
                if (ModEntry.isUnderwater.Value)
                {
                    if (ModEntry.oxygen.Value >= 0)
                    {
                        if (!SwimUtils.IsWearingScubaGear())
                            ModEntry.oxygen.Value--;
                        else
                        {
                            if (ModEntry.oxygen.Value < SwimUtils.MaxOxygen())
                                ModEntry.oxygen.Value++;
                            if (ModEntry.oxygen.Value < SwimUtils.MaxOxygen())
                                ModEntry.oxygen.Value++;
                        }
                    }
                    if (ModEntry.oxygen.Value < 0 && !surfacing.Value)
                    {
                        surfacing.Value = true;
                        Game1.playSound("pullItemFromWater");
                        DiveLocation diveLocation = ModEntry.diveMaps[Game1.player.currentLocation.Name].DiveLocations.Last();
                        SwimUtils.DiveTo(diveLocation);
                    }
                }
                else
                {
                    surfacing.Value = false;
                    if (ModEntry.oxygen.Value < SwimUtils.MaxOxygen())
                        ModEntry.oxygen.Value++;
                    if (ModEntry.oxygen.Value < SwimUtils.MaxOxygen())
                        ModEntry.oxygen.Value++;
                }
            }

            if (SwimUtils.IsWearingScubaGear())
            {
                ticksWearingScubaGear.Value++;
                if (Config.BreatheSound && breatheEffect.Value != null && (lastBreatheSound.Value == 0 || ticksWearingScubaGear.Value - lastBreatheSound.Value > 6000 / 16))
                {
                    Monitor.Log("Playing breathe sound");
                    lastBreatheSound.Value = ticksWearingScubaGear.Value;
                    breatheEffect.Value.Play(0.5f * Game1.options.soundVolumeLevel, 0f, 0f);
                }
            }
            else
            {
                if (breatheEffect.Value != null && lastBreatheSound.Value != 0)
                {
                    breatheEffect.Value.Dispose();
                    LoadBreatheSound();
                }
                lastBreatheSound.Value = 0;
                ticksWearingScubaGear.Value = 0;
            }

            if (isJumping.Value)
            {
                float difx = endJumpLoc.Value.X - startJumpLoc.Value.X;
                float dify = endJumpLoc.Value.Y - startJumpLoc.Value.Y;
                float completed = Game1.player.freezePause / (float)Config.JumpTimeInMilliseconds;
                if (Game1.player.freezePause <= 0)
                {
                    Game1.player.position.Value = endJumpLoc.Value;
                    isJumping.Value = false;
                    if (ModEntry.willSwim.Value)
                    {
                        Game1.player.currentLocation.playSound("waterSlosh");
                        Game1.player.swimming.Value = true;
                    }
                    else
                    {
                        if (!Config.SwimSuitAlways)
                            Game1.player.changeOutOfSwimSuit();
                    }
                    return;
                }
                Game1.player.position.Value = new Vector2(endJumpLoc.Value.X - (difx * completed), endJumpLoc.Value.Y - (dify * completed) - (float)Math.Sin(completed * Math.PI) * 64);
                return;
            }
            if (!SwimUtils.CanSwimHere())
                return;

            // only if ready to swim from here on!
            var readyToAutoSwim = Config.ReadyToSwim;
            var manualSwim = Helper.Input.IsDown(Config.ManualJumpButton);

            // !IMP: Conditions, with locations (i.e. locations with restricted swimming), must be checked here.
            if ((!readyToAutoSwim && !manualSwim) || !Context.IsPlayerFree)
            {
                return;
            }

            if (Game1.player.swimming.Value && !SwimUtils.IsInWater() && !isJumping.Value)
            {
                Monitor.Log("Swimming out of water");
                ModEntry.willSwim.Value = false;
                Game1.player.freezePause = Config.JumpTimeInMilliseconds;
                Game1.player.currentLocation.playSound("dwop");
                Game1.player.currentLocation.playSound("waterSlosh");
                isJumping.Value = true;
                startJumpLoc.Value = Game1.player.position.Value;
                endJumpLoc.Value = Game1.player.position.Value;

                Game1.player.swimming.Value = false;
                if (Game1.player.bathingClothes.Value && !Config.SwimSuitAlways)
                    Game1.player.changeOutOfSwimSuit();
            }

            if (Game1.player.swimming.Value)
            {
                DiveMap dm = null;
                Point edgePos = Game1.player.TilePoint;

                if (ModEntry.diveMaps.ContainsKey(Game1.player.currentLocation.Name))
                {
                    dm = ModEntry.diveMaps[Game1.player.currentLocation.Name];
                }

                if (Game1.player.position.Y > Game1.viewport.Y + Game1.viewport.Height - 16)
                {
                    Game1.player.position.Value = new Vector2(Game1.player.position.X, Game1.viewport.Y + Game1.viewport.Height - 17);
                    if (dm != null)
                    {
                        EdgeWarp edge = dm.EdgeWarps.Find((x) => x.ThisMapEdge == "Bottom" && x.FirstTile <= edgePos.X && x.LastTile >= edgePos.X);
                        if (edge != null)
                        {
                            Point pos = SwimUtils.GetEdgeWarpDestination(edgePos.X, edge);
                            if (pos != Point.Zero)
                            {
                                Monitor.Log("warping south");
                                Game1.warpFarmer(edge.OtherMapName, pos.X, pos.Y, false);
                            }
                        }
                    }
                }
                else if (Game1.player.position.Y < Game1.viewport.Y - 16)
                {
                    Game1.player.position.Value = new Vector2(Game1.player.position.X, Game1.viewport.Y - 15);

                    if (dm != null)
                    {
                        EdgeWarp edge = dm.EdgeWarps.Find((x) => x.ThisMapEdge == "Top" && x.FirstTile <= edgePos.X && x.LastTile >= edgePos.X);
                        if (edge != null)
                        {
                            Point pos = SwimUtils.GetEdgeWarpDestination(edgePos.X, edge);
                            if (pos != Point.Zero)
                            {
                                Monitor.Log("warping north");
                                Game1.warpFarmer(edge.OtherMapName, pos.X, pos.Y, false);
                            }
                        }
                    }
                }
                else if (Game1.player.position.X > Game1.viewport.X + Game1.viewport.Width - 32)
                {
                    Game1.player.position.Value = new Vector2(Game1.viewport.X + Game1.viewport.Width - 33, Game1.player.position.Y);

                    if (dm != null)
                    {
                        EdgeWarp edge = dm.EdgeWarps.Find((x) => x.ThisMapEdge == "Right" && x.FirstTile <= edgePos.Y && x.LastTile >= edgePos.Y);
                        if (edge != null)
                        {
                            Point pos = SwimUtils.GetEdgeWarpDestination(edgePos.Y, edge);
                            if (pos != Point.Zero)
                            {
                                Monitor.Log("warping east");
                                Game1.warpFarmer(edge.OtherMapName, pos.X, pos.Y, false);
                            }
                        }
                    }

                    if (Game1.player.currentLocation.Name == "Forest")
                    {
                        if (Game1.player.position.Y / 64 > 74)
                            Game1.warpFarmer("Beach", 0, 13, false);
                        else
                            Game1.warpFarmer("Town", 0, 100, false);
                    }
                }
                else if (Game1.player.position.X < Game1.viewport.X - 32)
                {
                    Game1.player.position.Value = new Vector2(Game1.viewport.X - 31, Game1.player.position.Y);

                    if (dm != null)
                    {
                        EdgeWarp edge = dm.EdgeWarps.Find((x) => x.ThisMapEdge == "Left" && x.FirstTile <= edgePos.Y && x.LastTile >= edgePos.Y);
                        if (edge != null)
                        {
                            Point pos = SwimUtils.GetEdgeWarpDestination(edgePos.Y, edge);
                            if (pos != Point.Zero)
                            {
                                Monitor.Log("warping west");
                                Game1.warpFarmer(edge.OtherMapName, pos.X, pos.Y, false);
                            }
                        }
                    }

                    if (Game1.player.currentLocation.Name == "Town")
                    {
                        Game1.warpFarmer("Forest", 119, 43, false);
                    }
                    else if (Game1.player.currentLocation.Name == "Beach")
                    {
                        Game1.warpFarmer("Forest", 119, 111, false);
                    }
                }

                if (Game1.player.bathingClothes.Value && SwimUtils.IsWearingScubaGear() && !Config.SwimSuitAlways)
                    Game1.player.changeOutOfSwimSuit();
                else if (!Game1.player.bathingClothes.Value && !Config.NoAutoSwimSuit && (!SwimUtils.IsWearingScubaGear() || Config.SwimSuitAlways))
                    Game1.player.changeIntoSwimsuit();

                if (Game1.player.boots.Value != null && ModEntry.scubaFinsID.Value != -1 && Game1.player.boots.Value.indexInTileSheet.Value == ModEntry.scubaFinsID.Value)
                {
                    string buffId = "42883167";
                    Buff buff = Game1.player.buffs.AppliedBuffs.Values.FirstOrDefault((Buff p) => p.Equals(buffId));
                    if (buff == null)
                    {
                        BuffsDisplay buffsDisplay = Game1.buffsDisplay;
                        Buff buff2 = new Buff("42883167",  "Scuba Fins", Helper.Translation.Get("scuba-fins"));
                        
                        buff = buff2;
                        buffsDisplay.updatedIDs.Add(buff2.id);
                    }
                    buff.millisecondsDuration = 50;
                }
            }
            else
            {
                if (SwimUtils.IsInWater() && !isJumping.Value)
                {
                    Monitor.Log("In water not swimming");

                    ModEntry.willSwim.Value = true;
                    Game1.player.freezePause = Config.JumpTimeInMilliseconds;
                    Game1.player.currentLocation.playSound("dwop");
                    isJumping.Value = true;
                    startJumpLoc.Value = Game1.player.position.Value;
                    endJumpLoc.Value = Game1.player.position.Value;


                    Game1.player.swimming.Value = true;
                    if (!Game1.player.bathingClothes.Value && !SwimUtils.IsWearingScubaGear() && !Config.NoAutoSwimSuit)
                        Game1.player.changeIntoSwimsuit();
                }

            }

            SwimUtils.CheckIfMyButtonDown();

            if (!ModEntry.myButtonDown.Value || Game1.player.millisecondsPlayed - lastJump.Value < 250 || SwimUtils.IsMapUnderwater(Game1.player.currentLocation.Name))
                return;

            // !IMP: Conditions with hand tools (i.e. rods), must be placed here.
            if ((Helper.Input.IsDown(SButton.MouseLeft) && !Game1.player.swimming.Value && (Game1.player.CurrentTool is WateringCan || Game1.player.CurrentTool is FishingRod)) ||
                (Helper.Input.IsDown(SButton.MouseRight) && !Game1.player.swimming.Value && Game1.player.CurrentTool is MeleeWeapon))
                return;

            List<Vector2> tiles = SwimUtils.GetTilesInDirection(5);
            Vector2 jumpLocation = Vector2.Zero;

            double distance = -1;
            int maxDistance = 0;
            switch (Game1.player.FacingDirection)
            {
                case 0:
                    distance = Math.Abs(Game1.player.position.Y - tiles.Last().Y * Game1.tileSize);
                    maxDistance = (int)Math.Round(Config.TriggerDistanceUp * Config.TriggerDistanceMult);
                    break;
                case 2:
                    distance = Math.Abs(Game1.player.position.Y - tiles.Last().Y * Game1.tileSize);
                    maxDistance = (int)Math.Round(Config.TriggerDistanceDown * Config.TriggerDistanceMult);
                    break;
                case 1:
                    distance = Math.Abs(Game1.player.position.X - tiles.Last().X * Game1.tileSize);
                    maxDistance = (int)Math.Round(Config.TriggerDistanceRight * Config.TriggerDistanceMult);
                    break;
                case 3:
                    distance = Math.Abs(Game1.player.position.X - tiles.Last().X * Game1.tileSize);
                    maxDistance = (int)Math.Round(Config.TriggerDistanceLeft * Config.TriggerDistanceMult);
                    break;
            }
            if (Helper.Input.IsDown(SButton.MouseLeft))
            {
                try
                {
                    int xTile = (Game1.viewport.X + Game1.getOldMouseX()) / 64;
                    int yTile = (Game1.viewport.Y + Game1.getOldMouseY()) / 64;
                    bool water = Game1.player.currentLocation.waterTiles[xTile, yTile];
                    if (Game1.player.swimming.Value != water)
                    {
                        distance = -1;
                    }
                }
                catch
                {

                }
            }
            //Monitor.Value.Log("Distance: " + distance);

            bool nextToLand = Game1.player.swimming.Value && !SwimUtils.IsWaterTile(tiles[tiles.Count - 2]) && distance < maxDistance;


            bool nextToWater = false;
            try
            {
                nextToWater = !Game1.player.swimming.Value &&
                    !SwimUtils.IsTilePassable(Game1.player.currentLocation, new Location((int)tiles.Last().X, (int)tiles.Last().Y), Game1.viewport) &&
                    (Game1.player.currentLocation.waterTiles[(int)tiles.Last().X, (int)tiles.Last().Y]
                        || SwimUtils.IsWaterTile(tiles[tiles.Count - 2]))
                    && distance < maxDistance;
            }
            catch
            {
                //Monitor.Value.Log($"exception trying to get next to water: {ex}");
            }

            //Monitor.Value.Log($"next passable {Game1.player.currentLocation.isTilePassable(new Location((int)tiles.Last().X, (int)tiles.Last().Y), Game1.viewport)} next to land: {nextToLand}, next to water: {nextToWater}");

            if (nextToLand || nextToWater)
            {
                //Monitor.Value.Log("okay to jump");
                for (int i = 0; i < tiles.Count; i++)
                {
                    Vector2 tileV = tiles[i];
                    bool isWater = false;
                    bool isPassable = false;
                    try
                    {
                        Tile tile = Game1.player.currentLocation.map.GetLayer("Buildings").PickTile(new Location((int)tileV.X * Game1.tileSize, (int)tileV.Y * Game1.tileSize), Game1.viewport.Size);
                        isWater = SwimUtils.IsWaterTile(tileV);
                        isPassable = (nextToLand && !isWater && SwimUtils.IsTilePassable(Game1.player.currentLocation, new Location((int)tileV.X, (int)tileV.Y), Game1.viewport)) || (nextToWater && isWater && (tile == null || tile.TileIndex == 76));
                        //Monitor.Value.Log($"Trying {tileV} is passable {isPassable} isWater {isWater}");
                        if (!SwimUtils.IsTilePassable(Game1.player.currentLocation, new Location((int)tileV.X, (int)tileV.Y), Game1.viewport) && !isWater && nextToLand)
                        {
                            //Monitor.Value.Log($"Nixing {tileV}");
                            jumpLocation = Vector2.Zero;
                        }
                    }
                    catch (Exception ex)
                    {
                        Monitor.Log("" + ex);
                    }
                    if (nextToLand && !isWater && isPassable)
                    {
                        Monitor.Log($"Jumping to {tileV}");
                        jumpLocation = tileV;

                    }

                    if (nextToWater && isWater && isPassable)
                    {
                        Monitor.Log($"Jumping to {tileV}");
                        jumpLocation = tileV;

                    }


                }
            }

            // Monitor.Log($"Current \"JumpLocation\" state: {jumpLocation}. Equals state: {jumpLocation == Vector2.Zero}.");

            if (jumpLocation != Vector2.Zero)
            {
                lastJump.Value = Game1.player.millisecondsPlayed;
                //Monitor.Value.Log("got swim location");
                if (Game1.player.swimming.Value)
                {
                    ModEntry.willSwim.Value = false;
                    Game1.player.swimming.Value = false;
                    Game1.player.freezePause = Config.JumpTimeInMilliseconds;
                    Game1.player.currentLocation.playSound("dwop");
                    Game1.player.currentLocation.playSound("waterSlosh");
                }
                else
                {
                    ModEntry.willSwim.Value = true;
                    if (!SwimUtils.IsWearingScubaGear() && !Config.NoAutoSwimSuit)
                        Game1.player.changeIntoSwimsuit();

                    Game1.player.freezePause = Config.JumpTimeInMilliseconds;
                    Game1.player.currentLocation.playSound("dwop");
                }
                isJumping.Value = true;
                startJumpLoc.Value = Game1.player.position.Value;
                endJumpLoc.Value = new Vector2(jumpLocation.X * Game1.tileSize, jumpLocation.Y * Game1.tileSize);
            }

        }

        public static void AbigailCaveTick()
        {

            Game1.player.CurrentToolIndex = Game1.player.Items.Count;

            List<NPC> list = Game1.player.currentLocation.characters.ToList().FindAll((n) => (n is Monster) && (n as Monster).Health <= 0);
            foreach (NPC n in list)
            {
                Game1.player.currentLocation.characters.Remove(n);
            }

            if (abigailTicks.Value < 0)
            {
                return;
            }
            Game1.exitActiveMenu();

            if (abigailTicks.Value == 0)
            {
                AccessTools.Field(Game1.player.currentLocation.characters.GetType(), "OnValueRemoved").SetValue(Game1.player.currentLocation.characters, null);
            }

            Vector2 v = Vector2.Zero;
            float yrt = (float)(1 / Math.Sqrt(2));
            if (Helper.Input.IsDown(SButton.Up) || Helper.Input.IsDown(SButton.RightThumbstickUp))
            {
                if (Helper.Input.IsDown(SButton.Right) || Helper.Input.IsDown(SButton.RightThumbstickRight))
                    v = new Vector2(yrt, -yrt);
                else if (Helper.Input.IsDown(SButton.Left) || Helper.Input.IsDown(SButton.RightThumbstickLeft))
                    v = new Vector2(-yrt, -yrt);
                else
                    v = new Vector2(0, -1);
            }
            else if (Helper.Input.IsDown(SButton.Down) || Helper.Input.IsDown(SButton.RightThumbstickDown))
            {
                if (Helper.Input.IsDown(SButton.Right) || Helper.Input.IsDown(SButton.RightThumbstickRight))
                    v = new Vector2(yrt, yrt);
                else if (Helper.Input.IsDown(SButton.Left) || Helper.Input.IsDown(SButton.RightThumbstickLeft))
                    v = new Vector2(-yrt, yrt);
                else
                    v = new Vector2(0, 1);
            }
            else if (Helper.Input.IsDown(SButton.Right) || Helper.Input.IsDown(SButton.RightThumbstickDown))
                v = new Vector2(1, 0);
            else if (Helper.Input.IsDown(SButton.Left) || Helper.Input.IsDown(SButton.RightThumbstickLeft))
                v = new Vector2(-1, 0);
            else if (Helper.Input.IsDown(SButton.MouseLeft))
            {
                float x = Game1.viewport.X + Game1.getOldMouseX() - Game1.player.position.X;
                float y = Game1.viewport.Y + Game1.getOldMouseY() - Game1.player.position.Y;
                float dx = Math.Abs(x);
                float dy = Math.Abs(y);
                if (y < 0)
                {
                    if (x > 0)
                    {
                        if (dy > dx)
                        {
                            if (dy - dx > dy / 2)
                                v = new Vector2(0, -1);
                            else
                                v = new Vector2(yrt, -yrt);

                        }
                        else
                        {
                            if (dx - dy > x / 2)
                                v = new Vector2(1, 0);
                            else
                                v = new Vector2(yrt, -yrt);
                        }
                    }
                    else
                    {
                        if (dy > dx)
                        {
                            if (dy - dx > dy / 2)
                                v = new Vector2(0, -1);
                            else
                                v = new Vector2(-yrt, -yrt);

                        }
                        else
                        {
                            if (dx - dy > x / 2)
                                v = new Vector2(-1, 0);
                            else
                                v = new Vector2(-yrt, -yrt);
                        }
                    }
                }
                else
                {
                    if (x > 0)
                    {
                        if (dy > dx)
                        {
                            if (dy - dx > dy / 2)
                                v = new Vector2(0, 1);
                            else
                                v = new Vector2(yrt, yrt);

                        }
                        else
                        {
                            if (dx - dy > x / 2)
                                v = new Vector2(1, 0);
                            else
                                v = new Vector2(yrt, yrt);
                        }
                    }
                    else
                    {
                        if (dy > dx)
                        {
                            if (dy - dx > dy / 2)
                                v = new Vector2(0, -1);
                            else
                                v = new Vector2(-yrt, yrt);

                        }
                        else
                        {
                            if (dx - dy > x / 2)
                                v = new Vector2(-1, 0);
                            else
                                v = new Vector2(-yrt, yrt);
                        }
                    }
                }
            }

            if (v != Vector2.Zero && Game1.player.millisecondsPlayed - lastProjectile.Value > 350)
            {
                Game1.player.currentLocation.projectiles.Add(new AbigailProjectile(1, 383, 0, 0, 0, v.X * 6, v.Y * 6, new Vector2(Game1.player.StandingPixel.X - 24, Game1.player.StandingPixel.Y - 48), "Cowboy_monsterDie", null, "Cowboy_gunshot", false, true, Game1.player.currentLocation, Game1.player));
                lastProjectile.Value = Game1.player.millisecondsPlayed;
            }

            foreach (SButton button in abigailShootButtons)
            {
                if (Helper.Input.IsDown(button))
                {
                    switch (button)
                    {
                        case SButton.Up:
                            break;
                        case SButton.Right:
                            v = new Vector2(1, 0);
                            break;
                        case SButton.Down:
                            v = new Vector2(0, 1);
                            break;
                        default:
                            v = new Vector2(-1, 0);
                            break;
                    }
                }
            }


            abigailTicks.Value++;
            if (abigailTicks.Value > 80000 / 16f)
            {
                if (Game1.player.currentLocation.characters.ToList().FindAll((n) => (n is Monster)).Count > 0)
                    return;

                abigailTicks.Value = -1;
                Game1.player.hat.Value = null;
                Game1.stopMusicTrack(StardewValley.GameData.MusicContext.Default);

                if (!Game1.player.mailReceived.Contains("ScubaFins"))
                {
                    Game1.playSound("Cowboy_Secret");
                    SwimMaps.AddScubaChest(Game1.player.currentLocation, new Vector2(8, 8), "ScubaFins");
                }

                Game1.player.currentLocation.setMapTile(8, 16, 91, "Buildings", null);
                Game1.player.currentLocation.setMapTile(9, 16, 92, "Buildings", null);
                Game1.player.currentLocation.setTileProperty(9, 16, "Back", "Water", "T");
                Game1.player.currentLocation.setMapTile(10, 16, 93, "Buildings", null);
                Game1.player.currentLocation.setMapTile(8, 17, 107, "Buildings", null);
                Game1.player.currentLocation.setMapTile(9, 17, 108, "Back", null);
                Game1.player.currentLocation.setTileProperty(9, 17, "Back", "Water", "T");
                Game1.player.currentLocation.removeTile(9, 17, "Buildings");
                Game1.player.currentLocation.setMapTile(10, 17, 109, "Buildings", null);
                Game1.player.currentLocation.setMapTile(8, 18, 139, "Buildings", null);
                Game1.player.currentLocation.setMapTile(9, 18, 140, "Buildings", null);
                Game1.player.currentLocation.setMapTile(10, 18, 141, "Buildings", null);
                SwimMaps.AddWaterTiles(Game1.player.currentLocation);
            }
            else
            {
                if (Game1.random.NextDouble() < 0.03)
                {
                    int which = Game1.random.Next(3);
                    Point p = new Point();
                    switch (Game1.random.Next(4))
                    {
                        case 0:
                            p = new Point(8 + which, 1);
                            break;
                        case 1:
                            p = new Point(1, 8 + which);
                            break;
                        case 2:
                            p = new Point(8 + which, 16);
                            break;
                        case 3:
                            p = new Point(16, 8 + which);
                            break;
                    }
                    Game1.player.currentLocation.characters.Add(new AbigailMetalHead(new Vector2(p.X * Game1.tileSize, p.Y * Game1.tileSize), 0));
                }

            }
        }

    }
}
