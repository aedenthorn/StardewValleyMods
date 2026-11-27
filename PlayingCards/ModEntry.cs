using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Collections.Generic;

namespace PlayingCards
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        private static Texture2D cardTexture;
        private static Texture2D backTexture;
        private static readonly string deckKey = "aedenthorn.PlayingCards/deck";
        private static readonly string cardPath = "Mods/aedenthorn.PlayingCards/cards";
        private static readonly string backPath = "Mods/aedenthorn.PlayingCards/backs";

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;


            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.placementAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_placementAction_prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.draw), new System.Type[] { typeof(SpriteBatch),typeof(int),typeof(int),typeof(float) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_draw_prefix_1))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.draw), new System.Type[] { typeof(SpriteBatch),typeof(int),typeof(int),typeof(float),typeof(float) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_draw_prefix_2))
            );
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            try
            {
                cardTexture = Game1.content.Load<Texture2D>(cardPath);
            }
            catch
            {
                cardTexture = Helper.Content.Load<Texture2D>("assets/cards.png");
            }

            try
            {
                backTexture = Game1.content.Load<Texture2D>(backPath);
            }
            catch
            {
                backTexture = Helper.Content.Load<Texture2D>("assets/backs.png");
            }
        }

        private void Input_ButtonsChanged(object sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
        {
            if (Context.IsPlayerFree && Config.CreateDeckButton.JustPressed())
            {
                Monitor.Log("Creating new deck");
                Object deck = new Object(Vector2.Zero, 0);
                deck.modData[deckKey] = new CardDeck().GetDeckString();
                Game1.player.addItemToInventoryBool(deck, true);
            }
            else if (Context.IsPlayerFree && Config.ShuffleDeckButton.JustPressed() && Game1.player.CurrentItem is Object && (Game1.player.CurrentItem as Object).modData.TryGetValue(deckKey, out string deckString))
            {
                Monitor.Log("Shuffling deck");
                CardDeck deck = new CardDeck(deckString);
                deck.ShuffleCards();
                Game1.player.CurrentItem.modData[deckKey] = deck.GetDeckString();
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
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
                    name: () => "Mod Enabled",
                    getValue: () => Config.EnableMod,
                    setValue: value => Config.EnableMod = value
                );
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => "Deal Down",
                    getValue: () => Config.DealDownModButton,
                    setValue: value => Config.DealDownModButton = value
                );
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => "Deal Up",
                    getValue: () => Config.DealUpModButton,
                    setValue: value => Config.DealUpModButton = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => "Create Deck",
                    getValue: () => Config.CreateDeckButton.ToString(),
                    setValue: delegate (string value) { try { Config.CreateDeckButton = KeybindList.Parse(value); } catch { }; }
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => "Shuffle Deck",
                    getValue: () => Config.ShuffleDeckButton.ToString(),
                    setValue: delegate (string value) { try { Config.ShuffleDeckButton = KeybindList.Parse(value); } catch { }; }
                );
            }

        }
    }
}