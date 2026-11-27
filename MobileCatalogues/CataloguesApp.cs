using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;

namespace MobileCatalogues
{
    internal class CataloguesApp
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;
        private static IMobilePhoneApi api;
        public static bool opening;
        public static List<string> catalogueList = new List<string>();

        public static void Initialize(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }
        internal static void OpenCatalogueApp()
        {
            api = ModEntry.api;
            Helper.Events.Input.ButtonPressed += HelperEvents.Input_ButtonPressed;
            Helper.Events.Input.MouseWheelScrolled += HelperEvents.Input_MouseWheelScrolled; ;
            api.SetAppRunning(true);
            api.SetRunningApp(Helper.ModRegistry.ModID);
            Helper.Events.Display.RenderedWorld += Visuals.Display_RenderedWorld;
            opening = true;
        }


        public static void CloseApp()
        {
            api.SetAppRunning(false);
            api.SetRunningApp(null);
            Helper.Events.Input.ButtonPressed -= HelperEvents.Input_ButtonPressed;
            Helper.Events.Input.MouseWheelScrolled -= HelperEvents.Input_MouseWheelScrolled; ;
            Helper.Events.Display.RenderedWorld -= Visuals.Display_RenderedWorld;
        }


        public static void ClickRow(Point mousePos)
        {
            int idx = (int)((mousePos.Y - api.GetScreenPosition().Y - Config.MarginY - Visuals.offsetY - Config.AppHeaderHeight) / (Config.MarginY + Config.AppRowHeight));
            Monitor.Log($"clicked index: {idx}");
            if (idx < catalogueList.Count && idx >= 0)
            {
                if (!Config.RequireCataloguePurchase || Game1.player.mailReceived.Contains($"BoughtCatalogue{catalogueList[idx]}"))
                {
                    Catalogues.OpenCatalogue(catalogueList[idx]);
                }
                else
                {
                    PurchaseCatalogue(catalogueList[idx]);
                }
            }
        }

        internal static int GetCataloguePrice(string name)
        {
            if (Config.CataloguePrices is null)
            {
                Config.CataloguePrices = new();
                foreach (var kvp in DataLoader.Shops(Game1.content))
                {
                    Config.CataloguePrices[kvp.Key] = -1;
                }
                Helper.WriteConfig(Config);
            }
            else if (Config.CataloguePrices.TryGetValue(name, out var price) && price >= 0)
                return price;
            return Config.DefaultPrice;
        }
        internal static void PurchaseCatalogue(string id)
        {
            int price = GetCataloguePrice(id);
            string name = id;
            Response[] responses = new Response[]
            {
                new Response($"Yes_{price}_{name}", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")),
                new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No"))
            };
            Game1.player.currentLocation.createQuestionDialogue(string.Format(Helper.Translation.Get("buy-catalogue-question"), name, price), responses, DoPurchaseCatalogue);
        }

        private static void DoPurchaseCatalogue(Farmer who, string whichAnswer)
        {
            if (whichAnswer.StartsWith("Yes_"))
            {
                string[] parts = whichAnswer.Split('_', 3);

                if(who.Money < int.Parse(parts[1]))
                {
                    Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("not-enough-money"));
                    return;
                }

                who.mailReceived.Add($"BoughtCatalogue{parts[2]}");
                who.Money -= int.Parse(parts[1]);
                Game1.addHUDMessage(new HUDMessage(string.Format(Helper.Translation.Get("bought-catalogue"), parts[2]), 1));
            }
        }
    }
}