using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;

namespace MobileCatalogues
{
    internal class CataloguesApp
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;
        private static IMobilePhoneApi api;
        public static bool opening;
        internal static List<string> catalogueList = new List<string>();

        // call this method from your Entry class
        public static void Initialize(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;

            if (Config.EnableCatalogue)
                catalogueList.Add("catalogue");
            if (Config.EnableFurnitureCatalogue)
                catalogueList.Add("furniture-catalogue");
            if (Config.EnableSeedCatalogue)
                catalogueList.Add("seed-catalogue");
            if (Config.EnableTravelingCatalogue)
                catalogueList.Add("travel-catalogue");
            if (Config.EnableDesertCatalogue)
                catalogueList.Add("desert-catalogue");
            if (Config.EnableHatCatalogue)
                catalogueList.Add("hat-catalogue");
            if (Config.EnableClothingCatalogue)
                catalogueList.Add("clothing-catalogue");
            if (Config.EnableDwarfCatalogue)
                catalogueList.Add("dwarf-catalogue");
            if (Config.EnableKrobusCatalogue)
                catalogueList.Add("krobus-catalogue");
            if (Config.EnableGuildCatalogue)
                catalogueList.Add("guild-catalogue");
        }
        internal static void OpenCatalogueApp()
        {
            api = ModEntry.api;
            Helper.Events.Input.ButtonPressed += HelperEvents.Input_ButtonPressed;
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
            Helper.Events.Display.RenderedWorld -= Visuals.Display_RenderedWorld;
        }


        public static void ClickRow(Point mousePos)
        {
            int idx = (int)((mousePos.Y - api.GetScreenPosition().Y - Config.MarginY - Visuals.offsetY - Config.AppHeaderHeight) / (Config.MarginY + Config.AppRowHeight));
            Monitor.Log($"clicked index: {idx}");
            if (idx < catalogueList.Count && idx >= 0)
            {
                if (!Config.RequireCataloguePurchase || Game1.player.mailReceived.Contains($"BoughtCatalogue{Helper.Translation.Get(catalogueList[idx])}"))
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
            switch (name)
            {
                case "catalogue":
                    return Config.PriceCatalogue;
                case "furniture-catalogue":
                    return Config.PriceFurnitureCatalogue;
                case "seed-catalogue":
                    return Config.PriceSeedCatalogue;
                case "travel-catalogue":
                    return Config.PriceTravelingCatalogue;
                case "desert-catalogue":
                    return Config.PriceDesertCatalogue;
                case "hat-catalogue":
                    return Config.PriceHatCatalogue;
                case "clothing-catalogue":
                    return Config.PriceClothingCatalogue;
                case "dwarf-catalogue":
                    return Config.PriceDwarfCatalogue;
                case "krobus-catalogue":
                    return Config.PriceKrobusCatalogue;
                case "guild-catalogue":
                    return Config.PriceGuildCatalogue;
            }
            return 0;
        }
        internal static void PurchaseCatalogue(string id)
        {
            int price = GetCataloguePrice(id);
            string name = Helper.Translation.Get(id);
            Response[] responses = new Response[]
            {
                new Response($"Yes_{name}_{price}", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")),
                new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No"))
            };
            Game1.player.currentLocation.createQuestionDialogue(string.Format(Helper.Translation.Get("buy-catalogue-question"), name, price), responses, DoPurchaseCatalogue);
        }

        private static void DoPurchaseCatalogue(Farmer who, string whichAnswer)
        {
            if (whichAnswer.StartsWith("Yes_"))
            {
                string[] parts = whichAnswer.Split('_');

                if(who.Money < int.Parse(parts[2]))
                {
                    Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("not-enough-money"));
                    return;
                }

                who.mailReceived.Add($"BoughtCatalogue{parts[1]}");
                who.Money -= int.Parse(parts[2]);
                Game1.addHUDMessage(new HUDMessage(string.Format(Helper.Translation.Get("bought-catalogue"), parts[1]), 1));
            }
        }
    }
}