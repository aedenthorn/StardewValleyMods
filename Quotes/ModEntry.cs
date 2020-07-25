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

		internal static ModConfig Config;
        private Random myRand;

        public static string[] quotestrings = new string[0];
        public static List<Quote> quotes = new List<Quote>();
        private float lastFadeAlpha = 1f;
        private Quote dailyQuote;
        private int displayTicks = 0;
        private bool clickedOnQuote = true;
        private List<string> seasons = new List<string>
        {
            "spring",
            "summer",
            "fall",
            "winter"
        };
        private IMobilePhoneApi api;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
		{
			context = this;
			Config = Helper.ReadConfig<ModConfig>();
			if (!Config.EnableMod)
				return;

			myRand = new Random(Guid.NewGuid().GetHashCode());

            LoadQuotes();

            if(quotestrings.Length > 0)
            {
                Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
                Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
                if(Config.ClickToDispelQuote || Config.QuoteDurationPerLineMult < 0)
                    Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            api = Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");
            if (api != null)
            {
                Texture2D appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "app_icon.png"));
                bool success = api.AddApp(Helper.ModRegistry.ModID, "Random Quote", ShowRandomQuote, appIcon);
                Monitor.Log($"loaded phone app successfully: {success}", LogLevel.Debug);
            }
        }

        private void ShowRandomQuote()
        {
            lastFadeAlpha = 1f;
            displayTicks = 0;
            clickedOnQuote = false;
            dailyQuote = GetAQuote(true);
            if (dailyQuote != null)
            {
                Game1.drawObjectDialogue($"{dailyQuote.quote}\r\n\r\n{Config.AuthorPrefix}{dailyQuote.author}");
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!clickedOnQuote)
                clickedOnQuote = true;
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            lastFadeAlpha = 1f;
            displayTicks = 0;
            clickedOnQuote = false;
            dailyQuote = GetAQuote(Config.RandomQuote);
            if(dailyQuote != null)
            {
                Helper.Events.Display.Rendering += Display_Rendering;
                Helper.Events.Display.Rendered += Display_Rendered;
            }
        }

        private void Display_Rendering(object sender, StardewModdingAPI.Events.RenderingEventArgs e)
        {
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
            Color thisColor = Config.QuoteColor;
            thisColor.A = (byte)Math.Round(255 * Game1.fadeToBlackAlpha);
            for (int i = 0; i < dailyQuote.quoteLines.Count; i++)
            {
                e.SpriteBatch.DrawString(Game1.dialogueFont, dailyQuote.quoteLines[i], new Vector2(Game1.viewport.Width / 2 - (Config.QuoteCharPerLine * 10), Game1.viewport.Height / 2 - (dailyQuote.quoteLines.Count / 2) * 45 + 45 * i), thisColor);
            }
            if (dailyQuote.author != null)
                e.SpriteBatch.DrawString(Game1.dialogueFont, Config.AuthorPrefix + dailyQuote.author, new Vector2(Game1.viewport.Width / 2 + (Config.QuoteCharPerLine * 10) - (dailyQuote.author.Length * 20 + Config.AuthorPrefix.Length * 20), Game1.viewport.Height / 2 - (dailyQuote.quoteLines.Count / 2) * 45 + 45 * dailyQuote.quoteLines.Count), thisColor);
        }

        private void LoadQuotes()
        {
            string file = Path.Combine(Helper.DirectoryPath, "assets", "quotes.txt");
            if (!File.Exists(file))
            {
                Monitor.Log($"No quotes.txt file, using quotes_default.txt", LogLevel.Debug);
                file = Path.Combine(Helper.DirectoryPath, "assets", "quotes_default.txt");
            }
            if (File.Exists(file))
            {
                quotestrings = File.ReadAllLines(file);
                Monitor.Log($"Loaded {quotestrings.Length} quotes from {file}", LogLevel.Debug);
                foreach(string quote in quotestrings)
                {
                    if(quote.Length > 0)
                        quotes.Add(new Quote(quote));
                }
            }
            else
            {
                Monitor.Log($"Quotes file not found at {file}!", LogLevel.Error);
            }
        }

        private Quote GetAQuote(bool random)
        {
            if (quotes.Count == 0)
                return null;

            int dayIdx = Game1.dayOfMonth + seasons.IndexOf(Game1.currentSeason) * 28 - 1;
            int idx = (random || quotes.Count <= dayIdx ) ? myRand.Next(quotes.Count) : dayIdx;
            Monitor.Log($"Today's quote (#{idx + 1}): {quotes[idx].quote}\r\n\r\n-- {quotes[idx].author}", LogLevel.Debug);
            return quotes[idx];
        }
    }
}
