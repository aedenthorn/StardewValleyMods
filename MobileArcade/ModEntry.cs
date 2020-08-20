using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MobileArcade
{
    public class ModEntry : Mod
    {
        public static ModEntry context;

        internal static ModConfig Config;

        private IMobilePhoneApi api;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            api = Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");
            if (api != null)
            {
                Texture2D appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "app_icon_prairie.png"));
                bool success = api.AddApp(Helper.ModRegistry.ModID + "Prairie", "Prairie King", OpenPrairieKing, appIcon);
                Monitor.Log($"loaded Prairie King app successfully: {success}", LogLevel.Debug);
                appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "app_icon_junimo.png"));
                success = api.AddApp(Helper.ModRegistry.ModID + "Junimo", "Junimo Kart", OpenJunimoKart, appIcon);
                Monitor.Log($"loaded Junimo Kart app successfully: {success}", LogLevel.Debug);
            }
        }

        private void OpenPrairieKing()
        {
            api.SetPhoneOpened(false);
            Game1.currentMinigame = new AbigailGame(false);
        }

        private void OpenJunimoKart()
        {
            api.SetPhoneOpened(false);
            Response[] junimoKartOptions = new Response[]
            {
                new Response("Progress", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_ProgressMode")),
                new Response("Endless", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_EndlessMode")),
                new Response("Exit", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Exit"))
            };
            Game1.player.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Menu"), junimoKartOptions, RunJunimoKart);
        }

        private void RunJunimoKart(Farmer who, string whichAnswer)
        {
            if(whichAnswer == "Progress")
            {
                Game1.currentMinigame = new MineCart(0, 3);
            }
            else if (whichAnswer == "Endless")
            {
                Game1.currentMinigame = new MineCart(0, 2);
            }
        }
    }
}
