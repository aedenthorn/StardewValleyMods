using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;

namespace Quotes
{
    public class ModEntry : Mod 
    {
        public static ModEntry context;

        public static ModConfig Config;
        public static Random myRand;

        public static string[] quotestrings = new string[0];
        public static List<Quote> quotes = new List<Quote>();
        public static float lastFadeAlpha = 1f;
        public static Quote dailyQuote;
        public static int displayTicks = 0;
        public static bool clickedOnQuote = true;
        public static List<string> seasons = new List<string>
        {
            "spring",
            "summer",
            "fall",
            "winter"
        };
        public static IMobilePhoneApi mobileAPI;
        public static IModHelper SHelper;
        public static IMonitor SMonitor { get; private set; }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();

            SHelper = helper;
            SMonitor = Monitor;

            myRand = new Random(Guid.NewGuid().GetHashCode());

            LoadQuotes();

            if(quotestrings.Length > 0)
            {
                Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
                Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
                Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
                if(Config.ClickToDispelQuote || Config.QuoteDurationPerLineMult < 0)
                    Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            if (Config.EnableApp && Config.EnableMod)
            {
                mobileAPI = Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");
                if (mobileAPI != null)
                {
                    Texture2D appIcon = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "app_icon.png"));
                    bool success = mobileAPI.AddApp(Helper.ModRegistry.ModID, "Random Quote", ShowRandomQuote, appIcon);
                    Monitor.Log($"loaded phone app successfully: {success}", LogLevel.Debug);
                }
            }
        }

        public override object GetApi()
        {
            return new QuotesAPI();
        }


        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "App Enabled",
                getValue: () => Config.EnableApp,
                setValue: value => Config.EnableApp = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Daily Quote",
                getValue: () => Config.ShowDailyQuote,
                setValue: value => Config.ShowDailyQuote = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Click To Dispel Quote",
                getValue: () => Config.ClickToDispelQuote,
                setValue: value => Config.ClickToDispelQuote = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Random Quote",
                getValue: () => Config.RandomQuote,
                setValue: value => Config.RandomQuote = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Characters Per Line",
                getValue: () => Config.QuoteCharPerLine,
                setValue: value => Config.QuoteCharPerLine = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Quote Width",
                getValue: () => Config.QuoteWidth,
                setValue: value => Config.QuoteWidth = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Line Spacing",
                getValue: () => Config.LineSpacing,
                setValue: value => Config.LineSpacing = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Author Prefix",
                getValue: () => Config.AuthorPrefix,
                setValue: value => Config.AuthorPrefix = value
            );
        }

        public static void ShowRandomQuote()
        {
            if (!Config.EnableMod)
                return;
            if (clickedOnQuote == false)
                return;

            dailyQuote = GetAQuote(true);

            if (dailyQuote != null)
            {
                Game1.drawObjectDialogue($"{dailyQuote.quote}\r\n\r\n{Config.AuthorPrefix}{dailyQuote.author}");
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (!clickedOnQuote)
                clickedOnQuote = true;
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if (!Config.EnableMod || !Config.ShowDailyQuote)
                return;
            lastFadeAlpha = 1f;
            displayTicks = 0;
            clickedOnQuote = false;
            dailyQuote = GetAQuote(Config.RandomQuote);
            if (dailyQuote != null)
            {
                Monitor.Log($"Today's quote: {dailyQuote.quote}\r\n\r\n-- {dailyQuote.author}", LogLevel.Debug);
                Helper.Events.Display.Rendering += Display_Rendering;
                Helper.Events.Display.Rendered += Display_Rendered;
            }
        }


        private void Display_Rendering(object sender, StardewModdingAPI.Events.RenderingEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (Game1.fadeToBlackAlpha > 0)
            {
                if ((Config.QuoteDurationPerLineMult < 0 || ++displayTicks < Config.QuoteDurationPerLineMult * dailyQuote.quoteLines.Count * 200) && !clickedOnQuote)
                {

                    Game1.fadeToBlackAlpha = 1f;
                    return;
                }

                Game1.fadeToBlackAlpha = lastFadeAlpha - 0.005f * Config.QuoteFadeMult;
                lastFadeAlpha = Game1.fadeToBlackAlpha;
            }
            else
            {
                Monitor.Log($"fade in completed");
                Helper.Events.Display.Rendering -= Display_Rendering;
                Helper.Events.Display.Rendered -= Display_Rendered;
                lastFadeAlpha = 1f;
                displayTicks = 0;
                clickedOnQuote = true;
            }
        }
        private void Display_Rendered(object sender, StardewModdingAPI.Events.RenderedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            Color thisColor = Config.QuoteColor;
            thisColor.A = (byte)Math.Round(255 * Game1.fadeToBlackAlpha);
            int lineSpacing = Game1.dialogueFont.LineSpacing + Config.LineSpacing;

            for (int i = 0; i < dailyQuote.quoteLines.Count; i++)
            {
                e.SpriteBatch.DrawString(Game1.dialogueFont, dailyQuote.quoteLines[i], new Vector2(Game1.viewport.Width / 2 - (Config.QuoteCharPerLine * 10), Game1.viewport.Height / 2 - (dailyQuote.quoteLines.Count / 2) * lineSpacing + lineSpacing * i), thisColor);
            }
            if (dailyQuote.author != null)
                e.SpriteBatch.DrawString(Game1.dialogueFont, Config.AuthorPrefix + dailyQuote.author, new Vector2(Game1.viewport.Width / 2 + (Config.QuoteCharPerLine * 10) - (dailyQuote.author.Length * 20 + Config.AuthorPrefix.Length * 20), Game1.viewport.Height / 2 - (dailyQuote.quoteLines.Count / 2) * lineSpacing + lineSpacing * dailyQuote.quoteLines.Count), thisColor);


            //SpriteText.drawString(e.SpriteBatch, dailyQuote.quote, x, y, 999999, Config.QuoteWidth, 999999, Game1.fadeToBlackAlpha, 0.88f, false, -1, "", colorCode, SpriteText.ScrollTextAlignment.Left);
            //SpriteText.drawString(e.SpriteBatch, Config.AuthorPrefix + dailyQuote.author, x, y + (int)Math.Ceiling(dailyQuote.quoteSize.X / Config.QuoteWidth) * Game1.dialogueFont.LineSpacing, 999999, Config.QuoteWidth, 999999, Game1.fadeToBlackAlpha, 0.88f, false, -1, "", colorCode, SpriteText.ScrollTextAlignment.Right);
        }

        private static void LoadQuotes()
        {

            string file = Path.Combine(SHelper.DirectoryPath, "assets", "quotes.txt");
            if (!File.Exists(file))
            {
                SMonitor.Log($"No quotes.txt file, using quotes_default.txt", LogLevel.Debug);
                file = Path.Combine(SHelper.DirectoryPath, "assets", "quotes_default.txt");
            }
            if (File.Exists(file))
            {
                quotestrings = File.ReadAllLines(file);
                SMonitor.Log($"Loaded {quotestrings.Length} quotes from {file}", LogLevel.Debug);
                foreach(string quote in quotestrings)
                {
                    if(quote.Length > 0)
                        quotes.Add(new Quote(quote));
                }
            }
            else
            {
                SMonitor.Log($"Quotes file not found at {file}!", LogLevel.Warn);
            }
        }

        public static Quote GetAQuote(bool random)
        {
            if (quotes.Count == 0)
                return null;

            int dayIdx = Game1.dayOfMonth + seasons.IndexOf(Game1.currentSeason) * 28 - 1;
            int idx = (random || quotes.Count <= dayIdx ) ? myRand.Next(quotes.Count) : dayIdx;
            return quotes[idx];
        }
    }
}
